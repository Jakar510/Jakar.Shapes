// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

namespace Jakar.Shapes.Interfaces;


/// <summary>
/// A four-sided shape given by its vertices in order, so that AB, BC, CD and DA are the edges.
/// <para>
/// The specialisations (Square, Rhombus, Trapezoid, Kite, Parallelogram) are distinguished by name rather than by
/// invariant -- nothing stops a <see cref="Square"/> holding four arbitrary points. Use the matching predicate
/// (IsSquare, IsRhombus, IsTrapezoid, IsKite, IsParallelogram) when the classification has to hold.
/// </para>
/// </summary>
public interface IQuadrilateral<TSelf> : IShape<TSelf>, IShapeLocation
    where TSelf : struct, IQuadrilateral<TSelf>
{
    ReadOnlyPoint A { get; }
    ReadOnlyPoint B { get; }
    ReadOnlyPoint C { get; }
    ReadOnlyPoint D { get; }


    [Pure] public abstract static TSelf Create( ReadOnlyPoint a, ReadOnlyPoint b, ReadOnlyPoint c, ReadOnlyPoint d );
}
