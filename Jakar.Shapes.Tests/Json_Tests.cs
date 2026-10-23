// Jakar.Extensions :: Jakar.Shapes.Tests
// 10/23/2026

using Jakar.Json;



namespace Jakar.Shapes.Tests;


[TestFixture]
public sealed class Json_Tests : Assert
{
    private static ReadOnlyPoint P( double x, double y ) => new(x, y);

    private static void RoundTrip<TShape>( TShape value )
        where TShape : IJsonSerializable<TShape>, IEquatable<TShape>
    {
        string json = JsonCodec.ToJson(value);
        TestContext.Out.WriteLine($"{typeof(TShape).Name}: {json}");

        TShape back = JsonCodec.FromJson<TShape>(json);
        Assert.That(back.Equals(value), Is.True, json);
        Assert.That(JsonCodec.ToJson(back), Is.EqualTo(json));
    }


    [Test] public void ReadOnlyPoint_RoundTrips()      => RoundTrip(P(1.5, -2.25));
    [Test] public void ReadOnlyPointF_RoundTrips()     => RoundTrip(new ReadOnlyPointF(1.5f, -2.25f));
    [Test] public void ReadOnlySize_RoundTrips()       => RoundTrip(new ReadOnlySize(3, 4.5));
    [Test] public void ReadOnlySizeF_RoundTrips()      => RoundTrip(new ReadOnlySizeF(3f, 4.5f));
    [Test] public void MutableSize_RoundTrips()        => RoundTrip(new MutableSize(3, 4.5));
    [Test] public void ReadOnlyRectangle_RoundTrips()  => RoundTrip(new ReadOnlyRectangle(1, 2, 3, 4));
    [Test] public void ReadOnlyRectangleF_RoundTrips() => RoundTrip(new ReadOnlyRectangleF(1f, 2f, 3f, 4f));
    [Test] public void MutableRectangle_RoundTrips()   => RoundTrip(new MutableRectangle(1, 2, 3, 4));
    [Test] public void ReadOnlyThickness_RoundTrips()  => RoundTrip(new ReadOnlyThickness(1, 2, 3, 4));
    [Test] public void ReadOnlyLine_RoundTrips()       => RoundTrip(new ReadOnlyLine(P(0, 0), P(3, 4), false));
    [Test] public void Circle_RoundTrips()             => RoundTrip(new Circle(P(1, 2), 5));
    [Test] public void Ellipse_RoundTrips()            => RoundTrip(new Ellipse(P(1, 2), 5, 3));
    [Test] public void Triangle_RoundTrips()           => RoundTrip(new Triangle(P(0, 0), P(4, 0), P(0, 3)));
    [Test] public void Square_RoundTrips()             => RoundTrip(new Square(P(0, 0), P(1, 0), P(1, 1), P(0, 1)));
    [Test] public void Rhombus_RoundTrips()            => RoundTrip(new Rhombus(P(0, 0), P(2, 1), P(4, 0), P(2, -1)));
    [Test] public void Kite_RoundTrips()               => RoundTrip(new Kite(P(0, 0), P(1, 2), P(0, 5), P(-1, 2)));
    [Test] public void Trapezoid_RoundTrips()          => RoundTrip(new Trapezoid(P(0, 0), P(4, 0), P(3, 2), P(1, 2)));
    [Test] public void Parallelogram_RoundTrips()      => RoundTrip(new Parallelogram(P(0, 0), P(4, 0), P(5, 2), P(1, 2)));
    [Test] public void Pentagon_RoundTrips()           => RoundTrip(new Pentagon(P(1, 2), 3, new Radians(0.5)));
    [Test] public void Hexagon_RoundTrips()            => RoundTrip(new Hexagon(P(1, 2), 3, new Radians(0.5)));
    [Test] public void Heptagon_RoundTrips()           => RoundTrip(new Heptagon(P(1, 2), 3, new Radians(0.5)));
    [Test] public void Octagon_RoundTrips()            => RoundTrip(new Octagon(P(1, 2), 3, new Radians(0.5)));
    [Test] public void Nonagon_RoundTrips()            => RoundTrip(new Nonagon(P(1, 2), 3, new Radians(0.5)));
    [Test] public void Decagon_RoundTrips()            => RoundTrip(new Decagon(P(1, 2), 3, new Radians(0.5)));
    [Test] public void Spline_RoundTrips()             => RoundTrip(new Spline(P(0, 0), P(1, 2), P(3, 1)));
    [Test] public void Polygon_RoundTrips()            => RoundTrip(new Polygon(P(0, 0), P(4, 0), P(2, 3)));
    [Test] public void Degrees_RoundTrips()            => RoundTrip(new Degrees(90));
    [Test] public void Radians_RoundTrips()            => RoundTrip(new Radians(1.25));


