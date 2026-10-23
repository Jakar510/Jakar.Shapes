// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026



namespace Jakar.Shapes;


/// <summary> Four vertices in order. Use IsRhombus() to verify all four sides are equal. </summary>
[DefaultValue(nameof(Zero))]
[JsonModel]
public readonly partial struct Rhombus( ReadOnlyPoint a, ReadOnlyPoint b, ReadOnlyPoint c, ReadOnlyPoint d ) : IQuadrilateral<Rhombus>
{
    public static readonly Rhombus    Invalid = new(ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid, ReadOnlyPoint.Invalid);
    public static readonly Rhombus    Zero    = new(ReadOnlyPoint.Zero, ReadOnlyPoint.Zero, ReadOnlyPoint.Zero, ReadOnlyPoint.Zero);
    public static readonly Rhombus    One     = new(ReadOnlyPoint.Zero, new ReadOnlyPoint(1, 0), ReadOnlyPoint.One, new ReadOnlyPoint(0, 1));
    public readonly        ReadOnlyPoint A = a;
    public readonly        ReadOnlyPoint B = b;
    public readonly        ReadOnlyPoint C = c;
    public readonly        ReadOnlyPoint D = d;


    static ref readonly Rhombus IShape<Rhombus>.Zero     => ref Zero;
    static ref readonly Rhombus IShape<Rhombus>.One      => ref One;
    static ref readonly Rhombus IShape<Rhombus>.Invalid  => ref Invalid;
    ReadOnlyPoint IShapeLocation.        Location => this.Centroid();
    double IShapeLocation.               X        => this.Centroid().X;
    double IShapeLocation.               Y        => this.Centroid().Y;
    bool IValidator.                     IsValid  => this.IsValid();
    ReadOnlyPoint IQuadrilateral<Rhombus>.   A        => A;
    ReadOnlyPoint IQuadrilateral<Rhombus>.   B        => B;
    ReadOnlyPoint IQuadrilateral<Rhombus>.   C        => C;
    ReadOnlyPoint IQuadrilateral<Rhombus>.   D        => D;
    [JsonMember(Ignore = true)] public bool                          IsNaN    => A.IsNaN() || B.IsNaN() || C.IsNaN() || D.IsNaN();


    [Pure] public static Rhombus Create( ReadOnlyPoint a, ReadOnlyPoint b, ReadOnlyPoint c, ReadOnlyPoint d ) => new(a, b, c, d);

    /// <summary> Built from a centre and the two diagonal lengths, the first horizontal. </summary>
    [Pure] public static Rhombus Create( in ReadOnlyPoint center, double horizontalDiagonal, double verticalDiagonal )
    {
        double x = horizontalDiagonal / 2;
        double y = verticalDiagonal   / 2;
        return new Rhombus(new ReadOnlyPoint(center.X - x, center.Y), new ReadOnlyPoint(center.X, center.Y - y), new ReadOnlyPoint(center.X + x, center.Y), new ReadOnlyPoint(center.X, center.Y + y));
    }




    public int CompareTo( Rhombus other )
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

        return obj is Rhombus other
                   ? CompareTo(other)
                   : throw new ExpectedValueTypeException(obj, typeof(Rhombus));
    }
    public          bool   Equals( Rhombus     other )                                     => A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D);
    public override bool   Equals( object? other )                                     => other is Rhombus x && Equals(x);
    public override int    GetHashCode()                                               => HashCode.Combine(A, B, C, D);
    public override string ToString()                                                  => ToString(null, null);
    public          string ToString( string? format, IFormatProvider? formatProvider ) => this.ToString(format);


    public static bool operator ==( Rhombus?  left, Rhombus?                        right ) => Nullable.Equals(left, right);
    public static bool operator !=( Rhombus?  left, Rhombus?                        right ) => !Nullable.Equals(left, right);
    public static bool operator ==( Rhombus   left, Rhombus                         right ) => EqualityComparer<Rhombus>.Default.Equals(left, right);
    public static bool operator !=( Rhombus   left, Rhombus                         right ) => !EqualityComparer<Rhombus>.Default.Equals(left, right);
    public static bool operator >( Rhombus    left, Rhombus                         right ) => Comparer<Rhombus>.Default.Compare(left, right) > 0;
    public static bool operator >=( Rhombus   left, Rhombus                         right ) => Comparer<Rhombus>.Default.Compare(left, right) >= 0;
    public static bool operator <( Rhombus    left, Rhombus                         right ) => Comparer<Rhombus>.Default.Compare(left, right) < 0;
    public static bool operator <=( Rhombus   left, Rhombus                         right ) => Comparer<Rhombus>.Default.Compare(left, right) <= 0;
    public static Rhombus operator +( Rhombus self, Rhombus                             value ) => self.Add(value);
    public static Rhombus operator -( Rhombus self, Rhombus                             value ) => self.Subtract(value);
    public static Rhombus operator *( Rhombus self, Rhombus                             value ) => self.Multiply(value);
    public static Rhombus operator /( Rhombus self, Rhombus                             value ) => self.Divide(value);
    public static Rhombus operator +( Rhombus self, (int xOffset, int yOffset)       value ) => self.Add(value);
    public static Rhombus operator -( Rhombus self, (int xOffset, int yOffset)       value ) => self.Subtract(value);
    public static Rhombus operator *( Rhombus self, (int xOffset, int yOffset)       value ) => self.Multiply(value);
    public static Rhombus operator /( Rhombus self, (int xOffset, int yOffset)       value ) => self.Divide(value);
    public static Rhombus operator +( Rhombus self, (float xOffset, float yOffset)   value ) => self.Add(value);
    public static Rhombus operator -( Rhombus self, (float xOffset, float yOffset)   value ) => self.Subtract(value);
    public static Rhombus operator *( Rhombus self, (float xOffset, float yOffset)   value ) => self.Multiply(value);
    public static Rhombus operator /( Rhombus self, (float xOffset, float yOffset)   value ) => self.Divide(value);
    public static Rhombus operator +( Rhombus self, (double xOffset, double yOffset) value ) => self.Add(value);
    public static Rhombus operator -( Rhombus self, (double xOffset, double yOffset) value ) => self.Subtract(value);
    public static Rhombus operator *( Rhombus self, (double xOffset, double yOffset) value ) => self.Multiply(value);
    public static Rhombus operator /( Rhombus self, (double xOffset, double yOffset) value ) => self.Divide(value);
    public static Rhombus operator +( Rhombus self, int                              value ) => self.Add(value);
    public static Rhombus operator -( Rhombus self, int                              value ) => self.Subtract(value);
    public static Rhombus operator *( Rhombus self, int                              value ) => self.Multiply(value);
    public static Rhombus operator /( Rhombus self, int                              value ) => self.Divide(value);
    public static Rhombus operator +( Rhombus self, float                            value ) => self.Add(value);
    public static Rhombus operator -( Rhombus self, float                            value ) => self.Subtract(value);
    public static Rhombus operator *( Rhombus self, float                            value ) => self.Multiply(value);
    public static Rhombus operator /( Rhombus self, float                            value ) => self.Divide(value);
    public static Rhombus operator +( Rhombus self, double                           value ) => self.Add(value);
    public static Rhombus operator -( Rhombus self, double                           value ) => self.Subtract(value);
    public static Rhombus operator *( Rhombus self, double                           value ) => self.Multiply(value);
    public static Rhombus operator /( Rhombus self, double                           value ) => self.Divide(value);
}
