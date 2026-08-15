// Jakar.Extensions :: Jakar.Shapes.Tests
// 08/13/2026

namespace Jakar.Shapes.Tests;


[TestFixture]
[TestOf(typeof(LineOfBestFit))]
public sealed class LineOfBestFit_Tests : Assert
{
    private const double ABSOLUTE_TOLERANCE = 1e-7;
    private const double RELATIVE_TOLERANCE = 1e-7;


    // ---------------------------------------------------------------------------------------------------------
    // helpers
    // ---------------------------------------------------------------------------------------------------------

    private static Spline Of( params ReadOnlySpan<(double X, double Y)> points )
    {
        Span<ReadOnlyPoint> buffer = stackalloc ReadOnlyPoint[points.Length];
        for ( int i = 0; i < points.Length; i++ ) { buffer[i] = new ReadOnlyPoint(points[i].X, points[i].Y); }

        return new Spline(buffer);
    }


    /// <summary> Evaluates <c> a_0 + a_1*t + a_2*t^2 + ... </c> where <c> t </c> is <c> x </c>, or <c> 1/x </c> for a Laurent fit. </summary>
    private static double Evaluate( ReadOnlySpan<double> ascending, sbyte degree, double x )
    {
        double t = degree < 0
                       ? 1.0 / x
                       : x;

        double result = ascending[^1];
        for ( int i = ascending.Length - 2; i >= 0; i-- ) { result = ( result * t ) + ascending[i]; }

        return result;
    }


    /// <summary> Samples the polynomial exactly at each x, so the fit has a zero-error solution to find. </summary>
    private static Spline Curve( ReadOnlySpan<double> ascending, sbyte degree, params ReadOnlySpan<double> xValues )
    {
        Span<ReadOnlyPoint> buffer = stackalloc ReadOnlyPoint[xValues.Length];
        for ( int i = 0; i < xValues.Length; i++ ) { buffer[i] = new ReadOnlyPoint(xValues[i], Evaluate(ascending, degree, xValues[i])); }

        return new Spline(buffer);
    }


    private static void AreClose( double expected, double actual ) => Assert.That(actual, Is.EqualTo(expected).Within(( Math.Abs(expected) * RELATIVE_TOLERANCE ) + ABSOLUTE_TOLERANCE));
    private static void IsInvalidAt( CalculatedLine line, double x ) => Assert.That(double.IsNaN(line[x]), Is.True, $"expected an invalid (NaN) result at x = {x}");


    private static void ReproducesAllPoints( ref readonly Spline spline, PolynomialFit fit )
    {
        Assert.That(fit.IsValid, Is.True, "fit should be valid");
        foreach ( ref readonly ReadOnlyPoint point in spline.Span ) { AreClose(point.Y, fit[point.X]); }
    }


    private static void HasCoefficients( PolynomialFit fit, params ReadOnlySpan<double> ascending )
    {
        Assert.That(fit.IsValid,           Is.True);
        Assert.That(fit.Coefficients.Length, Is.EqualTo(ascending.Length));
        for ( int i = 0; i < ascending.Length; i++ ) { AreClose(ascending[i], fit.Coefficients[i]); }
    }


    // ---------------------------------------------------------------------------------------------------------
    // 1. the full polynomial -- every intermediate term must be fitted, not just the leading one
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Cubic_RecoversEveryCoefficient()
    {
        // y = 2x^3 - 5x^2 + 3x + 7
        Spline        spline = Curve([7, 3, -5, 2], 3, 0.5, 1.0, 1.7, 2.3, 3.0, 3.8, 4.5, 5.2);
        PolynomialFit fit    = LineOfBestFit.Fit(in spline, 3);

        HasCoefficients(fit, 7, 3, -5, 2);
        Assert.That(fit.Degree,            Is.EqualTo((sbyte)3));
        Assert.That(fit.SumOfSquaredError, Is.LessThan(1e-18));
        ReproducesAllPoints(in spline, fit);
    }


