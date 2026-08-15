// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

using System.Buffers;



namespace Jakar.Shapes;


/// <summary>
/// Least-squares polynomial regression over a <see cref="Spline"/>'s points.
/// <para>
/// A positive <c> primaryPower </c> of <c> d </c> fits the FULL polynomial of that degree --
/// <c> a_d*x^d + a_(d-1)*x^(d-1) + ... + a_1*x + a_0 </c> -- not just the leading term and a constant.
/// A negative <c> d </c> fits the Laurent mirror <c> a_d*x^-d + ... + a_1*x^-1 + a_0 </c>, and <c> 0 </c> fits the
/// constant <c> a_0 </c> (the mean of Y).
/// </para>
/// <para>
/// When <c> primaryPower </c> is <see langword="null"/> the degree is chosen automatically. Because polynomial models
/// are nested -- a degree-6 fit can always reproduce a degree-3 fit -- raw squared error never rises with degree and
/// would always select the maximum. Instead the lowest degree is chosen that no higher degree improves on
/// significantly, measured by an F-ratio against the residual variance. On data that lies exactly on a polynomial this
/// recovers the generating degree; on noisy data it prefers the simplest equation that explains the spread.
/// </para>
/// <para>
/// Coefficients are solved by Householder QR on the Vandermonde matrix rather than by normal equations. Normal
/// equations square the condition number and lose up to eight significant digits at degree 6; QR holds machine
/// precision across the same inputs.
/// </para>
/// </summary>
public static class LineOfBestFit
{
    /// <summary> Inclusive bound on the magnitude of the degree searched when <c> primaryPower </c> is <see langword="null"/>. </summary>
    public const sbyte MAX_AUTO_SEARCH_POWER = 6;

    /// <summary> Maximum term count for any candidate, i.e. <c> MAX_AUTO_SEARCH_POWER + 1 </c>. </summary>
    private const int MAX_TERMS = MAX_AUTO_SEARCH_POWER + 1;

    /// <summary> Candidate count across the whole search: degree 0, then +/-1 through +/-<see cref="MAX_AUTO_SEARCH_POWER"/>. </summary>
    private const int CANDIDATE_COUNT = ( 2 * MAX_AUTO_SEARCH_POWER ) + 1;

    /// <summary>
    /// F-ratio a higher degree must exceed before it is considered a genuine improvement. Raising this favours simpler
    /// equations; lowering it admits more terms. 4.0 is the conventional ~5% significance threshold.
    /// </summary>
    private const double SIGNIFICANCE_THRESHOLD = 4.0;

    /// <summary> Machine epsilon -- the gap between 1.0 and the next <see cref="double"/>. NOT <see cref="double.Epsilon"/>, which is the smallest subnormal. </summary>
    private const double MACHINE_EPSILON = 2.220446049250313E-16;

    /// <summary> Slack multiplier over the theoretical rounding bound, covering accumulation across the sums. </summary>
    private const double NOISE_FLOOR_SLACK = 16.0;

    /// <summary> Pivots below this are treated as rank deficiency. </summary>
    private const double RANK_TOLERANCE = 1e-300;

    /// <summary> Point count above which scratch buffers come from <see cref="ArrayPool{T}"/> instead of the stack. </summary>
    private const int STACK_LIMIT = 128;


    /// <summary> Fits the best equation and returns it as an evaluable line. See <see cref="Fit(ReadOnlySpan{ReadOnlyPoint}, sbyte?)"/> for the coefficients. </summary>
    [Pure] public static CalculatedLine Calculate( ref readonly Spline line, sbyte? primaryPower = null ) => Fit(line.Span, primaryPower).ToCalculatedLine();

    /// <summary> Fits the points directly, with no <see cref="Spline"/> in between. </summary>
    [Pure] public static CalculatedLine Calculate( ReadOnlySpan<ReadOnlyPoint> points, sbyte? primaryPower = null ) => Fit(points, primaryPower).ToCalculatedLine();


