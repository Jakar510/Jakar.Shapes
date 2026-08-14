// Jakar.Extensions :: Jakar.Shapes.Tests
// 08/13/2026

using Jakar.Shapes.Interfaces;



namespace Jakar.Shapes.Tests;


[TestFixture]
[TestOf(typeof(Square))]
public sealed class Quadrilateral_Tests : Assert
{
    private const double TOLERANCE = 1e-9;


    private static void AreClose( double expected, double actual, double tolerance = TOLERANCE ) => Assert.That(actual,
                                                                                                                Is.EqualTo(expected)
                                                                                                                  .Within(( Math.Abs(expected) * tolerance ) + tolerance));
    private static ReadOnlyPoint P( double x, double y ) => new(x, y);


    // ---------------------------------------------------------------------------------------------------------
    // square
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Square_Measurements()
    {
        Square shape = Square.Create(P(0, 0), 4);
        AreClose(16, shape.Area());
        AreClose(16, shape.Perimeter());

        AreClose(2,
                 shape.Centroid()
                      .X);

        AreClose(2,
                 shape.Centroid()
                      .Y);

        AreClose(Math.Sqrt(32),
                 shape.DiagonalLengths()
                      .AC);

        AreClose(Math.Sqrt(32),
                 shape.DiagonalLengths()
                      .BD);
    }


    [Test] public void Square_SatisfiesEveryWeakerClassification()
    {
        Square shape = Square.Create(P(0, 0), 4);
        this.IsTrue(shape.IsSquare());
        this.IsTrue(shape.IsRhombus());
        this.IsTrue(shape.IsRectangle());
        this.IsTrue(shape.IsParallelogram());
        this.IsTrue(shape.IsTrapezoid());
        this.IsTrue(shape.IsConvex());
    }


    [Test] public void Square_InteriorAngles_AreRightAnglesSummingTo360()
    {
        ( Degrees a, Degrees b, Degrees c, Degrees d ) = Square.Create(P(0, 0), 4)
                                                               .Angles();

        AreClose(90,  a.Value);
        AreClose(90,  b.Value);
        AreClose(90,  c.Value);
        AreClose(90,  d.Value);
        AreClose(360, a.Value + b.Value + c.Value + d.Value);
    }


    // ---------------------------------------------------------------------------------------------------------
    // rhombus, trapezoid, kite, parallelogram
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Rhombus_AreaIsHalfTheProductOfTheDiagonals()
    {
        Rhombus shape = Rhombus.Create(P(0, 0), 6, 8);
        AreClose(24, shape.Area()); // 6 * 8 / 2
        AreClose(20, shape.Perimeter());

        AreClose(5,
                 shape.SideLengths()
                      .AB);

        this.IsTrue(shape.IsRhombus());
        this.IsFalse(shape.IsSquare());
    }


    [Test] public void Trapezoid_AreaIsMeanWidthTimesHeight()
    {
        Trapezoid shape = Trapezoid.Create(P(0, 0), 6, 4, 3);
        AreClose(15,                shape.Area()); // (6 + 4) / 2 * 3
        AreClose(16.32455532033676, shape.Perimeter());
        this.IsTrue(shape.IsTrapezoid());
        this.IsFalse(shape.IsParallelogram());
        this.IsTrue(shape.IsConvex());
    }


    [Test] public void Kite_AreaIsHalfTheProductOfTheDiagonals()
    {
        Kite shape = Kite.Create(P(0, 0), 6, 4, 8);
        AreClose(36,                shape.Area()); // 6 * 12 / 2
        AreClose(27.08800749063506, shape.Perimeter());
        this.IsTrue(shape.IsKite());
        this.IsFalse(shape.IsRhombus());
    }


    [Test] public void Parallelogram_AreaIsTheCrossProductOfItsEdges()
    {
        Parallelogram shape = Parallelogram.Create(P(0, 0), P(5, 0), P(2, 3));
        AreClose(15,                        shape.Area()); // |5 * 3|
        AreClose(2 * ( 5 + Math.Sqrt(13) ), shape.Perimeter());
        this.IsTrue(shape.IsParallelogram());
        this.IsFalse(shape.IsRectangle());
    }


    [Test] public void AngleSum_IsAlways360()
    {
        check(Square.Create(P(0,    0), 4));
        check(Rhombus.Create(P(0,   0), 6, 8));
        check(Trapezoid.Create(P(0, 0), 6, 4, 3));
        check(Kite.Create(P(0,      0), 6, 4, 8));
        return;

        static void check<TQuad>( TQuad shape )
            where TQuad : struct, IQuadrilateral<TQuad>
        {
            ( Degrees a, Degrees b, Degrees c, Degrees d ) = shape.Angles();
            AreClose(360, a.Value + b.Value + c.Value + d.Value, 1e-6);
        }
    }


    // ---------------------------------------------------------------------------------------------------------
    // edges and diagonals
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Edges_RunBetweenConsecutiveVertices()
    {
        Square shape = Square.Create(P(0, 0), 4);

        Assert.That(shape.Ab()
                         .Start,
                    Is.EqualTo(shape.A));

        Assert.That(shape.Ab()
                         .End,
                    Is.EqualTo(shape.B));

        Assert.That(shape.Da()
                         .End,
                    Is.EqualTo(shape.A));

        AreClose(4,
                 shape.Ab()
                      .Length);
    }


