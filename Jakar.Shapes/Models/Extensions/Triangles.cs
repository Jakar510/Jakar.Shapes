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
        public double        Area()       => Math.Abs(0.5 * ( self.B.X - self.A.X ) * ( self.C.Y - self.A.Y ) - ( self.C.X - self.A.X ) * ( self.B.Y - self.A.Y ));
        public ReadOnlyPoint Centroid()   => new(( self.A.X + self.B.X + self.C.X ) / 3, ( self.A.Y + self.B.Y + self.C.Y ) / 3);
        public Degrees       Abc()        => new(self.A.AngleBetween(self.B, self.C));
        public Degrees       Bac()        => new(self.B.AngleBetween(self.A, self.C));
        public Degrees       Cab()        => new(self.C.AngleBetween(self.A, self.B));
        public TTriangle     Abs()        => TTriangle.Create(self.A.Abs(), self.B.Abs(), self.C.Abs());
        public bool          IsFinite()   => self.A.IsFinite() && self.B.IsFinite() && self.C.IsFinite();
        public bool          IsInfinity() => self.A.IsInfinity() || self.B.IsInfinity() || self.C.IsInfinity();
        public bool          IsInteger()  => self.A.IsInteger() && self.B.IsInteger() && self.C.IsInteger();
        public bool          IsNaN()      => self.A.IsNaN()      || self.B.IsNaN()      || self.C.IsNaN();
        public bool          IsNegative() => self.A.IsNegative() || self.B.IsNegative() || self.C.IsNegative();
        public bool          IsValid()    => !self.IsNaN() && self.IsFinite() && !( self.A.IsOneOf(self.B, self.C) || self.B.IsOneOf(self.A, self.C) || self.C.IsOneOf(self.A, self.B) );
        public bool          IsPositive() => self.A.IsPositive() || self.B.IsPositive() || self.C.IsPositive();
        public bool          IsZero()     => self.A.IsZero()     || self.B.IsZero()     || self.C.IsZero();
      
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
      
        public TTriangle Add( double                                value ) => TTriangle.Create(self.A + value, self.B + value, self.C + value);
        public TTriangle Subtract( double                           value ) => TTriangle.Create(self.A - value, self.B - value, self.C - value);
        public TTriangle Multiply( double                           value ) => TTriangle.Create(self.A / value, self.B / value, self.C / value);
        public TTriangle Divide( double                             value ) => TTriangle.Create(self.A / value, self.B / value, self.C / value);
        public TTriangle Add( float                                 value ) => TTriangle.Create(self.A + value, self.B + value, self.C + value);
        public TTriangle Subtract( float                            value ) => TTriangle.Create(self.A - value, self.B - value, self.C - value);
        public TTriangle Divide( float                              value ) => TTriangle.Create(self.A / value, self.B / value, self.C / value);
        public TTriangle Multiply( float                            value ) => TTriangle.Create(self.A * value, self.B * value, self.C * value);
        public TTriangle Add( int                                   value ) => TTriangle.Create(self.A + value, self.B + value, self.C + value);
        public TTriangle Subtract( int                              value ) => TTriangle.Create(self.A - value, self.B - value, self.C - value);
        public TTriangle Divide( int                                value ) => TTriangle.Create(self.A / value, self.B / value, self.C / value);
        public TTriangle Multiply( int                              value ) => TTriangle.Create(self.A * value, self.B * value, self.C * value);
    }
}