    [Test] public void Quartic_RecoversEveryCoefficient()
    {
        // y = x^4 - 2x^3 + 3x^2 - 4x + 5
        Spline        spline = Curve([5, -4, 3, -2, 1], 4, 0.4, 1.1, 1.9, 2.6, 3.4, 4.2, 5.0, 5.9);
        PolynomialFit fit    = LineOfBestFit.Fit(in spline, 4);

        HasCoefficients(fit, 5, -4, 3, -2, 1);
        ReproducesAllPoints(in spline, fit);
    }


    [Test] public void Quadratic_MiddleTermIsNotDropped()
    {
        // y = 3x^2 + 11x + 2 -- a leading-term-only model cannot represent the 11x
        Spline        spline = Curve([2, 11, 3], 2, 1, 2, 3, 4, 5);
        PolynomialFit fit    = LineOfBestFit.Fit(in spline, 2);

        HasCoefficients(fit, 2, 11, 3);
        AreClose(2,   fit[0]);
        AreClose(176, fit[6]);   // 3(36) + 11(6) + 2
    }


    [TestCase((sbyte)1)]
    [TestCase((sbyte)2)]
    [TestCase((sbyte)3)]
    [TestCase((sbyte)4)]
    [TestCase((sbyte)5)]
    [TestCase((sbyte)6)]
    public void EveryDegree_WithAllTermsPresent_IsRecovered( sbyte degree )
    {
        Span<double> ascending = stackalloc double[degree + 1];
        for ( int k = 0; k <= degree; k++ ) { ascending[k] = 1.0 + ( 0.5 * k ); }   // every term non-zero

        Span<double> xs = stackalloc double[degree + 4];
        for ( int i = 0; i < xs.Length; i++ ) { xs[i] = 0.5 + ( 0.7 * i ); }

        Spline        spline = Curve(ascending, degree, xs);
        PolynomialFit fit    = LineOfBestFit.Fit(in spline, degree);

        HasCoefficients(fit, ascending);
        ReproducesAllPoints(in spline, fit);
    }


    // ---------------------------------------------------------------------------------------------------------
    // 2. negative degree -- the Laurent mirror
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Laurent_RecoversEveryCoefficient()
    {
        // y = 4x^-2 + 3x^-1 + 5
        Spline        spline = Curve([5, 3, 4], -2, 0.5, 0.9, 1.4, 2.2, 3.1, 4.5);
        PolynomialFit fit    = LineOfBestFit.Fit(in spline, -2);

        HasCoefficients(fit, 5, 3, 4);
        Assert.That(fit.Degree, Is.EqualTo((sbyte)-2));
        ReproducesAllPoints(in spline, fit);
    }


    [TestCase((sbyte)-1)]
    [TestCase((sbyte)-2)]
    [TestCase((sbyte)-3)]
    [TestCase((sbyte)-4)]
    [TestCase((sbyte)-5)]
    [TestCase((sbyte)-6)]
    public void EveryNegativeDegree_IsRecovered( sbyte degree )
    {
        int      terms     = Math.Abs(degree) + 1;
        Span<double> ascending = stackalloc double[terms];
        for ( int k = 0; k < terms; k++ ) { ascending[k] = 1.0 + ( 0.5 * k ); }

        Span<double> xs = stackalloc double[terms + 3];
        for ( int i = 0; i < xs.Length; i++ ) { xs[i] = 0.6 + ( 0.6 * i ); }

        Spline        spline = Curve(ascending, degree, xs);
        PolynomialFit fit    = LineOfBestFit.Fit(in spline, degree);

        HasCoefficients(fit, ascending);
        ReproducesAllPoints(in spline, fit);
    }


    [TestCase((sbyte)-1)]
    [TestCase((sbyte)-3)]
    [TestCase((sbyte)-6)]
    public void NegativeDegree_WhenAnyXIsZero_IsInvalid( sbyte degree )
    {
        Spline spline = Of((0, 1), (1, 2), (2, 3), (3, 4), (4, 5), (5, 6), (6, 7), (7, 8));
        Assert.That(LineOfBestFit.Fit(in spline, degree)
                                 .IsValid,
                    Is.False);
    }


