// Jakar.Extensions :: Jakar.Shapes
// 10/18/2025  00:34

using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using ZLinq;
using ZLinq.Linq;



namespace Jakar.Shapes;


[DefaultValue(nameof(Invalid))]
[method: JsonConstructor]
// The array parameter is required: this is the JsonConstructor, and System.Text.Json binds constructor
// arguments by reflection, which cannot pass a ref struct. Polygon also has to own an array, since a struct
// cannot hold a Span field. Prefer the ReadOnlySpan constructor and Create overload from calling code.
public readonly struct Polygon( params ReadOnlyPoint[]? points ) : ISpline<Polygon>
{
    // __empty must be declared first: static initialisers run in declaration order, and the three
    // sentinels below consume it. Declared after, it is still null when they are built, so their
    // Points field ends up null and every member that reads Points.Length throws.
    private static readonly ReadOnlyPoint[] __empty = [];
    public static readonly  Polygon         Invalid = new(null);
    public static readonly  Polygon         Zero    = new(ReadOnlyPoint.Zero);
    public static readonly  Polygon         One     = new(ReadOnlyPoint.One);
    public readonly         ReadOnlyPoint[] Points  = points ?? __empty;


    public ref readonly ReadOnlyPoint this[ int   index ] => ref Points[index];
    public ref readonly ReadOnlyPoint this[ Index index ] => ref Points[index];
    public Spline this[ Range                     index ] { [Pure] get => new(Points[index]); }
    static ref readonly Polygon IShape<Polygon>.                Zero    => ref Zero;
    static ref readonly Polygon IShape<Polygon>.                One     => ref One;
    static ref readonly Polygon IShape<Polygon>.                Invalid => ref Invalid;
    [JsonIgnore] public ReadOnlySpan<ReadOnlyPoint>             Span    => Points;
    public              int                                     Length  => Points?.Length ?? 0;
    ReadOnlySpan<ReadOnlyPoint> ISpline<Polygon, ReadOnlyPoint>.Points  => Points;
    public bool                                                 IsEmpty => Points is not { Length: > 1 };
    public bool IsNaN
    {
        get
        {
            ReadOnlySpan<ReadOnlyPoint> span = Span;
            return span.Any(static ( ref readonly ReadOnlyPoint x ) => x.IsNaN());
        }
    }
    public bool IsValid => !IsEmpty && !IsNaN;


    /// <summary>
    /// Copies <paramref name="points"/> into a new polygon, so callers can pass a stackalloc buffer.
    /// <para>
    /// The copy is unavoidable: a polygon owns its points for its whole lifetime, and a struct cannot hold a
    /// <see cref="Span{T}"/> field. Pooling the backing store is not an option either -- <c>ArrayPool.Rent</c> hands
    /// back an oversized array, which would corrupt <see cref="Length"/> and equality, and a struct has no
    /// deterministic point at which to return it. For transient work, prefer an API that takes the span directly,
    /// such as <c>LineOfBestFit.Fit(ReadOnlySpan&lt;ReadOnlyPoint&gt;)</c>, which allocates nothing.
    /// </para>
    /// </summary>
    public Polygon( params ReadOnlySpan<ReadOnlyPoint> points ) : this(points.IsEmpty
                                                                       ? __empty
                                                                       : [.. points]) { }
    public static implicit operator Polygon( ReadOnlyPoint[]?               points ) => Create(points);
    [Pure] public static            Polygon Create( params ReadOnlyPoint[]? points ) => new(points);

    /// <summary> Copies <paramref name="points"/>, so callers can pass a stackalloc buffer. </summary>
    [Pure] public static Polygon Create( params ReadOnlySpan<ReadOnlyPoint> points ) => new(points);
    [Pure] public Polygon Round() => new(AsValueEnumerable()
                                        .Select(static x => x.Round())
                                        .ToArray());
    [Pure] public Polygon Floor() => new(AsValueEnumerable()
                                        .Select(static x => x.Floor())
                                        .ToArray());


    [Pure] public ValueEnumerable<FromArray<ReadOnlyPoint>, ReadOnlyPoint> AsValueEnumerable() => new(new FromArray<ReadOnlyPoint>(Points));


    public static bool TryFromJson( string? json, out Polygon result )
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
    public static Polygon FromJson( string json ) => json.FromJson<Polygon>();


    public int CompareTo( object? other, IComparer comparer ) => other is Polygon spline
                                                                     ? CompareTo(spline)
                                                                     : throw new ExpectedValueTypeException(other, typeof(Polygon));
    public int CompareTo( Polygon other, IComparer<Polygon> comparer ) => comparer.Compare(this, other);
    public int CompareTo( Polygon other )
    {
        int lengthComparison = Length.CompareTo(other.Length);
        if ( lengthComparison != 0 ) { return lengthComparison; }

        for ( int i = 0; i < Length; i++ )
        {
            int pointComparison = Points[i]
               .CompareTo(other.Points[i]);

            if ( pointComparison != 0 ) { return pointComparison; }
        }

        return 0;
    }
    public int CompareTo( object? other )
    {
        if ( other is null ) { return 1; }

        return other is Polygon spline
                   ? CompareTo(spline)
                   : throw new ExpectedValueTypeException(other, typeof(Polygon));
    }
    public bool Equals( Polygon other )
    {
        if ( !Length.Equals(other.Length) ) { return false; }

        for ( int i = 0; i < Length; i++ )
        {
            if ( !Points[i]
                    .Equals(other.Points[i]) ) { return false; }
        }

        return true;
    }
    public override bool   Equals( object? other ) => other is Polygon x && Equals(x);
    public override int    GetHashCode()           => Points?.GetHashCode() ?? 0;
    public override string ToString()              => ToString(null, null);
    public string ToString( string? format, IFormatProvider? formatProvider )
    {
        switch ( format )
        {
            case "json":
            case "JSON":
            case "Json":
                return this.ToJson();

            case ",":
            {
                StringBuilder sb     = new();
                int           length = 0;

                foreach ( ref readonly ReadOnlyPoint point in Span )
                {
                    sb.Append(point.ToString(format, formatProvider));
                    if ( length++ < Length ) { sb.Append(','); }
                }

                return sb.ToString();
            }

            case "-":
            {
                StringBuilder sb     = new();
                int           length = 0;

                foreach ( ref readonly ReadOnlyPoint point in Span )
                {
                    sb.Append(point.ToString(format, formatProvider));
                    if ( length++ < Length ) { sb.Append('-'); }
                }

                return sb.ToString();
            }

            case EMPTY:
            case null:
            default:
                return $"{nameof(Polygon)}<{nameof(Length)}: {Length}>";
        }
    }


    public static bool operator ==( Polygon?  left, Polygon?                         right ) => Nullable.Equals(left, right);
    public static bool operator !=( Polygon?  left, Polygon?                         right ) => !Nullable.Equals(left, right);
    public static bool operator ==( Polygon   left, Polygon                          right ) => EqualityComparer<Polygon>.Default.Equals(left, right);
    public static bool operator !=( Polygon   left, Polygon                          right ) => !EqualityComparer<Polygon>.Default.Equals(left, right);
    public static bool operator >( Polygon    left, Polygon                          right ) => Comparer<Polygon>.Default.Compare(left, right) > 0;
    public static bool operator >=( Polygon   left, Polygon                          right ) => Comparer<Polygon>.Default.Compare(left, right) >= 0;
    public static bool operator <( Polygon    left, Polygon                          right ) => Comparer<Polygon>.Default.Compare(left, right) < 0;
    public static bool operator <=( Polygon   left, Polygon                          right ) => Comparer<Polygon>.Default.Compare(left, right) <= 0;
    public static Polygon operator +( Polygon self, int                              other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x + other);
    public static Polygon operator +( Polygon self, float                            other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x + other);
    public static Polygon operator +( Polygon self, double                           other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x + other);
    public static Polygon operator -( Polygon self, int                              other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x - other);
    public static Polygon operator -( Polygon self, float                            other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x - other);
    public static Polygon operator -( Polygon self, double                           other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x - other);
    public static Polygon operator *( Polygon self, int                              other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x * other);
    public static Polygon operator *( Polygon self, float                            other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x * other);
    public static Polygon operator *( Polygon self, double                           other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x * other);
    public static Polygon operator /( Polygon self, int                              other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x / other);
    public static Polygon operator /( Polygon self, float                            other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x / other);
    public static Polygon operator /( Polygon self, double                           other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x / other);
    public static Polygon operator +( Polygon self, (int xOffset, int yOffset)       other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x + other);
    public static Polygon operator +( Polygon self, (float xOffset, float yOffset)   other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x + other);
    public static Polygon operator +( Polygon self, (double xOffset, double yOffset) other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x + other);
    public static Polygon operator -( Polygon self, (int xOffset, int yOffset)       other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x - other);
    public static Polygon operator -( Polygon self, (float xOffset, float yOffset)   other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x - other);
    public static Polygon operator -( Polygon self, (double xOffset, double yOffset) other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x - other);
    public static Polygon operator *( Polygon self, (int xOffset, int yOffset)       other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x * other);
    public static Polygon operator *( Polygon self, (float xOffset, float yOffset)   other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x * other);
    public static Polygon operator *( Polygon self, (double xOffset, double yOffset) other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x * other);
    public static Polygon operator /( Polygon self, (int xOffset, int yOffset)       other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x / other);
    public static Polygon operator /( Polygon self, (float xOffset, float yOffset)   other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x / other);
    public static Polygon operator /( Polygon self, (double xOffset, double yOffset) other ) => self.Points.Create<ReadOnlyPoint>(( ref readonly ReadOnlyPoint x ) => x / other);
}
