// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;



namespace Jakar.Shapes;


/// <summary> Four vertices in order. Use IsKite() to verify two disjoint pairs of adjacent equal sides. </summary>
[DefaultValue(nameof(Zero))]
[method: JsonConstructor]
public readonly struct Kite( ReadOnlyPoint a, ReadOnlyPoint b, ReadOnlyPoint c, ReadOnlyPoint d ) : IQuadrilateral<Kite>
{
    public static readonly Kite    Invalid = new(ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid);
    public static readonly Kite    Zero    = new(ReadOnlyPoint.Zero, ReadOnlyPoint.Zero, ReadOnlyPoint.Zero, ReadOnlyPoint.Zero);
    public static readonly Kite    One     = new(ReadOnlyPoint.Zero, new ReadOnlyPoint(1, 0), ReadOnlyPoint.One, new ReadOnlyPoint(0, 1));
    public readonly        ReadOnlyPoint A = a;
    public readonly        ReadOnlyPoint B = b;
    public readonly        ReadOnlyPoint C = c;
    public readonly        ReadOnlyPoint D = d;


    static ref readonly Kite IShape<Kite>.Zero     => ref Zero;
    static ref readonly Kite IShape<Kite>.One      => ref One;
    static ref readonly Kite IShape<Kite>.Invalid  => ref Invalid;
    ReadOnlyPoint IShapeLocation.        Location => this.Centroid();
    double IShapeLocation.               X        => this.Centroid().X;
    double IShapeLocation.               Y        => this.Centroid().Y;
    bool IValidator.                     IsValid  => this.IsValid();
    ReadOnlyPoint IQuadrilateral<Kite>.   A        => A;
    ReadOnlyPoint IQuadrilateral<Kite>.   B        => B;
    ReadOnlyPoint IQuadrilateral<Kite>.   C        => C;
    ReadOnlyPoint IQuadrilateral<Kite>.   D        => D;
    public bool                          IsNaN    => A.IsNaN() || B.IsNaN() || C.IsNaN() || D.IsNaN();


    [Pure] public static Kite Create( ReadOnlyPoint a, ReadOnlyPoint b, ReadOnlyPoint c, ReadOnlyPoint d ) => new(a, b, c, d);

    /// <summary> Built from a centre, the horizontal diagonal, and the two vertical arms measured from that diagonal. </summary>
    [Pure] public static Kite Create( in ReadOnlyPoint center, double horizontalDiagonal, double upperArm, double lowerArm )
    {
        double x = horizontalDiagonal / 2;
        return new Kite(new ReadOnlyPoint(center.X, center.Y + upperArm), new ReadOnlyPoint(center.X + x, center.Y), new ReadOnlyPoint(center.X, center.Y - lowerArm), new ReadOnlyPoint(center.X - x, center.Y));
    }


    public static bool TryFromJson( string? json, out Kite result )
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
    public static Kite FromJson( string json ) => json.FromJson<Kite>();


    public int CompareTo( Kite other )
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

        return obj is Kite other
                   ? CompareTo(other)
                   : throw new ExpectedValueTypeException(obj, typeof(Kite));
    }
    public          bool   Equals( Kite     other )                                     => A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D);
    public override bool   Equals( object? other )                                     => other is Kite x && Equals(x);
    public override int    GetHashCode()                                               => HashCode.Combine(A, B, C, D);
    public override string ToString()                                                  => ToString(null, null);
    public          string ToString( string? format, IFormatProvider? formatProvider ) => this.ToString(format);


    public static bool operator ==( Kite?  left, Kite?                        right ) => Nullable.Equals(left, right);
    public static bool operator !=( Kite?  left, Kite?                        right ) => !Nullable.Equals(left, right);
    public static bool operator ==( Kite   left, Kite                         right ) => EqualityComparer<Kite>.Default.Equals(left, right);
    public static bool operator !=( Kite   left, Kite                         right ) => !EqualityComparer<Kite>.Default.Equals(left, right);
    public static bool operator >( Kite    left, Kite                         right ) => Comparer<Kite>.Default.Compare(left, right) > 0;
    public static bool operator >=( Kite   left, Kite                         right ) => Comparer<Kite>.Default.Compare(left, right) >= 0;
    public static bool operator <( Kite    left, Kite                         right ) => Comparer<Kite>.Default.Compare(left, right) < 0;
    public static bool operator <=( Kite   left, Kite                         right ) => Comparer<Kite>.Default.Compare(left, right) <= 0;
    public static Kite operator +( Kite self, Kite                             value ) => self.Add(value);
    public static Kite operator -( Kite self, Kite                             value ) => self.Subtract(value);
    public static Kite operator *( Kite self, Kite                             value ) => self.Multiply(value);
    public static Kite operator /( Kite self, Kite                             value ) => self.Divide(value);
    public static Kite operator +( Kite self, (int xOffset, int yOffset)       value ) => self.Add(value);
    public static Kite operator -( Kite self, (int xOffset, int yOffset)       value ) => self.Subtract(value);
    public static Kite operator *( Kite self, (int xOffset, int yOffset)       value ) => self.Multiply(value);
    public static Kite operator /( Kite self, (int xOffset, int yOffset)       value ) => self.Divide(value);
    public static Kite operator +( Kite self, (float xOffset, float yOffset)   value ) => self.Add(value);
    public static Kite operator -( Kite self, (float xOffset, float yOffset)   value ) => self.Subtract(value);
    public static Kite operator *( Kite self, (float xOffset, float yOffset)   value ) => self.Multiply(value);
    public static Kite operator /( Kite self, (float xOffset, float yOffset)   value ) => self.Divide(value);
    public static Kite operator +( Kite self, (double xOffset, double yOffset) value ) => self.Add(value);
    public static Kite operator -( Kite self, (double xOffset, double yOffset) value ) => self.Subtract(value);
    public static Kite operator *( Kite self, (double xOffset, double yOffset) value ) => self.Multiply(value);
    public static Kite operator /( Kite self, (double xOffset, double yOffset) value ) => self.Divide(value);
    public static Kite operator +( Kite self, int                              value ) => self.Add(value);
    public static Kite operator -( Kite self, int                              value ) => self.Subtract(value);
    public static Kite operator *( Kite self, int                              value ) => self.Multiply(value);
    public static Kite operator /( Kite self, int                              value ) => self.Divide(value);
    public static Kite operator +( Kite self, float                            value ) => self.Add(value);
    public static Kite operator -( Kite self, float                            value ) => self.Subtract(value);
    public static Kite operator *( Kite self, float                            value ) => self.Multiply(value);
    public static Kite operator /( Kite self, float                            value ) => self.Divide(value);
    public static Kite operator +( Kite self, double                           value ) => self.Add(value);
    public static Kite operator -( Kite self, double                           value ) => self.Subtract(value);
    public static Kite operator *( Kite self, double                           value ) => self.Multiply(value);
    public static Kite operator /( Kite self, double                           value ) => self.Divide(value);
}
