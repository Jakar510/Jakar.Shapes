// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;



namespace Jakar.Shapes;


/// <summary> A regular decagon: 10 equal sides, described by its centre, circumradius and rotation. </summary>
[DefaultValue(nameof(Zero))]
[method: JsonConstructor]
public readonly struct Decagon( ReadOnlyPoint center, double circumradius, Radians rotation ) : IRegularPolygon<Decagon>
{
    /// <summary> Number of sides, always 10. </summary>
    public const int SIDES = 10;

    public static readonly Decagon           Invalid      = new(ReadOnlyPoint.Invalid, double.NaN, Radians.Zero);
    public static readonly Decagon           Zero         = new(ReadOnlyPoint.Zero, 0, Radians.Zero);
    public static readonly Decagon           One          = new(ReadOnlyPoint.Zero, 1, Radians.Zero);
    public readonly        ReadOnlyPoint Center       = center;
    public readonly        double        Circumradius = circumradius;
    public readonly        Radians       Rotation     = rotation;


    static ref readonly Decagon IShape<Decagon>.Zero         => ref Zero;
    static ref readonly Decagon IShape<Decagon>.One          => ref One;
    static ref readonly Decagon IShape<Decagon>.Invalid      => ref Invalid;
    static int IRegularPolygon<Decagon>.     SideCount    => SIDES;
    ReadOnlyPoint IShapeLocation.        Location     => Center;
    double IShapeLocation.               X            => Center.X;
    double IShapeLocation.               Y            => Center.Y;
    bool IValidator.                     IsValid      => this.IsValid();
    ReadOnlyPoint IRegularPolygon<Decagon>.  Center       => Center;
    double IRegularPolygon<Decagon>.         Circumradius => Circumradius;
    Radians IRegularPolygon<Decagon>.        Rotation     => Rotation;


    public Decagon( ReadOnlyPoint center, double circumradius ) : this(center, circumradius, Radians.Zero) { }
    public static implicit operator Decagon( double circumradius ) => new(ReadOnlyPoint.Zero, circumradius, Radians.Zero);
    public static implicit operator Decagon( int    circumradius ) => new(ReadOnlyPoint.Zero, circumradius, Radians.Zero);


    [Pure] public static Decagon Create( in ReadOnlyPoint center, double circumradius, Radians rotation ) => new(center, circumradius, rotation);
    [Pure] public static Decagon Create( in ReadOnlyPoint center, double circumradius )                   => new(center, circumradius, Radians.Zero);

    /// <summary> Builds the decagon from the length of one side rather than the circumradius. </summary>
    [Pure] public static Decagon FromSideLength( in ReadOnlyPoint center, double sideLength ) => new(center, sideLength / ( 2 * Math.Sin(Math.PI / SIDES) ), Radians.Zero);


    public static bool TryFromJson( string? json, out Decagon result )
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
    public static Decagon FromJson( string json ) => json.FromJson<Decagon>();


    public int CompareTo( Decagon other )
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

        return obj is Decagon other
                   ? CompareTo(other)
                   : throw new ExpectedValueTypeException(obj, typeof(Decagon));
    }
    public          bool   Equals( Decagon     other )                                     => Center.Equals(other.Center) && Circumradius.Equals(other.Circumradius) && Rotation.Equals(other.Rotation);
    public override bool   Equals( object? other )                                     => other is Decagon x && Equals(x);
    public override int    GetHashCode()                                               => HashCode.Combine(Center, Circumradius, Rotation);
    public override string ToString()                                                  => ToString(null, null);
    public          string ToString( string? format, IFormatProvider? formatProvider ) => this.ToString(format);


    public static bool operator ==( Decagon?  left, Decagon?                        right ) => Nullable.Equals(left, right);
    public static bool operator !=( Decagon?  left, Decagon?                        right ) => !Nullable.Equals(left, right);
    public static bool operator ==( Decagon   left, Decagon                         right ) => EqualityComparer<Decagon>.Default.Equals(left, right);
    public static bool operator !=( Decagon   left, Decagon                         right ) => !EqualityComparer<Decagon>.Default.Equals(left, right);
    public static bool operator >( Decagon    left, Decagon                         right ) => Comparer<Decagon>.Default.Compare(left, right) > 0;
    public static bool operator >=( Decagon   left, Decagon                         right ) => Comparer<Decagon>.Default.Compare(left, right) >= 0;
    public static bool operator <( Decagon    left, Decagon                         right ) => Comparer<Decagon>.Default.Compare(left, right) < 0;
    public static bool operator <=( Decagon   left, Decagon                         right ) => Comparer<Decagon>.Default.Compare(left, right) <= 0;
    public static Decagon operator +( Decagon self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Decagon operator -( Decagon self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Decagon operator *( Decagon self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Decagon operator /( Decagon self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Decagon operator +( Decagon self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Decagon operator -( Decagon self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Decagon operator *( Decagon self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Decagon operator /( Decagon self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Decagon operator +( Decagon self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Decagon operator -( Decagon self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Decagon operator *( Decagon self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Decagon operator /( Decagon self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Decagon operator +( Decagon self, int                              value ) => self.Grow(value);
    public static Decagon operator -( Decagon self, int                              value ) => self.Grow(-value);
    public static Decagon operator *( Decagon self, int                              value ) => self.Scale(value);
    public static Decagon operator /( Decagon self, int                              value ) => self.Scale(1.0 / value);
    public static Decagon operator +( Decagon self, float                            value ) => self.Grow(value);
    public static Decagon operator -( Decagon self, float                            value ) => self.Grow(-value);
    public static Decagon operator *( Decagon self, float                            value ) => self.Scale(value);
    public static Decagon operator /( Decagon self, float                            value ) => self.Scale(1.0 / value);
    public static Decagon operator +( Decagon self, double                           value ) => self.Grow(value);
    public static Decagon operator -( Decagon self, double                           value ) => self.Grow(-value);
    public static Decagon operator *( Decagon self, double                           value ) => self.Scale(value);
    public static Decagon operator /( Decagon self, double                           value ) => self.Scale(1.0 / value);
}