    // ---------------------------------------------------------------------------------------------------------
    // 3. degree zero
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void ZeroDegree_ReturnsArithmeticMeanOfY()
    {
        Spline        spline = Of((1, 2), (2, 4), (3, 6), (4, 8));
        PolynomialFit fit    = LineOfBestFit.Fit(in spline, 0);

        HasCoefficients(fit, 5);
        AreClose(5, fit[0]);
        AreClose(5, fit[-17.5]);
        AreClose(5, fit[1e9]);
    }


    [Test] public void ZeroDegree_WithNegativeY_ReturnsMean()
    {
        Spline spline = Of((1, -4), (2, -8), (3, 0), (4, 4));
        HasCoefficients(LineOfBestFit.Fit(in spline, 0), -2);
    }


    // ---------------------------------------------------------------------------------------------------------
    // 4. automatic degree selection
    // ---------------------------------------------------------------------------------------------------------

    [TestCase((sbyte)0)]
    [TestCase((sbyte)1)]
    [TestCase((sbyte)2)]
    [TestCase((sbyte)3)]
    [TestCase((sbyte)4)]
    [TestCase((sbyte)5)]
    [TestCase((sbyte)6)]
    public void AutoSearch_RecoversGeneratingDegree( sbyte degree )
    {
        Span<double> ascending = stackalloc double[degree + 1];
        for ( int k = 0; k <= degree; k++ ) { ascending[k] = 1.0 + ( 0.5 * k ); }

        Span<double> xs = stackalloc double[degree + 5];
        for ( int i = 0; i < xs.Length; i++ ) { xs[i] = 0.5 + ( 0.7 * i ); }

        Spline        spline = Curve(ascending, degree, xs);
        PolynomialFit fit    = LineOfBestFit.Fit(in spline);

        Assert.That(fit.Degree, Is.EqualTo(degree), $"expected degree {degree}, chose {fit.Degree}");
        HasCoefficients(fit, ascending);
    }


    [TestCase((sbyte)-1)]
    [TestCase((sbyte)-2)]
    [TestCase((sbyte)-3)]
    public void AutoSearch_RecoversNegativeGeneratingDegree( sbyte degree )
    {
        int      terms     = Math.Abs(degree) + 1;
        Span<double> ascending = stackalloc double[terms];
        for ( int k = 0; k < terms; k++ ) { ascending[k] = 1.0 + ( 0.5 * k ); }

        Span<double> xs = stackalloc double[terms + 4];
        for ( int i = 0; i < xs.Length; i++ ) { xs[i] = 0.6 + ( 0.6 * i ); }

        Spline spline = Curve(ascending, degree, xs);
        Assert.That(LineOfBestFit.Fit(in spline)
                                 .Degree,
                    Is.EqualTo(degree));
    }


    [Test] public void AutoSearch_MissingMiddleTerm_DoesNotStopEarly()
    {
        // y = x^2 is symmetric, so degree 1 is no better than degree 0.
        // A naive "stop at the first degree that does not help" rule would wrongly return 0.
        Spline        spline = Of((-3, 9), (-2, 4), (-1, 1), (0, 0), (1, 1), (2, 4), (3, 9));
        PolynomialFit fit    = LineOfBestFit.Fit(in spline);

        Assert.That(fit.Degree, Is.EqualTo((sbyte)2));
        AreClose(0,  fit[0]);
        AreClose(25, fit[5]);
    }


    [Test] public void AutoSearch_DoesNotOverfitStraightLine()
    {
        Spline        spline = Of((1, 3), (2, 5), (3, 7), (4, 9), (5, 11), (6, 13), (7, 15));
        PolynomialFit fit    = LineOfBestFit.Fit(in spline);

        Assert.That(fit.Degree, Is.EqualTo((sbyte)1));
        AreClose(1,   fit[0]);
        AreClose(201, fit[100]);
    }


