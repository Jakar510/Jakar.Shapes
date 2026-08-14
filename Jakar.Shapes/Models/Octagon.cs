// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;



namespace Jakar.Shapes;


/// <summary> A regular octagon: 8 equal sides, described by its centre, circumradius and rotation. </summary>
[DefaultValue(nameof(Zero))]
[method: JsonConstructor]
public readonly struct Octagon( ReadOnlyPoint center, double circumradius, Radians rotation ) : IRegularPolygon<Octagon>
{
    /// <summary> Number of sides, always 8. </summary>
    public const int SIDES = 8;

    public static readonly Octagon           Invalid      = new(ReadOnlyPoint.Invalid, double.NaN, Radians.Zero);
    public static readonly Octagon           Zero         = new(ReadOnlyPoint.Zero, 0, Radians.Zero);
    public static readonly Octagon           One          = new(ReadOnlyPoint.Zero, 1, Radians.Zero);
    public readonly        ReadOnlyPoint Center       = center;
    public readonly        double        Circumradius = circumradius;
    public readonly        Radians       Rotation     = rotation;


    static ref readonly Octagon IShape<Octagon>.Zero         => ref Zero;
    static ref readonly Octagon IShape<Octagon>.One          => ref One;
    static ref readonly Octagon IShape<Octagon>.Invalid      => ref Invalid;
    static int IRegularPolygon<Octagon>.     SideCount    => SIDES;
    ReadOnlyPoint IShapeLocation.        Location     => Center;
    double IShapeLocation.               X            => Center.X;
    double IShapeLocation.               Y            => Center.Y;
    bool IValidator.                     IsValid      => this.IsValid();
    ReadOnlyPoint IRegularPolygon<Octagon>.  Center       => Center;
    double IRegularPolygon<Octagon>.         Circumradius => Circumradius;
    Radians IRegularPolygon<Octagon>.        Rotation     => Rotation;


    public Octagon( ReadOnlyPoint center, double circumradius ) : this(center, circumradius, Radians.Zero) { }
    public static implicit operator Octagon( double circumradius ) => new(ReadOnlyPoint.Zero, circumradius, Radians.Zero);
    public static implicit operator Octagon( int    circumradius ) => new(ReadOnlyPoint.Zero, circumradius, Radians.Zero);


    [Pure] public static Octagon Create( in ReadOnlyPoint center, double circumradius, Radians rotation ) => new(center, circumradius, rotation);
    [Pure] public static Octagon Create( in ReadOnlyPoint center, double circumradius )                   => new(center, circumradius, Radians.Zero);

    /// <summary> Builds the octagon from the length of one side rather than the circumradius. </summary>
    [Pure] public static Octagon FromSideLength( in ReadOnlyPoint center, double sideLength ) => new(center, sideLength / ( 2 * Math.Sin(Math.PI / SIDES) ), Radians.Zero);


    public static bool TryFromJson( string? json, out Octagon result )
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
    public static Octagon FromJson( string json ) => json.FromJson<Octagon>();


    public int CompareTo( Octagon other )
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

        return obj is Octagon other
                   ? CompareTo(other)
                   : throw new ExpectedValueTypeException(obj, typeof(Octagon));
    }
    public          bool   Equals( Octagon     other )                                     => Center.Equals(other.Center) && Circumradius.Equals(other.Circumradius) && Rotation.Equals(other.Rotation);
    public override bool   Equals( object? other )                                     => other is Octagon x && Equals(x);
    public override int    GetHashCode()                                               => HashCode.Combine(Center, Circumradius, Rotation);
    public override string ToString()                                                  => ToString(null, null);
    public          string ToString( string? format, IFormatProvider? formatProvider ) => this.ToString(format);


    public static bool operator ==( Octagon?  left, Octagon?                        right ) => Nullable.Equals(left, right);
    public static bool operator !=( Octagon?  left, Octagon?                        right ) => !Nullable.Equals(left, right);
    public static bool operator ==( Octagon   left, Octagon                         right ) => EqualityComparer<Octagon>.Default.Equals(left, right);
    public static bool operator !=( Octagon   left, Octagon                         right ) => !EqualityComparer<Octagon>.Default.Equals(left, right);
    public static bool operator >( Octagon    left, Octagon                         right ) => Comparer<Octagon>.Default.Compare(left, right) > 0;
    public static bool operator >=( Octagon   left, Octagon                         right ) => Comparer<Octagon>.Default.Compare(left, right) >= 0;
    public static bool operator <( Octagon    left, Octagon                         right ) => Comparer<Octagon>.Default.Compare(left, right) < 0;
    public static bool operator <=( Octagon   left, Octagon                         right ) => Comparer<Octagon>.Default.Compare(left, right) <= 0;
    public static Octagon operator +( Octagon self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Octagon operator -( Octagon self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Octagon operator *( Octagon self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Octagon operator /( Octagon self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Octagon operator +( Octagon self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Octagon operator -( Octagon self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Octagon operator *( Octagon self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Octagon operator /( Octagon self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Octagon operator +( Octagon self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Octagon operator -( Octagon self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Octagon operator *( Octagon self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Octagon operator /( Octagon self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Octagon operator +( Octagon self, int                              value ) => self.Grow(value);
    public static Octagon operator -( Octagon self, int                              value ) => self.Grow(-value);
    public static Octagon operator *( Octagon self, int                              value ) => self.Scale(value);
    public static Octagon operator /( Octagon self, int                              value ) => self.Scale(1.0 / value);
    public static Octagon operator +( Octagon self, float                            value ) => self.Grow(value);
    public static Octagon operator -( Octagon self, float                            value ) => self.Grow(-value);
    public static Octagon operator *( Octagon self, float                            value ) => self.Scale(value);
    public static Octagon operator /( Octagon self, float                            value ) => self.Scale(1.0 / value);
    public static Octagon operator +( Octagon self, double                           value ) => self.Grow(value);
    public static Octagon operator -( Octagon self, double                           value ) => self.Grow(-value);
    public static Octagon operator *( Octagon self, double                           value ) => self.Scale(value);
    public static Octagon operator /( Octagon self, double                           value ) => self.Scale(1.0 / value);
}
