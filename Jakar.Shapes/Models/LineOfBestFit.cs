// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

namespace Jakar.Shapes;


/// <summary>
/// Fits a power-regression "line of best fit" to a <see cref="Spline"/>'s points using least squares:
/// <c> y = A * x^primaryPower + b </c>.
/// <para>
/// When <c>primaryPower</c> is <see langword="null"/>, every whole power in
/// <c>[-<see cref="MAX_AUTO_SEARCH_POWER"/>, +<see cref="MAX_AUTO_SEARCH_POWER"/>]</c> is tried and the one with
/// the lowest sum-of-squared-error is returned. Candidates are visited in order of ascending magnitude
/// (<c>0, 1, -1, 2, -2, ...</c>) so that when several powers fit equally well the simplest one wins.
/// </para>
/// </summary>
public static class LineOfBestFit
{
    /// <summary> Inclusive bound on the magnitude of the power searched when <c>primaryPower</c> is <see langword="null"/>. </summary>
    public const sbyte MAX_AUTO_SEARCH_POWER = 6;

    /// <summary> A candidate must beat the incumbent by this relative margin to replace it, so float noise never outranks a simpler power. </summary>
    private const double RELATIVE_IMPROVEMENT = 1e-12;

    /// <summary> Machine epsilon -- the gap between 1.0 and the next <see cref="double"/>. Note this is NOT <see cref="double.Epsilon"/>, which is the smallest subnormal. </summary>
    private const double MACHINE_EPSILON = 2.220446049250313E-16;

    /// <summary> Slack multiplier over the theoretical rounding bound, covering accumulation across the sums. </summary>
    private const double NOISE_FLOOR_SLACK = 16.0;


    [Pure] public static CalculatedLine Calculate( ref readonly Spline line, sbyte? primaryPower = null )
    {
        if ( line.IsEmpty ) { return CalculatedLine.Invalid; }

        ReadOnlySpan<ReadOnlyPoint> points = line.Span;

        return primaryPower is { } power
                   ? Fit(points, power)
                   : FindBestFit(points);
    }


    [Pure] private static CalculatedLine FindBestFit( ReadOnlySpan<ReadOnlyPoint> points )
    {
        double         noiseFloor = GetNoiseFloor(points);
        CalculatedLine best       = CalculatedLine.Invalid;
        double         bestError  = double.PositiveInfinity;

        for ( int i = 0; i <= 2 * MAX_AUTO_SEARCH_POWER; i++ )
        {
            sbyte power = GetSearchPower(i);

            CalculatedLine candidate = Fit(points, power);
            double         error     = SumSquaredError(points, candidate);
            if ( error >= ( bestError * ( 1.0 - RELATIVE_IMPROVEMENT ) ) - noiseFloor ) { continue; }

            bestError = error;
            best      = candidate;
        }

        return best;
    }


    /// <summary>
    /// The squared-error level below which two candidates are indistinguishable given <see cref="double"/> precision.
    /// Without this, perfectly constant data whose mean happens to round by one ulp scores a hair worse than some
    /// unrelated power that lands on exactly zero, and the exotic power wins on pure noise.
    /// </summary>
    [Pure] private static double GetNoiseFloor( ReadOnlySpan<ReadOnlyPoint> points )
    {
        double sumOfSquares = 0.0;
        foreach ( ref readonly ReadOnlyPoint point in points ) { sumOfSquares += point.Y * point.Y; }

        double floor = NOISE_FLOOR_SLACK * points.Length * MACHINE_EPSILON * MACHINE_EPSILON * sumOfSquares;

        return double.IsFinite(floor)
                   ? floor
                   : 0.0;
    }


    /// <summary> Maps a search index onto the powers <c>0, 1, -1, 2, -2, ...</c> so simpler models are considered first. </summary>
    [Pure] private static sbyte GetSearchPower( int index ) => (sbyte)( ( index + 1 ) / 2 * ( index % 2 == 0
                                                                                                  ? -1
                                                                                                  : 1 ) );


    [Pure] private static CalculatedLine Fit( ReadOnlySpan<ReadOnlyPoint> points, sbyte power )
    {
        if ( points.Length < 2 ) { return CalculatedLine.Invalid; }
        if ( power == 0 ) { return FitConstant(points); }

        double S_u2 = 0.0;
        double S_u  = 0.0;
        double S_uy = 0.0;
        double S_y  = 0.0;
        int    n    = 0;

        foreach ( ref readonly ReadOnlyPoint point in points )
        {
            double u = Math.Pow(point.X, power);
            if ( double.IsNaN(u) || double.IsInfinity(u) ) { return CalculatedLine.Invalid; }

            S_u2 += u * u;
            S_u  += u;
            S_uy += u * point.Y;
            S_y  += point.Y;
            n++;
        }

        double det = S_u2 * n - S_u * S_u;
        if ( det == 0.0 ) { return CalculatedLine.Invalid; }

        double A = ( n * S_uy - S_u * S_y ) / det;
        double b = ( S_u2 * S_y - S_u * S_uy ) / det;
        if ( double.IsNaN(A) || double.IsInfinity(A) || double.IsNaN(b) || double.IsInfinity(b) ) { return CalculatedLine.Invalid; }

        return CalculatedLine.Create(Evaluate);

        double Evaluate( double x )
        {
            double u         = Math.Pow(x, power);
            double predicted = A * u + b;

            if ( double.IsNaN(predicted) ) { return double.NaN; }

            if ( double.IsPositiveInfinity(predicted) ) { return double.PositiveInfinity; }

            if ( double.IsNegativeInfinity(predicted) ) { return double.NegativeInfinity; }

            return predicted;
        }
    }


    [Pure] private static CalculatedLine FitConstant( ReadOnlySpan<ReadOnlyPoint> points )
    {
        double sum = 0.0;
        foreach ( ref readonly ReadOnlyPoint point in points ) { sum += point.Y; }

        double average = sum / points.Length;
        if ( double.IsNaN(average) || double.IsInfinity(average) ) { return CalculatedLine.Invalid; }

        return CalculatedLine.Create(_ => average);
    }


    /// <summary> Scores a candidate. Any non-finite prediction disqualifies it via <see cref="double.PositiveInfinity"/>, which keeps the comparison in <see cref="FindBestFit"/> NaN-safe. </summary>
    [Pure] private static double SumSquaredError( ReadOnlySpan<ReadOnlyPoint> points, CalculatedLine line )
    {
        double sum = 0.0;

        foreach ( ref readonly ReadOnlyPoint point in points )
        {
            double predicted = line[point.X];
            if ( double.IsNaN(predicted) || double.IsInfinity(predicted) ) { return double.PositiveInfinity; }

            double residual = point.Y - predicted;
            sum += residual * residual;
        }

        return double.IsNaN(sum)
                   ? double.PositiveInfinity
                   : sum;
    }
}
