// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

namespace Jakar.Shapes.Interfaces;


/// <summary>
/// An equilateral, equiangular polygon described by its centre, circumradius (centre to vertex) and rotation.
/// <see cref="SideCount"/> is fixed per type, so Pentagon and Hexagon are distinct types you can overload on.
/// </summary>
public interface IRegularPolygon<TSelf> : IShape<TSelf>, IShapeLocation
    where TSelf : struct, IRegularPolygon<TSelf>
{
    /// <summary> Distance from the centre to any vertex. </summary>
    double Circumradius { get; }

    ReadOnlyPoint Center { get; }

    /// <summary> Rotation applied about the centre; zero places the first vertex along +X. </summary>
    Radians Rotation { get; }

    /// <summary> Number of sides. Fixed per implementing type. </summary>
    public abstract static int SideCount { get; }


    [Pure] public abstract static TSelf Create( in ReadOnlyPoint center, double circumradius, Radians rotation );
    [Pure] public abstract static TSelf Create( in ReadOnlyPoint center, double circumradius );
}
