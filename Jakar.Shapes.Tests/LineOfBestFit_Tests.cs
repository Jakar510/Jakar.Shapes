// Jakar.Extensions :: Jakar.Shapes.Tests
// 08/13/2026

namespace Jakar.Shapes.Tests;


[TestFixture]
[TestOf(typeof(LineOfBestFit))]
public sealed class LineOfBestFit_Tests : Assert
{
    private const double ABSOLUTE_TOLERANCE = 1e-9;
    private const double RELATIVE_TOLERANCE = 1e-9;


    // ---------------------------------------------------------------------------------------------------------
    // helpers
    // ---------------------------------------------------------------------------------------------------------

    /// <summary> Builds a spline from raw (x, y) pairs. </summary>
    private static Spline Of( params (double X, double Y)[] points )
    {
        ReadOnlyPoint[] array = new ReadOnlyPoint[points.Length];
        for ( int i = 0; i < points.Length; i++ ) { array[i] = new ReadOnlyPoint(points[i].X, points[i].Y); }

        return new Spline(array);
    }


    /// <summary> Samples <c> y = a * x^power + b </c> at every <paramref name="xValues"/> to produce exactly-fitting data. </summary>
    private static Spline Curve( double a, sbyte power, double b, params double[] xValues )
    {
        ReadOnlyPoint[] array = new ReadOnlyPoint[xValues.Length];
        for ( int i = 0; i < xValues.Length; i++ ) { array[i] = new ReadOnlyPoint(xValues[i], a * Math.Pow(xValues[i], power) + b); }

        return new Spline(array);
    }


    private static void AreClose( double expected, double actual ) => Assert.That(actual, Is.EqualTo(expected).Within(( Math.Abs(expected) * RELATIVE_TOLERANCE ) + ABSOLUTE_TOLERANCE));
    private static void IsInvalidAt( CalculatedLine line, double x ) => Assert.That(double.IsNaN(line[x]), Is.True, $"expected an invalid (NaN) result at x = {x}");


    /// <summary> Asserts the fitted line reproduces every point it was built from. </summary>
    private static void ReproducesAllPoints( ref readonly Spline spline, CalculatedLine line )
    {
        foreach ( ref readonly ReadOnlyPoint point in spline.Span ) { AreClose(point.Y, line[point.X]); }
    }


    // ---------------------------------------------------------------------------------------------------------
    // 1. positive powers -- explicit
    // ---------------------------------------------------------------------------------------------------------