    [Test] public void SideLengths_AreReportedInOrder()
    {
        ( double ab, double bc, double cd, double da ) = Trapezoid.Create(P(0, 0), 6, 4, 3)
                                                                  .SideLengths();

        AreClose(6,             ab);
        AreClose(Math.Sqrt(10), bc);
        AreClose(4,             cd);
        AreClose(Math.Sqrt(10), da);
    }


    // ---------------------------------------------------------------------------------------------------------
    // hit testing
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Contains_InteriorEdgeAndExterior()
    {
        Square shape = Square.Create(P(0, 0), 4);
        this.IsTrue(shape.Contains(P(2,   2)));
        this.IsTrue(shape.Contains(P(0,   2))); // on an edge
        this.IsTrue(shape.Contains(P(0,   0))); // on a vertex
        this.IsFalse(shape.Contains(P(5,  2)));
        this.IsFalse(shape.Contains(P(-1, -1)));
    }


    [Test] public void Intersects_OverlappingTouchingAndDisjoint()
    {
        Square shape = Square.Create(P(0, 0), 4);
        this.IsTrue(shape.Intersects(Square.Create(P(2,   2),  4)));
        this.IsTrue(shape.Intersects(Square.Create(P(4,   0),  4))); // edge to edge
        this.IsFalse(shape.Intersects(Square.Create(P(50, 50), 4)));
    }


    // ---------------------------------------------------------------------------------------------------------
    // transforms
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Scale_ChangesAreaByTheSquareOfTheFactor()
    {
        Square shape = Square.Create(P(0, 0), 4);

        AreClose(16 * 9,
                 shape.Scale(3)
                      .Area());

        AreClose(2,
                 shape.Scale(3)
                      .Centroid()
                      .X); // the centroid is preserved
    }


    [Test] public void Translate_PreservesAreaAndShiftsTheCentroid()
    {
        Square shape = Square.Create(P(0, 0), 4);
        Square moved = shape.Translate(7, -2);
        AreClose(16, moved.Area());

        AreClose(9,
                 moved.Centroid()
                      .X);

        AreClose(0,
                 moved.Centroid()
                      .Y);
    }


    [Test] public void Rotate_PreservesAreaPerimeterAndSquareness()
    {
        Square shape   = Square.Create(P(0, 0), 4);
        Square rotated = shape.Rotate(new Radians(0.42));
        AreClose(16, rotated.Area());
        AreClose(16, rotated.Perimeter());
        this.IsTrue(rotated.IsSquare());
    }


    [Test] public void BoundingBox_TightlyWrapsTheVertices()
    {
        ReadOnlyRectangle box = Square.Create(P(1, 2), 4)
                                      .BoundingBox();

        AreClose(1, box.X);
        AreClose(2, box.Y);
        AreClose(4, box.Width);
        AreClose(4, box.Height);
    }


    // ---------------------------------------------------------------------------------------------------------
    // operators, validity, equality
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Operators_TranslateAndScale()
    {
        Square shape = Square.Create(P(0, 0), 4);
        AreClose(16, ( shape + ( 3.0, 4.0 ) ).Area());
        AreClose(64, ( shape * 2 ).Area());
        AreClose(4,  ( shape / 2 ).Area());
    }


    [Test] public void Validity()
    {
        this.IsTrue(Square.Create(P(0, 0), 4)
                          .IsValid());

        this.IsFalse(Square.Invalid.IsValid());
        this.IsFalse(Square.Zero.IsValid()); // zero area
        this.IsTrue(Square.One.IsValid());
        AreClose(1, Square.One.Area());
        this.IsTrue(Square.Invalid.IsNaN());
    }


    [Test] public void Equality()
    {
        this.IsTrue(Square.Create(P(0, 0), 4) == Square.Create(P(0, 0), 4));
        this.IsTrue(Square.Create(P(0, 0), 4) != Square.Create(P(1, 0), 4));

        this.IsTrue(Square.Create(P(0, 0), 4)
                          .Equals(Square.Create(P(0, 0), 4)));

        this.IsFalse(Square.Create(P(0, 0), 4)
                           .Equals(Square.Create(P(0, 0), 5)));
    }


    [Test] public void Equals_Object_MatchesAnIdenticalInstance()
    {
        Square shape = Square.Create(P(0, 0), 4);
        object boxed = Square.Create(P(0, 0), 4);
        this.IsTrue(shape.Equals(boxed));
    }


    [Test] public void Deconstruct_YieldsTheFourVerticesInOrder()
    {
        Square shape = Square.Create(P(0, 0), 4);
        ( ReadOnlyPoint a, ReadOnlyPoint b, ReadOnlyPoint c, ReadOnlyPoint d ) = shape;
        Assert.That(a, Is.EqualTo(shape.A));
        Assert.That(b, Is.EqualTo(shape.B));
        Assert.That(c, Is.EqualTo(shape.C));
        Assert.That(d, Is.EqualTo(shape.D));
    }
}
