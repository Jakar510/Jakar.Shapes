// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

namespace Jakar.Shapes;


public static class Ellipses
{
    private const double TOLERANCE = 1e-9;


    extension<TEllipse>( TEllipse self )
        where TEllipse : struct, IEllipse<TEllipse>
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
                    return $"{self.Center},{self.RadiusX},{self.RadiusY}";

                case "-":
                    return $"{self.Center}-{self.RadiusX}-{self.RadiusY}";

                case EMPTY:
                case null:
                default:
                    return $"{typeof(TEllipse).Name}<{nameof(self.Center)}: {self.Center}, {nameof(self.RadiusX)}: {self.RadiusX}, {nameof(self.RadiusY)}: {self.RadiusY}>";
            }
        }

        public void Deconstruct( out ReadOnlyPoint center, out double radiusX, out double radiusY )
        {
            center  = self.Center;
            radiusX = self.RadiusX;
            radiusY = self.RadiusY;
        }


        // ----------------------------------------------------------------------------- axes

        /// <summary> The longer of the two radii. </summary>
        public double SemiMajorAxis() => Math.Max(self.RadiusX, self.RadiusY);

        /// <summary> The shorter of the two radii. </summary>
        public double SemiMinorAxis() => Math.Min(self.RadiusX, self.RadiusY);

        public double MajorAxis() => 2 * self.SemiMajorAxis();
        public double MinorAxis() => 2 * self.SemiMinorAxis();

        /// <summary> Axis lengths, for parity with the polygon APIs. </summary>
        public (double Major, double Minor) SideLengths() => (self.MajorAxis(), self.MinorAxis());

        /// <summary> Both axes as lines through the centre. </summary>
        public (ReadOnlyLine Major, ReadOnlyLine Minor) DiagonalLengths() => self.Axes();

        public (ReadOnlyLine Major, ReadOnlyLine Minor) Axes()
        {
            ReadOnlyLine horizontal = new(new ReadOnlyPoint(self.Center.X - self.RadiusX, self.Center.Y), new ReadOnlyPoint(self.Center.X + self.RadiusX, self.Center.Y));
            ReadOnlyLine vertical   = new(new ReadOnlyPoint(self.Center.X, self.Center.Y - self.RadiusY), new ReadOnlyPoint(self.Center.X, self.Center.Y + self.RadiusY));

            return self.RadiusX >= self.RadiusY
                       ? (horizontal, vertical)
                       : (vertical, horizontal);
        }


        // ----------------------------------------------------------------------------- measurements

        public double Area() => Math.PI * self.RadiusX * self.RadiusY;

        /// <summary>
        /// Perimeter by Ramanujan's second approximation, which is accurate to better than 1e-10 relative for
        /// eccentricities below about 0.99. An ellipse has no closed-form perimeter.
        /// </summary>
        public double Perimeter()
        {
            double a = self.SemiMajorAxis();
            double b = self.SemiMinorAxis();
            if ( a <= 0 && b <= 0 ) { return 0; }

            double h = ( a - b ) * ( a - b ) / (( a + b ) * ( a + b ));
            return Math.PI * ( a + b ) * ( 1 + ( 3 * h / ( 10 + Math.Sqrt(4 - ( 3 * h )) ) ));
        }

        public ReadOnlyPoint Centroid() => self.Center;

        public ReadOnlyRectangle BoundingBox() => new(self.Center.X - self.RadiusX, self.Center.Y - self.RadiusY, 2 * self.RadiusX, 2 * self.RadiusY);

        /// <summary> How far the ellipse departs from circular: 0 is a circle, values approach 1 as it flattens. </summary>
        public double Eccentricity()
        {
            double a = self.SemiMajorAxis();
            double b = self.SemiMinorAxis();
            if ( a <= 0 ) { return 0; }

            double ratio = b / a;
            return Math.Sqrt(Math.Max(0, 1 - ( ratio * ratio )));
        }

        /// <summary> The two focal points, which coincide at the centre when the ellipse is a circle. </summary>
        public (ReadOnlyPoint First, ReadOnlyPoint Second) Foci()
        {
            double a = self.SemiMajorAxis();
            double b = self.SemiMinorAxis();
            double c = Math.Sqrt(Math.Max(0, ( a * a ) - ( b * b )));

            return self.RadiusX >= self.RadiusY
                       ? (new ReadOnlyPoint(self.Center.X - c, self.Center.Y), new ReadOnlyPoint(self.Center.X + c, self.Center.Y))
                       : (new ReadOnlyPoint(self.Center.X, self.Center.Y - c), new ReadOnlyPoint(self.Center.X, self.Center.Y + c));
        }

        /// <summary> True when both radii are equal, so the ellipse is a circle. </summary>
        public bool IsCircle() => Math.Abs(self.RadiusX - self.RadiusY) <= TOLERANCE * Math.Max(1.0, Math.Abs(self.RadiusX));

        /// <summary> The point on the boundary at parametric angle <paramref name="angle"/>. </summary>
        public ReadOnlyPoint PointAt( Radians angle ) => new(self.Center.X + ( self.RadiusX * Math.Cos(angle.Value) ), self.Center.Y + ( self.RadiusY * Math.Sin(angle.Value) ));


        // ----------------------------------------------------------------------------- hit testing

        /// <summary> True when <paramref name="point"/> lies inside or on the boundary. </summary>
        public bool Contains( in ReadOnlyPoint point )
        {
            if ( self.RadiusX <= 0 || self.RadiusY <= 0 ) { return false; }

            double dx = ( point.X - self.Center.X ) / self.RadiusX;
            double dy = ( point.Y - self.Center.Y ) / self.RadiusY;
            return ( dx * dx ) + ( dy * dy ) <= 1 + TOLERANCE;
        }

        /// <summary> Conservative overlap test using the bounding boxes and a radial check. </summary>
        public bool Intersects<TOther>( TOther other )
            where TOther : struct, IEllipse<TOther>
        {
            double dx = Math.Abs(self.Center.X - other.Center.X);
            double dy = Math.Abs(self.Center.Y - other.Center.Y);
            return dx <= self.RadiusX + other.RadiusX + TOLERANCE && dy <= self.RadiusY + other.RadiusY + TOLERANCE;
        }

        public bool Intersects( in Circle circle ) => self.Center.DistanceTo(circle.Center) <= self.SemiMajorAxis() + circle.Radius + TOLERANCE;


        // ----------------------------------------------------------------------------- transforms

        public TEllipse Scale( double factor )                      => TEllipse.Create(self.Center, self.RadiusX * factor, self.RadiusY * factor);
        public TEllipse Scale( double xFactor, double yFactor )     => TEllipse.Create(self.Center, self.RadiusX * xFactor, self.RadiusY * yFactor);
        public TEllipse Grow( double amount )                       => TEllipse.Create(self.Center, self.RadiusX + amount, self.RadiusY + amount);
        public TEllipse Translate( double xOffset, double yOffset ) => TEllipse.Create(new ReadOnlyPoint(self.Center.X + xOffset, self.Center.Y + yOffset), self.RadiusX, self.RadiusY);
        public TEllipse MoveTo( in ReadOnlyPoint center )           => TEllipse.Create(center, self.RadiusX, self.RadiusY);

        /// <summary> Rotating an axis-aligned ellipse by a quarter turn swaps its radii; other angles cannot be represented. </summary>
        public TEllipse Rotate( Radians angle )
        {
            double quarter = Math.PI / 2;
            long   steps   = (long)Math.Round(angle.Value / quarter);
            return Math.Abs(( steps * quarter ) - angle.Value) > TOLERANCE
                       ? TEllipse.Create(self.Center, double.NaN, double.NaN)
                       : ( steps & 1 ) == 0
                           ? TEllipse.Create(self.Center, self.RadiusX, self.RadiusY)
                           : TEllipse.Create(self.Center, self.RadiusY, self.RadiusX);
        }

        public TEllipse Abs()   => TEllipse.Create(self.Center.Abs(),   Math.Abs(self.RadiusX),   Math.Abs(self.RadiusY));
        public TEllipse Round() => TEllipse.Create(self.Center.Round(), Math.Round(self.RadiusX), Math.Round(self.RadiusY));
        public TEllipse Floor() => TEllipse.Create(self.Center.Floor(), Math.Floor(self.RadiusX), Math.Floor(self.RadiusY));


        // ----------------------------------------------------------------------------- predicates

        public bool IsFinite()   => self.Center.IsFinite() && double.IsFinite(self.RadiusX) && double.IsFinite(self.RadiusY);
        public bool IsInfinity() => self.Center.IsInfinity() || double.IsInfinity(self.RadiusX) || double.IsInfinity(self.RadiusY);
        public bool IsInteger()  => self.Center.IsInteger() && double.IsInteger(self.RadiusX) && double.IsInteger(self.RadiusY);
        public bool IsNaN()      => self.Center.IsNaN() || double.IsNaN(self.RadiusX) || double.IsNaN(self.RadiusY);
        public bool IsNegative() => self.RadiusX < 0 || self.RadiusY < 0;
        public bool IsPositive() => self.RadiusX > 0 && self.RadiusY > 0;
        public bool IsZero()     => self.Center.IsZero() && self.RadiusX is 0 && self.RadiusY is 0;
        public bool IsValid()    => !self.IsNaN() && self.IsFinite() && self.RadiusX > 0 && self.RadiusY > 0;
    }
}
