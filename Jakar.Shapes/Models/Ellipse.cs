// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;



namespace Jakar.Shapes;


/// <summary> An axis-aligned ellipse (oval). A <see cref="Circle"/> is the special case where both radii are equal. </summary>
[DefaultValue(nameof(Zero))]
[method: JsonConstructor]
public readonly struct Ellipse( ReadOnlyPoint center, double radiusX, double radiusY ) : IEllipse<Ellipse>
{
    public static readonly Ellipse       Invalid = new(ReadOnlyPoint.Invalid, double.NaN, double.NaN);
    public static readonly Ellipse       Zero    = new(ReadOnlyPoint.Zero, 0, 0);
    public static readonly Ellipse       One     = new(ReadOnlyPoint.Zero, 1, 1);
    public readonly        ReadOnlyPoint Center  = center;
    public readonly        double        RadiusX = radiusX;
    public readonly        double        RadiusY = radiusY;


    static ref readonly Ellipse IShape<Ellipse>.Zero     => ref Zero;
    static ref readonly Ellipse IShape<Ellipse>.One      => ref One;
    static ref readonly Ellipse IShape<Ellipse>.Invalid  => ref Invalid;
    ReadOnlyPoint IShapeLocation.               Location => Center;
    double IShapeLocation.                      X        => Center.X;
    double IShapeLocation.                      Y        => Center.Y;
    bool IValidator.                            IsValid  => this.IsValid();
    ReadOnlyPoint IEllipse<Ellipse>.            Center   => Center;
    double IEllipse<Ellipse>.                   RadiusX  => RadiusX;
    double IEllipse<Ellipse>.                   RadiusY  => RadiusY;


    public static implicit operator Ellipse( Circle circle ) => new(circle.Center, circle.Radius, circle.Radius);
    public static implicit operator Ellipse( double radius ) => new(ReadOnlyPoint.Zero, radius, radius);
    public static implicit operator Ellipse( int    radius ) => new(ReadOnlyPoint.Zero, radius, radius);


    [Pure] public static Ellipse Create( in ReadOnlyPoint center, double radiusX, double radiusY ) => new(center, radiusX, radiusY);


    public static bool TryFromJson( string? json, out Ellipse result )
    {
        try
        {
            if ( string.IsNullOrWhiteSpace(json) )
            {
                result = Invalid;
                return false;
            }

            result = FromJson(json);
            return true;
        }
        catch ( Exception e ) { SelfLogger.WriteLine("{Exception}", e.ToString()); }

        result = Invalid;
        return false;
    }
    public static Ellipse FromJson( string json ) => json.FromJson<Ellipse>();


    public int CompareTo( Ellipse other )
    {
        int a = Center.CompareTo(other.Center);
        if ( a != 0 ) { return a; }

        int b = RadiusX.CompareTo(other.RadiusX);
        return b != 0
                   ? b
                   : RadiusY.CompareTo(other.RadiusY);
    }
    public int CompareTo( object? obj )
    {
        if ( obj is null ) { return 1; }

        return obj is Ellipse other
                   ? CompareTo(other)
                   : throw new ExpectedValueTypeException(obj, typeof(Ellipse));
    }
    public          bool   Equals( Ellipse other )                                     => Center.Equals(other.Center) && RadiusX.Equals(other.RadiusX) && RadiusY.Equals(other.RadiusY);
    public override bool   Equals( object?  other )                                    => other is Ellipse x && Equals(x);
    public override int    GetHashCode()                                               => HashCode.Combine(Center, RadiusX, RadiusY);
    public override string ToString()                                                  => ToString(null, null);
    public          string ToString( string? format, IFormatProvider? formatProvider ) => this.ToString(format);


    public static bool operator ==( Ellipse?  left, Ellipse?                        right ) => Nullable.Equals(left, right);
    public static bool operator !=( Ellipse?  left, Ellipse?                        right ) => !Nullable.Equals(left, right);
    public static bool operator ==( Ellipse   left, Ellipse                         right ) => EqualityComparer<Ellipse>.Default.Equals(left, right);
    public static bool operator !=( Ellipse   left, Ellipse                         right ) => !EqualityComparer<Ellipse>.Default.Equals(left, right);
    public static bool operator >( Ellipse    left, Ellipse                         right ) => Comparer<Ellipse>.Default.Compare(left, right) > 0;
    public static bool operator >=( Ellipse   left, Ellipse                         right ) => Comparer<Ellipse>.Default.Compare(left, right) >= 0;
    public static bool operator <( Ellipse    left, Ellipse                         right ) => Comparer<Ellipse>.Default.Compare(left, right) < 0;
    public static bool operator <=( Ellipse   left, Ellipse                         right ) => Comparer<Ellipse>.Default.Compare(left, right) <= 0;
    public static Ellipse operator +( Ellipse self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Ellipse operator -( Ellipse self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Ellipse operator *( Ellipse self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Ellipse operator /( Ellipse self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Ellipse operator +( Ellipse self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Ellipse operator -( Ellipse self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Ellipse operator *( Ellipse self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Ellipse operator /( Ellipse self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Ellipse operator +( Ellipse self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Ellipse operator -( Ellipse self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Ellipse operator *( Ellipse self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Ellipse operator /( Ellipse self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Ellipse operator +( Ellipse self, int                              value ) => self.Grow(value);
    public static Ellipse operator -( Ellipse self, int                              value ) => self.Grow(-value);
    public static Ellipse operator *( Ellipse self, int                              value ) => self.Scale(value);
    public static Ellipse operator /( Ellipse self, int                              value ) => self.Scale(1.0 / value);
    public static Ellipse operator +( Ellipse self, float                            value ) => self.Grow(value);
    public static Ellipse operator -( Ellipse self, float                            value ) => self.Grow(-value);
    public static Ellipse operator *( Ellipse self, float                            value ) => self.Scale(value);
    public static Ellipse operator /( Ellipse self, float                            value ) => self.Scale(1.0 / value);
    public static Ellipse operator +( Ellipse self, double                           value ) => self.Grow(value);
    public static Ellipse operator -( Ellipse self, double                           value ) => self.Grow(-value);
    public static Ellipse operator *( Ellipse self, double                           value ) => self.Scale(value);
    public static Ellipse operator /( Ellipse self, double                           value ) => self.Scale(1.0 / value);
}
