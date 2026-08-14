// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;



namespace Jakar.Shapes;


/// <summary> Four vertices in order. Use IsTrapezoid() to verify at least one pair of parallel sides. </summary>
[DefaultValue(nameof(Zero))]
[method: JsonConstructor]
public readonly struct Trapezoid( ReadOnlyPoint a, ReadOnlyPoint b, ReadOnlyPoint c, ReadOnlyPoint d ) : IQuadrilateral<Trapezoid>
{
    public static readonly Trapezoid    Invalid = new(ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid);
    public static readonly Trapezoid    Zero    = new(ReadOnlyPoint.Zero, ReadOnlyPoint.Zero, ReadOnlyPoint.Zero, ReadOnlyPoint.Zero);
    public static readonly Trapezoid    One     = new(ReadOnlyPoint.Zero, new ReadOnlyPoint(1, 0), ReadOnlyPoint.One, new ReadOnlyPoint(0, 1));
    public readonly        ReadOnlyPoint A = a;
    public readonly        ReadOnlyPoint B = b;
    public readonly        ReadOnlyPoint C = c;
    public readonly        ReadOnlyPoint D = d;


    static ref readonly Trapezoid IShape<Trapezoid>.Zero     => ref Zero;
    static ref readonly Trapezoid IShape<Trapezoid>.One      => ref One;
    static ref readonly Trapezoid IShape<Trapezoid>.Invalid  => ref Invalid;
    ReadOnlyPoint IShapeLocation.        Location => this.Centroid();
    double IShapeLocation.               X        => this.Centroid().X;
    double IShapeLocation.               Y        => this.Centroid().Y;
    bool IValidator.                     IsValid  => this.IsValid();
    ReadOnlyPoint IQuadrilateral<Trapezoid>.   A        => A;
    ReadOnlyPoint IQuadrilateral<Trapezoid>.   B        => B;
    ReadOnlyPoint IQuadrilateral<Trapezoid>.   C        => C;
    ReadOnlyPoint IQuadrilateral<Trapezoid>.   D        => D;
    public bool                          IsNaN    => A.IsNaN() || B.IsNaN() || C.IsNaN() || D.IsNaN();


    [Pure] public static Trapezoid Create( ReadOnlyPoint a, ReadOnlyPoint b, ReadOnlyPoint c, ReadOnlyPoint d ) => new(a, b, c, d);

    /// <summary> Isosceles trapezoid: <paramref name="origin"/> is the lower-left corner of the bottom edge. </summary>
    [Pure] public static Trapezoid Create( in ReadOnlyPoint origin, double bottomWidth, double topWidth, double height )
    {
        double inset = ( bottomWidth - topWidth ) / 2;
        return new Trapezoid(origin, new ReadOnlyPoint(origin.X + bottomWidth, origin.Y), new ReadOnlyPoint(origin.X + inset + topWidth, origin.Y + height), new ReadOnlyPoint(origin.X + inset, origin.Y + height));
    }


    public static bool TryFromJson( string? json, out Trapezoid result )
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
    public static Trapezoid FromJson( string json ) => json.FromJson<Trapezoid>();


    public int CompareTo( Trapezoid other )
    {
        int a1 = A.CompareTo(other.A);
        if ( a1 != 0 ) { return a1; }

        int b1 = B.CompareTo(other.B);
        if ( b1 != 0 ) { return b1; }

        int c1 = C.CompareTo(other.C);
        if ( c1 != 0 ) { return c1; }

        return D.CompareTo(other.D);
    }
    public int CompareTo( object? obj )
    {
        if ( obj is null ) { return 1; }

        return obj is Trapezoid other
                   ? CompareTo(other)
                   : throw new ExpectedValueTypeException(obj, typeof(Trapezoid));
    }
    public          bool   Equals( Trapezoid     other )                                     => A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D);
    public override bool   Equals( object? other )                                     => other is Trapezoid x && Equals(x);
    public override int    GetHashCode()                                               => HashCode.Combine(A, B, C, D);
    public override string ToString()                                                  => ToString(null, null);
    public          string ToString( string? format, IFormatProvider? formatProvider ) => this.ToString(format);


    public static bool operator ==( Trapezoid?  left, Trapezoid?                        right ) => Nullable.Equals(left, right);
    public static bool operator !=( Trapezoid?  left, Trapezoid?                        right ) => !Nullable.Equals(left, right);
    public static bool operator ==( Trapezoid   left, Trapezoid                         right ) => EqualityComparer<Trapezoid>.Default.Equals(left, right);
    public static bool operator !=( Trapezoid   left, Trapezoid                         right ) => !EqualityComparer<Trapezoid>.Default.Equals(left, right);
    public static bool operator >( Trapezoid    left, Trapezoid                         right ) => Comparer<Trapezoid>.Default.Compare(left, right) > 0;
    public static bool operator >=( Trapezoid   left, Trapezoid                         right ) => Comparer<Trapezoid>.Default.Compare(left, right) >= 0;
    public static bool operator <( Trapezoid    left, Trapezoid                         right ) => Comparer<Trapezoid>.Default.Compare(left, right) < 0;
    public static bool operator <=( Trapezoid   left, Trapezoid                         right ) => Comparer<Trapezoid>.Default.Compare(left, right) <= 0;
    public static Trapezoid operator +( Trapezoid self, Trapezoid                             value ) => self.Add(value);
    public static Trapezoid operator -( Trapezoid self, Trapezoid                             value ) => self.Subtract(value);
    public static Trapezoid operator *( Trapezoid self, Trapezoid                             value ) => self.Multiply(value);
    public static Trapezoid operator /( Trapezoid self, Trapezoid                             value ) => self.Divide(value);
    public static Trapezoid operator +( Trapezoid self, (int xOffset, int yOffset)       value ) => self.Add(value);
    public static Trapezoid operator -( Trapezoid self, (int xOffset, int yOffset)       value ) => self.Subtract(value);
    public static Trapezoid operator *( Trapezoid self, (int xOffset, int yOffset)       value ) => self.Multiply(value);
    public static Trapezoid operator /( Trapezoid self, (int xOffset, int yOffset)       value ) => self.Divide(value);
    public static Trapezoid operator +( Trapezoid self, (float xOffset, float yOffset)   value ) => self.Add(value);
    public static Trapezoid operator -( Trapezoid self, (float xOffset, float yOffset)   value ) => self.Subtract(value);
    public static Trapezoid operator *( Trapezoid self, (float xOffset, float yOffset)   value ) => self.Multiply(value);
    public static Trapezoid operator /( Trapezoid self, (float xOffset, float yOffset)   value ) => self.Divide(value);
    public static Trapezoid operator +( Trapezoid self, (double xOffset, double yOffset) value ) => self.Add(value);
    public static Trapezoid operator -( Trapezoid self, (double xOffset, double yOffset) value ) => self.Subtract(value);
    public static Trapezoid operator *( Trapezoid self, (double xOffset, double yOffset) value ) => self.Multiply(value);
    public static Trapezoid operator /( Trapezoid self, (double xOffset, double yOffset) value ) => self.Divide(value);
    public static Trapezoid operator +( Trapezoid self, int                              value ) => self.Add(value);
    public static Trapezoid operator -( Trapezoid self, int                              value ) => self.Subtract(value);
    public static Trapezoid operator *( Trapezoid self, int                              value ) => self.Multiply(value);
    public static Trapezoid operator /( Trapezoid self, int                              value ) => self.Divide(value);
    public static Trapezoid operator +( Trapezoid self, float                            value ) => self.Add(value);
    public static Trapezoid operator -( Trapezoid self, float                            value ) => self.Subtract(value);
    public static Trapezoid operator *( Trapezoid self, float                            value ) => self.Multiply(value);
    public static Trapezoid operator /( Trapezoid self, float                            value ) => self.Divide(value);
    public static Trapezoid operator +( Trapezoid self, double                           value ) => self.Add(value);
    public static Trapezoid operator -( Trapezoid self, double                           value ) => self.Subtract(value);
    public static Trapezoid operator *( Trapezoid self, double                           value ) => self.Multiply(value);
    public static Trapezoid operator /( Trapezoid self, double                           value ) => self.Divide(value);
}
