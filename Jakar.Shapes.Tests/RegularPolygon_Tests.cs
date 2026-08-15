// Jakar.Extensions :: Jakar.Shapes.Tests
// 08/13/2026

namespace Jakar.Shapes.Tests;


[TestFixture]
[TestOf(typeof(Pentagon))]
public sealed class RegularPolygon_Tests : Assert
{
    private const double TOLERANCE = 1e-9;


    private static void AreClose( double expected, double actual, double tolerance = TOLERANCE ) => Assert.That(actual, Is.EqualTo(expected).Within(( Math.Abs(expected) * tolerance ) + tolerance));
    private static ReadOnlyPoint P( double x, double y ) => new(x, y);


    // ---------------------------------------------------------------------------------------------------------
    // side counts
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void SideCounts_MatchTheirNames()
    {
        Assert.That(Pentagon.SIDES, Is.EqualTo(5));
        Assert.That(Hexagon.SIDES,  Is.EqualTo(6));
        Assert.That(Heptagon.SIDES, Is.EqualTo(7));
        Assert.That(Octagon.SIDES,  Is.EqualTo(8));
        Assert.That(Nonagon.SIDES,  Is.EqualTo(9));
        Assert.That(Decagon.SIDES,  Is.EqualTo(10));
    }


    [Test] public void VertexCount_MatchesSideCount()
    {
        Span<ReadOnlyPoint> points = stackalloc ReadOnlyPoint[RegularPolygons.MAX_SIDES];
        Assert.That(new Pentagon(P(0, 0), 2).Vertices(points).Length, Is.EqualTo(5));
        Assert.That(new Hexagon(P(0, 0),  2).Vertices(points).Length, Is.EqualTo(6));
        Assert.That(new Heptagon(P(0, 0), 2).Vertices(points).Length, Is.EqualTo(7));
        Assert.That(new Octagon(P(0, 0),  2).Vertices(points).Length, Is.EqualTo(8));
        Assert.That(new Nonagon(P(0, 0),  2).Vertices(points).Length, Is.EqualTo(9));
        Assert.That(new Decagon(P(0, 0),  2).Vertices(points).Length, Is.EqualTo(10));

        Span<ReadOnlyLine> edges = stackalloc ReadOnlyLine[RegularPolygons.MAX_SIDES];
        Assert.That(new Octagon(P(0, 0), 2).Edges(edges).Length, Is.EqualTo(8));
    }


    [Test] public void VertexCountProperty_MatchesTheBufferItNeeds()
    {
        Assert.That(new Pentagon(P(0, 0), 2).VertexCount,   Is.EqualTo(5));
        Assert.That(new Decagon(P(0, 0),  2).VertexCount,   Is.EqualTo(10));
        Assert.That(new Pentagon(P(0, 0), 2).DiagonalCount, Is.EqualTo(2));    // n - 3
        Assert.That(new Decagon(P(0, 0),  2).DiagonalCount, Is.EqualTo(7));
    }


    [Test] public void Vertices_RejectsAnUndersizedDestination()
    {
        // written with an explicit try/catch rather than Assert.That(lambda, Throws...):
        // a stackalloc'd Span cannot be carried into a lambda that is converted to a delegate
        bool threw = false;

        try
        {
            Span<ReadOnlyPoint> tooSmall = stackalloc ReadOnlyPoint[3];
            new Decagon(P(0, 0), 2).Vertices(tooSmall);
        }
        catch ( ArgumentException ) { threw = true; }

        Assert.That(threw, Is.True, "a destination smaller than SideCount must be rejected");
    }


    // ---------------------------------------------------------------------------------------------------------
    // measurements against the closed-form values
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Pentagon_Measurements()
    {
        Pentagon shape = new(P(0, 0), 2);
        AreClose(9.510565162951535,  shape.Area());
        AreClose(2.3511410091698925, shape.SideLength());
        AreClose(11.755705045849464, shape.Perimeter());
        AreClose(1.618033988749895,  shape.Apothem());
        AreClose(108, shape.InteriorAngle().Value);
        AreClose(72,  shape.ExteriorAngle().Value);
    }


    [Test] public void Hexagon_Measurements()
    {
        Hexagon shape = new(P(0, 0), 2);
        AreClose(10.392304845413264, shape.Area());
        AreClose(2,                  shape.SideLength());   // a hexagon's side equals its circumradius
        AreClose(12,                 shape.Perimeter());
        AreClose(1.7320508075688774, shape.Apothem());
        AreClose(120, shape.InteriorAngle().Value);
    }


