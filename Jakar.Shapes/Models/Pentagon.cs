// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;



namespace Jakar.Shapes;


/// <summary> A regular pentagon: 5 equal sides, described by its centre, circumradius and rotation. </summary>
[DefaultValue(nameof(Zero))]
[method: JsonConstructor]
public readonly struct Pentagon( ReadOnlyPoint center, double circumradius, Radians rotation ) : IRegularPolygon<Pentagon>
{
    /// <summary> Number of sides, always 5. </summary>
    public const int SIDES = 5;

    public static readonly Pentagon           Invalid      = new(ReadOnlyPoint.Invalid, double.NaN, Radians.Zero);
    public static readonly Pentagon           Zero         = new(ReadOnlyPoint.Zero, 0, Radians.Zero);
    public static readonly Pentagon           One          = new(ReadOnlyPoint.Zero, 1, Radians.Zero);
    public readonly        ReadOnlyPoint Center       = center;
    public readonly        double        Circumradius = circumradius;
    public readonly        Radians       Rotation     = rotation;


    static ref readonly Pentagon IShape<Pentagon>.Zero         => ref Zero;
    static ref readonly Pentagon IShape<Pentagon>.One          => ref One;
    static ref readonly Pentagon IShape<Pentagon>.Invalid      => ref Invalid;
    static int IRegularPolygon<Pentagon>.     SideCount    => SIDES;
    ReadOnlyPoint IShapeLocation.        Location     => Center;
    double IShapeLocation.               X            => Center.X;
    double IShapeLocation.               Y            => Center.Y;
    bool IValidator.                     IsValid      => this.IsValid();
    ReadOnlyPoint IRegularPolygon<Pentagon>.  Center       => Center;
    double IRegularPolygon<Pentagon>.         Circumradius => Circumradius;
    Radians IRegularPolygon<Pentagon>.        Rotation     => Rotation;


    public Pentagon( ReadOnlyPoint center, double circumradius ) : this(center, circumradius, Radians.Zero) { }
    public static implicit operator Pentagon( double circumradius ) => new(ReadOnlyPoint.Zero, circumradius, Radians.Zero);
    public static implicit operator Pentagon( int    circumradius ) => new(ReadOnlyPoint.Zero, circumradius, Radians.Zero);


    [Pure] public static Pentagon Create( in ReadOnlyPoint center, double circumradius, Radians rotation ) => new(center, circumradius, rotation);
    [Pure] public static Pentagon Create( in ReadOnlyPoint center, double circumradius )                   => new(center, circumradius, Radians.Zero);

    /// <summary> Builds the pentagon from the length of one side rather than the circumradius. </summary>
    [Pure] public static Pentagon FromSideLength( in ReadOnlyPoint center, double sideLength ) => new(center, sideLength / ( 2 * Math.Sin(Math.PI / SIDES) ), Radians.Zero);


    public static bool TryFromJson( string? json, out Pentagon result )
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
    public static Pentagon FromJson( string json ) => json.FromJson<Pentagon>();


    public int CompareTo( Pentagon other )
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

        return obj is Pentagon other
                   ? CompareTo(other)
                   : throw new ExpectedValueTypeException(obj, typeof(Pentagon));
    }
    public          bool   Equals( Pentagon     other )                                     => Center.Equals(other.Center) && Circumradius.Equals(other.Circumradius) && Rotation.Equals(other.Rotation);
    public override bool   Equals( object? other )                                     => other is Pentagon x && Equals(x);
    public override int    GetHashCode()                                               => HashCode.Combine(Center, Circumradius, Rotation);
    public override string ToString()                                                  => ToString(null, null);
    public          string ToString( string? format, IFormatProvider? formatProvider ) => this.ToString(format);


    public static bool operator ==( Pentagon?  left, Pentagon?                        right ) => Nullable.Equals(left, right);
    public static bool operator !=( Pentagon?  left, Pentagon?                        right ) => !Nullable.Equals(left, right);
    public static bool operator ==( Pentagon   left, Pentagon                         right ) => EqualityComparer<Pentagon>.Default.Equals(left, right);
    public static bool operator !=( Pentagon   left, Pentagon                         right ) => !EqualityComparer<Pentagon>.Default.Equals(left, right);
    public static bool operator >( Pentagon    left, Pentagon                         right ) => Comparer<Pentagon>.Default.Compare(left, right) > 0;
    public static bool operator >=( Pentagon   left, Pentagon                         right ) => Comparer<Pentagon>.Default.Compare(left, right) >= 0;
    public static bool operator <( Pentagon    left, Pentagon                         right ) => Comparer<Pentagon>.Default.Compare(left, right) < 0;
    public static bool operator <=( Pentagon   left, Pentagon                         right ) => Comparer<Pentagon>.Default.Compare(left, right) <= 0;
    public static Pentagon operator +( Pentagon self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Pentagon operator -( Pentagon self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Pentagon operator *( Pentagon self, (int xOffset, int yOffset)       value ) => self.Translate(value.xOffset, value.yOffset);
    public static Pentagon operator /( Pentagon self, (int xOffset, int yOffset)       value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Pentagon operator +( Pentagon self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Pentagon operator -( Pentagon self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Pentagon operator *( Pentagon self, (float xOffset, float yOffset)   value ) => self.Translate(value.xOffset, value.yOffset);
    public static Pentagon operator /( Pentagon self, (float xOffset, float yOffset)   value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Pentagon operator +( Pentagon self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Pentagon operator -( Pentagon self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Pentagon operator *( Pentagon self, (double xOffset, double yOffset) value ) => self.Translate(value.xOffset, value.yOffset);
    public static Pentagon operator /( Pentagon self, (double xOffset, double yOffset) value ) => self.Translate(-value.xOffset, -value.yOffset);
    public static Pentagon operator +( Pentagon self, int                              value ) => self.Grow(value);
    public static Pentagon operator -( Pentagon self, int                              value ) => self.Grow(-value);
    public static Pentagon operator *( Pentagon self, int                              value ) => self.Scale(value);
    public static Pentagon operator /( Pentagon self, int                              value ) => self.Scale(1.0 / value);
    public static Pentagon operator +( Pentagon self, float                            value ) => self.Grow(value);
    public static Pentagon operator -( Pentagon self, float                            value ) => self.Grow(-value);
    public static Pentagon operator *( Pentagon self, float                            value ) => self.Scale(value);
    public static Pentagon operator /( Pentagon self, float                            value ) => self.Scale(1.0 / value);
    public static Pentagon operator +( Pentagon self, double                           value ) => self.Grow(value);
    public static Pentagon operator -( Pentagon self, double                           value ) => self.Grow(-value);
    public static Pentagon operator *( Pentagon self, double                           value ) => self.Scale(value);
    public static Pentagon operator /( Pentagon self, double                           value ) => self.Scale(1.0 / value);
}