    /// <inheritdoc cref="Fit(ReadOnlySpan{ReadOnlyPoint}, sbyte?)"/>
    [Pure] public static PolynomialFit Fit( ref readonly Spline line, sbyte? primaryPower = null ) => Fit(line.Span, primaryPower);


    /// <summary>
    /// Fits the best equation and returns its coefficients, degree and residual error.
    /// <para>
    /// This overload takes the points directly, so a stackalloc buffer can be fitted without materialising a
    /// <see cref="Spline"/>. Nothing is allocated beyond the returned coefficients:
    /// <code>
    /// Span&lt;ReadOnlyPoint&gt; points = stackalloc ReadOnlyPoint[count];
    /// PolynomialFit fit = LineOfBestFit.Fit(points);
    /// </code>
    /// </para>
    /// </summary>
    /// <param name="points"> Points to fit. Fewer than two cannot determine a line. </param>
    /// <param name="primaryPower"> Degree to fit, or <see langword="null"/> to choose one automatically. </param>
    [Pure] public static PolynomialFit Fit( ReadOnlySpan<ReadOnlyPoint> points, sbyte? primaryPower = null )
    {
        if ( points.Length is 0 or 1 ) { return PolynomialFit.Invalid; }

        return primaryPower is { } degree
                   ? FitDegree(points, degree)
                   : FindBestFit(points);
    }


    [Pure] private static PolynomialFit FitDegree( ReadOnlySpan<ReadOnlyPoint> points, sbyte degree )
    {
        if ( degree is > MAX_AUTO_SEARCH_POWER or < -MAX_AUTO_SEARCH_POWER ) { return PolynomialFit.Invalid; }

        Span<double> coefficients = stackalloc double[MAX_TERMS];

        return TrySolve(points, degree, coefficients, out int terms, out double error)
                   ? new PolynomialFit(coefficients[..terms], degree, error)
                   : PolynomialFit.Invalid;
    }


    /// <summary> Maps a search index onto the degrees <c> 0, 1, -1, 2, -2, ... </c> so simpler equations are considered first. </summary>
    [Pure] private static sbyte GetSearchDegree( int index ) => (sbyte)( ( index + 1 ) / 2 * ( index % 2 == 0
                                                                                                   ? -1
                                                                                                   : 1 ) );


    [Pure] private static PolynomialFit FindBestFit( ReadOnlySpan<ReadOnlyPoint> points )
    {
        int cap = Math.Min(MAX_AUTO_SEARCH_POWER, points.Length - 1);

        Span<double> allCoefficients = stackalloc double[CANDIDATE_COUNT * MAX_TERMS];
        Span<double> errors          = stackalloc double[CANDIDATE_COUNT];
        Span<int>    termCounts      = stackalloc int[CANDIDATE_COUNT];
        Span<sbyte>  degrees         = stackalloc sbyte[CANDIDATE_COUNT];
        int          found           = 0;
        double       best            = double.PositiveInfinity;

        for ( int i = 0; i < CANDIDATE_COUNT; i++ )
        {
            sbyte degree = GetSearchDegree(i);
            if ( Math.Abs(degree) > cap ) { continue; }

            Span<double> slot = allCoefficients.Slice(found * MAX_TERMS, MAX_TERMS);
            if ( !TrySolve(points, degree, slot, out int terms, out double error) ) { continue; }

            errors[found]     = error;
            termCounts[found] = terms;
            degrees[found]    = degree;
            found++;

            if ( error < best ) { best = error; }
        }

        if ( found is 0 ) { return PolynomialFit.Invalid; }

        double floor = GetNoiseFloor(points);

        // an exact fit exists: take the simplest degree that reaches it
        if ( best <= floor )
        {
            for ( int i = 0; i < found; i++ )
            {
                if ( errors[i] <= floor ) { return Build(allCoefficients, termCounts, degrees, errors, i); }
            }
        }

        // otherwise take the simplest degree that no higher degree significantly improves upon
        for ( int i = 0; i < found; i++ )
        {
            if ( IsBeaten(points.Length, errors, termCounts, degrees, found, i, floor) ) { continue; }

            return Build(allCoefficients, termCounts, degrees, errors, i);
        }

        return Build(allCoefficients, termCounts, degrees, errors, 0);
    }


