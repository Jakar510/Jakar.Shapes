// Jakar.Extensions :: Jakar.Shapes
// 09/29/2025  08:52

namespace Jakar.Shapes;


public static class Rectangles
{
    extension<TRectangle>( TRectangle self )
        where TRectangle : struct, IRectangle<TRectangle>
    {
        public bool IsAtLeast<TOther>( in TOther other )
            where TOther : struct, IRectangle<TOther> => other.Width <= self.Width && other.Height <= self.Height;
        public bool Contains<TPoint>( in TPoint other )
            where TPoint : struct, IPoint<TPoint> => other.X >= self.X && other.X < self.MaxWidth() && other.Y >= self.Y && other.Y < self.MaxHeight();
        public bool Contains( in MutableRectangle other ) => self.X <= other.MaxWidth() && other.MaxWidth() >= self.X && self.Y <= other.MaxHeight() && self.MaxHeight() >= other.Y;
        public bool Contains<TPoint>( params ReadOnlySpan<TPoint> points )
            where TPoint : struct, IPoint<TPoint>
        {
            foreach ( ref readonly TPoint point in points )
            {
                if ( self.Contains(in point) ) { return true; }
            }

            return false;
        }
        public bool ContainsAny<TPoint>( params ReadOnlySpan<TPoint> others )
            where TPoint : struct, IPoint<TPoint>
        {
            foreach ( ref readonly TPoint point in others )
            {
                if ( self.Contains(in point) ) { return true; }
            }

            return false;
        }
        public bool ContainsAll<TPoint>( params ReadOnlySpan<TPoint> others )
            where TPoint : struct, IPoint<TPoint>
        {
            foreach ( ref readonly TPoint point in others )
            {
                if ( !self.Contains(in point) ) { return false; }
            }

            return true;
        }

        [Pure] public ReadOnlyPoint TopLeft()     => new(self.X, self.Y);
        [Pure] public ReadOnlyPoint TopRight()    => new(self.MaxWidth(), self.Y);
        [Pure] public ReadOnlyPoint BottomLeft()  => new(self.X, self.MaxHeight());
        [Pure] public ReadOnlyPoint BottomRight() => new(self.MaxWidth(), self.MaxHeight());
        [Pure] public double        MaxHeight()   => self.Y + self.Height;
        [Pure] public double        MaxWidth()    => self.X + self.Width;
        [Pure] public ReadOnlyLine  BottomSide()  => new(self.BottomLeft(), self.BottomRight());
        [Pure] public ReadOnlyLine  LeftSide()    => new(self.TopLeft(), self.BottomLeft());
        [Pure] public ReadOnlyLine  RightSide()   => new(self.TopRight(), self.BottomRight());
        [Pure] public ReadOnlyLine  TopSide()     => new(self.TopLeft(), self.TopRight());
        [Pure] public TRectangle    Reverse()     => TRectangle.Create(self.Location.Reverse(), self.Size.Reverse());
        [Pure] public TRectangle    Round()       => TRectangle.Create(self.X.Round(),          self.Y.Round(), self.Width.Round(), self.Height.Round());
        [Pure] public TRectangle    Floor()       => TRectangle.Create(self.X.Floor(),          self.Y.Floor(), self.Width.Floor(), self.Height.Floor());


        [Pure] public TRectangle Union<TOther>( in TOther other )
            where TOther : struct, IRectangle<TOther>
        {
            double x      = Math.Max(self.X, other.X);
            double y      = Math.Max(self.Y, other.Y);
            double width  = Math.Min(self.MaxWidth(),  other.MaxWidth())  - x;
            double height = Math.Min(self.MaxHeight(), other.MaxHeight()) - y;

            return width < 0 || height < 0
                       ? TRectangle.Zero
                       : TRectangle.Create(x, y, width, height);
        }
        [Pure] public TRectangle SharedArea<TOther>( in TOther other )
            where TOther : struct, IRectangle<TOther>
        {
            double x      = Math.Max(self.X, other.X);
            double y      = Math.Max(self.Y, other.Y);
            double width  = Math.Min(self.MaxWidth(),  other.MaxWidth())  - x;
            double height = Math.Min(self.MaxHeight(), other.MaxHeight()) - y;

            return width < 0 || height < 0
                       ? TRectangle.Zero
                       : TRectangle.Create(x, y, width, height);
        }
     
        public bool IntersectsWith<TOther>( in TOther other )
            where TOther : struct, IRectangle<TOther> => !( ( self.X <= other.MaxWidth() && other.MaxWidth() >= self.X ) || ( self.Y <= other.MaxHeight() && self.MaxHeight() >= other.Y ) );
        public bool DoesLineIntersect<TPoint>( in TPoint source, in TPoint target )
            where TPoint : struct, IPoint<TPoint>
        {
            double               t0          = 0.0;
            double               t1          = 1.0;
            double               dx          = target.X - source.X;
            double               dy          = target.Y - source.Y;
            ReadOnlySpan<double> boundariesX = [self.X, self.MaxWidth()];
            ReadOnlySpan<double> boundariesY = [self.Y, self.MaxHeight()];

            for ( int i = 0; i < 2; i++ )
            {
                double pX = i == 0
                                ? -dx
                                : dx;

                double pY = i == 0
                                ? -dy
                                : dy;

                for ( int j = 0; j < 2; j++ )
                {
                    double qX = j == 0
                                    ? source.X       - boundariesX[i]
                                    : boundariesX[i] - source.X;

                    double qY = j == 0
                                    ? source.Y       - boundariesY[i]
                                    : boundariesY[i] - source.Y;

                    if ( pX == 0 && qX < 0 ) { return false; } // Line is parallel to the self's horizontal edge and outside of it

                    if ( pY == 0 && qY < 0 ) { return false; } // Line is parallel to the self's vertical edge and outside of it

                    double rX = pX != 0
                                    ? qX / pX
                                    : double.MaxValue;

                    double rY = pY != 0
                                    ? qY / pY
                                    : double.MaxValue;

                    if ( pX < 0 ) { t0 = Math.Max(t0, rX); }
                    else { t1          = Math.Min(t1, rX); }

                    if ( pY < 0 ) { t0 = Math.Max(t0, rY); }
                    else { t1          = Math.Min(t1, rY); }

                    if ( t0 > t1 ) { return false; }
                }
            }

            return true;
        }

        public void Deconstruct( out float x, out float y, out float width, out float height )
        {
            x      = (float)self.X;
            y      = (float)self.Y;
            width  = (float)self.Width;
            height = (float)self.Height;
        }
        public void Deconstruct( out double x, out double y, out double width, out double height )
        {
            x      = self.X;
            y      = self.Y;
            width  = self.Width;
            height = self.Height;
        }
        public void Deconstruct( out ReadOnlyPoint point, out ReadOnlySize size )
        {
            size  = self.Size;
            point = self.Location;
        }
        public void Deconstruct( out ReadOnlyPointF point, out ReadOnlySizeF size )
        {
            size  = self.Size;
            point = self.Location;
        }

        public ReadOnlyPoint Center()     => new(self.MaxWidth() / 2, self.MaxHeight() / 2);
        public TRectangle    Abs()        => TRectangle.Create(double.Abs(self.X), double.Abs(self.Y), double.Abs(self.Width), double.Abs(self.Height));
        public bool          IsFinite()   => double.IsFinite(self.X) && double.IsFinite(self.Y) && double.IsFinite(self.Width) && double.IsFinite(self.Height);
        public bool          IsInfinity() => double.IsInfinity(self.X) || double.IsInfinity(self.Y) || double.IsInfinity(self.Width) || double.IsInfinity(self.Height);
        public bool          IsInteger()  => double.IsInteger(self.X) && double.IsInteger(self.Y) && double.IsInteger(self.Width) && double.IsInteger(self.Height);
        public bool          IsNaN()      => double.IsNaN(self.X) || double.IsNaN(self.Y) || double.IsNaN(self.Width) || double.IsNaN(self.Height);
        public bool          IsNegative() => self.Width < 0       || self.Height < 0;
        public bool          IsValid()    => self.IsNaN()         || ( self.IsFinite() && ( self.Width <= 0 || self.Height <= 0 ) );
        public bool          IsPositive() => self.Width > 0       || self.Height > 0;
        public bool          IsZero()     => self.Width == 0      || self.Height == 0;

        public TRectangle Add<TOther>( TOther other )
            where TOther : struct, IRectangle<TOther> => TRectangle.Create(self.X + other.X, self.Y + other.Y, self.Width + other.Width, self.Height + other.Height);
        public TRectangle Subtract<TOther>( TOther other )
            where TOther : struct, IRectangle<TOther> => TRectangle.Create(self.X - other.X, self.Y - other.Y, self.Width - other.Width, self.Height - other.Height);
        public TRectangle Multiply<TOther>( TOther other )
            where TOther : struct, IRectangle<TOther> => TRectangle.Create(self.X * other.X, self.Y * other.Y, self.Width * other.Width, self.Height * other.Height);
        public TRectangle Divide<TOther>( TOther other )
            where TOther : struct, IRectangle<TOther> => TRectangle.Create(self.X / other.X, self.Y / other.Y, self.Width / other.Width, self.Height / other.Height);

        public TRectangle Add( (int xOffset, int yOffset)            other ) => TRectangle.Create(self.Location, self.Size + other);
        public TRectangle Subtract( (int xOffset, int yOffset)       other ) => TRectangle.Create(self.Location, self.Size - other);
        public TRectangle Divide( (int xOffset, int yOffset)         other ) => TRectangle.Create(self.Location, self.Size / other);
        public TRectangle Multiply( (int xOffset, int yOffset)       other ) => TRectangle.Create(self.Location, self.Size * other);
        public TRectangle Add( (float xOffset, float yOffset)        other ) => TRectangle.Create(self.Location, self.Size + other);
        public TRectangle Subtract( (float xOffset, float yOffset)   other ) => TRectangle.Create(self.Location, self.Size - other);
        public TRectangle Divide( (float xOffset, float yOffset)     other ) => TRectangle.Create(self.Location, self.Size / other);
        public TRectangle Multiply( (float xOffset, float yOffset)   other ) => TRectangle.Create(self.Location, self.Size * other);
        public TRectangle Add( (double xOffset, double yOffset)      other ) => TRectangle.Create(self.Location, self.Size + other);
        public TRectangle Subtract( (double xOffset, double yOffset) other ) => TRectangle.Create(self.Location, self.Size - other);
        public TRectangle Divide( (double xOffset, double yOffset)   other ) => TRectangle.Create(self.Location, self.Size / other);
        public TRectangle Multiply( (double xOffset, double yOffset) other ) => TRectangle.Create(self.Location, self.Size * other);

        public TRectangle Add( double      other ) => TRectangle.Create(self.X, self.Y, self.Width + other, self.Height + other);
        public TRectangle Subtract( double other ) => TRectangle.Create(self.X, self.Y, self.Width - other, self.Height - other);
        public TRectangle Multiply( double other ) => TRectangle.Create(self.X, self.Y, self.Width * other, self.Height * other);
        public TRectangle Divide( double   other ) => TRectangle.Create(self.X, self.Y, self.Width / other, self.Height / other);
        public TRectangle Add( float       other ) => TRectangle.Create(self.X, self.Y, self.Width + other, self.Height + other);
        public TRectangle Subtract( float  other ) => TRectangle.Create(self.X, self.Y, self.Width - other, self.Height - other);
        public TRectangle Divide( float    other ) => TRectangle.Create(self.X, self.Y, self.Width / other, self.Height / other);
        public TRectangle Multiply( float  other ) => TRectangle.Create(self.X, self.Y, self.Width * other, self.Height * other);
        public TRectangle Add( int         other ) => TRectangle.Create(self.X, self.Y, self.Width + other, self.Height + other);
        public TRectangle Subtract( int    other ) => TRectangle.Create(self.X, self.Y, self.Width - other, self.Height - other);
        public TRectangle Divide( int      other ) => TRectangle.Create(self.X, self.Y, self.Width / other, self.Height / other);
        public TRectangle Multiply( int    other ) => TRectangle.Create(self.X, self.Y, self.Width * other, self.Height * other);
    }
}
