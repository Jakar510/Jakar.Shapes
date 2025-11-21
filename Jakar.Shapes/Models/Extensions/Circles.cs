// Jakar.Extensions :: Jakar.Shapes
// 09/29/2025  08:54

namespace Jakar.Shapes;


public static class Circles
{
    public const double TOLERANCE = 1e-8;



    extension<TCircle>( TCircle self )
        where TCircle : struct, ICircle<TCircle>
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
                    return $"{typeof(TCircle).Name}<{nameof(self.Center)}: {self.Center}, {nameof(self.Radius)}: {self.Radius}>";
            }
        }

        [Pure] public TCircle Reverse() => TCircle.Create(self.Y,         self.X);
        [Pure] public TCircle Round()   => TCircle.Create(self.X.Round(), self.Y.Round());
        [Pure] public TCircle Floor()   => TCircle.Create(self.X.Floor(), self.Y.Floor());

        [Pure] public ReadOnlyLine RadiusLine( in Radians radians ) =>
            new(self.Center, new ReadOnlyPoint(self.Center.X + self.Radius * Math.Cos(radians.Value), self.Center.Y + self.Radius * Math.Sin(radians.Value)));
        [Pure] public CalculatedLine RadiusCalculatedLine( in Radians radians )
        {
            ReadOnlyPoint center = self.Center;
            ReadOnlyPoint end    = new(self.Center.X + self.Radius * Math.Cos(radians.Value), self.Center.Y + self.Radius * Math.Sin(radians.Value));

            double dx = end.X - center.X;
            double dy = end.Y - center.Y;

            if ( Math.Abs(dx) < double.Epsilon )
            {
                double value = self.Center.X;
                return CalculatedLine.Create(x => value);
            }

            double m = dy / dx;
            double b = center.Y - m * center.X;

            return CalculatedLine.Create(x => m * x + b);
        }
        [Pure] public ReadOnlyLine DiameterLine( in Radians radians )
        {
            ReadOnlyPoint center = self.Center;
            ReadOnlyPoint start  = new(center.X - self.Radius * Math.Cos(radians.Value), center.Y - self.Radius * Math.Sin(radians.Value));
            ReadOnlyPoint end    = new(center.X + self.Radius * Math.Cos(radians.Value), center.Y + self.Radius * Math.Sin(radians.Value));
            return new ReadOnlyLine(start, end);
        }
        [Pure] public CalculatedLine DiameterCalculatedLine( in Radians radians )
        {
            ReadOnlyPoint center = self.Center;
            ReadOnlyPoint start  = new(center.X - self.Radius * Math.Cos(radians.Value), center.Y - self.Radius * Math.Sin(radians.Value));
            ReadOnlyPoint end    = new(center.X + self.Radius * Math.Cos(radians.Value), center.Y + self.Radius * Math.Sin(radians.Value));

            double dx = end.X - start.X;
            double dy = end.Y - start.Y;

            if ( Math.Abs(dx) < double.Epsilon ) { return CalculatedLine.Create(x => start.X); }

            double m = dy / dx;
            double b = start.Y - m * start.X;

            return CalculatedLine.Create(x => m * x + b);
        }

        public void Deconstruct( out float x, out float y, out float radius )
        {
            x      = (float)self.X;
            y      = (float)self.Y;
            radius = (float)self.Radius;
        }
        public void Deconstruct( out double x, out double y, out double radius )
        {
            x      = self.X;
            y      = self.Y;
            radius = self.Radius;
        }
        public void Deconstruct( out ReadOnlyPoint point, out double radius )
        {
            radius = self.Radius;
            point  = self.Location;
        }
        public void Deconstruct( out ReadOnlyPointF point, out double radius )
        {
            radius = self.Radius;
            point  = self.Location;
        }

        public ReadOnlyPoint Center()     => self.Location;
        public TCircle       Abs()        => TCircle.Create(self.Location.Abs(), double.Abs(self.Radius));
        public bool          IsFinite()   => double.IsFinite(self.X) && double.IsFinite(self.Y) && double.IsFinite(self.Radius);
        public bool          IsInfinity() => double.IsInfinity(self.X) || double.IsInfinity(self.Y) || double.IsInfinity(self.Radius);
        public bool          IsInteger()  => double.IsInteger(self.X) && double.IsInteger(self.Y) && double.IsInteger(self.Radius);
        public bool          IsNaN()      => double.IsNaN(self.X)                                || double.IsNaN(self.Y) || double.IsNaN(self.Radius);
        public bool          IsNegative() => self.Radius                                     < 0 || self.Location.IsNegative();
        public bool          IsValid()    => !self.IsNaN() && self.IsFinite() && self.Radius > 0 && self.Location.IsValid();
        public bool          IsPositive() => self.Radius                                     > 0  || self.Location.IsPositive();
        public bool          IsZero()     => self.Radius                                     == 0 || self.Location.IsZero();

        public double DistanceTo( TCircle other ) =>
            self.DistanceTo(other.Center);
        public double DistanceTo<TPoint>( TPoint other )
            where TPoint : struct, IPoint<TPoint> =>
            self.Center.DistanceTo(other);

        public bool IsTangent( ReadOnlyLine    line )                                                                                         => self.GetLineRelation(line) is CircleLineRelation.Tangent;
        public bool IsSecant( ReadOnlyLine     line )                                                                                         => self.GetLineRelation(line) is CircleLineRelation.Secant;
        public bool IsDisjoint( ReadOnlyLine   line )                                                                                         => self.GetLineRelation(line) is CircleLineRelation.Disjoint;
        public bool IsTangent( CalculatedLine  line, in double xMin, in double xMax, in int samples = 1000, in double tolerance = TOLERANCE ) => self.GetLineRelation(line, xMin, xMax, samples, tolerance) is CircleLineRelation.Tangent;
        public bool IsSecant( CalculatedLine   line, double    xMin, double    xMax, int    samples = 1000, double    tolerance = TOLERANCE ) => self.GetLineRelation(line, xMin, xMax, samples, tolerance) is CircleLineRelation.Secant;
        public bool IsDisjoint( CalculatedLine line, double    xMin, double    xMax, int    samples = 1000, double    tolerance = TOLERANCE ) => self.GetLineRelation(line, xMin, xMax, samples, tolerance) is CircleLineRelation.Disjoint;
        public CircleLineRelation GetLineRelation<TLine>( TLine line )
            where TLine : struct, ILine<TLine>
        {
            double dx           = line.End.X   - line.Start.X;
            double dy           = line.End.Y   - line.Start.Y;
            double fx           = line.Start.X - self.Center.X;
            double fy           = line.Start.Y - self.Center.Y;
            double a            = dx * dx      + dy * dy;
            double b            = 2 * ( fx * dx + fy * dy );
            double c            = fx * fx + fy * fy - self.Radius * self.Radius;
            double discriminant = b                               * b - 4 * a * c;
            if ( discriminant < 0 ) { return CircleLineRelation.Disjoint; }

            if ( !line.IsFinite )
            {
                return discriminant == 0
                           ? CircleLineRelation.Tangent
                           : CircleLineRelation.Secant;
            }

            // Finite segment: check if intersection lies on segment
            discriminant = Math.Sqrt(discriminant);
            double t1 = ( -b - discriminant ) / ( 2 * a );
            double t2 = ( -b + discriminant ) / ( 2 * a );

            bool onSegment1 = t1 is >= 0 and <= 1;
            bool onSegment2 = t2 is >= 0 and <= 1;

            if ( onSegment1 && onSegment2 ) { return CircleLineRelation.Secant; }

            if ( onSegment1 || onSegment2 )
            {
                return discriminant == 0
                           ? CircleLineRelation.Tangent
                           : CircleLineRelation.Secant;
            }

            return CircleLineRelation.Disjoint;
        }
        public CircleLineRelation GetLineRelation( CalculatedLine curve, double xMin, double xMax, int samples = 1000, double tolerance = TOLERANCE )
        {
            double r2          = self.Radius     * self.Radius;
            double range       = ( xMax - xMin ) / samples;
            bool   foundOn     = false;
            bool   foundInside = false;

            for ( int i = 0; i <= samples; i++ )
            {
                double x = xMin + range * i;
                double y = curve[x];

                if ( double.IsNaN(y) || double.IsInfinity(y) ) { continue; }

                double dx    = x       - self.Center.X;
                double dy    = y       - self.Center.Y;
                double dist2 = dx * dx + dy * dy;
                double diff  = dist2   - r2;

                if ( Math.Abs(diff) < tolerance ) { foundOn = true; }
                else if ( diff      < 0 ) { foundInside     = true; }

                if ( foundInside && foundOn ) { return CircleLineRelation.Secant; }
            }

            if ( foundOn ) { return CircleLineRelation.Tangent; }

            return CircleLineRelation.Disjoint;
        }
        [Pure] [MustDisposeResource] public ArrayBuffer<ReadOnlyPoint> Intersections<TLine>( CalculatedLine curve, double xMin, double xMax, int samples = 1000, double tolerance = TOLERANCE )
            where TLine : struct, ILine<TLine>
        {
            ReadOnlyPoint              center        = self.Center;
            double                     r2            = self.Radius * self.Radius;
            ArrayBuffer<ReadOnlyPoint> intersections = new(samples);
            double                     step          = ( xMax - xMin ) / samples;

            double prevX = xMin;
            double prevD = helper(prevX, curve, center, r2);

            for ( int i = 1; i <= samples; i++ )
            {
                double currX = xMin + i * step;
                double currD = helper(currX, curve, center, r2);

                if ( double.IsNaN(prevD) || double.IsNaN(currD) )
                {
                    prevX = currX;
                    prevD = currD;
                    continue;
                }

                // Check for sign change or very close to zero
                if ( Math.Abs(prevD) < tolerance )
                {
                    ReadOnlyPoint point = new(prevX, curve[prevX]);
                    intersections.Add(in point);
                }
                else if ( prevD * currD < 0 ) // sign change → root in between
                {
                    double        root  = bisection(prevX, currX, tolerance, 50, curve, center, r2);
                    ReadOnlyPoint point = new(root, curve[root]);
                    intersections.Add(in point);
                }

                prevX = currX;
                prevD = currD;
            }

            return intersections;

            static double helper( double x, CalculatedLine curve, ReadOnlyPoint center, double r2 )
            {
                double y = curve[x];
                if ( double.IsNaN(y) || double.IsInfinity(y) ) { return double.NaN; }

                double dx = x - center.X;
                double dy = y - center.Y;
                return dx * dx + dy * dy - r2;
            }

            static double bisection( double a, double b, double tolerance, int maxIter, CalculatedLine curve, ReadOnlyPoint center, double r2 )
            {
                double fa  = helper(a, curve, center, r2);
                double fb  = helper(b, curve, center, r2);
                double mid = 0.5 * ( a + b );

                for ( int i = 0; i < maxIter; i++ )
                {
                    double fm = helper(mid, curve, center, r2);

                    if ( Math.Abs(fm) < tolerance ) { return mid; }

                    if ( fa * fm < 0 )
                    {
                        b  = mid;
                        fb = fm;
                    }
                    else
                    {
                        a  = mid;
                        fa = fm;
                    }
                }

                return mid;
            }
        }
    }
}
