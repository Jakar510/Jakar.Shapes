// Jakar.Extensions :: Jakar.Shapes.Tests
// 08/13/2026

namespace Jakar.Shapes.Tests;


/// <summary> Covers the Area / Perimeter / BoundingBox / Contains / transform surface added to the pre-existing shapes. </summary>
[TestFixture]
[TestOf(typeof(Circle))]
public sealed class ShapeMeasurement_Tests : Assert
{
    private const double TOLERANCE = 1e-9;


    private static void AreClose( double expected, double actual, double tolerance = TOLERANCE ) => Assert.That(actual, Is.EqualTo(expected).Within(( Math.Abs(expected) * tolerance ) + tolerance));
    private static ReadOnlyPoint P( double x, double y ) => new(x, y);


    // ---------------------------------------------------------------------------------------------------------
    // circle
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Circle_Measurements()
    {
        Circle shape = new(P(0, 0), 3);
        AreClose(Math.PI * 9,     shape.Area());
        AreClose(2 * Math.PI * 3, shape.Perimeter());
        AreClose(2 * Math.PI * 3, shape.Circumference());
        AreClose(6,               shape.Diameter());
        AreClose(0,               shape.Centroid().X);
    }


    [Test] public void Circle_BoundingBox_IsTheEnclosingSquare()
    {
        ReadOnlyRectangle box = new Circle(P(1, 2), 3).BoundingBox();
        AreClose(-2, box.X);
        AreClose(-1, box.Y);
        AreClose(6,  box.Width);
        AreClose(6,  box.Height);
    }


    [Test] public void Circle_ContainsIntersectsEncloses()
    {
        Circle shape = new(P(0, 0), 3);
        this.IsTrue(shape.Contains(P(1,  1)));
        this.IsTrue(shape.Contains(P(3,  0)));   // on the boundary
        this.IsFalse(shape.Contains(P(4, 0)));
        this.IsTrue(shape.Intersects(new Circle(P(5,   0), 3)));
        this.IsFalse(shape.Intersects(new Circle(P(50, 0), 3)));
        this.IsTrue(shape.Encloses(new Circle(P(0,     0), 1)));
        this.IsFalse(shape.Encloses(new Circle(P(0,    0), 5)));
    }


    [Test] public void Circle_Transforms()
    {
        Circle shape = new(P(0, 0), 3);
        AreClose(Math.PI * 36, shape.Scale(2).Area());
        AreClose(4,            shape.Grow(1).Radius);
        AreClose(5,            shape.Translate(5, 0).Center.X);
        AreClose(3,            shape.Rotate(new Radians(1.2)).Radius);   // rotating about its own centre is the identity
    }


    // ---------------------------------------------------------------------------------------------------------
    // triangle
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Triangle_Measurements()
    {
        Triangle shape = new(P(0, 0), P(4, 0), P(0, 3));   // the 3-4-5 right triangle
        AreClose(6,  shape.Area());
        AreClose(12, shape.Perimeter());

        (double ab, double bc, double ca) = shape.SideLengths();
        AreClose(4, ab);
        AreClose(5, bc);
        AreClose(3, ca);
    }


    [Test] public void Triangle_Area_IsInvariantUnderRotation()
    {
        // guards the operator-precedence defect where the 0.5 multiplied only the first product,
        // which happened to give the right answer for axis-aligned triangles and the wrong one otherwise
        Triangle shape = new(P(0, 0), P(4, 0), P(0, 3));
        AreClose(6, shape.Rotate(new Radians(1.1)).Area());
        AreClose(6, shape.Rotate(new Radians(0.3)).Area());
        AreClose(6, shape.Translate(17, -9).Area());
    }


    [Test] public void Triangle_InscribedAndCircumscribedCircles()
    {
        Triangle shape = new(P(0, 0), P(4, 0), P(0, 3));
        AreClose(1,   shape.InscribedCircle().Radius);      // r = 2A / P
        AreClose(2.5, shape.CircumscribedCircle().Radius);  // half the hypotenuse
    }


    [Test] public void Triangle_ContainsAndIntersects()
    {
        Triangle shape = new(P(0, 0), P(4, 0), P(0, 3));
        this.IsTrue(shape.Contains(P(1,  1)));
        this.IsTrue(shape.Contains(P(0,  0)));
        this.IsFalse(shape.Contains(P(5, 5)));
        this.IsTrue(shape.Intersects(new Triangle(P(0,   0), P(1, 0), P(0, 1))));
        this.IsFalse(shape.Intersects(new Triangle(P(50, 50), P(51, 50), P(50, 51))));
    }


    [Test] public void Triangle_BoundingBoxAndScale()
    {
        Triangle          shape = new(P(0, 0), P(4, 0), P(0, 3));
        ReadOnlyRectangle box   = shape.BoundingBox();
        AreClose(4,  box.Width);
        AreClose(3,  box.Height);
        AreClose(24, shape.Scale(2).Area());
    }


    // ---------------------------------------------------------------------------------------------------------
    // rectangle
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Rectangle_Measurements()
    {
        ReadOnlyRectangle shape = new(0, 0, 4, 3);
        AreClose(12, shape.Area());
        AreClose(14, shape.Perimeter());
        AreClose(5,  shape.DiagonalLength());
        AreClose(5,  shape.DiagonalLengths().First);
        AreClose(90, shape.Angles().TopLeft.Value);
    }


    [Test] public void Rectangle_IntersectsAndTransforms()
    {
        ReadOnlyRectangle shape = new(0, 0, 4, 3);
        this.IsTrue(shape.Intersects(new ReadOnlyRectangle(2,   2,  4, 3)));
        this.IsFalse(shape.Intersects(new ReadOnlyRectangle(20, 20, 4, 3)));
        AreClose(48, shape.Scale(2).Area());
        AreClose(30, shape.Grow(1).Area());   // 6 x 5
        AreClose(5,  shape.Translate(5, 0).X);
    }
}
