// Jakar.Extensions :: Jakar.Shapes.Tests
// 08/13/2026

namespace Jakar.Shapes.Tests;


[TestFixture]
[TestOf(typeof(Circles))]
public sealed class CirclePerimeter_Tests : Assert
{
    private const double TOLERANCE = 1e-9;


    private static void AreClose( double expected, double actual, double tolerance = TOLERANCE ) => Assert.That(actual, Is.EqualTo(expected).Within(( Math.Abs(expected) * tolerance ) + tolerance));
    private static ReadOnlyPoint P( double x, double y ) => new(x, y);


    // ---------------------------------------------------------------------------------------------------------
    // count and placement
    // ---------------------------------------------------------------------------------------------------------

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(8)]
    [TestCase(64)]
    [TestCase(360)]
    public void Resolution_DeterminesTheNumberOfPoints( int resolution )
    {
        Circle                      shape  = new(P(0, 0), 5);
        Span<ReadOnlyPoint>         buffer = new ReadOnlyPoint[resolution];
        ReadOnlySpan<ReadOnlyPoint> points = shape.PerimeterPoints(buffer, resolution);

        Assert.That(points.Length, Is.EqualTo(resolution));
        foreach ( ref readonly ReadOnlyPoint point in points ) { AreClose(5, shape.Center.DistanceTo(point)); }
    }


    [TestCase(3)]
    [TestCase(7)]
    [TestCase(12)]
    [TestCase(90)]
    public void Points_AreEvenlySpaced( int resolution )
    {
        Circle                      shape  = new(P(2, -1), 3);
        Span<ReadOnlyPoint>         buffer = new ReadOnlyPoint[resolution];
        ReadOnlySpan<ReadOnlyPoint> points = shape.PerimeterPoints(buffer, resolution);

        double expected = points[0].DistanceTo(points[1]);
        for ( int i = 0; i < resolution; i++ ) { AreClose(expected, points[i].DistanceTo(points[( i + 1 ) % resolution])); }
    }


    [Test] public void FirstPoint_SitsOnThePositiveXAxis()
    {
        Circle              shape  = new(P(3, -2), 4);
        Span<ReadOnlyPoint> buffer = stackalloc ReadOnlyPoint[8];

        ReadOnlySpan<ReadOnlyPoint> points = shape.PerimeterPoints(buffer, 8);
        AreClose(7,  points[0].X);
        AreClose(-2, points[0].Y);

        // a quarter of the way round is straight up
        AreClose(3, points[2].X);
        AreClose(2, points[2].Y);
    }


    [Test] public void TheLoopIsNotClosed()
    {
        Circle              shape  = new(P(0, 0), 1);
        Span<ReadOnlyPoint> buffer = stackalloc ReadOnlyPoint[8];

        ReadOnlySpan<ReadOnlyPoint> points = shape.PerimeterPoints(buffer, 8);
        Assert.That(points[0].Equals(points[^1]), Is.False, "the last point must be one step short of the first, not a duplicate of it");

        double last = Math.Atan2(points[^1].Y, points[^1].X);
        if ( last < 0 ) { last += 2 * Math.PI; }   // Atan2 returns (-pi, pi]

        AreClose(( 2 * Math.PI ) - ( 2 * Math.PI / 8 ), last);
    }


    [Test] public void Rotation_MovesTheStartingPoint()
    {
        Circle              shape  = new(P(3, -2), 4);
        Span<ReadOnlyPoint> buffer = stackalloc ReadOnlyPoint[4];

        ReadOnlySpan<ReadOnlyPoint> points = shape.PerimeterPoints(buffer, 4, new Radians(Math.PI / 2));
        AreClose(3, points[0].X);
        AreClose(2, points[0].Y);
    }


    [Test] public void PointAt_AgreesWithTheSampledPoints()
    {
        Circle              shape  = new(P(3, -2), 4);
        Span<ReadOnlyPoint> buffer = stackalloc ReadOnlyPoint[8];

        ReadOnlySpan<ReadOnlyPoint> points = shape.PerimeterPoints(buffer, 8);

        for ( int i = 0; i < 8; i++ )
        {
            ReadOnlyPoint expected = shape.PointAt(new Radians(i * 2 * Math.PI / 8));
            AreClose(expected.X, points[i].X);
            AreClose(expected.Y, points[i].Y);
        }
    }


    // ---------------------------------------------------------------------------------------------------------
    // the samples are an inscribed regular polygon
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void SamplingMatchesTheInscribedRegularPolygon()
    {
        Circle              shape   = new(P(1, 2), 3);
        Span<ReadOnlyPoint> fromCircle = stackalloc ReadOnlyPoint[RegularPolygons.MAX_SIDES];
        Span<ReadOnlyPoint> fromPoly   = stackalloc ReadOnlyPoint[RegularPolygons.MAX_SIDES];

        Compare(shape.PerimeterPoints(fromCircle, 5),  new Pentagon(shape.Center, shape.Radius).Vertices(fromPoly));
        Compare(shape.PerimeterPoints(fromCircle, 6),  new Hexagon(shape.Center,  shape.Radius).Vertices(fromPoly));
        Compare(shape.PerimeterPoints(fromCircle, 8),  new Octagon(shape.Center,  shape.Radius).Vertices(fromPoly));
        Compare(shape.PerimeterPoints(fromCircle, 10), new Decagon(shape.Center,  shape.Radius).Vertices(fromPoly));
        return;

        static void Compare( ReadOnlySpan<ReadOnlyPoint> circle, ReadOnlySpan<ReadOnlyPoint> polygon )
        {
            Assert.That(circle.Length, Is.EqualTo(polygon.Length));
            for ( int i = 0; i < circle.Length; i++ )
            {
                AreClose(polygon[i].X, circle[i].X);
                AreClose(polygon[i].Y, circle[i].Y);
            }
        }
    }


    [Test] public void SampledAreaAndPerimeter_ConvergeFromBelow()
    {
        // an inscribed polygon always under-estimates, and tightens as the resolution rises
        Circle shape     = new(P(0, 0), 2);
        double trueArea  = shape.Area();
        double truePerim = shape.Perimeter();
        double lastArea = 0, lastPerim = 0;

        foreach ( int resolution in new[] { 3, 6, 12, 48, 360, 10_000 } )
        {
            Span<ReadOnlyPoint>         buffer = new ReadOnlyPoint[resolution];
            ReadOnlySpan<ReadOnlyPoint> points = shape.PerimeterPoints(buffer, resolution);

            double area = 0, perimeter = 0;
            for ( int i = 0; i < resolution; i++ )
            {
                ReadOnlyPoint a = points[i];
                ReadOnlyPoint b = points[( i + 1 ) % resolution];
                area      += ( a.X * b.Y ) - ( b.X * a.Y );
                perimeter += a.DistanceTo(b);
            }

            area = Math.Abs(area) / 2;

            Assert.That(area,      Is.LessThanOrEqualTo(trueArea  + TOLERANCE), $"resolution {resolution} over-estimated the area");
            Assert.That(perimeter, Is.LessThanOrEqualTo(truePerim + TOLERANCE), $"resolution {resolution} over-estimated the perimeter");
            Assert.That(area,      Is.GreaterThan(lastArea));
            Assert.That(perimeter, Is.GreaterThan(lastPerim));

            lastArea  = area;
            lastPerim = perimeter;
        }

        AreClose(trueArea,  lastArea,  1e-6);
        AreClose(truePerim, lastPerim, 1e-6);
    }


    // ---------------------------------------------------------------------------------------------------------
    // guards and edge cases
    // ---------------------------------------------------------------------------------------------------------

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-100)]
    public void ResolutionBelowOne_Throws( int resolution )
    {
        // written with an explicit try/catch rather than Assert.That(lambda, Throws...):
        // a stackalloc'd Span cannot be carried into a lambda that is converted to a delegate
        bool threw = false;

        try
        {
            Span<ReadOnlyPoint> buffer = stackalloc ReadOnlyPoint[4];
            new Circle(P(0, 0), 1).PerimeterPoints(buffer, resolution);
        }
        catch ( ArgumentOutOfRangeException ) { threw = true; }

        Assert.That(threw, Is.True, $"a resolution of {resolution} must be rejected");
    }


    [Test] public void UndersizedDestination_Throws()
    {
        bool threw = false;

        try
        {
            Span<ReadOnlyPoint> buffer = stackalloc ReadOnlyPoint[4];
            new Circle(P(0, 0), 1).PerimeterPoints(buffer, 9);
        }
        catch ( ArgumentOutOfRangeException ) { threw = false; }   // the resolution itself is fine; only the buffer is too small
        catch ( ArgumentException ) { threw = true; }

        Assert.That(threw, Is.True, "a destination smaller than the resolution must be rejected");
    }


    [Test] public void OversizedDestination_IsFineAndReusable()
    {
        Circle              shape  = new(P(0, 0), 1);
        Span<ReadOnlyPoint> buffer = stackalloc ReadOnlyPoint[32];

        Assert.That(shape.PerimeterPoints(buffer, 5).Length,  Is.EqualTo(5));
        Assert.That(shape.PerimeterPoints(buffer, 12).Length, Is.EqualTo(12));
        Assert.That(shape.PerimeterPoints(buffer, 1).Length,  Is.EqualTo(1));
    }


    [Test] public void DegenerateCircles_PropagateRatherThanThrow()
    {
        Span<ReadOnlyPoint> buffer = stackalloc ReadOnlyPoint[4];

        Assert.That(new Circle(P(0, 0), 0).PerimeterPoints(buffer, 4)[0], Is.EqualTo(P(0, 0)));
        Assert.That(double.IsNaN(Circle.Invalid.PerimeterPoints(buffer, 4)[0].X), Is.True);
    }


    [Test] public void ToPolygon_CarriesTheSampledPoints()
    {
        Circle  shape   = new(P(0, 0), 3);
        Polygon polygon = shape.ToPolygon(12);

        Assert.That(polygon.Length, Is.EqualTo(12));
        foreach ( ref readonly ReadOnlyPoint point in polygon.Span ) { AreClose(3, shape.Center.DistanceTo(point)); }
    }
}