    /// <summary> True when some higher-degree candidate reduces the error by more than chance would explain. </summary>
    [Pure]
    private static bool IsBeaten( int pointCount, ReadOnlySpan<double> errors, ReadOnlySpan<int> termCounts, ReadOnlySpan<sbyte> degrees, int found, int index, double floor )
    {
        double error = errors[index];

        for ( int j = 0; j < found; j++ )
        {
            if ( Math.Abs(degrees[j]) <= Math.Abs(degrees[index]) ) { continue; }

            int extra = Math.Abs(degrees[j]) - Math.Abs(degrees[index]);
            int dof   = pointCount - termCounts[j];
            if ( dof <= 0 || extra <= 0 ) { continue; }

            double richer = Math.Max(errors[j], floor);
            if ( richer <= 0 ) { continue; }

            double ratio = ( ( error - errors[j] ) / extra ) / ( richer / dof );
            if ( ratio > SIGNIFICANCE_THRESHOLD ) { return true; }
        }

        return false;
    }


    [Pure]
    private static PolynomialFit Build( ReadOnlySpan<double> allCoefficients, ReadOnlySpan<int> termCounts, ReadOnlySpan<sbyte> degrees, ReadOnlySpan<double> errors, int index ) =>
        new(allCoefficients.Slice(index * MAX_TERMS, termCounts[index]), degrees[index], errors[index]);


    /// <summary> The squared-error level below which two candidates are indistinguishable given <see cref="double"/> precision. </summary>
    [Pure] private static double GetNoiseFloor( ReadOnlySpan<ReadOnlyPoint> points )
    {
        double sumOfSquares = 0.0;
        foreach ( ref readonly ReadOnlyPoint point in points ) { sumOfSquares += point.Y * point.Y; }

        double floor = NOISE_FLOOR_SLACK * points.Length * MACHINE_EPSILON * MACHINE_EPSILON * sumOfSquares;

        return double.IsFinite(floor)
                   ? floor
                   : 0.0;
    }


    /// <summary>
    /// Solves the least-squares system for one degree by Householder QR on the Vandermonde matrix.
    /// </summary>
    private static bool TrySolve( ReadOnlySpan<ReadOnlyPoint> points, sbyte degree, Span<double> coefficients, out int terms, out double sumOfSquaredError )
    {
        terms             = Math.Abs(degree) + 1;
        sumOfSquaredError = double.PositiveInfinity;

        int rows = points.Length;
        if ( rows < terms ) { return false; }

        int       cells  = rows * terms;
        double[]? rented = rows > STACK_LIMIT
                               ? ArrayPool<double>.Shared.Rent(cells + ( 2 * rows ))
                               : null;

        Span<double> scratch = rented is null
                                   ? stackalloc double[( STACK_LIMIT * MAX_TERMS ) + ( 2 * STACK_LIMIT )]
                                   : rented;

        try
        {
            Span<double> matrix = scratch[..cells];
            Span<double> rhs    = scratch.Slice(cells,        rows);
            Span<double> vector = scratch.Slice(cells + rows, rows);

            if ( !TryBuildVandermonde(points, degree, terms, matrix, rhs) ) { return false; }
            if ( !TryDecompose(rows, terms, matrix, rhs, vector) ) { return false; }
            if ( !TryBackSubstitute(rows, terms, matrix, rhs, coefficients) ) { return false; }

            sumOfSquaredError = Score(points, degree, terms, coefficients);
            return double.IsFinite(sumOfSquaredError);
        }
        finally
        {
            if ( rented is not null ) { ArrayPool<double>.Shared.Return(rented); }
        }
    }


    private static bool TryBuildVandermonde( ReadOnlySpan<ReadOnlyPoint> points, sbyte degree, int terms, Span<double> matrix, Span<double> rhs )
    {
        for ( int row = 0; row < points.Length; row++ )
        {
            double y = points[row].Y;
            if ( !double.IsFinite(y) ) { return false; }

            double t = degree < 0
                           ? 1.0 / points[row].X
                           : points[row].X;

            if ( !double.IsFinite(t) ) { return false; }

            double value = 1.0;
            int    start = row * terms;

            for ( int column = 0; column < terms; column++ )
            {
                if ( !double.IsFinite(value) ) { return false; }

                matrix[start + column] =  value;
                value                  *= t;
            }

            rhs[row] = y;
        }

        return true;
    }