    [Test] public void MaxAutoSearchPower_IsSix()
    {
        Assert.That(LineOfBestFit.MAX_AUTO_SEARCH_POWER, Is.EqualTo((sbyte)6));

        Spline spline = Of((1, 1), (2, 2));
        Assert.That(LineOfBestFit.Fit(in spline, 7)
                                 .IsValid,
                    Is.False,
                    "degrees beyond the bound must be rejected");
    }


    // ---------------------------------------------------------------------------------------------------------
    // 5. least squares on noisy data -- expected values independently computed via numpy lstsq
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void NoisyQuadratic_MatchesIndependentLeastSquares()
    {
        Spline        spline = Of((1, 3.1), (2, 8.9), (3, 19.2), (4, 33.1), (5, 50.8), (6, 72.9));
        PolynomialFit fit    = LineOfBestFit.Fit(in spline, 2);

        HasCoefficients(fit, 0.98999999999998611, 0.072500000000006004, 1.9839285714285702);
        AreClose(0.082357142857143281, fit.SumOfSquaredError);
        AreClose(13.570803571428565,   fit[2.5]);
        AreClose(98.709999999999965,   fit[7.0]);
    }


    [Test] public void NoisyCubic_MatchesIndependentLeastSquares()
    {
        Spline        spline = Of((0.5, 7.4), (1.2, 5.9), (2.1, 7.1), (3.0, 25.2), (3.9, 58.9), (4.8, 112.4), (5.5, 168.1));
        PolynomialFit fit    = LineOfBestFit.Fit(in spline, 3);

        HasCoefficients(fit, 13.746554526511291, -13.225327052541477, 3.5204894310618933, 0.72701140486382465);
        AreClose(5.2128867888156503, fit.SumOfSquaredError);
        AreClose(343.03815914904555, fit[7.0]);
    }


    [Test] public void NoisyLaurent_MatchesIndependentLeastSquares()
    {
        Spline        spline = Of((0.5, 21.2), (0.9, 10.1), (1.4, 7.3), (2.2, 6.2), (3.1, 5.8), (4.5, 5.4));
        PolynomialFit fit    = LineOfBestFit.Fit(in spline, -2);

        HasCoefficients(fit, 5.3593031762251186, -0.17662298738417981, 4.0453811038878573);
        AreClose(5.9359149578935035, fit[2.5]);
        AreClose(5.4166301189230488, fit[7.0]);
    }


    [Test] public void NoisyData_ResidualsAreOrthogonalToEveryBasisColumn()
    {
        // the defining property of a least-squares solution: V^T (y - Vc) = 0
        Spline        spline = Of((1, 3.1), (2, 8.9), (3, 19.2), (4, 33.1), (5, 50.8), (6, 72.9));
        PolynomialFit fit    = LineOfBestFit.Fit(in spline, 2);

        for ( int k = 0; k <= 2; k++ )
        {
            double dot = 0;
            foreach ( ref readonly ReadOnlyPoint point in spline.Span ) { dot += Math.Pow(point.X, k) * ( point.Y - fit[point.X] ); }

            Assert.That(Math.Abs(dot), Is.LessThan(1e-9), $"residuals not orthogonal to column x^{k}");
        }
    }


    // ---------------------------------------------------------------------------------------------------------
    // 6. coefficients and rendering
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Coefficients_AreAscending()
    {
        Spline        spline = Curve([7, 3, -5, 2], 3, 0.5, 1.0, 1.7, 2.3, 3.0, 3.8, 4.5);
        PolynomialFit fit    = LineOfBestFit.Fit(in spline, 3);

        AreClose(7,  fit.Coefficients[0]);   // constant
        AreClose(2,  fit.Coefficients[3]);   // leading
        Assert.That(fit.Length, Is.EqualTo(4));
    }


