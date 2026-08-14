// Jakar.Extensions :: Jakar.Shapes.Tests
// 08/13/2026

namespace Jakar.Shapes.Tests;


[TestFixture]
[TestOf(typeof(Ellipse))]
public sealed class Ellipse_Tests : Assert
{
    private const double TOLERANCE = 1e-9;


    private static void AreClose( double expected, double actual, double tolerance = TOLERANCE ) => Assert.That(actual, Is.EqualTo(expected).Within(( Math.Abs(expected) * tolerance ) + tolerance));
    private static ReadOnlyPoint P( double x, double y ) => new(x, y);


    [Test] public void Measurements()
    {
        Ellipse shape = new(P(0, 0), 3, 2);
        AreClose(18.84955592153876,  shape.Area());        // pi * 3 * 2
        AreClose(15.865439589251233, shape.Perimeter());   // Ramanujan's second approximation
        AreClose(3, shape.SemiMajorAxis());
        AreClose(2, shape.SemiMinorAxis());
        AreClose(6, shape.MajorAxis());
        AreClose(4, shape.MinorAxis());
    }


    [Test] public void Eccentricity_AndFoci()
    {
        Ellipse shape = new(P(0, 0), 3, 2);
        AreClose(0.7453559924999299, shape.Eccentricity());

        (ReadOnlyPoint first, ReadOnlyPoint second) = shape.Foci();
        AreClose(-2.23606797749979, first.X);    // c = sqrt(a^2 - b^2)
        AreClose(2.23606797749979,  second.X);
        AreClose(0, first.Y);
    }


    [Test] public void EqualRadii_BehaveAsACircle()
    {
        Ellipse shape = new(P(0, 0), 5, 5);
        this.IsTrue(shape.IsCircle());
        AreClose(Math.PI * 25,   shape.Area());
        AreClose(2 * Math.PI * 5, shape.Perimeter(), 1e-6);
        AreClose(0, shape.Eccentricity());
        AreClose(0, shape.Foci().First.X);   // the foci collapse onto the centre
    }


    [Test] public void TallEllipse_PutsTheMajorAxisVertical()
    {
        Ellipse shape = new(P(0, 0), 2, 5);
        AreClose(5, shape.SemiMajorAxis());
        AreClose(2, shape.SemiMinorAxis());
        AreClose(0, shape.Foci().First.X);
        Assert.That(Math.Abs(shape.Foci().First.Y), Is.GreaterThan(0));
    }


    [Test] public void Contains_InteriorBoundaryAndExterior()
    {
        Ellipse shape = new(P(0, 0), 3, 2);
        this.IsTrue(shape.Contains(P(0,    0)));
        this.IsTrue(shape.Contains(P(3,    0)));   // on the boundary
        this.IsTrue(shape.Contains(P(0,    2)));
        this.IsFalse(shape.Contains(P(3.1, 0)));
        this.IsFalse(shape.Contains(P(3,   2)));   // corner of the bounding box, outside the curve
    }


    [Test] public void PointAt_WalksTheBoundary()
    {
        Ellipse shape = new(P(0, 0), 3, 2);
        AreClose(3, shape.PointAt(new Radians(0)).X);
        AreClose(0, shape.PointAt(new Radians(0)).Y);
        AreClose(2, shape.PointAt(new Radians(Math.PI / 2)).Y);
        this.IsTrue(shape.Contains(shape.PointAt(new Radians(1.234))));
    }


    [Test] public void BoundingBox_SpansBothDiameters()
    {
        ReadOnlyRectangle box = new Ellipse(P(1, 2), 3, 2).BoundingBox();
        AreClose(-2, box.X);
        AreClose(0,  box.Y);
        AreClose(6,  box.Width);
        AreClose(4,  box.Height);
    }


    [Test] public void Transforms()
    {
        Ellipse shape = new(P(0, 0), 3, 2);
        AreClose(shape.Area() * 4, shape.Scale(2).Area());
        AreClose(shape.Area(),     shape.Translate(4, 5).Area());
        AreClose(4,                shape.Translate(4, 5).Center.X);
        AreClose(6,                shape.Scale(2, 1).RadiusX);
        AreClose(2,                shape.Scale(2, 1).RadiusY);
    }


    [Test] public void Rotate_ByAQuarterTurn_SwapsTheRadii()
    {
        Ellipse shape   = new(P(0, 0), 3, 2);
        Ellipse rotated = shape.Rotate(new Radians(Math.PI / 2));
        AreClose(2, rotated.RadiusX);
        AreClose(3, rotated.RadiusY);
        AreClose(shape.Area(), rotated.Area());
    }


    [Test] public void Rotate_ByAnArbitraryAngle_IsNotRepresentable()
    {
        // an axis-aligned ellipse cannot express an oblique rotation, so the result is explicitly invalid
        Ellipse rotated = new Ellipse(P(0, 0), 3, 2).Rotate(new Radians(0.4));
        this.IsFalse(rotated.IsValid());
        this.IsTrue(rotated.IsNaN());
    }


    [Test] public void ConvertsFromCircle()
    {
        Ellipse shape = new Circle(P(1, 2), 4);
        AreClose(4, shape.RadiusX);
        AreClose(4, shape.RadiusY);
        AreClose(1, shape.Center.X);
        this.IsTrue(shape.IsCircle());
    }


    [Test] public void Validity()
    {
        this.IsTrue(new Ellipse(P(0, 0), 3, 2).IsValid());
        this.IsFalse(Ellipse.Invalid.IsValid());
        this.IsFalse(Ellipse.Zero.IsValid());
        this.IsTrue(Ellipse.One.IsValid());
        this.IsTrue(Ellipse.Invalid.IsNaN());
        this.IsFalse(new Ellipse(P(0, 0), -1, 2).IsValid());
    }


    [Test] public void Equality()
    {
        this.IsTrue(new Ellipse(P(1,  2), 3, 4) == new Ellipse(P(1, 2), 3, 4));
        this.IsTrue(new Ellipse(P(1,  2), 3, 4) != new Ellipse(P(1, 2), 3, 5));
        this.IsTrue(new Ellipse(P(1,  2), 3, 4).Equals(new Ellipse(P(1, 2), 3, 4)));
        this.IsTrue(new Ellipse(P(1,  2), 3, 4).Equals((object)new Ellipse(P(1, 2), 3, 4)));
    }


    [Test] public void Deconstruct()
    {
        (ReadOnlyPoint center, double radiusX, double radiusY) = new Ellipse(P(1, 2), 3, 4);
        AreClose(1, center.X);
        AreClose(3, radiusX);
        AreClose(4, radiusY);
    }
}
