// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;



namespace Jakar.Shapes;


/// <summary> Four vertices in order. Squareness is not an invariant of the type -- use IsSquare() to check. </summary>
[DefaultValue(nameof(Zero))]
[method: JsonConstructor]
public readonly struct Square( ReadOnlyPoint a, ReadOnlyPoint b, ReadOnlyPoint c, ReadOnlyPoint d ) : IQuadrilateral<Square>
{
    public static readonly Square    Invalid = new(ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid);
    public static readonly Square    Zero    = new(ReadOnlyPoint.Zero, ReadOnlyPoint.Zero, ReadOnlyPoint.Zero, ReadOnlyPoint.Zero);
    public static readonly Square    One     = new(ReadOnlyPoint.Zero, new ReadOnlyPoint(1, 0), ReadOnlyPoint.One, new ReadOnlyPoint(0, 1));
    public readonly        ReadOnlyPoint A = a;
    public readonly        ReadOnlyPoint B = b;
    public readonly        ReadOnlyPoint C = c;
    public readonly        ReadOnlyPoint D = d;


    static ref readonly Square IShape<Square>.Zero     => ref Zero;
    static ref readonly Square IShape<Square>.One      => ref One;
    static ref readonly Square IShape<Square>.Invalid  => ref Invalid;
    ReadOnlyPoint IShapeLocation.        Location => this.Centroid();
    double IShapeLocation.               X        => this.Centroid().X;
    double IShapeLocation.               Y        => this.Centroid().Y;
    bool IValidator.                     IsValid  => this.IsValid();
    ReadOnlyPoint IQuadrilateral<Square>.   A        => A;
    ReadOnlyPoint IQuadrilateral<Square>.   B        => B;
    ReadOnlyPoint IQuadrilateral<Square>.   C        => C;
    ReadOnlyPoint IQuadrilateral<Square>.   D        => D;
    public bool                          IsNaN    => A.IsNaN() || B.IsNaN() || C.IsNaN() || D.IsNaN();


    [Pure] public static Square Create( ReadOnlyPoint a, ReadOnlyPoint b, ReadOnlyPoint c, ReadOnlyPoint d ) => new(a, b, c, d);

    /// <summary> Axis-aligned square whose lower-left corner is <paramref name="origin"/>. </summary>
    [Pure] public static Square Create( in ReadOnlyPoint origin, double side ) =>
        new(origin, new ReadOnlyPoint(origin.X + side, origin.Y), new ReadOnlyPoint(origin.X + side, origin.Y + side), new ReadOnlyPoint(origin.X, origin.Y + side));


    public static bool TryFromJson( string? json, out Square result )
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
    public static Square FromJson( string json ) => json.FromJson<Square>();


    public int CompareTo( Square other )
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

        return obj is Square other
                   ? CompareTo(other)
                   : throw new ExpectedValueTypeException(obj, typeof(Square));
    }
    public          bool   Equals( Square     other )                                     => A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D);
    public override bool   Equals( object? other )                                     => other is Square x && Equals(x);
    public override int    GetHashCode()                                               => HashCode.Combine(A, B, C, D);
    public override string ToString()                                                  => ToString(null, null);
    public          string ToString( string? format, IFormatProvider? formatProvider ) => this.ToString(format);


    public static bool operator ==( Square?  left, Square?                        right ) => Nullable.Equals(left, right);
    public static bool operator !=( Square?  left, Square?                        right ) => !Nullable.Equals(left, right);
    public static bool operator ==( Square   left, Square                         right ) => EqualityComparer<Square>.Default.Equals(left, right);
    public static bool operator !=( Square   left, Square                         right ) => !EqualityComparer<Square>.Default.Equals(left, right);
    public static bool operator >( Square    left, Square                         right ) => Comparer<Square>.Default.Compare(left, right) > 0;
    public static bool operator >=( Square   left, Square                         right ) => Comparer<Square>.Default.Compare(left, right) >= 0;
    public static bool operator <( Square    left, Square                         right ) => Comparer<Square>.Default.Compare(left, right) < 0;
    public static bool operator <=( Square   left, Square                         right ) => Comparer<Square>.Default.Compare(left, right) <= 0;
    public static Square operator +( Square self, Square                             value ) => self.Add(value);
    public static Square operator -( Square self, Square                             value ) => self.Subtract(value);
    public static Square operator *( Square self, Square                             value ) => self.Multiply(value);
    public static Square operator /( Square self, Square                             value ) => self.Divide(value);
    public static Square operator +( Square self, (int xOffset, int yOffset)       value ) => self.Add(value);
    public static Square operator -( Square self, (int xOffset, int yOffset)       value ) => self.Subtract(value);
    public static Square operator *( Square self, (int xOffset, int yOffset)       value ) => self.Multiply(value);
    public static Square operator /( Square self, (int xOffset, int yOffset)       value ) => self.Divide(value);
    public static Square operator +( Square self, (float xOffset, float yOffset)   value ) => self.Add(value);
    public static Square operator -( Square self, (float xOffset, float yOffset)   value ) => self.Subtract(value);
    public static Square operator *( Square self, (float xOffset, float yOffset)   value ) => self.Multiply(value);
    public static Square operator /( Square self, (float xOffset, float yOffset)   value ) => self.Divide(value);
    public static Square operator +( Square self, (double xOffset, double yOffset) value ) => self.Add(value);
    public static Square operator -( Square self, (double xOffset, double yOffset) value ) => self.Subtract(value);
    public static Square operator *( Square self, (double xOffset, double yOffset) value ) => self.Multiply(value);
    public static Square operator /( Square self, (double xOffset, double yOffset) value ) => self.Divide(value);
    public static Square operator +( Square self, int                              value ) => self.Add(value);
    public static Square operator -( Square self, int                              value ) => self.Subtract(value);
    public static Square operator *( Square self, int                              value ) => self.Multiply(value);
    public static Square operator /( Square self, int                              value ) => self.Divide(value);
    public static Square operator +( Square self, float                            value ) => self.Add(value);
    public static Square operator -( Square self, float                            value ) => self.Subtract(value);
    public static Square operator *( Square self, float                            value ) => self.Multiply(value);
    public static Square operator /( Square self, float                            value ) => self.Divide(value);
    public static Square operator +( Square self, double                           value ) => self.Add(value);
    public static Square operator -( Square self, double                           value ) => self.Subtract(value);
    public static Square operator *( Square self, double                           value ) => self.Multiply(value);
    public static Square operator /( Square self, double                           value ) => self.Divide(value);
}
