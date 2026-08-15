// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

namespace Jakar.Shapes;


public static class RegularPolygons
{
    private const double TOLERANCE = 1e-9;

    /// <summary> Largest side count in the library. A <c> stackalloc </c> of this size fits any regular polygon here. </summary>
    public const int MAX_SIDES = 10;


    extension<TPolygon>( TPolygon self )
        where TPolygon : struct, IRegularPolygon<TPolygon>
    {
        public string ToString( string? format )
        {
            switch ( format )
            {
                case "json":
                case "JSON":
                case "Json":
                    return self.ToJson();

                case ",":
                    return $"{self.Center},{self.Circumradius}";

                case "-":
                    return $"{self.Center}-{self.Circumradius}";

                case EMPTY:
                case null:
                default:
                    return $"{typeof(TPolygon).Name}<{nameof(self.Center)}: {self.Center}, {nameof(self.Circumradius)}: {self.Circumradius}, Sides: {TPolygon.SideCount}>";
            }
        }

        public void Deconstruct( out ReadOnlyPoint center, out double circumradius )
        {
            center       = self.Center;
            circumradius = self.Circumradius;
        }
        public void Deconstruct( out ReadOnlyPoint center, out double circumradius, out Radians rotation )
        {
            center       = self.Center;
            circumradius = self.Circumradius;
            rotation     = self.Rotation;
        }


        // ----------------------------------------------------------------------------- buffer sizes

        /// <summary> Entries <see cref="Vertices"/>, <see cref="Edges"/>, <see cref="SideLengths"/> and <see cref="Angles"/> will write. </summary>
        public int VertexCount => TPolygon.SideCount;

        /// <summary> Entries <see cref="DiagonalLengths"/> will write: the distinct diagonals leaving one vertex. </summary>
        public int DiagonalCount => Math.Max(0, TPolygon.SideCount - 3);


        // ----------------------------------------------------------------------------- vertices & edges

        /// <summary>
        /// Writes the vertices counter-clockwise from <see cref="IRegularPolygon{TSelf}.Rotation"/> into
        /// <paramref name="destination"/> and returns the filled slice. The caller owns the buffer, so nothing is allocated:
        /// <code> Span&lt;ReadOnlyPoint&gt; buffer = stackalloc ReadOnlyPoint[shape.VertexCount]; </code>
        /// </summary>
        /// <exception cref="ArgumentException"> <paramref name="destination"/> holds fewer than <see cref="VertexCount"/> entries. </exception>
        public ReadOnlySpan<ReadOnlyPoint> Vertices( Span<ReadOnlyPoint> destination )
        {
            int sides = TPolygon.SideCount;
            if ( destination.Length < sides ) { throw new ArgumentException($"Need at least {sides} entries, got {destination.Length}.", nameof(destination)); }

            double step = 2 * Math.PI / sides;

            for ( int i = 0; i < sides; i++ )
            {
                double angle = self.Rotation.Value + ( i * step );
                destination[i] = new ReadOnlyPoint(self.Center.X + ( self.Circumradius * Math.Cos(angle) ), self.Center.Y + ( self.Circumradius * Math.Sin(angle) ));
            }

            return destination[..sides];
        }

        /// <summary> Writes the edges, each running from one vertex to the next, and returns the filled slice. </summary>
        /// <exception cref="ArgumentException"> <paramref name="destination"/> holds fewer than <see cref="VertexCount"/> entries. </exception>
        public ReadOnlySpan<ReadOnlyLine> Edges( Span<ReadOnlyLine> destination )
        {
            int sides = TPolygon.SideCount;
            if ( destination.Length < sides ) { throw new ArgumentException($"Need at least {sides} entries, got {destination.Length}.", nameof(destination)); }

            Span<ReadOnlyPoint>         corners = stackalloc ReadOnlyPoint[MAX_SIDES];
            ReadOnlySpan<ReadOnlyPoint> points  = self.Vertices(corners);

            for ( int i = 0; i < sides; i++ ) { destination[i] = new ReadOnlyLine(points[i], points[( i + 1 ) % sides]); }

            return destination[..sides];
        }

        /// <summary> The vertices as a <see cref="Polygon"/>. The one accessor that must allocate, because Polygon owns an array. </summary>
        public Polygon ToPolygon()
        {
            Span<ReadOnlyPoint> corners = stackalloc ReadOnlyPoint[MAX_SIDES];

            return new Polygon(self.Vertices(corners)
                                   .ToArray());
        }


        // ----------------------------------------------------------------------------- lengths

        /// <summary> Length of one side. All sides are equal. </summary>
        public double SideLength() => 2 * self.Circumradius * Math.Sin(Math.PI / TPolygon.SideCount);

        /// <summary> Fills <paramref name="destination"/> with every side length, for parity with the quadrilateral API. All entries are identical. </summary>
        /// <exception cref="ArgumentException"> <paramref name="destination"/> holds fewer than <see cref="VertexCount"/> entries. </exception>
        public ReadOnlySpan<double> SideLengths( Span<double> destination )
        {
            int sides = TPolygon.SideCount;
            if ( destination.Length < sides ) { throw new ArgumentException($"Need at least {sides} entries, got {destination.Length}.", nameof(destination)); }

            Span<double> slice = destination[..sides];
            slice.Fill(self.SideLength());
            return slice;
        }

        /// <summary> Distance from the centre to the midpoint of any side (the inradius). </summary>
        public double Apothem() => self.Circumradius * Math.Cos(Math.PI / TPolygon.SideCount);

        /// <summary> Writes the distinct diagonal lengths leaving one vertex and returns the filled slice. </summary>
        /// <exception cref="ArgumentException"> <paramref name="destination"/> holds fewer than <see cref="DiagonalCount"/> entries. </exception>
        public ReadOnlySpan<double> DiagonalLengths( Span<double> destination )
        {
            int sides = TPolygon.SideCount;
            int count = Math.Max(0, sides - 3);
            if ( count is 0 ) { return ReadOnlySpan<double>.Empty; }

            if ( destination.Length < count ) { throw new ArgumentException($"Need at least {count} entries, got {destination.Length}.", nameof(destination)); }

            for ( int k = 2; k <= sides - 2; k++ ) { destination[k - 2] = 2 * self.Circumradius * Math.Sin(Math.PI * k / sides); }

            return destination[..count];
        }


        // ----------------------------------------------------------------------------- angles

        /// <summary> Interior angle at every vertex: (n - 2) * 180 / n. </summary>
        public Degrees InteriorAngle() => new(( TPolygon.SideCount - 2 ) * 180.0 / TPolygon.SideCount);

        /// <summary> Exterior angle at every vertex: 360 / n. </summary>
        public Degrees ExteriorAngle() => new(360.0 / TPolygon.SideCount);

        /// <summary> Angle subtended at the centre by one side: 360 / n. </summary>
        public Degrees CentralAngle() => new(360.0 / TPolygon.SideCount);

        /// <summary> Fills <paramref name="destination"/> with every interior angle, for parity with the quadrilateral API. All entries are identical. </summary>
        /// <exception cref="ArgumentException"> <paramref name="destination"/> holds fewer than <see cref="VertexCount"/> entries. </exception>
        public ReadOnlySpan<Degrees> Angles( Span<Degrees> destination )
        {
            int sides = TPolygon.SideCount;
            if ( destination.Length < sides ) { throw new ArgumentException($"Need at least {sides} entries, got {destination.Length}.", nameof(destination)); }

            Span<Degrees> slice = destination[..sides];
            slice.Fill(self.InteriorAngle());
            return slice;
        }


        // ----------------------------------------------------------------------------- measurements

        /// <summary> Area as (1/2) * n * R^2 * sin(2*pi/n). </summary>
        public double Area() => 0.5 * TPolygon.SideCount * self.Circumradius * self.Circumradius * Math.Sin(2 * Math.PI / TPolygon.SideCount);

        public double Perimeter() => TPolygon.SideCount * self.SideLength();

        public ReadOnlyPoint Centroid() => self.Center;

        public ReadOnlyRectangle BoundingBox()
        {
            Span<ReadOnlyPoint>         corners = stackalloc ReadOnlyPoint[MAX_SIDES];
            ReadOnlySpan<ReadOnlyPoint> points  = self.Vertices(corners);

            double minX = double.PositiveInfinity, minY = double.PositiveInfinity, maxX = double.NegativeInfinity, maxY = double.NegativeInfinity;

            foreach ( ref readonly ReadOnlyPoint point in points )
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }

            return new ReadOnlyRectangle(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary> The largest circle that fits inside, touching the midpoint of every side. </summary>
        public Circle InscribedCircle() => new(self.Center, self.Apothem());

        /// <summary> The smallest circle containing every vertex. </summary>
        public Circle CircumscribedCircle() => new(self.Center, self.Circumradius);


        // ----------------------------------------------------------------------------- hit testing

        /// <summary> True when <paramref name="point"/> lies inside or on the boundary. </summary>
        public bool Contains( in ReadOnlyPoint point )
        {
            double dx       = point.X - self.Center.X;
            double dy       = point.Y - self.Center.Y;
            double distance = Math.Sqrt(( dx * dx ) + ( dy * dy ));

            // cheap rejects first: inside the inradius is always in, outside the circumradius is always out
            if ( distance <= self.Apothem() + TOLERANCE ) { return true; }

            if ( distance > self.Circumradius + TOLERANCE ) { return false; }

            Span<ReadOnlyPoint>         corners = stackalloc ReadOnlyPoint[MAX_SIDES];
            ReadOnlySpan<ReadOnlyPoint> points  = self.Vertices(corners);
            bool                        inside  = false;

            for ( int i = 0, j = points.Length - 1; i < points.Length; j = i++ )
            {
                ReadOnlyPoint p = points[i];
                ReadOnlyPoint q = points[j];
                if ( ( p.Y > point.Y ) != ( q.Y > point.Y ) && point.X < ( ( q.X - p.X ) * ( point.Y - p.Y ) / ( q.Y - p.Y ) ) + p.X ) { inside = !inside; }
            }

            return inside;
        }

        /// <summary> True when the two polygons overlap. </summary>
        public bool Intersects<TOther>( TOther other )
            where TOther : struct, IRegularPolygon<TOther>
        {
            if ( self.Center.DistanceTo(other.Center) > self.Circumradius + other.Circumradius + TOLERANCE ) { return false; }

            if ( self.Contains(other.Center) || other.Contains(self.Center) ) { return true; }

            Span<ReadOnlyPoint> theirs = stackalloc ReadOnlyPoint[MAX_SIDES];

            foreach ( ref readonly ReadOnlyPoint point in other.Vertices(theirs) )
            {
                if ( self.Contains(point) ) { return true; }
            }

            Span<ReadOnlyPoint> mine = stackalloc ReadOnlyPoint[MAX_SIDES];

            foreach ( ref readonly ReadOnlyPoint point in self.Vertices(mine) )
            {
                if ( other.Contains(point) ) { return true; }
            }

            return false;
        }

        /// <summary> True when the polygon overlaps <paramref name="circle"/>. </summary>
        public bool Intersects( in Circle circle ) => self.Center.DistanceTo(circle.Center) <= self.Circumradius + circle.Radius + TOLERANCE;


        // ----------------------------------------------------------------------------- transforms

        public TPolygon Scale( double factor )                      => TPolygon.Create(self.Center, self.Circumradius * factor, self.Rotation);
        public TPolygon Grow( double amount )                       => TPolygon.Create(self.Center, self.Circumradius + amount, self.Rotation);
        public TPolygon Rotate( Radians angle )                     => TPolygon.Create(self.Center, self.Circumradius, Radians.Normalize(self.Rotation.Value + angle.Value));
        public TPolygon Translate( double xOffset, double yOffset ) => TPolygon.Create(new ReadOnlyPoint(self.Center.X + xOffset, self.Center.Y + yOffset), self.Circumradius, self.Rotation);
        public TPolygon MoveTo( in ReadOnlyPoint center )           => TPolygon.Create(center, self.Circumradius, self.Rotation);
        public TPolygon Abs()                                       => TPolygon.Create(self.Center.Abs(),   Math.Abs(self.Circumradius),   self.Rotation);
        public TPolygon Round()                                     => TPolygon.Create(self.Center.Round(), Math.Round(self.Circumradius), self.Rotation);
        public TPolygon Floor()                                     => TPolygon.Create(self.Center.Floor(), Math.Floor(self.Circumradius), self.Rotation);


        // ----------------------------------------------------------------------------- predicates

        public bool IsFinite()   => self.Center.IsFinite() && double.IsFinite(self.Circumradius);
        public bool IsInfinity() => self.Center.IsInfinity() || double.IsInfinity(self.Circumradius);
        public bool IsInteger()  => self.Center.IsInteger() && double.IsInteger(self.Circumradius);
        public bool IsNaN()      => self.Center.IsNaN() || double.IsNaN(self.Circumradius);
        public bool IsNegative() => self.Circumradius < 0;
        public bool IsPositive() => self.Circumradius > 0;
        public bool IsZero()     => self.Center.IsZero() && self.Circumradius is 0;
        public bool IsValid()    => !self.IsNaN() && self.IsFinite() && self.Circumradius > 0;
    }
}
