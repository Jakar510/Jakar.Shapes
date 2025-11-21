// Jakar.Extensions :: Jakar.Shapes
// 09/29/2025  09:01

namespace Jakar.Shapes;


public static class Sizes
{
    extension<TSize>( TSize self )
        where TSize : struct, ISize<TSize>
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
                    return $"{self.Width},{self.Height}";

                case "-":
                    return $"{self.Width}-{self.Height}";

                case EMPTY:
                case null:
                default:
                    return $"{typeof(TSize).Name}<{nameof(self.Width)}: {self.Width}, {nameof(self.Height)}: {self.Height}>";
            }
        }

        [Pure] public TSize Reverse() => TSize.Create(self.Height,        self.Width);
        [Pure] public TSize Round()   => TSize.Create(self.Width.Round(), self.Height.Round());
        [Pure] public TSize Floor()   => TSize.Create(self.Width.Floor(), self.Height.Floor());

        public void Deconstruct( out float width, out float height )
        {
            width  = (float)self.Width;
            height = (float)self.Height;
        }
        public void Deconstruct( out double width, out double height )
        {
            width  = self.Width;
            height = self.Height;
        }

        public bool  IsPortrait()  => self.Width > self.Height;
        public bool  IsLandscape() => self.Width > self.Height;
        public TSize Abs()         => TSize.Create(double.Abs(self.Width), double.Abs(self.Height));
        public bool  IsFinite()    => double.IsFinite(self.Width) && double.IsFinite(self.Height);
        public bool  IsInfinity()  => double.IsInfinity(self.Width) || double.IsInfinity(self.Height);
        public bool  IsInteger()   => double.IsInteger(self.Width) && double.IsInteger(self.Height);
        public bool  IsNaN()       => double.IsNaN(self.Width) || double.IsNaN(self.Height);
        public bool  IsNegative()  => self is { Width: < 0, Height: < 0 };
        public bool  IsValid()     => !self.IsNaN() && self.IsFinite() && self.IsPositive();
        public bool  IsPositive()  => self is { Width: > 0, Height: > 0 };
        public bool  IsZero()      => self is { Width: 0, Height  : 0 };

        public TSize Add( Size       value ) => TSize.Create(self.Width + value.Width, self.Height + value.Height);
        public TSize Subtract( Size  value ) => TSize.Create(self.Width - value.Width, self.Height - value.Height);
        public TSize Multiply( Size  value ) => TSize.Create(self.Width * value.Width, self.Height * value.Height);
        public TSize Divide( Size    value ) => TSize.Create(self.Width / value.Width, self.Height / value.Height);
        public TSize Add( SizeF      value ) => TSize.Create(self.Width + value.Width, self.Height + value.Height);
        public TSize Subtract( SizeF value ) => TSize.Create(self.Width - value.Width, self.Height - value.Height);
        public TSize Multiply( SizeF value ) => TSize.Create(self.Width * value.Width, self.Height * value.Height);
        public TSize Divide( SizeF   value ) => TSize.Create(self.Width / value.Width, self.Height / value.Height);

        public TSize Add<TOther>( TOther value )
            where TOther : struct, ISize<TOther> => TSize.Create(self.Width + value.Width, self.Height + value.Height);
        public TSize Subtract<TOther>( TOther value )
            where TOther : struct, ISize<TOther> => TSize.Create(self.Width - value.Width, self.Height - value.Height);
        public TSize Multiply<TOther>( TOther value )
            where TOther : struct, ISize<TOther> => TSize.Create(self.Width * value.Width, self.Height * value.Height);
        public TSize Divide<TOther>( TOther value )
            where TOther : struct, ISize<TOther> => TSize.Create(self.Width / value.Width, self.Height / value.Height);

        public TSize Add( (int xOffset, int yOffset)            value ) => TSize.Create(self.Width + value.xOffset, self.Height + value.yOffset);
        public TSize Subtract( (int xOffset, int yOffset)       value ) => TSize.Create(self.Width - value.xOffset, self.Height - value.yOffset);
        public TSize Divide( (int xOffset, int yOffset)         value ) => TSize.Create(self.Width / value.xOffset, self.Height / value.yOffset);
        public TSize Multiply( (int xOffset, int yOffset)       value ) => TSize.Create(self.Width * value.xOffset, self.Height * value.yOffset);
        public TSize Add( (float xOffset, float yOffset)        value ) => TSize.Create(self.Width + value.xOffset, self.Height + value.yOffset);
        public TSize Multiply( (float xOffset, float yOffset)   value ) => TSize.Create(self.Width * value.xOffset, self.Height * value.yOffset);
        public TSize Divide( (float xOffset, float yOffset)     value ) => TSize.Create(self.Width / value.xOffset, self.Height / value.yOffset);
        public TSize Subtract( (float xOffset, float yOffset)   value ) => TSize.Create(self.Width - value.xOffset, self.Height - value.yOffset);
        public TSize Add( (double xOffset, double yOffset)      value ) => TSize.Create(self.Width + value.xOffset, self.Height + value.yOffset);
        public TSize Subtract( (double xOffset, double yOffset) value ) => TSize.Create(self.Width - value.xOffset, self.Height - value.yOffset);
        public TSize Divide( (double xOffset, double yOffset)   value ) => TSize.Create(self.Width / value.xOffset, self.Height / value.yOffset);
        public TSize Multiply( (double xOffset, double yOffset) value ) => TSize.Create(self.Width * value.xOffset, self.Height * value.yOffset);

        public TSize Add( double      value ) => TSize.Create(self.Width + value, self.Height + value);
        public TSize Subtract( double value ) => TSize.Create(self.Width - value, self.Height - value);
        public TSize Multiply( double value ) => TSize.Create(self.Width * value, self.Height * value);
        public TSize Divide( double   value ) => TSize.Create(self.Width / value, self.Height / value);
        public TSize Add( float       value ) => TSize.Create(self.Width + value, self.Height + value);
        public TSize Subtract( float  value ) => TSize.Create(self.Width - value, self.Height - value);
        public TSize Divide( float    value ) => TSize.Create(self.Width / value, self.Height / value);
        public TSize Multiply( float  value ) => TSize.Create(self.Width * value, self.Height * value);
        public TSize Add( int         value ) => TSize.Create(self.Width + value, self.Height + value);
        public TSize Subtract( int    value ) => TSize.Create(self.Width - value, self.Height - value);
        public TSize Divide( int      value ) => TSize.Create(self.Width / value, self.Height / value);
        public TSize Multiply( int    value ) => TSize.Create(self.Width * value, self.Height * value);
    }
}
