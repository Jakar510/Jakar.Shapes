// Jakar.Extensions :: Jakar.Shapes
// 09/29/2025  09:01

namespace Jakar.Shapes;


public static class Lines
{
    extension<TLine>( TLine self )
        where TLine : struct, ILine<TLine>
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
                    return $"{self.Start.ToString(format, null)},{self.End.ToString(format, null)}";

                case "-":
                    return $"{self.Start.ToString(format, null)}-{self.End.ToString(format, null)}";

                case EMPTY:
                case null:
                default:
                    return $"{typeof(TLine).Name}<{nameof(self.Start)}: {self.Start}, {nameof(self.End)}: {self.End}, {nameof(IsFinite)}: {self.IsFinite}>";
            }
        }
     
        public void Deconstruct( out ReadOnlyPoint start, out ReadOnlyPoint end, out bool isFinite )
        {
            start    = self.Start;
            end      = self.End;
            isFinite = self.IsFinite;
        }
        public void Deconstruct( out float x1, out float y1, out float x2, out float y2 )
        {
            ReadOnlyPoint start = self.Start;
            ReadOnlyPoint end   = self.End;
            x1 = (float)start.X;
            y1 = (float)start.Y;
            x2 = (float)end.X;
            y2 = (float)end.Y;
        }
        public void Deconstruct( out double x1, out double y1, out double x2, out double y2 )
        {
            ReadOnlyPoint start = self.Start;
            ReadOnlyPoint end   = self.End;
            x1 = start.X;
            y1 = start.Y;
            x2 = end.X;
            y2 = end.Y;
        }
        public void Deconstruct( out ReadOnlyPointF start, out ReadOnlyPointF end )
        {
            start = self.Start;
            end   = self.End;
        }
        public void Deconstruct( out ReadOnlyPoint start, out ReadOnlyPoint end )
        {
            start = self.Start;
            end   = self.End;
        }
      
        [Pure] public TLine         Round()      => TLine.Create(self.Start.Round(), self.End.Round(), self.IsFinite);
        [Pure] public TLine         Floor()      => TLine.Create(self.Start.Floor(), self.End.Floor(), self.IsFinite);
        public        ReadOnlyPoint Center()     => new(( self.Start.Y + self.End.Y ) / 2, ( self.Start.X + self.End.X ) / 2);
        public        double        Slope()      => ( self.End.Y - self.Start.Y ) / ( self.End.X - self.Start.X );
        public        TLine         Abs()        => TLine.Create(self.Start.Abs(), self.End.Abs(), self.IsFinite);
        public        bool          IsFinite()   => self.Start.IsFinite() && self.End.IsFinite();
        public        bool          IsInfinity() => self.Start.IsInfinity() || self.End.IsInfinity();
        public        bool          IsInteger()  => self.Start.IsInteger()  || self.End.IsInteger();
        public        bool          IsNaN()      => self.Start.IsNaN()      || self.End.IsNaN();
        public        bool          IsNegative() => self.Start.IsNegative() || self.End.IsNegative();
        public        bool          IsValid()    => self.Start.IsValid()    || self.End.IsValid();
        public        bool          IsPositive() => self.Start.IsNaN()      || self.End.IsNaN();
        public        bool          IsZero()     => self.Start.IsZero()     || self.End.IsZero();


        public TLine Add<TOther>( TOther value )
            where TOther : struct, ILine<TOther> => TLine.Create(self.Start + value.Start, self.End + value.End);
        public TLine Subtract<TOther>( TOther value )
            where TOther : struct, ILine<TOther> => TLine.Create(self.Start - value.Start, self.End - value.End);
        public TLine Multiply<TOther>( TOther value )
            where TOther : struct, ILine<TOther> => TLine.Create(self.Start * value.Start, self.End * value.End);
        public TLine Divide<TOther>( TOther value )
            where TOther : struct, ILine<TOther> => TLine.Create(self.Start / value.Start, self.End / value.End);

        public TLine Add( (int xOffset, int yOffset)            value ) => TLine.Create(self.Start + value.xOffset, self.End + value.yOffset);
        public TLine Subtract( (int xOffset, int yOffset)       value ) => TLine.Create(self.Start - value.xOffset, self.End - value.yOffset);
        public TLine Divide( (int xOffset, int yOffset)         value ) => TLine.Create(self.Start / value.xOffset, self.End / value.yOffset);
        public TLine Multiply( (int xOffset, int yOffset)       value ) => TLine.Create(self.Start * value.xOffset, self.End * value.yOffset);
        public TLine Add( (float xOffset, float yOffset)        value ) => TLine.Create(self.Start + value.xOffset, self.End + value.yOffset);
        public TLine Multiply( (float xOffset, float yOffset)   value ) => TLine.Create(self.Start * value.xOffset, self.End * value.yOffset);
        public TLine Divide( (float xOffset, float yOffset)     value ) => TLine.Create(self.Start / value.xOffset, self.End / value.yOffset);
        public TLine Subtract( (float xOffset, float yOffset)   value ) => TLine.Create(self.Start - value.xOffset, self.End - value.yOffset);
        public TLine Add( (double xOffset, double yOffset)      value ) => TLine.Create(self.Start + value.xOffset, self.End + value.yOffset);
        public TLine Subtract( (double xOffset, double yOffset) value ) => TLine.Create(self.Start - value.xOffset, self.End - value.yOffset);
        public TLine Divide( (double xOffset, double yOffset)   value ) => TLine.Create(self.Start / value.xOffset, self.End / value.yOffset);
        public TLine Multiply( (double xOffset, double yOffset) value ) => TLine.Create(self.Start * value.xOffset, self.End * value.yOffset);

        public TLine Add( double      value ) => TLine.Create(self.Start + value, self.End + value);
        public TLine Subtract( double value ) => TLine.Create(self.Start - value, self.End - value);
        public TLine Multiply( double value ) => TLine.Create(self.Start * value, self.End * value);
        public TLine Divide( double   value ) => TLine.Create(self.Start / value, self.End / value);
        public TLine Add( float       value ) => TLine.Create(self.Start + value, self.End + value);
        public TLine Subtract( float  value ) => TLine.Create(self.Start - value, self.End - value);
        public TLine Divide( float    value ) => TLine.Create(self.Start / value, self.End / value);
        public TLine Multiply( float  value ) => TLine.Create(self.Start * value, self.End * value);
        public TLine Add( int         value ) => TLine.Create(self.Start + value, self.End + value);
        public TLine Subtract( int    value ) => TLine.Create(self.Start - value, self.End - value);
        public TLine Divide( int      value ) => TLine.Create(self.Start / value, self.End / value);
        public TLine Multiply( int    value ) => TLine.Create(self.Start * value, self.End * value);
    }



    public static TLine WithStart<TLine, TOther>( this TLine self, TOther value )
        where TLine : struct, ILine<TLine, TOther>
        where TOther : struct, IPoint<TOther> => TLine.Create(value, self.End);
    public static TLine WithEnd<TLine, TOther>( this TLine self, TOther value )
        where TLine : struct, ILine<TLine, TOther>
        where TOther : struct, IPoint<TOther> => TLine.Create(self.Start, value);
}
