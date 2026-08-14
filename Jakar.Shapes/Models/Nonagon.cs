// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;



namespace Jakar.Shapes;


/// <summary> A regular nonagon: 9 equal sides, described by its centre, circumradius and rotation. </summary>
[DefaultValue(nameof(Zero))]
[method: JsonConstructor]
public readonly struct Nonagon( ReadOnlyPoint center, double circumradius, Radians rotation ) : IRegularPolygon<Nonagon>
{
    /// <summary> Number of sides, always 9. </summary>
    public const int SIDES = 9;

    public static readonly Nonagon           Invalid      = new(ReadOnlyPoint.Invalid, double.NaN, Radians.Zero);
    public static readonly Nonagon           Zero         = new(ReadOnlyPoint.Zero, 0, Radians.Zero);
    public static readonly Nonagon           One          = new(ReadOnlyPoint.Zero, 1, Radians.Zero);
    public readonly        ReadOnlyPoint Center       = center;
    public readonly        double        Circumradius = circumradius;
    public readonly        Radians       Rotation     = rotation;


    static ref readonly Nonagon IShape<Nonagon>.Zero         => ref Zero;
    static ref readonly Nonagon IShape<Nonagon>.One          => ref One;
    static ref readonly Nonagon IShape<Nonagon>.Invalid      => ref Invalid;
    static int IRegularPolygon<Nonagon>.     SideCount    => SIDES;
    ReadOnlyPoint IShapeLocation.        Location     => Center;
    double IShapeLocation.               X            => Center.X;
    double IShapeLocation.               Y            => Center.Y;
    bool IValidator.                     IsValid      => this.IsValid();
    ReadOnlyPoint IRegularPolygon<Nonagon>.  Center       => Center;
    double IRegularPolygon<Nonagon>.         Circumradius => Circumradius;
    Radians IRegularPolygon<Nonagon>.        Rotation     => Rotation;


    public Nonagon( ReadOnlyPoint center, double circumradius ) : this(center, circumradius, Radians.Zero) { }
    public static implicit operator Nonagon( double circumradius ) => new(ReadOnlyPoint.Zero, circumradius, Radians.Zero);
    public static implicit operator Nonagon( int    circumradius ) => new(ReadOnlyPoint.Zero, circumradius, Radians.Zero);


    [Pure] public static Nonagon Create( in ReadOnlyPoint center, double circumradius, Radians rotation ) => new(center, circumradius, rotation);
    [Pure] public static Nonagon Create( in ReadOnlyPoint center, double circumradius )                   => new(center, circumradius, Radians.Zero);

    /// <summary> Builds the nonagon from the length of one side rather than the circumradius. </summary>
    [Pure] public static Nonagon FromSideLength( in ReadOnlyPoint center, double sideLength ) => new(center, sideLength / ( 2 * Math.Sin(Math.PI / SIDES) ), Radians.Zero);


    public static bool TryFromJson( string? json, out Nonagon result )
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
    public static Nonagon FromJson( string json ) => json.FromJson<Nonagon>();


    public int CompareTo( Nonagon other )
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

        return obj is Nonagon other
                   ? CompareTo(other)
                   : throw new ExpectedValueTypeException(obj, typeof(Nonagon));
    }
    public          bool   Equals( Nonagon     other )                                     => Center.Equals(other.Center) && Circumradius.Equals(other.Circumradius) && Rotation.Equals(other.Rotation);
    public override bool   Equals( object? other )                                     => other is Nonagon x && Equals(x);
    public override int    GetHashCode()                                               => HashCode.Combine(Center, Circumradius, Rotation);
    public override string ToString()                                                  => ToString(null, null);
    public          string ToString( string? format, IFormatProvider? formatProvider ) => this.ToString(format);


    public static bool operator ==( Nonagon?  left, Nonagon?                        right ) => Nullable.Equals(left, right);
    public static bool operator !=( Nonagon?  left, Nonagon?                        right ) => !Nullable.Equals(left, right);
    public static bool operator ==( Nonagon   left, Nonagon                         right ) => EqualityComparer<Nonagon>.Default.Equals(left, right);
    public static bool operator !=( Nonagon   left, Nonagon                         right ) => !EqualityComparer<Nonagon>.Default.Equals(left, right);
    public static bool operator >( Nonagon    left, Nonagon                         right ) => Comparer<Nonagon>.Default.Compare(left, right) > 0;
    public static bool operator >=( Nonagon   left, Nonagon                         right ) => Comparer<Nonagon>.Default.Compare(left, right) >= 0;
    public static bool operator <( Nonagon    left, Nonagon                         right ) => Comparer<Nonagon>.Default.Compare(left, right) < 0;
    public static bool operator <=( Nonagon   left, Nonagon                         right ) => Comparer<Nonagon>.Default.Compare(left, right) <= 0;
    public static Nonagon operator +( Nonagon self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Nonagon operator -( Nonagon self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Nonagon operator *( Nonagon self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Nonagon operator /( Nonagon self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Nonagon operator +( Nonagon self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Nonagon operator -( Nonagon self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Nonagon operator *( Nonagon self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Nonagon operator /( Nonagon self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Nonagon operator +( Nonagon self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Nonagon operator -( Nonagon self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Nonagon operator *( Nonagon self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Nonagon operator /( Nonagon self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Nonagon operator +( Nonagon self, int                              value ) => self.Grow(value);
    public static Nonagon operator -( Nonagon self, int                              value ) => self.Grow(-value);
    public static Nonagon operator *( Nonagon self, int                              value ) => self.Scale(value);
    public static Nonagon operator /( Nonagon self, int                              value ) => self.Scale(1.0 / value);
    public static Nonagon operator +( Nonagon self, float                            value ) => self.Grow(value);
    public static Nonagon operator -( Nonagon self, float                            value ) => self.Grow(-value);
    public static Nonagon operator *( Nonagon self, float                            value ) => self.Scale(value);
    public static Nonagon operator /( Nonagon self, float                            value ) => self.Scale(1.0 / value);
    public static Nonagon operator +( Nonagon self, double                           value ) => self.Grow(value);
    public static Nonagon operator -( Nonagon self, double                           value ) => self.Grow(-value);
    public static Nonagon operator *( Nonagon self, double                           value ) => self.Scale(value);
    public static Nonagon operator /( Nonagon self, double                           value ) => self.Scale(1.0 / value);
}
