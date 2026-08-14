// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

namespace Jakar.Shapes.Interfaces;


/// <summary> An axis-aligned ellipse. A circle is the case where <see cref="RadiusX"/> equals <see cref="RadiusY"/>. </summary>
public interface IEllipse<TSelf> : IShape<TSelf>, IShapeLocation
    where TSelf : struct, IEllipse<TSelf>
{
    ReadOnlyPoint Center  { get; }
    double        RadiusX { get; }
    double        RadiusY { get; }


    [Pure] public abstract static TSelf Create( in ReadOnlyPoint center, double radiusX, double radiusY );
}