    [TestCase((sbyte)1)]
    [TestCase((sbyte)2)]
    [TestCase((sbyte)3)]
    [TestCase((sbyte)4)]
    [TestCase((sbyte)5)]
    [TestCase((sbyte)6)]
    public void PositivePower_ExactData_ReproducesEveryPoint( sbyte power )
    {
        Spline         spline = Curve(2.5, power, -1.25, 1, 2, 3, 4, 5);
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, power);
        ReproducesAllPoints(in spline, line);
    }


    [TestCase((sbyte)1)]
    [TestCase((sbyte)2)]
    [TestCase((sbyte)3)]
    [TestCase((sbyte)4)]
    [TestCase((sbyte)5)]
    [TestCase((sbyte)6)]
    public void PositivePower_ExactData_InterpolatesBetweenSamples( sbyte power )
    {
        const double A      = 2.5;
        const double B      = -1.25;
        Spline       spline = Curve(A, power, B, 1, 2, 3, 4, 5);

        CalculatedLine line = LineOfBestFit.Calculate(in spline, power);
        AreClose(( A * Math.Pow(2.5, power) ) + B, line[2.5]);
        AreClose(( A * Math.Pow(4.5, power) ) + B, line[4.5]);
    }


    [Test] public void PositivePower_Linear_MatchesKnownSlopeAndIntercept()
    {
        // y = 3x + 4 sampled exactly
        Spline         spline = Of((0, 4), (1, 7), (2, 10), (3, 13));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 1);
        AreClose(4,    line[0]);
        AreClose(19,   line[5]);
        AreClose(-2,   line[-2]);
        AreClose(3004, line[1000]);
    }


    [Test] public void PositivePower_Quadratic_MatchesKnownCoefficients()
    {
        // y = 2x^2 + 1
        Spline         spline = Of((0, 1), (1, 3), (2, 9), (3, 19), (4, 33));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 2);
        AreClose(1,   line[0]);
        AreClose(51,  line[5]);
        AreClose(13.5, line[2.5]);
    }


    // ---------------------------------------------------------------------------------------------------------
    // 2. negative powers -- explicit
    // ---------------------------------------------------------------------------------------------------------

    [TestCase((sbyte)-1)]
    [TestCase((sbyte)-2)]
    [TestCase((sbyte)-3)]
    [TestCase((sbyte)-4)]
    [TestCase((sbyte)-5)]
    [TestCase((sbyte)-6)]
    public void NegativePower_ExactData_ReproducesEveryPoint( sbyte power )
    {
        Spline         spline = Curve(2.5, power, -1.25, 1, 2, 3, 4, 5);
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, power);
        ReproducesAllPoints(in spline, line);
    }


    [TestCase((sbyte)-1)]
    [TestCase((sbyte)-2)]
    [TestCase((sbyte)-3)]
    [TestCase((sbyte)-4)]
    [TestCase((sbyte)-5)]
    [TestCase((sbyte)-6)]
    public void NegativePower_ExactData_InterpolatesBetweenSamples( sbyte power )
    {
        const double A      = 2.5;
        const double B      = -1.25;
        Spline       spline = Curve(A, power, B, 1, 2, 3, 4, 5);

        CalculatedLine line = LineOfBestFit.Calculate(in spline, power);
        AreClose(( A * Math.Pow(2.5, power) ) + B, line[2.5]);
        AreClose(( A * Math.Pow(4.5, power) ) + B, line[4.5]);
    }


    [Test] public void NegativePower_Inverse_MatchesKnownCoefficients()
    {
        // y = 12/x + 2
        Spline         spline = Of((1, 14), (2, 8), (3, 6), (4, 5), (6, 4));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, -1);
        AreClose(3.5, line[8]);
        AreClose(14,  line[1]);
        AreClose(2.2, line[60]);
    }


    [TestCase((sbyte)-1)]
    [TestCase((sbyte)-2)]
    [TestCase((sbyte)-3)]
    [TestCase((sbyte)-4)]
    [TestCase((sbyte)-5)]
    [TestCase((sbyte)-6)]
    public void NegativePower_WhenAnyXIsZero_IsInvalid( sbyte power )
    {
        // x^negative at x = 0 is +Infinity, so the whole fit must bail out
        Spline         spline = Of((0, 1), (1, 2), (2, 3), (3, 4));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, power);
        IsInvalidAt(line, 1);
    }


    // ---------------------------------------------------------------------------------------------------------
    // 3. zero power -- degenerates to the mean of Y
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void ZeroPower_ReturnsArithmeticMeanOfY()
    {
        Spline         spline = Of((1, 2), (2, 4), (3, 6), (4, 8));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 0);
        AreClose(5, line[0]);
        AreClose(5, line[1]);
        AreClose(5, line[-17.5]);
        AreClose(5, line[1e9]);
    }


    [Test] public void ZeroPower_WithNegativeYValues_ReturnsMean()
    {
        Spline         spline = Of((1, -4), (2, -8), (3, 0), (4, 4));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 0);
        AreClose(-2, line[3]);
    }


    [Test] public void ZeroPower_WithAllZeroY_ReturnsZero()
    {
        Spline         spline = Of((1, 0), (2, 0), (3, 0));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 0);
        AreClose(0, line[42]);
    }


    [Test] public void ZeroPower_IsFiniteEvenAtZeroX()
    {
        // the constant fit must not depend on Math.Pow at all
        Spline         spline = Of((0, 3), (1, 5), (2, 7));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 0);
        AreClose(5, line[0]);
        Assert.That(double.IsNaN(line[0]), Is.False);
    }


    // ---------------------------------------------------------------------------------------------------------
    // 4. auto-search (primaryPower: null) recovers the generating power
    // ---------------------------------------------------------------------------------------------------------

    [TestCase((sbyte)-6)]
    [TestCase((sbyte)-5)]
    [TestCase((sbyte)-4)]
    [TestCase((sbyte)-3)]
    [TestCase((sbyte)-2)]
    [TestCase((sbyte)-1)]
    [TestCase((sbyte)0)]
    [TestCase((sbyte)1)]
    [TestCase((sbyte)2)]
    [TestCase((sbyte)3)]
    [TestCase((sbyte)4)]
    [TestCase((sbyte)5)]
    [TestCase((sbyte)6)]
    public void AutoSearch_RecoversGeneratingCurve( sbyte truePower )
    {
        Spline         spline = Curve(3.0, truePower, 1.5, 1, 2, 3, 4, 5, 6);
        CalculatedLine line   = LineOfBestFit.Calculate(in spline);

        ReproducesAllPoints(in spline, line);
        AreClose(( 3.0 * Math.Pow(2.5, truePower) ) + 1.5, line[2.5]);
    }


    [TestCase((sbyte)-6)]
    [TestCase((sbyte)-4)]
    [TestCase((sbyte)-1)]
    [TestCase((sbyte)1)]
    [TestCase((sbyte)3)]
    [TestCase((sbyte)6)]
    public void AutoSearch_MatchesExplicitPower_WhenPowerIsObvious( sbyte truePower )
    {
        Spline spline = Curve(3.0, truePower, 1.5, 1, 2, 3, 4, 5, 6);

        CalculatedLine auto     = LineOfBestFit.Calculate(in spline);
        CalculatedLine explicitly = LineOfBestFit.Calculate(in spline, truePower);

        AreClose(explicitly[2.5], auto[2.5]);
        AreClose(explicitly[7.0], auto[7.0]);
    }


    [Test] public void AutoSearch_BoundaryPowers_AreInsideSearchRange()
    {
        Assert.That(LineOfBestFit.MAX_AUTO_SEARCH_POWER, Is.EqualTo((sbyte)6));

        Spline upper = Curve(1.0, LineOfBestFit.MAX_AUTO_SEARCH_POWER,           0.0, 1, 2, 3, 4, 5, 6);
        Spline lower = Curve(1.0, (sbyte)-LineOfBestFit.MAX_AUTO_SEARCH_POWER, 0.0, 1, 2, 3, 4, 5, 6);

        ReproducesAllPoints(in upper, LineOfBestFit.Calculate(in upper));
        ReproducesAllPoints(in lower, LineOfBestFit.Calculate(in lower));
    }


    // ---------------------------------------------------------------------------------------------------------
    // 5. least squares on NOISY data -- expected values independently computed via numpy lstsq
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void NoisyLinear_MatchesIndependentLeastSquares()
    {
        // numpy lstsq on [x, 1]: A = 1.9899999999999998, b = 0.04999999999999967
        Spline         spline = Of((1, 2.1), (2, 3.9), (3, 6.2), (4, 7.8), (5, 10.1));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 1);

        AreClose(0.04999999999999967, line[0]);
        AreClose(5.0249999999999995,  line[2.5]);
        AreClose(11.989999999999997,  line[6]);
    }


    [Test] public void NoisyQuadratic_MatchesIndependentLeastSquares()
    {
        // numpy lstsq on [x^2, 1]: A = 1.9852941176470589, b = 1.2017647058823606
        Spline         spline = Of((1, 3.2), (2, 9.1), (3, 19.2), (4, 32.8), (5, 50.9));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 2);

        AreClose(1.2017647058823606, line[0]);
        AreClose(13.609852941176479, line[2.5]);
        AreClose(72.67235294117647,  line[6]);
    }


    [Test] public void NoisyInverse_MatchesIndependentLeastSquares()
    {
        // numpy lstsq on [x^-1, 1]: A = 10.143984220907294, b = 0.050246548323472834
        Spline         spline = Of((1, 10.2), (2, 5.1), (4, 2.6), (5, 2.1), (8, 1.3));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, -1);

        AreClose(10.194230769230767, line[1]);
        AreClose(3.4315746219592373, line[3]);
        AreClose(1.0646449704142023, line[10]);
    }


    [Test] public void NoisyData_ResidualsAreOrthogonal_LeastSquaresNormalEquation()
    {
        // sum of residuals must vanish for any model carrying an intercept
        Spline         spline = Of((1, 2.1), (2, 3.9), (3, 6.2), (4, 7.8), (5, 10.1));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 1);

        double sum = 0;
        foreach ( ref readonly ReadOnlyPoint point in spline.Span ) { sum += point.Y - line[point.X]; }

        AreClose(0, sum);
    }


    [Test] public void NoisyData_FitBeatsAnyNearbyPerturbation()
    {
        // the returned fit must be a genuine minimum of the squared error
        Spline         spline = Of((1, 2.1), (2, 3.9), (3, 6.2), (4, 7.8), (5, 10.1));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 1);

        double best = SquaredError(in spline, line, 0);
        Assert.That(SquaredError(in spline, line, +0.05), Is.GreaterThan(best));
        Assert.That(SquaredError(in spline, line, -0.05), Is.GreaterThan(best));
        return;

        static double SquaredError( ref readonly Spline s, CalculatedLine l, double shift )
        {
            double total = 0;
            foreach ( ref readonly ReadOnlyPoint point in s.Span )
            {
                double residual = point.Y - ( l[point.X] + shift );
                total += residual * residual;
            }

            return total;
        }
    }


    // ---------------------------------------------------------------------------------------------------------
    // 6. negative and zero coordinates
    // ---------------------------------------------------------------------------------------------------------

    [TestCase((sbyte)1)]
    [TestCase((sbyte)2)]
    [TestCase((sbyte)3)]
    public void NegativeX_PositivePower_ReproducesEveryPoint( sbyte power )
    {
        Spline         spline = Curve(2.0, power, 1.0, -4, -3, -2, -1, 1, 2);
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, power);
        ReproducesAllPoints(in spline, line);
    }


    [TestCase((sbyte)-1)]
    [TestCase((sbyte)-2)]
    [TestCase((sbyte)-3)]
    public void NegativeX_NegativePower_ReproducesEveryPoint( sbyte power )
    {
        Spline         spline = Curve(2.0, power, 1.0, -4, -3, -2, -1, 1, 2);
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, power);
        ReproducesAllPoints(in spline, line);
    }


    [Test] public void NegativeX_OddPower_PreservesSign()
    {
        // y = 2x^3 + 1  ->  strictly increasing through the origin region
        Spline         spline = Curve(2.0, 3, 1.0, -3, -2, -1, 1, 2, 3);
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 3);
        AreClose(-15, line[-2]);
        AreClose(17,  line[2]);
        AreClose(1,   line[0]);
    }


    [Test] public void NegativeX_EvenPower_IsSymmetric()
    {
        // y = 2x^2 + 1 is even, so f(-x) == f(x)
        Spline         spline = Curve(2.0, 2, 1.0, -3, -2, -1, 1, 2, 3);
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 2);
        AreClose(line[2.5], line[-2.5]);
        AreClose(line[7.0], line[-7.0]);
    }


    [Test] public void ZeroY_AllPointsOnAxis_FitsFlatZero()
    {
        Spline         spline = Of((1, 0), (2, 0), (3, 0), (4, 0));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 1);
        AreClose(0, line[1]);
        AreClose(0, line[99]);
    }


    [Test] public void ZeroX_WithPositivePower_IsHandled()
    {
        // x = 0 is fine for positive powers: 0^n == 0
        Spline         spline = Of((0, 1), (1, 3), (2, 9), (3, 19));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 2);
        AreClose(1, line[0]);
    }


    [Test] public void MixedSignData_ProducesFiniteResults()
    {
        Spline         spline = Of((-3, -8), (-1, -2), (0, 1), (2, 5), (4, 11));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 1);

        foreach ( double x in new double[] { -10, -1, 0, 1, 10 } ) { Assert.That(double.IsFinite(line[x]), Is.True, $"expected a finite value at x = {x}"); }
    }


    // ---------------------------------------------------------------------------------------------------------
    // 7. degenerate / invalid input
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void EmptySpline_IsInvalid()
    {
        Spline spline = Spline.Invalid;
        IsInvalidAt(LineOfBestFit.Calculate(in spline),    1);
        IsInvalidAt(LineOfBestFit.Calculate(in spline, 1), 1);
        IsInvalidAt(LineOfBestFit.Calculate(in spline, 0), 1);
    }


    [Test] public void SinglePoint_IsInvalid()
    {
        Spline spline = Of((3, 7));
        IsInvalidAt(LineOfBestFit.Calculate(in spline),    1);
        IsInvalidAt(LineOfBestFit.Calculate(in spline, 2), 1);
    }


    [TestCase((sbyte)1)]
    [TestCase((sbyte)2)]
    [TestCase((sbyte)-1)]
    public void AllPointsShareTheSameX_IsInvalid( sbyte power )
    {
        // a vertical arrangement has no unique least-squares solution -> determinant is zero
        Spline         spline = Of((2, 1), (2, 5), (2, 9));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, power);
        IsInvalidAt(line, 2);
    }


    [Test] public void NaNCoordinates_AreInvalid()
    {
        Spline spline = Of((1, 2), (double.NaN, 5), (3, 6));
        IsInvalidAt(LineOfBestFit.Calculate(in spline, 1), 1);
    }


    [Test] public void InfiniteCoordinates_AreInvalid()
    {
        Spline spline = Of((1, 2), (double.PositiveInfinity, 5), (3, 6));
        IsInvalidAt(LineOfBestFit.Calculate(in spline, 1), 1);
    }


    [Test] public void TwoPoints_AreEnoughForAFit()
    {
        Spline         spline = Of((1, 3), (2, 5));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 1);
        AreClose(3, line[1]);
        AreClose(5, line[2]);
        AreClose(7, line[3]);
    }


    // ---------------------------------------------------------------------------------------------------------
    // 8. sentinel propagation -- NaN / +Infinity / -Infinity are distinguished
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Evaluate_AtNaN_ReturnsNaN()
    {
        Spline         spline = Of((1, 2), (2, 4), (3, 6));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, 1);
        Assert.That(double.IsNaN(line[double.NaN]), Is.True);
    }


    [Test] public void Evaluate_AtPositiveInfinity_KeepsSignOfSlope()
    {
        // positive slope -> +inf ; negative slope -> -inf
        Spline rising  = Of((1, 2), (2, 4), (3, 6));
        Spline falling = Of((1, 6), (2, 4), (3, 2));

        Assert.That(double.IsPositiveInfinity(LineOfBestFit.Calculate(in rising,  1)[double.PositiveInfinity]), Is.True);
        Assert.That(double.IsNegativeInfinity(LineOfBestFit.Calculate(in falling, 1)[double.PositiveInfinity]), Is.True);
    }


    [Test] public void Evaluate_AtNegativeInfinity_KeepsSignOfSlope()
    {
        Spline rising  = Of((1, 2), (2, 4), (3, 6));
        Spline falling = Of((1, 6), (2, 4), (3, 2));

        Assert.That(double.IsNegativeInfinity(LineOfBestFit.Calculate(in rising,  1)[double.NegativeInfinity]), Is.True);
        Assert.That(double.IsPositiveInfinity(LineOfBestFit.Calculate(in falling, 1)[double.NegativeInfinity]), Is.True);
    }


    [Test] public void Evaluate_NegativePowerAtZero_ReturnsSignedInfinity()
    {
        // y = 12/x + 2 blows up at the origin; the sign must follow A
        Spline         spline = Of((1, 14), (2, 8), (3, 6), (4, 5), (6, 4));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline, -1);
        Assert.That(double.IsPositiveInfinity(line[0]), Is.True);
    }


    // ---------------------------------------------------------------------------------------------------------
    // 9. regression guards -- auto-search must prefer the simplest power on ties
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void AutoSearch_ConstantData_PrefersConstantFit()
    {
        // every power fits constant data perfectly (A == 0); the constant model must win,
        // otherwise evaluating at x == 0 yields 0 * Infinity == NaN
        Spline         spline = Of((1, 7), (2, 7), (3, 7), (4, 7));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline);

        AreClose(7, line[1]);
        AreClose(7, line[5]);
        AreClose(7, line[0]);
        Assert.That(double.IsNaN(line[0]), Is.False, "constant data must stay finite at the origin");
    }


    [TestCase(5.9047,  2.4, 5.7, 5.9)]
    [TestCase(5.4016,  0.7, 1.1, 3.6)]
    [TestCase(10.8584, 1.9, 4.2, 4.9)]
    [TestCase(12.0232, 1.0, 3.7, 5.0)]
    [TestCase(6.1212,  0.7, 1.4, 2.3)]
    public void AutoSearch_ConstantData_WhoseMeanRounds_StillPrefersConstantFit( double value, params double[] xValues )
    {
        // the mean of these identical values rounds by one ulp, so the constant fit scores a hair above zero
        // while some unrelated power lands on exactly zero -- without a noise floor the exotic power wins,
        // and evaluating it at the origin yields NaN or -Infinity
        ReadOnlyPoint[] array = new ReadOnlyPoint[xValues.Length];
        for ( int i = 0; i < xValues.Length; i++ ) { array[i] = new ReadOnlyPoint(xValues[i], value); }

        Spline         spline = new(array);
        CalculatedLine line   = LineOfBestFit.Calculate(in spline);

        AreClose(value, line[0]);
        AreClose(value, line[1]);
        AreClose(value, line[99]);
        Assert.That(double.IsFinite(line[0]), Is.True, "constant data must stay finite at the origin");
    }


    [Test] public void AutoSearch_TwoCollinearPoints_PrefersStraightLine()
    {
        // two points fit ANY power perfectly, so the search must settle on power 1
        Spline         spline = Of((1, 1), (2, 2));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline);

        AreClose(3,  line[3]);
        AreClose(10, line[10]);
    }


    [Test] public void AutoSearch_PrefersLowerMagnitudePower_OnEqualError()
    {
        // y = x is exactly representable at power 1; nothing more exotic should be chosen
        Spline         spline = Of((1, 1), (2, 2), (3, 3), (4, 4));
        CalculatedLine line   = LineOfBestFit.Calculate(in spline);

        AreClose(0,   line[0]);
        AreClose(100, line[100]);
    }


    [Test] public void AutoSearch_NeverScoresWorseThanAnyExplicitPower()
    {
        Spline spline = Of((1, 2.1), (2, 3.9), (3, 6.2), (4, 7.8), (5, 10.1));

        double auto = SquaredError(in spline, LineOfBestFit.Calculate(in spline));

        for ( sbyte power = -LineOfBestFit.MAX_AUTO_SEARCH_POWER; power <= LineOfBestFit.MAX_AUTO_SEARCH_POWER; power++ )
        {
            double candidate = SquaredError(in spline, LineOfBestFit.Calculate(in spline, power));
            if ( double.IsNaN(candidate) || double.IsInfinity(candidate) ) { continue; }

            Assert.That(auto, Is.LessThanOrEqualTo(candidate + ABSOLUTE_TOLERANCE), $"auto-search lost to explicit power {power}");
        }

        return;

        static double SquaredError( ref readonly Spline s, CalculatedLine l )
        {
            double total = 0;
            foreach ( ref readonly ReadOnlyPoint point in s.Span )
            {
                double residual = point.Y - l[point.X];
                total += residual * residual;
            }

            return total;
        }
    }


    // ---------------------------------------------------------------------------------------------------------
    // 10. determinism & purity
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Calculate_IsDeterministic()
    {
        Spline spline = Of((1, 2.1), (2, 3.9), (3, 6.2), (4, 7.8), (5, 10.1));

        CalculatedLine first  = LineOfBestFit.Calculate(in spline);
        CalculatedLine second = LineOfBestFit.Calculate(in spline);

        AreClose(first[2.5], second[2.5]);
        AreClose(first[9.0], second[9.0]);
    }


    [Test] public void Calculate_DoesNotMutateTheSpline()
    {
        Spline spline   = Of((1, 2.1), (2, 3.9), (3, 6.2));
        Spline original = Of((1, 2.1), (2, 3.9), (3, 6.2));

        LineOfBestFit.Calculate(in spline);
        LineOfBestFit.Calculate(in spline, 3);

        this.IsTrue(spline.Equals(original));
    }
}
