// Jakar.Extensions :: Jakar.Shapes.Tests
// 08/13/2026

namespace Jakar.Shapes.Tests;


[TestFixture]
[TestOf(typeof(Spline))]
public sealed class Spline_Tests : Assert
{
    private static ReadOnlyPoint P( double x, double y ) => new(x, y);


    // ---------------------------------------------------------------------------------------------------------
    // static initialisation order
    //
    // __empty has to be declared before Invalid/Zero/One. Static initialisers run in declaration order, so with
    // __empty declared last it is still null while those three are being built, 'points ?? __empty' collapses to
    // null, and every member reading Points.Length throws NullReferenceException.
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void Invalid_HasABackingArray()
    {
        Assert.That(Spline.Invalid.Points,  Is.Not.Null);
        Assert.That(Polygon.Invalid.Points, Is.Not.Null);
        Assert.That(Spline.Zero.Points,     Is.Not.Null);
        Assert.That(Spline.One.Points,      Is.Not.Null);
    }


    [Test] public void Invalid_IsEmptyWithoutThrowing()
    {
        this.IsTrue(Spline.Invalid.IsEmpty);
        Assert.That(Spline.Invalid.Length,      Is.EqualTo(0));
        Assert.That(Spline.Invalid.Span.Length, Is.EqualTo(0));
        this.IsFalse(Spline.Invalid.IsValid);
        Assert.That(Spline.Invalid.GetHashCode(), Is.TypeOf<int>());
    }


    [Test] public void PolygonInvalid_IsEmptyWithoutThrowing()
    {
        this.IsTrue(Polygon.Invalid.IsEmpty);
        Assert.That(Polygon.Invalid.Length, Is.EqualTo(0));
        this.IsFalse(Polygon.Invalid.IsValid);
    }


    /// <summary> Field initialisers never run for <c> default(T) </c>, so Points is null there whatever the declaration order. </summary>
    [Test] public void Default_IsSafeToInspect()
    {
        Spline spline = default;
        this.IsTrue(spline.IsEmpty);
        Assert.That(spline.Length,      Is.EqualTo(0));
        Assert.That(spline.Span.Length, Is.EqualTo(0));
        Assert.That(spline.GetHashCode(), Is.TypeOf<int>());

        Polygon polygon = default;
        this.IsTrue(polygon.IsEmpty);
        Assert.That(polygon.Length, Is.EqualTo(0));
    }


    [Test] public void LineOfBestFit_RejectsAnEmptySplineInsteadOfThrowing()
    {
        Spline empty = Spline.Invalid;
        this.IsFalse(LineOfBestFit.Fit(in empty).IsValid);
        this.IsFalse(LineOfBestFit.Fit(in empty, 1).IsValid);
        this.IsFalse(LineOfBestFit.Fit(in empty, 0).IsValid);
        this.IsTrue(double.IsNaN(LineOfBestFit.Calculate(in empty)[1]));

        Spline fallback = default;
        this.IsFalse(LineOfBestFit.Fit(in fallback).IsValid);
    }


    // ---------------------------------------------------------------------------------------------------------
    // ordinary behaviour is unchanged
    // ---------------------------------------------------------------------------------------------------------

    [Test] public void PopulatedSplines_ReportTheirLength()
    {
        Spline one = new(P(3, 7));
        Spline two = new(P(1, 3), P(2, 5));

        this.IsTrue(one.IsEmpty);   // a single point is still "empty" for fitting purposes
        Assert.That(one.Length, Is.EqualTo(1));

        this.IsFalse(two.IsEmpty);
        Assert.That(two.Length, Is.EqualTo(2));
        this.IsTrue(two.IsValid);
    }


    [Test] public void Zero_And_One_HoldASinglePoint()
    {
        Assert.That(Spline.Zero.Length, Is.EqualTo(1));
        Assert.That(Spline.One.Length,  Is.EqualTo(1));
        this.IsTrue(Spline.Zero.IsEmpty);
    }
}