    [Test] public void ToString_RendersTheEquationDescending()
    {
        Spline spline = Curve([7, 3, -5, 2], 3, 0.5, 1.0, 1.7, 2.3, 3.0, 3.8, 4.5);
        Assert.That(LineOfBestFit.Fit(in spline, 3)
                                 .ToString(),
                    Is.EqualTo("2x^3 - 5x^2 + 3x + 7"));
    }


    [Test] public void ToString_RendersLaurentWithNegativeExponents()
    {
        Spline spline = Curve([5, 3, 4], -2, 0.5, 0.9, 1.4, 2.2, 3.1, 4.5);
        Assert.That(LineOfBestFit.Fit(in spline, -2)
                                 .ToString(),
                    Is.EqualTo("4x^-2 + 3x^-1 + 5"));
    }


    [Test] public void InvalidFit_RendersAsInvalid()
    {
        Assert.That(PolynomialFit.Invalid.IsValid,     Is.False);
        Assert.That(PolynomialFit.Invalid.ToString(),  Does.Contain("Invalid"));
        Assert.That(double.IsNaN(PolynomialFit.Invalid[1]), Is.True);
    }


    [Test] public void ToCalculatedLine_MatchesTheFit()
    {
        Spline         spline = Curve([7, 3, -5, 2], 3, 0.5, 1.0, 1.7, 2.3, 3.0, 3.8, 4.5);
        PolynomialFit  fit    = LineOfBestFit.Fit(in spline, 3);
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 3);

