// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

namespace Jakar.Shapes;


public static class RegularPolygons
{
    private const double TOLERANCE = 1e-9;


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


        // ----------------------------------------------------------------------------- vertices & edges

        /// <summary> The vertices in counter-clockwise order, starting from <see cref="IRegularPolygon{TSelf}.Rotation"/>. </summary>
        public ReadOnlyPoint[] Vertices()
        {
            int             sides  = TPolygon.SideCount;
            ReadOnlyPoint[] buffer = GC.AllocateUninitializedArray<ReadOnlyPoint>(sides);
            double          step   = 2 * Math.PI / sides;

            for ( int i = 0; i < sides; i++ )
            {
                double angle = self.Rotation.Value + ( i * step );
                buffer[i] = new ReadOnlyPoint(self.Center.X + ( self.Circumradius * Math.Cos(angle) ), self.Center.Y + ( self.Circumradius * Math.Sin(angle) ));
            }

            return buffer;
        }

        /// <summary> The vertices as a closed <see cref="Polygon"/>. </summary>
        public Polygon ToPolygon() => new(self.Vertices());

        /// <summary> The edges in order, each running from one vertex to the next. </summary>
        public ReadOnlyLine[] Edges()
        {
            ReadOnlyPoint[] points = self.Vertices();
            ReadOnlyLine[]  buffer = GC.AllocateUninitializedArray<ReadOnlyLine>(points.Length);
            for ( int i = 0; i < points.Length; i++ ) { buffer[i] = new ReadOnlyLine(points[i], points[( i + 1 ) % points.Length]); }

            return buffer;
        }

        /// <summary> Length of one side. All sides are equal. </summary>
        public double SideLength() => 2 * self.Circumradius * Math.Sin(Math.PI / TPolygon.SideCount);

        /// <summary> Every side length, for parity with the quadrilateral API. All entries are identical. </summary>
        public double[] SideLengths()
        {
            double   length = self.SideLength();
            double[] buffer = GC.AllocateUninitializedArray<double>(TPolygon.SideCount);
            Array.Fill(buffer, length);
            return buffer;
        }

        /// <summary> Distance from the centre to the midpoint of any side (the inradius). </summary>
        public double Apothem() => self.Circumradius * Math.Cos(Math.PI / TPolygon.SideCount);

        /// <summary> Diagonals from the first vertex to every non-adjacent vertex. </summary>
        public double[] DiagonalLengths()
        {
            int sides = TPolygon.SideCount;
            int count = sides - 3;
            if ( count <= 0 ) { return []; }

            double[] buffer = GC.AllocateUninitializedArray<double>(count);
            for ( int k = 2; k <= sides - 2; k++ ) { buffer[k - 2] = 2 * self.Circumradius * Math.Sin(Math.PI * k / sides); }

            return buffer;
        }


        // ----------------------------------------------------------------------------- angles

        /// <summary> Interior angle at every vertex: (n - 2) * 180 / n. </summary>
        public Degrees InteriorAngle() => new(( TPolygon.SideCount - 2 ) * 180.0 / TPolygon.SideCount);

        /// <summary> Exterior angle at every vertex: 360 / n. </summary>
        public Degrees ExteriorAngle() => new(360.0 / TPolygon.SideCount);

        /// <summary> Angle subtended at the centre by one side: 360 / n. </summary>
        public Degrees CentralAngle() => new(360.0 / TPolygon.SideCount);

        /// <summary> Every interior angle, for parity with the quadrilateral API. All entries are identical. </summary>
        public Degrees[] Angles()
        {
            Degrees   angle  = self.InteriorAngle();
            Degrees[] buffer = GC.AllocateUninitializedArray<Degrees>(TPolygon.SideCount);
            Array.Fill(buffer, angle);
            return buffer;
        }


        // ----------------------------------------------------------------------------- measurements

        /// <summary> Area as (1/2) * n * R^2 * sin(2*pi/n). </summary>
        public double Area() => 0.5 * TPolygon.SideCount * self.Circumradius * self.Circumradius * Math.Sin(2 * Math.PI / TPolygon.SideCount);

        public double Perimeter() => TPolygon.SideCount * self.SideLength();

        public ReadOnlyPoint Centroid() => self.Center;

        public ReadOnlyRectangle BoundingBox()
        {
            ReadOnlyPoint[] points = self.Vertices();
            double          minX = double.PositiveInfinity, minY = double.PositiveInfinity, maxX = double.NegativeInfinity, maxY = double.NegativeInfinity;

            foreach ( ReadOnlyPoint point in points )
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

            ReadOnlyPoint[] points = self.Vertices();
            bool            inside = false;

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
            where TOther : struct, IRegularPolygon<TOther> => self.Center.DistanceTo(other.Center) <= self.Circumradius + other.Circumradius + TOLERANCE &&
                                                             ( self.Contains(other.Center) || other.Contains(self.Center) || AnyVertexInside(self, other) );

        /// <summary> True when the polygon overlaps <paramref name="circle"/>. </summary>
        public bool Intersects( in Circle circle ) => self.Center.DistanceTo(circle.Center) <= self.Circumradius + circle.Radius + TOLERANCE;


        // ----------------------------------------------------------------------------- transforms

        public TPolygon Scale( double factor )                        => TPolygon.Create(self.Center, self.Circumradius * factor, self.Rotation);
        public TPolygon Grow( double amount )                         => TPolygon.Create(self.Center, self.Circumradius + amount, self.Rotation);
        public TPolygon Rotate( Radians angle )                       => TPolygon.Create(self.Center, self.Circumradius, Radians.Normalize(self.Rotation.Value + angle.Value));
        public TPolygon Translate( double xOffset, double yOffset )   => TPolygon.Create(new ReadOnlyPoint(self.Center.X + xOffset, self.Center.Y + yOffset), self.Circumradius, self.Rotation);
        public TPolygon MoveTo( in ReadOnlyPoint center )             => TPolygon.Create(center, self.Circumradius, self.Rotation);
        public TPolygon Abs()                                         => TPolygon.Create(self.Center.Abs(),   Math.Abs(self.Circumradius),         self.Rotation);
        public TPolygon Round()                                       => TPolygon.Create(self.Center.Round(), Math.Round(self.Circumradius),       self.Rotation);
        public TPolygon Floor()                                       => TPolygon.Create(self.Center.Floor(), Math.Floor(self.Circumradius),       self.Rotation);


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


    private static bool AnyVertexInside<TSelf, TOther>( TSelf self, TOther other )
        where TSelf : struct, IRegularPolygon<TSelf>
        where TOther : struct, IRegularPolygon<TOther>
    {
        foreach ( ReadOnlyPoint point in other.Vertices() )
        {
            if ( self.Contains(point) ) { return true; }
        }

        foreach ( ReadOnlyPoint point in self.Vertices() )
        {
            if ( other.Contains(point) ) { return true; }
        }

        return false;
    }
}