    /// <summary> In-place Householder reduction of <paramref name="matrix"/> to upper triangular, applying the same reflectors to <paramref name="rhs"/>. </summary>
    private static bool TryDecompose( int rows, int terms, Span<double> matrix, Span<double> rhs, Span<double> vector )
    {
        for ( int k = 0; k < terms; k++ )
        {
            double norm = 0.0;
            for ( int row = k; row < rows; row++ )
            {
                double cell = matrix[( row * terms ) + k];
                norm += cell * cell;
            }

            norm = Math.Sqrt(norm);
            if ( !double.IsFinite(norm) || norm < RANK_TOLERANCE ) { return false; }

            for ( int row = k; row < rows; row++ ) { vector[row] = matrix[( row * terms ) + k]; }

            double alpha = vector[k] >= 0
                               ? -norm
                               : norm;

            vector[k] -= alpha;

            double squared = 0.0;
            for ( int row = k; row < rows; row++ ) { squared += vector[row] * vector[row]; }

            if ( squared < RANK_TOLERANCE ) { continue; }

            for ( int column = k; column < terms; column++ )
            {
                double dot = 0.0;
                for ( int row = k; row < rows; row++ ) { dot += vector[row] * matrix[( row * terms ) + column]; }

                double factor = 2.0 * dot / squared;
                for ( int row = k; row < rows; row++ ) { matrix[( row * terms ) + column] -= factor * vector[row]; }
            }

            double dotRhs = 0.0;
            for ( int row = k; row < rows; row++ ) { dotRhs += vector[row] * rhs[row]; }

            double scale = 2.0 * dotRhs / squared;
            for ( int row = k; row < rows; row++ ) { rhs[row] -= scale * vector[row]; }
        }

        return true;
    }


    private static bool TryBackSubstitute( int rows, int terms, ReadOnlySpan<double> matrix, ReadOnlySpan<double> rhs, Span<double> coefficients )
    {
        // rank deficiency has to be judged RELATIVE to the matrix scale. An absolute floor never trips: points
        // sharing one x give a diagonal around 1e-16 of the leading pivot, which is singular but far above any
        // fixed epsilon.
        double largest = 0.0;
        for ( int i = 0; i < terms; i++ ) { largest = Math.Max(largest, Math.Abs(matrix[( i * terms ) + i])); }

        if ( largest <= RANK_TOLERANCE ) { return false; }

        double tolerance = Math.Max(rows, terms) * MACHINE_EPSILON * largest;

        for ( int i = terms - 1; i >= 0; i-- )
        {
            double sum = rhs[i];
            for ( int j = i + 1; j < terms; j++ ) { sum -= matrix[( i * terms ) + j] * coefficients[j]; }

            double diagonal = matrix[( i * terms ) + i];
            if ( Math.Abs(diagonal) <= tolerance ) { return false; }

            double value = sum / diagonal;
            if ( !double.IsFinite(value) ) { return false; }

            coefficients[i] = value;
        }

        return true;
    }


    [Pure] private static double Score( ReadOnlySpan<ReadOnlyPoint> points, sbyte degree, int terms, ReadOnlySpan<double> coefficients )
    {
        double total = 0.0;

        foreach ( ref readonly ReadOnlyPoint point in points )
        {
            double t = degree < 0
                           ? 1.0 / point.X
                           : point.X;

            double predicted = coefficients[terms - 1];
            for ( int i = terms - 2; i >= 0; i-- ) { predicted = ( predicted * t ) + coefficients[i]; }

            if ( !double.IsFinite(predicted) ) { return double.PositiveInfinity; }

            double residual = point.Y - predicted;
            total += residual * residual;
        }

        return double.IsNaN(total)
                   ? double.PositiveInfinity
                   : total;
    }
}