        AreClose(fit[2.5], line[2.5]);
        AreClose(fit[9.0], line[9.0]);
    }


    // ---------------------------------------------------------------------------------------------------------
    // 7. degenerate input
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void EmptySpline_IsInvalid()
    {
        Spline spline = Spline.Invalid;
        Assert.That(LineOfBestFit.Fit(in spline).IsValid, Is.False);
        IsInvalidAt(LineOfBestFit.Calculate(in spline), 1);
    }


    [Test] public void SinglePoint_IsInvalid()
    {
        Spline spline = Of((3, 7));
        Assert.That(LineOfBestFit.Fit(in spline).IsValid, Is.False);
    }


    [Test] public void FewerPointsThanTerms_IsInvalid()
    {
        // a cubic needs at least 4 points
        Spline spline = Of((1, 2), (2, 5), (3, 9));
        Assert.That(LineOfBestFit.Fit(in spline, 3).IsValid, Is.False);
        Assert.That(LineOfBestFit.Fit(in spline, 2).IsValid, Is.True);
    }


    [TestCase((sbyte)1)]
    [TestCase((sbyte)2)]
    [TestCase((sbyte)-1)]
    public void AllPointsShareTheSameX_IsInvalid( sbyte degree )
    {
        Spline spline = Of((2, 1), (2, 5), (2, 9), (2, 3), (2, 8), (2, 6), (2, 4), (2, 2));
        Assert.That(LineOfBestFit.Fit(in spline, degree).IsValid, Is.False);
    }


    [Test] public void NaNCoordinates_AreInvalid()
    {
        Spline spline = Of((1, 2), (double.NaN, 5), (3, 6), (4, 8));
        Assert.That(LineOfBestFit.Fit(in spline, 1).IsValid, Is.False);
    }


    [Test] public void InfiniteCoordinates_AreInvalid()
    {
        Spline spline = Of((1, 2), (double.PositiveInfinity, 5), (3, 6), (4, 8));
        Assert.That(LineOfBestFit.Fit(in spline, 1).IsValid, Is.False);
    }


    [Test] public void TwoPoints_FitALine()
    {
        Spline        spline = Of((1, 3), (2, 5));
        PolynomialFit fit    = LineOfBestFit.Fit(in spline, 1);
        HasCoefficients(fit, 1, 2);
        AreClose(7, fit[3]);
    }


    // ---------------------------------------------------------------------------------------------------------
    // 8. behaviour carried over from the earlier single-term implementation
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void AutoSearch_ConstantData_PrefersDegreeZero()
    {
        Spline        spline = Of((1, 7), (2, 7), (3, 7), (4, 7));
        PolynomialFit fit    = LineOfBestFit.Fit(in spline);

        Assert.That(fit.Degree, Is.EqualTo((sbyte)0));
        AreClose(7, fit[0]);
        Assert.That(double.IsFinite(fit[0]), Is.True, "constant data must stay finite at the origin");
    }


    [TestCase(5.9047,  2.4, 5.7, 5.9)]
    [TestCase(5.4016,  0.7, 1.1, 3.6)]
    [TestCase(10.8584, 1.9, 4.2, 4.9)]
    [TestCase(12.0232, 1.0, 3.7, 5.0)]
    [TestCase(6.1212,  0.7, 1.4, 2.3)]
    public void AutoSearch_ConstantData_WhoseMeanRounds_StaysFiniteAtOrigin( double value, double x1, double x2, double x3 )
    {
        // the mean of these identical values rounds by one ulp, so a richer model can score marginally
        // lower on pure noise; without the noise floor the chosen model returns NaN or -Infinity at x = 0
        Spline        spline = new(new ReadOnlyPoint(x1, value), new ReadOnlyPoint(x2, value), new ReadOnlyPoint(x3, value));
        PolynomialFit fit    = LineOfBestFit.Fit(in spline);

        AreClose(value, fit[0]);
        AreClose(value, fit[99]);
        Assert.That(double.IsFinite(fit[0]), Is.True);
    }


    [Test] public void AutoSearch_TwoCollinearPoints_PrefersStraightLine()
    {
        Spline        spline = Of((1, 1), (2, 2));
        PolynomialFit fit    = LineOfBestFit.Fit(in spline);

        Assert.That(fit.Degree, Is.EqualTo((sbyte)1));
        AreClose(3,  fit[3]);
        AreClose(10, fit[10]);
    }


    [Test] public void Evaluate_AtNaN_ReturnsNaN()
    {
        Spline spline = Of((1, 2), (2, 4), (3, 6));
        Assert.That(double.IsNaN(LineOfBestFit.Calculate(in spline, 1)[double.NaN]), Is.True);
    }


    [Test] public void Evaluate_AtInfinity_KeepsSignOfLeadingTerm()
    {
        Spline rising  = Of((1, 2), (2, 4), (3, 6));
        Spline falling = Of((1, 6), (2, 4), (3, 2));

        Assert.That(double.IsPositiveInfinity(LineOfBestFit.Calculate(in rising,  1)[double.PositiveInfinity]), Is.True);
        Assert.That(double.IsNegativeInfinity(LineOfBestFit.Calculate(in falling, 1)[double.PositiveInfinity]), Is.True);
        Assert.That(double.IsNegativeInfinity(LineOfBestFit.Calculate(in rising,  1)[double.NegativeInfinity]), Is.True);
        Assert.That(double.IsPositiveInfinity(LineOfBestFit.Calculate(in falling, 1)[double.NegativeInfinity]), Is.True);
    }


    [Test] public void Calculate_IsDeterministic()
    {
        Spline spline = Of((1, 3.1), (2, 8.9), (3, 19.2), (4, 33.1), (5, 50.8), (6, 72.9));

        PolynomialFit first  = LineOfBestFit.Fit(in spline);
        PolynomialFit second = LineOfBestFit.Fit(in spline);

        Assert.That(second.Degree, Is.EqualTo(first.Degree));
        AreClose(first[2.5], second[2.5]);
    }


    [Test] public void Calculate_DoesNotMutateTheSpline()
    {
        Spline spline   = Of((1, 2.1), (2, 3.9), (3, 6.2));
        Spline original = Of((1, 2.1), (2, 3.9), (3, 6.2));

        LineOfBestFit.Fit(in spline);
        LineOfBestFit.Fit(in spline, 2);

        this.IsTrue(spline.Equals(original));
    }
}