    // Computed members (IsNaN, Length, Location, Size, ...) are [JsonMember(Ignore = true)]: only the constructor data is written.
    [Test] public void ReadOnlyThickness_WritesOnlyItsSides()  => Assert.That(JsonCodec.ToJson(new ReadOnlyThickness(1, 2, 3, 4)),     Is.EqualTo("""{"Bottom" : 4,"Left" : 1,"Right" : 3,"Top" : 2}"""));
    [Test] public void ReadOnlyRectangle_WritesOnlyItsBounds() => Assert.That(JsonCodec.ToJson(new ReadOnlyRectangle(1, 2, 3, 4)),     Is.EqualTo("""{"X" : 1,"Y" : 2,"Width" : 3,"Height" : 4}"""));
    [Test] public void MutableRectangle_WritesOnlyItsBounds()  => Assert.That(JsonCodec.ToJson(new MutableRectangle(1, 2, 3, 4)),      Is.EqualTo("""{"X" : 1,"Y" : 2,"Width" : 3,"Height" : 4}"""));
    [Test] public void ReadOnlyLine_WritesOnlyItsEnds()        => Assert.That(JsonCodec.ToJson(new ReadOnlyLine(P(0, 0), P(3, 4))), Is.EqualTo("""{"Start" : {"X" : 0,"Y" : 0},"End" : {"X" : 3,"Y" : 4},"IsFinite" : true}"""));
    [Test] public void Triangle_WritesOnlyItsVertices()        => Assert.That(JsonCodec.ToJson(new Triangle(P(0, 0), P(4, 0), P(0, 3))), Is.EqualTo("""{"A" : {"X" : 0,"Y" : 0},"B" : {"X" : 4,"Y" : 0},"C" : {"X" : 0,"Y" : 3}}"""));
    [Test] public void Square_WritesOnlyItsVertices()          => Assert.That(JsonCodec.ToJson(new Square(P(0, 0), P(1, 0), P(1, 1), P(0, 1))), Is.EqualTo("""{"A" : {"X" : 0,"Y" : 0},"B" : {"X" : 1,"Y" : 0},"C" : {"X" : 1,"Y" : 1},"D" : {"X" : 0,"Y" : 1}}"""));
    [Test] public void Polygon_WritesOnlyItsPoints()           => Assert.That(JsonCodec.ToJson(new Polygon(P(0, 0), P(4, 0))),      Is.EqualTo("""{"Points" : [{"X" : 0,"Y" : 0},{"X" : 4,"Y" : 0}]}"""));
    [Test] public void Spline_WritesOnlyItsPoints()            => Assert.That(JsonCodec.ToJson(new Spline(P(0, 0), P(4, 0))),       Is.EqualTo("""{"Points" : [{"X" : 0,"Y" : 0},{"X" : 4,"Y" : 0}]}"""));


    [Test] public void ToString_Json_UsesJakarJson()
    {
        Circle circle = new(P(1, 2), 5);
        Assert.That(circle.ToString("json"), Is.EqualTo(JsonCodec.ToJson(circle)));
    }
}
