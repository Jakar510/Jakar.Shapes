// Jakar.Extensions :: Jakar.Shapes
// 09/29/2025  10:07

using ZLinq;



namespace Jakar.Shapes;


public static class Splines
{
    extension<TSpline>( TSpline self )
        where TSpline : struct, ISpline<TSpline>
    {
        public ReadOnlyPoint Center() => new(self.AsValueEnumerable()
                                                 .Sum(static x => x.X) /
                                             self.Length,
                                             self.AsValueEnumerable()
                                                 .Sum(static x => x.Y) /
                                             self.Length);

        public TSpline Abs() => TSpline.Create(self.AsValueEnumerable()
                                                   .Select(static x => x.Abs())
                                                   .ToArray());

        public bool IsFinite() => self.AsValueEnumerable()
                                      .All(static x => x.IsFinite());
        public bool IsInfinity() => self.AsValueEnumerable()
                                        .Any(static x => x.IsInfinity());
        public bool IsInteger() => self.AsValueEnumerable()
                                       .All(static x => x.IsInteger());
        public bool IsNaN() => self.AsValueEnumerable()
                                   .Any(static x => x.IsNaN());
        public bool IsNegative() => self.AsValueEnumerable()
                                        .Any(static x => x.IsNegative());
        public bool IsValid() => self.AsValueEnumerable()
                                     .All(static x => x.IsValid());
        public bool IsPositive() => self.AsValueEnumerable()
                                        .Any(static x => x.IsNaN());
        public bool IsZero() => self.AsValueEnumerable()
                                    .Any(static x => x.IsZero());
    }
}