    [Test] public void Heptagon_Measurements()
    {
        Heptagon shape = new(P(0, 0), 2);
        AreClose(10.945640754552418, shape.Area());
        AreClose(1.7355349564702325, shape.SideLength());
        AreClose(128.57142857142858, shape.InteriorAngle().Value);
    }


    [Test] public void Octagon_Measurements()
    {
        Octagon shape = new(P(0, 0), 2);
        AreClose(11.31370849898476,  shape.Area());
        AreClose(1.5307337294603591, shape.SideLength());
        AreClose(1.8477590650225735, shape.Apothem());
        AreClose(135, shape.InteriorAngle().Value);
    }


    [Test] public void Nonagon_Measurements()
    {
        Nonagon shape = new(P(0, 0), 2);
        AreClose(11.570176974357707, shape.Area());
        AreClose(1.3680805733026749, shape.SideLength());
        AreClose(140, shape.InteriorAngle().Value);
    }


    [Test] public void Decagon_Measurements()
    {
        Decagon shape = new(P(0, 0), 2);
        AreClose(11.755705045849464, shape.Area());
        AreClose(1.2360679774997896, shape.SideLength());
        AreClose(144, shape.InteriorAngle().Value);
    }


    // ---------------------------------------------------------------------------------------------------------
    // structural invariants -- these hold for every regular polygon
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void ClosedFormArea_MatchesShoelaceOverTheVertices()
    {
        Span<ReadOnlyPoint> buffer = stackalloc ReadOnlyPoint[RegularPolygons.MAX_SIDES];
        Shoelace(new Pentagon(P(0, 0),   2).Vertices(buffer),   new Pentagon(P(0, 0),   2).Area());
        Shoelace(new Hexagon(P(1, 2),    3).Vertices(buffer),   new Hexagon(P(1, 2),    3).Area());
        Shoelace(new Heptagon(P(0, 0),   2).Vertices(buffer),   new Heptagon(P(0, 0),   2).Area());
        Shoelace(new Octagon(P(-4, 5), 1.5).Vertices(buffer),   new Octagon(P(-4, 5), 1.5).Area());
        Shoelace(new Nonagon(P(0, 0),    2).Vertices(buffer),   new Nonagon(P(0, 0),    2).Area());
        Shoelace(new Decagon(P(2, -3),   4).Vertices(buffer),   new Decagon(P(2, -3),   4).Area());
        return;

        static void Shoelace( ReadOnlySpan<ReadOnlyPoint> points, double expected )
        {
            double sum = 0;
            for ( int i = 0; i < points.Length; i++ )
            {
                ReadOnlyPoint a = points[i];
                ReadOnlyPoint b = points[( i + 1 ) % points.Length];
                sum += ( a.X * b.Y ) - ( b.X * a.Y );
            }

            AreClose(expected, Math.Abs(sum) / 2);
        }
    }


    [Test] public void EveryVertex_LiesOnTheCircumscribedCircle()
    {
        Pentagon            shape  = new(P(3, -2), 5);
        Span<ReadOnlyPoint> buffer = stackalloc ReadOnlyPoint[shape.VertexCount];

        foreach ( ref readonly ReadOnlyPoint vertex in shape.Vertices(buffer) ) { AreClose(5, shape.Center.DistanceTo(vertex)); }
    }


    [Test] public void EverySide_HasTheSameLength()
    {
        Nonagon                     shape  = new(P(0, 0), 3);
        Span<ReadOnlyPoint>         buffer = stackalloc ReadOnlyPoint[shape.VertexCount];
        ReadOnlySpan<ReadOnlyPoint> points = shape.Vertices(buffer);

        for ( int i = 0; i < points.Length; i++ ) { AreClose(shape.SideLength(), points[i].DistanceTo(points[( i + 1 ) % points.Length])); }

        Span<double>         lengths = stackalloc double[shape.VertexCount];
        ReadOnlySpan<double> sides   = shape.SideLengths(lengths);
        Assert.That(sides.Length, Is.EqualTo(9));
        foreach ( double length in sides ) { AreClose(shape.SideLength(), length); }
    }


    [Test] public void InteriorAndExteriorAngles_SumTo180()
    {
        AreClose(180, new Pentagon(P(0, 0), 1).InteriorAngle().Value + new Pentagon(P(0, 0), 1).ExteriorAngle().Value);
        AreClose(180, new Decagon(P(0, 0),  1).InteriorAngle().Value + new Decagon(P(0, 0),  1).ExteriorAngle().Value);
    }


