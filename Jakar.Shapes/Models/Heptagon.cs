// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;



namespace Jakar.Shapes;


/// <summary> A regular heptagon: 7 equal sides, described by its centre, circumradius and rotation. </summary>
[DefaultValue(nameof(Zero))]
[method: JsonConstructor]
public readonly struct Heptagon( ReadOnlyPoint center, double circumradius, Radians rotation ) : IRegularPolygon<Heptagon>
{
    /// <summary> Number of sides, always 7. </summary>
    public const int SIDES = 7;

    public static readonly Heptagon           Invalid      = new(ReadOnlyPoint.Invalid, double.NaN, Radians.Zero);
    public static readonly Heptagon           Zero         = new(ReadOnlyPoint.Zero, 0, Radians.Zero);
    public static readonly Heptagon           One          = new(ReadOnlyPoint.Zero, 1, Radians.Zero);
    public readonly        ReadOnlyPoint Center       = center;
    public readonly        double        Circumradius = circumradius;
    public readonly        Radians       Rotation     = rotation;


    static ref readonly Heptagon IShape<Heptagon>.Zero         => ref Zero;
    static ref readonly Heptagon IShape<Heptagon>.One          => ref One;
    static ref readonly Heptagon IShape<Heptagon>.Invalid      => ref Invalid;
    static int IRegularPolygon<Heptagon>.     SideCount    => SIDES;
    ReadOnlyPoint IShapeLocation.        Location     => Center;
    double IShapeLocation.               X            => Center.X;
    double IShapeLocation.               Y            => Center.Y;
    bool IValidator.                     IsValid      => this.IsValid();
    ReadOnlyPoint IRegularPolygon<Heptagon>.  Center       => Center;
    double IRegularPolygon<Heptagon>.         Circumradius => Circumradius;
    Radians IRegularPolygon<Heptagon>.        Rotation     => Rotation;


    public Heptagon( ReadOnlyPoint center, double circumradius ) : this(center, circumradius, Radians.Zero) { }
    public static implicit operator Heptagon( double circumradius ) => new(ReadOnlyPoint.Zero, circumradius, Radians.Zero);
    public static implicit operator Heptagon( int    circumradius ) => new(ReadOnlyPoint.Zero, circumradius, Radians.Zero);


    [Pure] public static Heptagon Create( in ReadOnlyPoint center, double circumradius, Radians rotation ) => new(center, circumradius, rotation);
    [Pure] public static Heptagon Create( in ReadOnlyPoint center, double circumradius )                   => new(center, circumradius, Radians.Zero);

    /// <summary> Builds the heptagon from the length of one side rather than the circumradius. </summary>
    [Pure] public static Heptagon FromSideLength( in ReadOnlyPoint center, double sideLength ) => new(center, sideLength / ( 2 * Math.Sin(Math.PI / SIDES) ), Radians.Zero);


    public static bool TryFromJson( string? json, out Heptagon result )
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
    public static Heptagon FromJson( string json ) => json.FromJson<Heptagon>();


    public int CompareTo( Heptagon other )
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

        return obj is Heptagon other
                   ? CompareTo(other)
                   : throw new ExpectedValueTypeException(obj, typeof(Heptagon));
    }
    public          bool   Equals( Heptagon     other )                                     => Center.Equals(other.Center) && Circumradius.Equals(other.Circumradius) && Rotation.Equals(other.Rotation);
    public override bool   Equals( object? other )                                     => other is Heptagon x && Equals(x);
    public override int    GetHashCode()                                               => HashCode.Combine(Center, Circumradius, Rotation);
    public override string ToString()                                                  => ToString(null, null);
    public          string ToString( string? format, IFormatProvider? formatProvider ) => this.ToString(format);


    public static bool operator ==( Heptagon?  left, Heptagon?                        right ) => Nullable.Equals(left, right);
    public static bool operator !=( Heptagon?  left, Heptagon?                        right ) => !Nullable.Equals(left, right);
    public static bool operator ==( Heptagon   left, Heptagon                         right ) => EqualityComparer<Heptagon>.Default.Equals(left, right);
    public static bool operator !=( Heptagon   left, Heptagon                         right ) => !EqualityComparer<Heptagon>.Default.Equals(left, right);
    public static bool operator >( Heptagon    left, Heptagon                         right ) => Comparer<Heptagon>.Default.Compare(left, right) > 0;
    public static bool operator >=( Heptagon   left, Heptagon                         right ) => Comparer<Heptagon>.Default.Compare(left, right) >= 0;
    public static bool operator <( Heptagon    left, Heptagon                         right ) => Comparer<Heptagon>.Default.Compare(left, right) < 0;
    public static bool operator <=( Heptagon   left, Heptagon                         right ) => Comparer<Heptagon>.Default.Compare(left, right) <= 0;
    public static Heptagon operator +( Heptagon self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Heptagon operator -( Heptagon self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Heptagon operator *( Heptagon self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Heptagon operator /( Heptagon self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Heptagon operator +( Heptagon self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Heptagon operator -( Heptagon self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Heptagon operator *( Heptagon self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Heptagon operator /( Heptagon self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Heptagon operator +( Heptagon self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Heptagon operator -( Heptagon self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Heptagon operator *( Heptagon self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Heptagon operator /( Heptagon self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Heptagon operator +( Heptagon self, int                              value ) => self.Grow(value);
    public static Heptagon operator -( Heptagon self, int                              value ) => self.Grow(-value);
    public static Heptagon operator *( Heptagon self, int                              value ) => self.Scale(value);
    public static Heptagon operator /( Heptagon self, int                              value ) => self.Scale(1.0 / value);
    public static Heptagon operator +( Heptagon self, float                            value ) => self.Grow(value);
    public static Heptagon operator -( Heptagon self, float                            value ) => self.Grow(-value);
    public static Heptagon operator *( Heptagon self, float                            value ) => self.Scale(value);
    public static Heptagon operator /( Heptagon self, float                            value ) => self.Scale(1.0 / value);
    public static Heptagon operator +( Heptagon self, double                           value ) => self.Grow(value);
    public static Heptagon operator -( Heptagon self, double                           value ) => self.Grow(-value);
    public static Heptagon operator *( Heptagon self, double                           value ) => self.Scale(value);
    public static Heptagon operator /( Heptagon self, double                           value ) => self.Scale(1.0 / value);
}
