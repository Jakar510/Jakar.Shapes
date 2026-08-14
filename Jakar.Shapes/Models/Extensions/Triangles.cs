// Jakar.Extensions :: Jakar.Shapes
// 09/29/2025  09:01

namespace Jakar.Shapes;


public static class Triangles
{
    extension<TTriangle>( TTriangle self )
        where TTriangle : struct, ITriangle<TTriangle>
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
                    return $"{self.X},{self.Y}";

                case "-":
                    return $"{self.X}-{self.Y}";

                case EMPTY:
                case null:
                default:
                    return $"{typeof(TTriangle).Name}<{nameof(self.A)}: {self.A}, {nameof(self.B)}: {self.B}, {nameof(self.C)}: {self.C}>";
            }
        }

        public void Deconstruct( out ReadOnlyPoint a, out ReadOnlyPoint b, out ReadOnlyPoint c )
        {
            a = self.A;
            b = self.B;
            c = self.C;
        }
        public void Deconstruct( out ReadOnlyPointF a, out ReadOnlyPointF b, out ReadOnlyPointF c )
        {
            a = self.A;
            b = self.B;
            c = self.C;
        }

        public ReadOnlyLine  Ab()         => new(self.A, self.B);
        public ReadOnlyLine  Bc()         => new(self.B, self.C);
        public ReadOnlyLine  Ca()         => new(self.C, self.A);
        public double        Area()       => Math.Abs((( self.B.X - self.A.X ) * ( self.C.Y - self.A.Y )) - (( self.C.X - self.A.X ) * ( self.B.Y - self.A.Y ))) / 2.0;
        public ReadOnlyPoint Centroid()   => new(( self.A.X + self.B.X + self.C.X ) / 3, ( self.A.Y + self.B.Y + self.C.Y ) / 3);
        public Degrees       Abc()        => new(self.A.AngleBetween(self.B, self.C));
        public Degrees       Bac()        => new(self.B.AngleBetween(self.A, self.C));
        public Degrees       Cab()        => new(self.C.AngleBetween(self.A, self.B));
        public TTriangle     Abs()        => TTriangle.Create(self.A.Abs(), self.B.Abs(), self.C.Abs());
        public bool          IsFinite()   => self.A.IsFinite() && self.B.IsFinite() && self.C.IsFinite();
        public bool          IsInfinity() => self.A.IsInfinity() || self.B.IsInfinity() || self.C.IsInfinity();
        public bool          IsInteger()  => self.A.IsInteger() && self.B.IsInteger() && self.C.IsInteger();
        public bool          IsNaN()      => self.A.IsNaN() || self.B.IsNaN() || self.C.IsNaN();
        public bool          IsNegative() => self.A.IsNegative() && self.B.IsNegative() && self.C.IsNegative();
        public bool          IsValid()    => !self.IsNaN()       && self.IsFinite()     && !self.A.IsOneOf(self.B, self.C) && !self.B.IsOneOf(self.A, self.C) && !self.C.IsOneOf(self.A, self.B);
        public bool          IsPositive() => self.A.IsPositive() && self.B.IsPositive() && self.C.IsPositive();
        public bool          IsZero()     => self.A.IsZero()     && self.B.IsZero()     && self.C.IsZero();

        public TTriangle Add<TOther>( TOther value )
            where TOther : struct, ITriangle<TOther> => TTriangle.Create(self.A + value.A, self.B + value.B, self.C + value.C);
        public TTriangle Subtract<TOther>( TOther value )
            where TOther : struct, ITriangle<TOther> => TTriangle.Create(self.A - value.A, self.B - value.B, self.C - value.C);
        public TTriangle Multiply<TOther>( TOther value )
            where TOther : struct, ITriangle<TOther> => TTriangle.Create(self.A * value.A, self.B * value.B, self.C * value.C);
        public TTriangle Divide<TOther>( TOther value )
            where TOther : struct, ITriangle<TOther> => TTriangle.Create(self.A / value.A, self.B / value.B, self.C / value.C);

        public TTriangle Add( (int xOffset, int yOffset)            value ) => TTriangle.Create(self.A + value, self.B + value, self.C + value);
        public TTriangle Subtract( (int xOffset, int yOffset)       value ) => TTriangle.Create(self.A - value, self.B - value, self.C - value);
        public TTriangle Divide( (int xOffset, int yOffset)         value ) => TTriangle.Create(self.A / value, self.B / value, self.C / value);
        public TTriangle Multiply( (int xOffset, int yOffset)       value ) => TTriangle.Create(self.A * value, self.B * value, self.C * value);
        public TTriangle Add( (float xOffset, float yOffset)        value ) => TTriangle.Create(self.A + value, self.B + value, self.C + value);
        public TTriangle Multiply( (float xOffset, float yOffset)   value ) => TTriangle.Create(self.A * value, self.B * value, self.C * value);
        public TTriangle Divide( (float xOffset, float yOffset)     value ) => TTriangle.Create(self.A / value, self.B / value, self.C / value);
        public TTriangle Subtract( (float xOffset, float yOffset)   value ) => TTriangle.Create(self.A - value, self.B - value, self.C - value);
        public TTriangle Add( (double xOffset, double yOffset)      value ) => TTriangle.Create(self.A + value, self.B + value, self.C + value);
        public TTriangle Subtract( (double xOffset, double yOffset) value ) => TTriangle.Create(self.A - value, self.B - value, self.C - value);
        public TTriangle Divide( (double xOffset, double yOffset)   value ) => TTriangle.Create(self.A / value, self.B / value, self.C / value);
        public TTriangle Multiply( (double xOffset, double yOffset) value ) => TTriangle.Create(self.A * value, self.B * value, self.C * value);

        public TTriangle Add( double      value ) => TTriangle.Create(self.A + value, self.B + value, self.C + value);
        public TTriangle Subtract( double value ) => TTriangle.Create(self.A - value, self.B - value, self.C - value);
        public TTriangle Multiply( double value ) => TTriangle.Create(self.A / value, self.B / value, self.C / value);
        public TTriangle Divide( double   value ) => TTriangle.Create(self.A / value, self.B / value, self.C / value);
        public TTriangle Add( float       value ) => TTriangle.Create(self.A + value, self.B + value, self.C + value);
        public TTriangle Subtract( float  value ) => TTriangle.Create(self.A - value, self.B - value, self.C - value);
        public TTriangle Divide( float    value ) => TTriangle.Create(self.A / value, self.B / value, self.C / value);
        public TTriangle Multiply( float  value ) => TTriangle.Create(self.A * value, self.B * value, self.C * value);
        public TTriangle Add( int         value ) => TTriangle.Create(self.A + value, self.B + value, self.C + value);
        public TTriangle Subtract( int    value ) => TTriangle.Create(self.A - value, self.B - value, self.C - value);
        public TTriangle Divide( int      value ) => TTriangle.Create(self.A / value, self.B / value, self.C / value);
        public TTriangle Multiply( int    value ) => TTriangle.Create(self.A * value, self.B * value, self.C * value);
    
        // ----------------------------------------------------------------------------- measurements

        /// <summary> Edge lengths in order: AB, BC, CA. </summary>
        public (double AB, double BC, double CA) SideLengths() => (self.A.DistanceTo(self.B), self.B.DistanceTo(self.C), self.C.DistanceTo(self.A));

        public double Perimeter()
        {
            (double ab, double bc, double ca) = self.SideLengths();
            return ab + bc + ca;
        }

        /// <summary> Interior angles at A, B and C. They sum to 180 degrees. </summary>
        public (Degrees A, Degrees B, Degrees C) Angles() => (self.Abc(), self.Bac(), self.Cab());

        public ReadOnlyRectangle BoundingBox()
        {
            double minX = Math.Min(self.A.X, Math.Min(self.B.X, self.C.X));
            double minY = Math.Min(self.A.Y, Math.Min(self.B.Y, self.C.Y));
            double maxX = Math.Max(self.A.X, Math.Max(self.B.X, self.C.X));
            double maxY = Math.Max(self.A.Y, Math.Max(self.B.Y, self.C.Y));
            return new ReadOnlyRectangle(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary> True when <paramref name="point"/> lies inside or on the boundary, tested by barycentric sign. </summary>
        public bool Contains( in ReadOnlyPoint point )
        {
            double d1 = Sign(point, self.A, self.B);
            double d2 = Sign(point, self.B, self.C);
            double d3 = Sign(point, self.C, self.A);
            bool   negative = d1 < -1e-9 || d2 < -1e-9 || d3 < -1e-9;
            bool   positive = d1 > 1e-9  || d2 > 1e-9  || d3 > 1e-9;
            return !( negative && positive );
        }

        public bool Intersects<TOther>( TOther other )
            where TOther : struct, ITriangle<TOther> => self.Contains(other.A)  || self.Contains(other.B)  || self.Contains(other.C) ||
                                                        other.Contains(self.A) || other.Contains(self.B) || other.Contains(self.C);

        /// <summary> The circle through all three vertices. </summary>
        public Circle CircumscribedCircle()
        {
            double ax = self.A.X, ay = self.A.Y, bx = self.B.X, by = self.B.Y, cx = self.C.X, cy = self.C.Y;
            double d  = 2 * ( ( ax * ( by - cy ) ) + ( bx * ( cy - ay ) ) + ( cx * ( ay - by ) ) );
            if ( Math.Abs(d) < 1e-12 ) { return Circle.Invalid; }

            double ux = ( ( ( ax * ax ) + ( ay * ay ) ) * ( by - cy ) ) + ( ( ( bx * bx ) + ( by * by ) ) * ( cy - ay ) ) + ( ( ( cx * cx ) + ( cy * cy ) ) * ( ay - by ) );
            double uy = ( ( ( ax * ax ) + ( ay * ay ) ) * ( cx - bx ) ) + ( ( ( bx * bx ) + ( by * by ) ) * ( ax - cx ) ) + ( ( ( cx * cx ) + ( cy * cy ) ) * ( bx - ax ) );
            ReadOnlyPoint center = new(ux / d, uy / d);
            return new Circle(center, center.DistanceTo(self.A));
        }

        /// <summary> The largest circle fitting inside the triangle. </summary>
        public Circle InscribedCircle()
        {
            (double ab, double bc, double ca) = self.SideLengths();
            double perimeter = ab + bc + ca;
            if ( perimeter <= 1e-12 ) { return Circle.Invalid; }

            ReadOnlyPoint center = new(( ( bc * self.A.X ) + ( ca * self.B.X ) + ( ab * self.C.X ) ) / perimeter, ( ( bc * self.A.Y ) + ( ca * self.B.Y ) + ( ab * self.C.Y ) ) / perimeter);
            return new Circle(center, 2 * self.Area() / perimeter);
        }


        // ----------------------------------------------------------------------------- transforms

        public TTriangle Scale( double factor )
        {
            ReadOnlyPoint c = self.Centroid();
            return TTriangle.Create(ScaleAbout(self.A, c, factor), ScaleAbout(self.B, c, factor), ScaleAbout(self.C, c, factor));
        }

        public TTriangle Translate( double xOffset, double yOffset ) =>
            TTriangle.Create(new ReadOnlyPoint(self.A.X + xOffset, self.A.Y + yOffset), new ReadOnlyPoint(self.B.X + xOffset, self.B.Y + yOffset), new ReadOnlyPoint(self.C.X + xOffset, self.C.Y + yOffset));

        public TTriangle Rotate( Radians angle ) => self.Rotate(angle, self.Centroid());

        public TTriangle Rotate( Radians angle, in ReadOnlyPoint origin )
        {
            double sin = Math.Sin(angle.Value);
            double cos = Math.Cos(angle.Value);
            return TTriangle.Create(RotateAbout(self.A, origin, sin, cos), RotateAbout(self.B, origin, sin, cos), RotateAbout(self.C, origin, sin, cos));
        }
}


    private static double Sign( in ReadOnlyPoint p, in ReadOnlyPoint a, in ReadOnlyPoint b ) => (( p.X - b.X ) * ( a.Y - b.Y )) - (( a.X - b.X ) * ( p.Y - b.Y ));

    private static ReadOnlyPoint ScaleAbout( in ReadOnlyPoint point, in ReadOnlyPoint origin, double factor ) =>
        new(origin.X + (( point.X - origin.X ) * factor), origin.Y + (( point.Y - origin.Y ) * factor));

    private static ReadOnlyPoint RotateAbout( in ReadOnlyPoint point, in ReadOnlyPoint origin, double sin, double cos )
    {
        double dx = point.X - origin.X;
        double dy = point.Y - origin.Y;
        return new ReadOnlyPoint(origin.X + ( dx * cos ) - ( dy * sin ), origin.Y + ( dx * sin ) + ( dy * cos ));
    }
}
