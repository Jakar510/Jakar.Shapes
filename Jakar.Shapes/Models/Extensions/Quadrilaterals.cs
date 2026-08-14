// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

namespace Jakar.Shapes;


public static class Quadrilaterals
{
    private const double TOLERANCE = 1e-9;


    extension<TQuad>( TQuad self )
        where TQuad : struct, IQuadrilateral<TQuad>
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
                    return $"{self.A},{self.B},{self.C},{self.D}";

                case "-":
                    return $"{self.A}-{self.B}-{self.C}-{self.D}";

                case EMPTY:
                case null:
                default:
                    return $"{typeof(TQuad).Name}<{nameof(self.A)}: {self.A}, {nameof(self.B)}: {self.B}, {nameof(self.C)}: {self.C}, {nameof(self.D)}: {self.D}>";
            }
        }

        public void Deconstruct( out ReadOnlyPoint a, out ReadOnlyPoint b, out ReadOnlyPoint c, out ReadOnlyPoint d )
        {
            a = self.A;
            b = self.B;
            c = self.C;
            d = self.D;
        }


        // ----------------------------------------------------------------------------- edges & diagonals

        public ReadOnlyLine Ab() => new(self.A, self.B);
        public ReadOnlyLine Bc() => new(self.B, self.C);
        public ReadOnlyLine Cd() => new(self.C, self.D);
        public ReadOnlyLine Da() => new(self.D, self.A);

        /// <summary> Diagonal from A to C. </summary>
        public ReadOnlyLine Ac() => new(self.A, self.C);

        /// <summary> Diagonal from B to D. </summary>
        public ReadOnlyLine Bd() => new(self.B, self.D);

        /// <summary> Edge lengths in order: AB, BC, CD, DA. </summary>
        public (double AB, double BC, double CD, double DA) SideLengths() => (self.A.DistanceTo(self.B), self.B.DistanceTo(self.C), self.C.DistanceTo(self.D), self.D.DistanceTo(self.A));

        /// <summary> Diagonal lengths: AC then BD. </summary>
        public (double AC, double BD) DiagonalLengths() => (self.A.DistanceTo(self.C), self.B.DistanceTo(self.D));

        /// <summary> Interior angles at A, B, C and D. They sum to 360 degrees for any simple quadrilateral. </summary>
        public (Degrees A, Degrees B, Degrees C, Degrees D) Angles() =>
            (Radians.Normalize(self.A.AngleBetween(self.D, self.B)), Radians.Normalize(self.B.AngleBetween(self.A, self.C)), Radians.Normalize(self.C.AngleBetween(self.B, self.D)),
             Radians.Normalize(self.D.AngleBetween(self.C, self.A)));


        // ----------------------------------------------------------------------------- measurements

        /// <summary> Area by the shoelace formula. Correct for any simple (non self-intersecting) quadrilateral. </summary>
        public double Area() =>
            Math.Abs((( self.A.X * self.B.Y ) - ( self.B.X * self.A.Y )) + (( self.B.X * self.C.Y ) - ( self.C.X * self.B.Y )) + (( self.C.X * self.D.Y ) - ( self.D.X * self.C.Y )) +
                     (( self.D.X * self.A.Y ) - ( self.A.X * self.D.Y ))) / 2.0;

        public double Perimeter()
        {
            (double ab, double bc, double cd, double da) = self.SideLengths();
            return ab + bc + cd + da;
        }

        /// <summary> Centroid of the vertices (the vertex average, not the area centroid). </summary>
        public ReadOnlyPoint Centroid() => new(( self.A.X + self.B.X + self.C.X + self.D.X ) / 4, ( self.A.Y + self.B.Y + self.C.Y + self.D.Y ) / 4);

        public ReadOnlyRectangle BoundingBox()
        {
            double minX = Math.Min(Math.Min(self.A.X, self.B.X), Math.Min(self.C.X, self.D.X));
            double minY = Math.Min(Math.Min(self.A.Y, self.B.Y), Math.Min(self.C.Y, self.D.Y));
            double maxX = Math.Max(Math.Max(self.A.X, self.B.X), Math.Max(self.C.X, self.D.X));
            double maxY = Math.Max(Math.Max(self.A.Y, self.B.Y), Math.Max(self.C.Y, self.D.Y));
            return new ReadOnlyRectangle(minX, minY, maxX - minX, maxY - minY);
        }


        // ----------------------------------------------------------------------------- classification

        /// <summary> True when the quadrilateral is convex, i.e. every turn goes the same way. </summary>
        public bool IsConvex()
        {
            double c1 = Cross(self.A, self.B, self.C);
            double c2 = Cross(self.B, self.C, self.D);
            double c3 = Cross(self.C, self.D, self.A);
            double c4 = Cross(self.D, self.A, self.B);
            return ( c1 >= -TOLERANCE && c2 >= -TOLERANCE && c3 >= -TOLERANCE && c4 >= -TOLERANCE ) || ( c1 <= TOLERANCE && c2 <= TOLERANCE && c3 <= TOLERANCE && c4 <= TOLERANCE );
        }

        public bool IsParallelogram()
        {
            (double ab, double bc, double cd, double da) = self.SideLengths();
            return NearlyEqual(ab, cd) && NearlyEqual(bc, da) && self.IsConvex();
        }

        public bool IsRhombus()
        {
            (double ab, double bc, double cd, double da) = self.SideLengths();
            return NearlyEqual(ab, bc) && NearlyEqual(bc, cd) && NearlyEqual(cd, da) && self.IsConvex();
        }

        public bool IsRectangle()
        {
            (double ac, double bd) = self.DiagonalLengths();
            return self.IsParallelogram() && NearlyEqual(ac, bd);
        }

        public bool IsSquare() => self.IsRhombus() && self.IsRectangle();

        /// <summary> True when at least one pair of opposite sides is parallel (the inclusive definition). </summary>
        public bool IsTrapezoid() => IsParallel(self.A, self.B, self.D, self.C) || IsParallel(self.B, self.C, self.A, self.D);

        /// <summary> True when two disjoint pairs of ADJACENT sides are equal. </summary>
        public bool IsKite()
        {
            (double ab, double bc, double cd, double da) = self.SideLengths();
            return ( NearlyEqual(ab, bc) && NearlyEqual(cd, da) && !NearlyEqual(ab, cd) ) || ( NearlyEqual(bc, cd) && NearlyEqual(da, ab) && !NearlyEqual(bc, da) );
        }


        // ----------------------------------------------------------------------------- hit testing

        /// <summary> True when <paramref name="point"/> lies inside or on the boundary. Uses winding, so it handles concave shapes. </summary>
        public bool Contains( in ReadOnlyPoint point )
        {
            bool inside = false;
            Span<ReadOnlyPoint> corners = [self.A, self.B, self.C, self.D];

            for ( int i = 0, j = 3; i < 4; j = i++ )
            {
                ReadOnlyPoint p = corners[i];
                ReadOnlyPoint q = corners[j];
                if ( OnSegment(p, q, point) ) { return true; }

                if ( ( p.Y > point.Y ) != ( q.Y > point.Y ) && point.X < ( ( q.X - p.X ) * ( point.Y - p.Y ) / ( q.Y - p.Y ) ) + p.X ) { inside = !inside; }
            }

            return inside;
        }

        /// <summary> True when the two shapes overlap, sharing at least one point. </summary>
        public bool Intersects<TOther>( TOther other )
            where TOther : struct, IQuadrilateral<TOther>
        {
            Span<ReadOnlyPoint> mine   = [self.A, self.B, self.C, self.D];
            Span<ReadOnlyPoint> theirs = [other.A, other.B, other.C, other.D];

            foreach ( ReadOnlyPoint point in theirs )
            {
                if ( self.Contains(point) ) { return true; }
            }

            foreach ( ReadOnlyPoint point in mine )
            {
                if ( other.Contains(point) ) { return true; }
            }

            for ( int i = 0; i < 4; i++ )
            {
                for ( int j = 0; j < 4; j++ )
                {
                    if ( SegmentsCross(mine[i], mine[( i + 1 ) % 4], theirs[j], theirs[( j + 1 ) % 4]) ) { return true; }
                }
            }

            return false;
        }


        // ----------------------------------------------------------------------------- transforms

        /// <summary> Scales about the centroid. </summary>
        public TQuad Scale( double factor )
        {
            ReadOnlyPoint c = self.Centroid();
            return TQuad.Create(ScalePoint(self.A, c, factor), ScalePoint(self.B, c, factor), ScalePoint(self.C, c, factor), ScalePoint(self.D, c, factor));
        }

        /// <summary> Grows every vertex outward from the centroid by an absolute distance. </summary>
        public TQuad Grow( double amount )
        {
            ReadOnlyPoint c = self.Centroid();
            return TQuad.Create(GrowPoint(self.A, c, amount), GrowPoint(self.B, c, amount), GrowPoint(self.C, c, amount), GrowPoint(self.D, c, amount));
        }

        public TQuad Translate( double xOffset, double yOffset ) =>
            TQuad.Create(new ReadOnlyPoint(self.A.X + xOffset, self.A.Y + yOffset), new ReadOnlyPoint(self.B.X + xOffset, self.B.Y + yOffset),
                         new ReadOnlyPoint(self.C.X + xOffset, self.C.Y + yOffset), new ReadOnlyPoint(self.D.X + xOffset, self.D.Y + yOffset));

        /// <summary> Rotates about the centroid. </summary>
        public TQuad Rotate( Radians angle ) => self.Rotate(angle, self.Centroid());

        /// <summary> Rotates about an arbitrary origin. </summary>
        public TQuad Rotate( Radians angle, in ReadOnlyPoint origin )
        {
            double sin = Math.Sin(angle.Value);
            double cos = Math.Cos(angle.Value);
            return TQuad.Create(RotatePoint(self.A, origin, sin, cos), RotatePoint(self.B, origin, sin, cos), RotatePoint(self.C, origin, sin, cos), RotatePoint(self.D, origin, sin, cos));
        }

        public TQuad Abs()   => TQuad.Create(self.A.Abs(),   self.B.Abs(),   self.C.Abs(),   self.D.Abs());
        public TQuad Round() => TQuad.Create(self.A.Round(), self.B.Round(), self.C.Round(), self.D.Round());
        public TQuad Floor() => TQuad.Create(self.A.Floor(), self.B.Floor(), self.C.Floor(), self.D.Floor());


        // ----------------------------------------------------------------------------- predicates

        public bool IsFinite()   => self.A.IsFinite()   && self.B.IsFinite()   && self.C.IsFinite()   && self.D.IsFinite();
        public bool IsInfinity() => self.A.IsInfinity() || self.B.IsInfinity() || self.C.IsInfinity() || self.D.IsInfinity();
        public bool IsInteger()  => self.A.IsInteger()  && self.B.IsInteger()  && self.C.IsInteger()  && self.D.IsInteger();
        public bool IsNaN()      => self.A.IsNaN()      || self.B.IsNaN()      || self.C.IsNaN()      || self.D.IsNaN();
        public bool IsNegative() => self.A.IsNegative() && self.B.IsNegative() && self.C.IsNegative() && self.D.IsNegative();
        public bool IsPositive() => self.A.IsPositive() && self.B.IsPositive() && self.C.IsPositive() && self.D.IsPositive();
        public bool IsZero()     => self.A.IsZero()     && self.B.IsZero()     && self.C.IsZero()     && self.D.IsZero();
        public bool IsValid()    => !self.IsNaN() && self.IsFinite() && self.Area() > TOLERANCE;


        // ----------------------------------------------------------------------------- arithmetic

        public TQuad Add<TOther>( TOther value )
            where TOther : struct, IQuadrilateral<TOther> => TQuad.Create(self.A + value.A, self.B + value.B, self.C + value.C, self.D + value.D);
        public TQuad Subtract<TOther>( TOther value )
            where TOther : struct, IQuadrilateral<TOther> => TQuad.Create(self.A - value.A, self.B - value.B, self.C - value.C, self.D - value.D);
        public TQuad Multiply<TOther>( TOther value )
            where TOther : struct, IQuadrilateral<TOther> => TQuad.Create(self.A * value.A, self.B * value.B, self.C * value.C, self.D * value.D);
        public TQuad Divide<TOther>( TOther value )
            where TOther : struct, IQuadrilateral<TOther> => TQuad.Create(self.A / value.A, self.B / value.B, self.C / value.C, self.D / value.D);

        public TQuad Add( (int xOffset, int yOffset)            value ) => TQuad.Create(self.A + value, self.B + value, self.C + value, self.D + value);
        public TQuad Subtract( (int xOffset, int yOffset)       value ) => TQuad.Create(self.A - value, self.B - value, self.C - value, self.D - value);
        public TQuad Multiply( (int xOffset, int yOffset)       value ) => TQuad.Create(self.A * value, self.B * value, self.C * value, self.D * value);
        public TQuad Divide( (int xOffset, int yOffset)         value ) => TQuad.Create(self.A / value, self.B / value, self.C / value, self.D / value);
        public TQuad Add( (float xOffset, float yOffset)        value ) => TQuad.Create(self.A + value, self.B + value, self.C + value, self.D + value);
        public TQuad Subtract( (float xOffset, float yOffset)   value ) => TQuad.Create(self.A - value, self.B - value, self.C - value, self.D - value);
        public TQuad Multiply( (float xOffset, float yOffset)   value ) => TQuad.Create(self.A * value, self.B * value, self.C * value, self.D * value);
        public TQuad Divide( (float xOffset, float yOffset)     value ) => TQuad.Create(self.A / value, self.B / value, self.C / value, self.D / value);
        public TQuad Add( (double xOffset, double yOffset)      value ) => TQuad.Create(self.A + value, self.B + value, self.C + value, self.D + value);
        public TQuad Subtract( (double xOffset, double yOffset) value ) => TQuad.Create(self.A - value, self.B - value, self.C - value, self.D - value);
        public TQuad Multiply( (double xOffset, double yOffset) value ) => TQuad.Create(self.A * value, self.B * value, self.C * value, self.D * value);
        public TQuad Divide( (double xOffset, double yOffset)   value ) => TQuad.Create(self.A / value, self.B / value, self.C / value, self.D / value);

        public TQuad Add( double      value ) => TQuad.Create(self.A + value, self.B + value, self.C + value, self.D + value);
        public TQuad Subtract( double value ) => TQuad.Create(self.A - value, self.B - value, self.C - value, self.D - value);
        public TQuad Multiply( double value ) => TQuad.Create(self.A * value, self.B * value, self.C * value, self.D * value);
        public TQuad Divide( double   value ) => TQuad.Create(self.A / value, self.B / value, self.C / value, self.D / value);
        public TQuad Add( float       value ) => TQuad.Create(self.A + value, self.B + value, self.C + value, self.D + value);
        public TQuad Subtract( float  value ) => TQuad.Create(self.A - value, self.B - value, self.C - value, self.D - value);
        public TQuad Multiply( float  value ) => TQuad.Create(self.A * value, self.B * value, self.C * value, self.D * value);
        public TQuad Divide( float    value ) => TQuad.Create(self.A / value, self.B / value, self.C / value, self.D / value);
        public TQuad Add( int         value ) => TQuad.Create(self.A + value, self.B + value, self.C + value, self.D + value);
        public TQuad Subtract( int    value ) => TQuad.Create(self.A - value, self.B - value, self.C - value, self.D - value);
        public TQuad Multiply( int    value ) => TQuad.Create(self.A * value, self.B * value, self.C * value, self.D * value);
        public TQuad Divide( int      value ) => TQuad.Create(self.A / value, self.B / value, self.C / value, self.D / value);
    }


    // --------------------------------------------------------------------------------- shared helpers

    private static bool   NearlyEqual( double left, double right ) => Math.Abs(left - right) <= TOLERANCE * Math.Max(1.0, Math.Max(Math.Abs(left), Math.Abs(right)));
    private static double Cross( in ReadOnlyPoint o, in ReadOnlyPoint a, in ReadOnlyPoint b ) => (( a.X - o.X ) * ( b.Y - o.Y )) - (( a.Y - o.Y ) * ( b.X - o.X ));

    private static bool IsParallel( in ReadOnlyPoint a1, in ReadOnlyPoint a2, in ReadOnlyPoint b1, in ReadOnlyPoint b2 ) =>
        Math.Abs((( a2.X - a1.X ) * ( b2.Y - b1.Y )) - (( a2.Y - a1.Y ) * ( b2.X - b1.X ))) <= TOLERANCE * Math.Max(1.0, Math.Abs(a2.X - a1.X) + Math.Abs(a2.Y - a1.Y) + Math.Abs(b2.X - b1.X) + Math.Abs(b2.Y - b1.Y));

    private static ReadOnlyPoint ScalePoint( in ReadOnlyPoint point, in ReadOnlyPoint origin, double factor ) =>
        new(origin.X + (( point.X - origin.X ) * factor), origin.Y + (( point.Y - origin.Y ) * factor));

    private static ReadOnlyPoint GrowPoint( in ReadOnlyPoint point, in ReadOnlyPoint origin, double amount )
    {
        double dx     = point.X - origin.X;
        double dy     = point.Y - origin.Y;
        double length = Math.Sqrt(( dx * dx ) + ( dy * dy ));
        if ( length <= TOLERANCE ) { return point; }

        double scale = ( length + amount ) / length;
        return new ReadOnlyPoint(origin.X + ( dx * scale ), origin.Y + ( dy * scale ));
    }

    private static ReadOnlyPoint RotatePoint( in ReadOnlyPoint point, in ReadOnlyPoint origin, double sin, double cos )
    {
        double dx = point.X - origin.X;
        double dy = point.Y - origin.Y;
        return new ReadOnlyPoint(origin.X + ( dx * cos ) - ( dy * sin ), origin.Y + ( dx * sin ) + ( dy * cos ));
    }

    private static bool OnSegment( in ReadOnlyPoint a, in ReadOnlyPoint b, in ReadOnlyPoint point )
    {
        double cross = (( b.X - a.X ) * ( point.Y - a.Y )) - (( b.Y - a.Y ) * ( point.X - a.X ));
        if ( Math.Abs(cross) > TOLERANCE ) { return false; }

        return point.X >= Math.Min(a.X, b.X) - TOLERANCE && point.X <= Math.Max(a.X, b.X) + TOLERANCE && point.Y >= Math.Min(a.Y, b.Y) - TOLERANCE && point.Y <= Math.Max(a.Y, b.Y) + TOLERANCE;
    }

    private static bool SegmentsCross( in ReadOnlyPoint p1, in ReadOnlyPoint p2, in ReadOnlyPoint q1, in ReadOnlyPoint q2 )
    {
        double d1 = Cross(q1, q2, p1);
        double d2 = Cross(q1, q2, p2);
        double d3 = Cross(p1, p2, q1);
        double d4 = Cross(p1, p2, q2);

        if ( (( d1 > TOLERANCE && d2 < -TOLERANCE ) || ( d1 < -TOLERANCE && d2 > TOLERANCE )) && (( d3 > TOLERANCE && d4 < -TOLERANCE ) || ( d3 < -TOLERANCE && d4 > TOLERANCE )) ) { return true; }

        return OnSegment(q1, q2, p1) || OnSegment(q1, q2, p2) || OnSegment(p1, p2, q1) || OnSegment(p1, p2, q2);
    }
}
