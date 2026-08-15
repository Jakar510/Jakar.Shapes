// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;



namespace Jakar.Shapes;


/// <summary> A regular hexagon: 6 equal sides, described by its centre, circumradius and rotation. </summary>
[DefaultValue(nameof(Zero))]
[method: JsonConstructor]
public readonly struct Hexagon( ReadOnlyPoint center, double circumradius, Radians rotation ) : IRegularPolygon<Hexagon>
{
    /// <summary> Number of sides, always 6. </summary>
    public const int SIDES = 6;

    public static readonly Hexagon           Invalid      = new(ReadOnlyPoint.Invalid, double.NaN, Radians.Zero);
    public static readonly Hexagon           Zero         = new(ReadOnlyPoint.Zero, 0, Radians.Zero);
    public static readonly Hexagon           One          = new(ReadOnlyPoint.Zero, 1, Radians.Zero);
    public readonly        ReadOnlyPoint Center       = center;
    public readonly        double        Circumradius = circumradius;
    public readonly        Radians       Rotation     = rotation;


    static ref readonly Hexagon IShape<Hexagon>.Zero         => ref Zero;
    static ref readonly Hexagon IShape<Hexagon>.One          => ref One;
    static ref readonly Hexagon IShape<Hexagon>.Invalid      => ref Invalid;
    static int IRegularPolygon<Hexagon>.     SideCount    => SIDES;
    ReadOnlyPoint IShapeLocation.        Location     => Center;
    double IShapeLocation.               X            => Center.X;
    double IShapeLocation.               Y            => Center.Y;
    bool IValidator.                     IsValid      => this.IsValid();
    ReadOnlyPoint IRegularPolygon<Hexagon>.  Center       => Center;
    double IRegularPolygon<Hexagon>.         Circumradius => Circumradius;
    Radians IRegularPolygon<Hexagon>.        Rotation     => Rotation;


    public Hexagon( ReadOnlyPoint center, double circumradius ) : this(center, circumradius, Radians.Zero) { }
    public static implicit operator Hexagon( double circumradius ) => new(ReadOnlyPoint.Zero, circumradius, Radians.Zero);
    public static implicit operator Hexagon( int    circumradius ) => new(ReadOnlyPoint.Zero, circumradius, Radians.Zero);


    [Pure] public static Hexagon Create( in ReadOnlyPoint center, double circumradius, Radians rotation ) => new(center, circumradius, rotation);
    [Pure] public static Hexagon Create( in ReadOnlyPoint center, double circumradius )                   => new(center, circumradius, Radians.Zero);

    /// <summary> Builds the hexagon from the length of one side rather than the circumradius. </summary>
    [Pure] public static Hexagon FromSideLength( in ReadOnlyPoint center, double sideLength ) => new(center, sideLength / ( 2 * Math.Sin(Math.PI / SIDES) ), Radians.Zero);


    public static bool TryFromJson( string? json, out Hexagon result )
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
    public static Hexagon FromJson( string json ) => json.FromJson<Hexagon>();


    public int CompareTo( Hexagon other )
    {
        int a = Center.CompareTo(other.Center);
        if ( a != 0 ) { return a; }

        int b = Circumradius.CompareTo(other.Circumradius);
        return b != 0
                   ? b
                   : Rotation.Value.CompareTo(other.Rotation.Value);
    }
    public int CompareTo( object? obj )
    {
        if ( obj is null ) { return 1; }

        return obj is Hexagon other
                   ? CompareTo(other)
                   : throw new ExpectedValueTypeException(obj, typeof(Hexagon));
    }
    public          bool   Equals( Hexagon     other )                                     => Center.Equals(other.Center) && Circumradius.Equals(other.Circumradius) && Rotation.Equals(other.Rotation);
    public override bool   Equals( object? other )                                     => other is Hexagon x && Equals(x);
    public override int    GetHashCode()                                               => HashCode.Combine(Center, Circumradius, Rotation);
    public override string ToString()                                                  => ToString(null, null);
    public          string ToString( string? format, IFormatProvider? formatProvider ) => this.ToString(format);


    public static bool operator ==( Hexagon?  left, Hexagon?                        right ) => Nullable.Equals(left, right);
    public static bool operator !=( Hexagon?  left, Hexagon?                        right ) => !Nullable.Equals(left, right);
    public static bool operator ==( Hexagon   left, Hexagon                         right ) => EqualityComparer<Hexagon>.Default.Equals(left, right);
    public static bool operator !=( Hexagon   left, Hexagon                         right ) => !EqualityComparer<Hexagon>.Default.Equals(left, right);
    public static bool operator >( Hexagon    left, Hexagon                         right ) => Comparer<Hexagon>.Default.Compare(left, right) > 0;
    public static bool operator >=( Hexagon   left, Hexagon                         right ) => Comparer<Hexagon>.Default.Compare(left, right) >= 0;
    public static bool operator <( Hexagon    left, Hexagon                         right ) => Comparer<Hexagon>.Default.Compare(left, right) < 0;
    public static bool operator <=( Hexagon   left, Hexagon                         right ) => Comparer<Hexagon>.Default.Compare(left, right) <= 0;
    public static Hexagon operator +( Hexagon self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Hexagon operator -( Hexagon self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Hexagon operator *( Hexagon self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Hexagon operator /( Hexagon self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Hexagon operator +( Hexagon self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Hexagon operator -( Hexagon self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Hexagon operator *( Hexagon self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Hexagon operator /( Hexagon self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Hexagon operator +( Hexagon self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Hexagon operator -( Hexagon self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Hexagon operator *( Hexagon self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Hexagon operator /( Hexagon self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Hexagon operator +( Hexagon self, int                              value ) => self.Grow(value);
    public static Hexagon operator -( Hexagon self, int                              value ) => self.Grow(-value);
    public static Hexagon operator *( Hexagon self, int                              value ) => self.Scale(value);
    public static Hexagon operator /( Hexagon self, int                              value ) => self.Scale(1.0 / value);
    public static Hexagon operator +( Hexagon self, float                            value ) => self.Grow(value);
    public static Hexagon operator -( Hexagon self, float                            value ) => self.Grow(-value);
    public static Hexagon operator *( Hexagon self, float                            value ) => self.Scale(value);
    public static Hexagon operator /( Hexagon self, float                            value ) => self.Scale(1.0 / value);
    public static Hexagon operator +( Hexagon self, double                           value ) => self.Grow(value);
    public static Hexagon operator -( Hexagon self, double                           value ) => self.Grow(-value);
    public static Hexagon operator *( Hexagon self, double                           value ) => self.Scale(value);
    public static Hexagon operator /( Hexagon self, double                           value ) => self.Scale(1.0 / value);
}
