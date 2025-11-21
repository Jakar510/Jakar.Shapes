// Jakar.Extensions :: Jakar.Shapes
// 09/29/2025  09:00

namespace Jakar.Shapes;


public static class Points
{
    extension<TPoint>( TPoint self )
        where TPoint : struct, IPoint<TPoint>
    {
        [Pure] public TPoint Reverse() => TPoint.Create(self.Y,         self.X);
        [Pure] public TPoint Round()   => TPoint.Create(self.X.Round(), self.Y.Round());
        [Pure] public TPoint Floor()   => TPoint.Create(self.X.Floor(), self.Y.Floor());


        public void Deconstruct( out float x, out float y )
        {
            x = (float)self.X;
            y = (float)self.Y;
        }
        public void Deconstruct( out double x, out double y )
        {
            x = self.X;
            y = self.Y;
        }


        public TPoint Abs()        => TPoint.Create(double.Abs(self.X), double.Abs(self.Y));
        public bool   IsFinite()   => double.IsFinite(self.X) && double.IsFinite(self.Y);
        public bool   IsInfinity() => double.IsInfinity(self.X) || double.IsInfinity(self.Y);
        public bool   IsInteger()  => double.IsInteger(self.X) && double.IsInteger(self.Y);
        public bool   IsNaN()      => double.IsNaN(self.X) || double.IsNaN(self.Y);
        public bool   IsNegative() => self is { X: < 0, Y: < 0 };
        public bool   IsValid()    => !self.IsNaN() && self.IsFinite();
        public bool   IsPositive() => self is { X: > 0, Y: > 0 };
        public bool   IsZero()     => self is { X: 0, Y  : 0 };


        public double DistanceTo<TOther>( TOther other )
            where TOther : struct, IPoint<TOther>
        {
            double x      = self.X - other.X;
            double y      = self.Y - other.Y;
            double x2     = x * x;
            double y2     = y * y;
            double result = Math.Sqrt(x2 + y2);
            return result;
        }
        public double Dot( TPoint other ) => self.X * other.X + self.Y * other.Y;
        public double Magnitude()         => Math.Sqrt(self.X * self.X + self.Y * self.Y);
        public double AngleBetween( TPoint p1, TPoint p2 )
        {
            TPoint v1 = self.Subtract(p1);
            TPoint v2 = self.Subtract(p2);

            double dot  = v1.Dot(v2);
            double mag1 = v1.Magnitude();
            double mag2 = v2.Magnitude();
            if ( mag1 == 0 || mag2 == 0 ) { return 0; }

            double cosTheta = dot / ( mag1 * mag2 );
            cosTheta = Math.Clamp(cosTheta, -1.0, 1.0); // Avoid NaN due to precision

            return Math.Acos(cosTheta); // In radians
        }


        public TPoint Add<TOther>( TOther value )
            where TOther : struct, IPoint<TOther> => TPoint.Create(self.X + value.X, self.Y + value.Y);
        public TPoint Subtract<TOther>( TOther value )
            where TOther : struct, IPoint<TOther> => TPoint.Create(self.X - value.X, self.Y - value.Y);
        public TPoint Multiply<TOther>( TOther value )
            where TOther : struct, IPoint<TOther> => TPoint.Create(self.X * value.X, self.Y * value.Y);
        public TPoint Divide<TOther>( TOther value )
            where TOther : struct, IPoint<TOther> => TPoint.Create(self.X / value.X, self.Y / value.Y);

        public TPoint Add( (int xOffset, int yOffset)            value ) => TPoint.Create(self.X + value.xOffset, self.Y + value.yOffset);
        public TPoint Subtract( (int xOffset, int yOffset)       value ) => TPoint.Create(self.X - value.xOffset, self.Y - value.yOffset);
        public TPoint Divide( (int xOffset, int yOffset)         value ) => TPoint.Create(self.X / value.xOffset, self.Y / value.yOffset);
        public TPoint Multiply( (int xOffset, int yOffset)       value ) => TPoint.Create(self.X * value.xOffset, self.Y * value.yOffset);
        public TPoint Add( (float xOffset, float yOffset)        value ) => TPoint.Create(self.X + value.xOffset, self.Y + value.yOffset);
        public TPoint Multiply( (float xOffset, float yOffset)   value ) => TPoint.Create(self.X * value.xOffset, self.Y * value.yOffset);
        public TPoint Divide( (float xOffset, float yOffset)     value ) => TPoint.Create(self.X / value.xOffset, self.Y / value.yOffset);
        public TPoint Subtract( (float xOffset, float yOffset)   value ) => TPoint.Create(self.X - value.xOffset, self.Y - value.yOffset);
        public TPoint Add( (double xOffset, double yOffset)      value ) => TPoint.Create(self.X + value.xOffset, self.Y + value.yOffset);
        public TPoint Subtract( (double xOffset, double yOffset) value ) => TPoint.Create(self.X - value.xOffset, self.Y - value.yOffset);
        public TPoint Divide( (double xOffset, double yOffset)   value ) => TPoint.Create(self.X / value.xOffset, self.Y / value.yOffset);
        public TPoint Multiply( (double xOffset, double yOffset) value ) => TPoint.Create(self.X * value.xOffset, self.Y * value.yOffset);

        public TPoint Add( double      value ) => TPoint.Create(self.X + value, self.Y + value);
        public TPoint Subtract( double value ) => TPoint.Create(self.X - value, self.Y - value);
        public TPoint Multiply( double value ) => TPoint.Create(self.X * value, self.Y * value);
        public TPoint Divide( double   value ) => TPoint.Create(self.X / value, self.Y / value);
        public TPoint Add( float       value ) => TPoint.Create(self.X + value, self.Y + value);
        public TPoint Subtract( float  value ) => TPoint.Create(self.X - value, self.Y - value);
        public TPoint Divide( float    value ) => TPoint.Create(self.X / value, self.Y / value);
        public TPoint Multiply( float  value ) => TPoint.Create(self.X * value, self.Y * value);
        public TPoint Add( int         value ) => TPoint.Create(self.X + value, self.Y + value);
        public TPoint Subtract( int    value ) => TPoint.Create(self.X - value, self.Y - value);
        public TPoint Divide( int      value ) => TPoint.Create(self.X / value, self.Y / value);
        public TPoint Multiply( int    value ) => TPoint.Create(self.X * value, self.Y * value);
    }
}
