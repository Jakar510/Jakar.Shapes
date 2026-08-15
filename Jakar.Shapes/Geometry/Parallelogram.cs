// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;



namespace Jakar.Shapes;


/// <summary> Four vertices in order. Use IsParallelogram() to verify opposite sides are parallel. </summary>
[DefaultValue(nameof(Zero))]
[method: JsonConstructor]
public readonly struct Parallelogram( ReadOnlyPoint a, ReadOnlyPoint b, ReadOnlyPoint c, ReadOnlyPoint d ) : IQuadrilateral<Parallelogram>
{
    public static readonly Parallelogram    Invalid = new(ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid);
    public static readonly Parallelogram    Zero    = new(ReadOnlyPoint.Zero, ReadOnlyPoint.Zero, ReadOnlyPoint.Zero, ReadOnlyPoint.Zero);
    public static readonly Parallelogram    One     = new(ReadOnlyPoint.Zero, new ReadOnlyPoint(1, 0), ReadOnlyPoint.One, new ReadOnlyPoint(0, 1));
    public readonly        ReadOnlyPoint A = a;
    public readonly        ReadOnlyPoint B = b;
    public readonly        ReadOnlyPoint C = c;
    public readonly        ReadOnlyPoint D = d;


    static ref readonly Parallelogram IShape<Parallelogram>.Zero     => ref Zero;
    static ref readonly Parallelogram IShape<Parallelogram>.One      => ref One;
    static ref readonly Parallelogram IShape<Parallelogram>.Invalid  => ref Invalid;
    ReadOnlyPoint IShapeLocation.        Location => this.Centroid();
    double IShapeLocation.               X        => this.Centroid().X;
    double IShapeLocation.               Y        => this.Centroid().Y;
    bool IValidator.                     IsValid  => this.IsValid();
    ReadOnlyPoint IQuadrilateral<Parallelogram>.   A        => A;
    ReadOnlyPoint IQuadrilateral<Parallelogram>.   B        => B;
    ReadOnlyPoint IQuadrilateral<Parallelogram>.   C        => C;
    ReadOnlyPoint IQuadrilateral<Parallelogram>.   D        => D;
    public bool                          IsNaN    => A.IsNaN() || B.IsNaN() || C.IsNaN() || D.IsNaN();


    [Pure] public static Parallelogram Create( ReadOnlyPoint a, ReadOnlyPoint b, ReadOnlyPoint c, ReadOnlyPoint d ) => new(a, b, c, d);

    /// <summary> Built from a corner and the two edge vectors leaving it. </summary>
    [Pure] public static Parallelogram Create( in ReadOnlyPoint origin, in ReadOnlyPoint edge1, in ReadOnlyPoint edge2 ) =>
        new(origin, origin + edge1, origin + edge1 + edge2, origin + edge2);


    public static bool TryFromJson( string? json, out Parallelogram result )
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
    public static Parallelogram FromJson( string json ) => json.FromJson<Parallelogram>();


    public int CompareTo( Parallelogram other )
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

        return obj is Parallelogram other
                   ? CompareTo(other)
                   : throw new ExpectedValueTypeException(obj, typeof(Parallelogram));
    }
    public          bool   Equals( Parallelogram     other )                                     => A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D);
    public override bool   Equals( object? other )                                     => other is Parallelogram x && Equals(x);
    public override int    GetHashCode()                                               => HashCode.Combine(A, B, C, D);
    public override string ToString()                                                  => ToString(null, null);
    public          string ToString( string? format, IFormatProvider? formatProvider ) => this.ToString(format);


    public static bool operator ==( Parallelogram?  left, Parallelogram?                        right ) => Nullable.Equals(left, right);
    public static bool operator !=( Parallelogram?  left, Parallelogram?                        right ) => !Nullable.Equals(left, right);
    public static bool operator ==( Parallelogram   left, Parallelogram                         right ) => EqualityComparer<Parallelogram>.Default.Equals(left, right);
    public static bool operator !=( Parallelogram   left, Parallelogram                         right ) => !EqualityComparer<Parallelogram>.Default.Equals(left, right);
    public static bool operator >( Parallelogram    left, Parallelogram                         right ) => Comparer<Parallelogram>.Default.Compare(left, right) > 0;
    public static bool operator >=( Parallelogram   left, Parallelogram                         right ) => Comparer<Parallelogram>.Default.Compare(left, right) >= 0;
    public static bool operator <( Parallelogram    left, Parallelogram                         right ) => Comparer<Parallelogram>.Default.Compare(left, right) < 0;
    public static bool operator <=( Parallelogram   left, Parallelogram                         right ) => Comparer<Parallelogram>.Default.Compare(left, right) <= 0;
    public static Parallelogram operator +( Parallelogram self, Parallelogram                             value ) => self.Add(value);
    public static Parallelogram operator -( Parallelogram self, Parallelogram                             value ) => self.Subtract(value);
    public static Parallelogram operator *( Parallelogram self, Parallelogram                             value ) => self.Multiply(value);
    public static Parallelogram operator /( Parallelogram self, Parallelogram                             value ) => self.Divide(value);
    public static Parallelogram operator +( Parallelogram self, (int xOffset, int yOffset)       value ) => self.Add(value);
    public static Parallelogram operator -( Parallelogram self, (int xOffset, int yOffset)       value ) => self.Subtract(value);
    public static Parallelogram operator *( Parallelogram self, (int xOffset, int yOffset)       value ) => self.Multiply(value);
    public static Parallelogram operator /( Parallelogram self, (int xOffset, int yOffset)       value ) => self.Divide(value);
    public static Parallelogram operator +( Parallelogram self, (float xOffset, float yOffset)   value ) => self.Add(value);
    public static Parallelogram operator -( Parallelogram self, (float xOffset, float yOffset)   value ) => self.Subtract(value);
    public static Parallelogram operator *( Parallelogram self, (float xOffset, float yOffset)   value ) => self.Multiply(value);
    public static Parallelogram operator /( Parallelogram self, (float xOffset, float yOffset)   value ) => self.Divide(value);
    public static Parallelogram operator +( Parallelogram self, (double xOffset, double yOffset) value ) => self.Add(value);
    public static Parallelogram operator -( Parallelogram self, (double xOffset, double yOffset) value ) => self.Subtract(value);
    public static Parallelogram operator *( Parallelogram self, (double xOffset, double yOffset) value ) => self.Multiply(value);
    public static Parallelogram operator /( Parallelogram self, (double xOffset, double yOffset) value ) => self.Divide(value);
    public static Parallelogram operator +( Parallelogram self, int                              value ) => self.Add(value);
    public static Parallelogram operator -( Parallelogram self, int                              value ) => self.Subtract(value);
    public static Parallelogram operator *( Parallelogram self, int                              value ) => self.Multiply(value);
    public static Parallelogram operator /( Parallelogram self, int                              value ) => self.Divide(value);
    public static Parallelogram operator +( Parallelogram self, float                            value ) => self.Add(value);
    public static Parallelogram operator -( Parallelogram self, float                            value ) => self.Subtract(value);
    public static Parallelogram operator *( Parallelogram self, float                            value ) => self.Multiply(value);
    public static Parallelogram operator /( Parallelogram self, float                            value ) => self.Divide(value);
    public static Parallelogram operator +( Parallelogram self, double                           value ) => self.Add(value);
    public static Parallelogram operator -( Parallelogram self, double                           value ) => self.Subtract(value);
    public static Parallelogram operator *( Parallelogram self, double                           value ) => self.Multiply(value);
    public static Parallelogram operator /( Parallelogram self, double                           value ) => self.Divide(value);
}