    [Test] public void ApothemIsAlwaysBelowCircumradius_AndInscribedFitsInsideCircumscribed()
    {
        Octagon shape = new(P(0, 0), 2);
        Assert.That(shape.Apothem(), Is.LessThan(shape.Circumradius));
        AreClose(shape.Apothem(),    shape.InscribedCircle().Radius);
        AreClose(shape.Circumradius, shape.CircumscribedCircle().Radius);
    }


    // ---------------------------------------------------------------------------------------------------------
    // hit testing
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Contains_CentreAndInterior_ButNotOutside()
    {
        Pentagon shape = new(P(0, 0), 2);
        this.IsTrue(shape.Contains(P(0,    0)));
        this.IsTrue(shape.Contains(P(1.5,  0)));
        this.IsFalse(shape.Contains(P(2.5, 0)));
        this.IsFalse(shape.Contains(P(99,  99)));
    }


    [Test] public void Intersects_OverlappingAndDisjoint()
    {
        Hexagon shape = new(P(0, 0), 3);
        this.IsTrue(shape.Intersects(new Hexagon(P(2,   0), 3)));
        this.IsFalse(shape.Intersects(new Hexagon(P(50, 0), 3)));
        this.IsTrue(shape.Intersects(new Circle(P(1,    1), 1)));
    }


    // ---------------------------------------------------------------------------------------------------------
    // transforms
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Scale_ChangesAreaByTheSquareOfTheFactor()
    {
        Pentagon shape = new(P(0, 0), 2);
        AreClose(shape.Area() * 4, shape.Scale(2).Area());
        AreClose(shape.Area() / 4, shape.Scale(0.5).Area());
    }


    [Test] public void Translate_MovesTheCentreAndPreservesSize()
    {
        Hexagon shape = new(P(0, 0), 2);
        Hexagon moved = shape.Translate(10, -5);
        AreClose(10, moved.Center.X);
        AreClose(-5, moved.Center.Y);
        AreClose(shape.Area(), moved.Area());
    }


    [Test] public void Rotate_PreservesAreaAndPerimeter()
    {
        Heptagon shape   = new(P(1, 1), 2);
        Heptagon rotated = shape.Rotate(new Radians(0.7));
        AreClose(shape.Area(),      rotated.Area());
        AreClose(shape.Perimeter(), rotated.Perimeter());
    }


    [Test] public void FromSideLength_RoundTrips()
    {
        AreClose(3, Hexagon.FromSideLength(P(0, 0), 3).SideLength());
        AreClose(2, Decagon.FromSideLength(P(0, 0), 2).SideLength());
    }


    [Test] public void BoundingBox_ContainsEveryVertex()
    {
        Octagon             shape  = new(P(2, -1), 3);
        ReadOnlyRectangle   box    = shape.BoundingBox();
        Span<ReadOnlyPoint> buffer = stackalloc ReadOnlyPoint[shape.VertexCount];

        foreach ( ref readonly ReadOnlyPoint vertex in shape.Vertices(buffer) )
        {
            Assert.That(vertex.X, Is.InRange(box.X - TOLERANCE,             box.X + box.Width  + TOLERANCE));
            Assert.That(vertex.Y, Is.InRange(box.Y - TOLERANCE,             box.Y + box.Height + TOLERANCE));
        }
    }


    // ---------------------------------------------------------------------------------------------------------
    // validity and equality
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Validity()
    {
        this.IsTrue(new Pentagon(P(0, 0), 2).IsValid());
        this.IsFalse(Pentagon.Invalid.IsValid());
        this.IsFalse(Pentagon.Zero.IsValid());
        this.IsTrue(Pentagon.One.IsValid());
        this.IsTrue(Pentagon.Invalid.IsNaN());
    }


    [Test] public void Equality()
    {
        this.IsTrue(new Pentagon(P(1,  2), 3) == new Pentagon(P(1, 2), 3));
        this.IsTrue(new Pentagon(P(1,  2), 3) != new Pentagon(P(1, 2), 4));
        this.IsTrue(new Hexagon(P(1,   2), 3).Equals(new Hexagon(P(1, 2), 3)));
        this.IsFalse(new Hexagon(P(1,  2), 3).Equals(new Hexagon(P(9, 2), 3)));
        Assert.That(new Octagon(P(1, 2), 3).GetHashCode(), Is.EqualTo(new Octagon(P(1, 2), 3).GetHashCode()));
    }


    [Test] public void Equals_Object_MatchesAnIdenticalInstance()
    {
        Pentagon shape = new(P(1, 2), 3);
        object   boxed = new Pentagon(P(1, 2), 3);
        this.IsTrue(shape.Equals(boxed));
    }
}
