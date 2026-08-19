# Jakar.Shapes

A dependency-light 2D geometry library for .NET. Every shape is a `struct`, every operation is
`[Pure]`, and the shared behaviour lives on generic interfaces with `static abstract` members so
algorithms can be written once against `IShape<TSelf>` and reused for every model.

**Contributions and ideas are welcome.**

- **Target framework:** `net10.0` (C# 14 — uses the new `extension` member blocks)
- **Dependencies:** [`Jakar.Extensions`](https://www.nuget.org/packages/Jakar.Extensions/), [`ZLinq`](https://www.nuget.org/packages/ZLinq/)
- **License:** MIT

## Installation

```sh
dotnet add package Jakar.Shapes
```

---

## Design

| Idea | What it means in practice |
| --- | --- |
| Value types only | Every model is a `readonly struct` (except `MutableRectangle` / `MutableSize`). No allocation for the shapes themselves. |
| `static abstract` factories | Each interface declares `Create(...)`, `Invalid`, `Zero`, `One`, and the arithmetic operators, so generic code can construct and combine shapes without a factory delegate. |
| Extension blocks | Shared geometry (area, perimeter, hit-testing, transforms) is written **once** per interface inside a C# 14 `extension<TShape>( TShape self )` block and applies to every implementer. |
| JSON built in | Shapes implement `IJsonModel<TSelf>` from `Jakar.Extensions` — `ToJson()`, `FromJson(...)`, `TryFromJson(...)`. |
| Validity over exceptions | `Invalid` is `NaN`-filled rather than null; check `IsValid` / `IsNaN` / `IsFinite` instead of catching. |
| Interop | Implicit conversions to and from `System.Drawing`'s `Point`, `PointF`, `Size`, `SizeF`, `Rectangle`, `RectangleF`, plus scalar promotion (`ReadOnlyPoint p = 5;`). |

### Interface hierarchy

```
IShape<TSelf>  : IValidator, IFormattable, IJsonModel<TSelf>, IEqualComparable<TSelf>
  ├── IPoint<TSelf>            + IShapeLocation
  ├── ISize<TSelf>             + IShapeSize
  ├── IThickness<TSelf>
  ├── ILine<TSelf, TPoint>     (+ ILine<TSelf> bound to ReadOnlyPoint)
  ├── ISpline<TSelf, TPoint>   + IStructuralComparable<TSelf>, IValueEnumerable<…>
  ├── IRectangle<TSelf>        + IShapeSize, IShapeLocation
  │     └── IMutableRectangle<TSelf>
  ├── ICircle<TSelf>           + IShapeLocation
  ├── IEllipse<TSelf>          + IShapeLocation
  ├── ITriangle<TSelf>         + IShapeLocation
  ├── IQuadrilateral<TSelf>    + IShapeLocation
  └── IRegularPolygon<TSelf>   + IShapeLocation
```

`IShapeLocation` supplies `Location`, `X`, `Y`; `IShapeSize` supplies `Size`, `Width`, `Height`.

---

## Models

### Primitives

| Type | Shape | Notes |
| --- | --- | --- |
| `ReadOnlyPoint` | `readonly struct (double x, double y)` | Canonical point. Cached constants `Zero`…`Ten` and `NegativeOne`…`NegativeTen`. Converts to/from `Point`, `PointF`, `ReadOnlyPointF`. |
| `ReadOnlyPointF` | `readonly struct (float x, float y)` | Single-precision counterpart. |
| `ReadOnlySize` | `readonly struct (double width, double height)` | Converts to/from `Size`, `SizeF`, `ReadOnlySizeF`. |
| `ReadOnlySizeF` | `readonly struct (float width, float height)` | Single-precision counterpart. |
| `MutableSize` | `struct (double width, double height)` | Settable `Width` / `Height` for layout code. |
| `ReadOnlyThickness` | `readonly struct (left, top, right, bottom)` | Margins/padding. Exposes `HorizontalThickness`, `VerticalThickness`, and `Deconstruct` to 4 or 2 values. |
| `Degrees` | `readonly record struct (double Value)` | Auto-normalised to `[0, 360)`. Implicit to/from `Radians` and `double`. `Angles` caches 0–359. |
| `Radians` | `readonly record struct (double Value)` | Auto-normalised to `[0, 2π)`. Implicit to/from `Degrees` and `double`. `NearlyEquals(other, tolerance)`. |

### Rectangles

| Type | Notes |
| --- | --- |
| `ReadOnlyRectangle` | Immutable `double` rectangle (`X`, `Y`, `Width`, `Height`). |
| `ReadOnlyRectangleF` | Immutable `float` rectangle. |
| `MutableRectangle` | Settable `X`/`Y`/`Width`/`Height`, plus `AddMargin(in ReadOnlyThickness)`, `Round()`, `Floor()`, `Reverse()`. Implements the full `IMutableRectangle<TSelf>` operator set (arithmetic against rectangles, sizes, points, and thicknesses). |

### Lines and curves

| Type | Notes |
| --- | --- |
| `ReadOnlyLine` | `Start` / `End` segment with an `IsFinite` flag (a `false` value models an infinite line through the two points). `Length`, `Slope`, `Center`. |
| `CalculatedLine` | A line defined by a `Func<double, double>` rather than endpoints. Built via `Create`, `CreateNoIntercept`, `CreateWithIntercept`, `CreateWithLog`; sample with `Get(x)` or materialise with `ToSpline(...)`. |
| `Spline` | `params ReadOnlyPoint[]` polyline. Indexable by `int`, `Index`, and `Range`; enumerable through ZLinq; `Round()`, `Floor()`, structural comparison. |
| `Polygon` | Same storage and API as `Spline`, but semantically closed. Produced by `Circle.ToPolygon(...)` and `IRegularPolygon.ToPolygon()`. |

### Curved shapes

| Type | Notes |
| --- | --- |
| `Circle` | `Center` + `Radius`. Implicitly constructible from a point or a scalar radius. |
| `Ellipse` | Axis-aligned `Center` + `RadiusX` + `RadiusY`. A circle is the `RadiusX == RadiusY` case. |

### Triangles and quadrilaterals

| Type | Vertices | Notes |
| --- | --- | --- |
| `Triangle` | `A`, `B`, `C` | Edges `Ab`/`Bc`/`Ca`, angles `Abc`/`Bac`/`Cab`, inscribed and circumscribed circles. |
| `Square`, `Rhombus`, `Trapezoid`, `Kite`, `Parallelogram` | `A`, `B`, `C`, `D` | All implement `IQuadrilateral<TSelf>`. |

> The quadrilateral specialisations are distinguished **by name, not by invariant** — nothing stops a
> `Square` holding four arbitrary points. When the classification has to hold, call the matching
> predicate: `IsSquare`, `IsRhombus`, `IsRectangle`, `IsTrapezoid`, `IsKite`, `IsParallelogram`, `IsConvex`.

### Regular polygons

Each is a `readonly struct (ReadOnlyPoint center, double circumradius, Radians rotation)` implementing
`IRegularPolygon<TSelf>`, with `SideCount` fixed per type so you can overload on the shape:

| Type | `SIDES` | | Type | `SIDES` |
| --- | --- | --- | --- | --- |
| `Pentagon` | 5 | | `Octagon` | 8 |
| `Hexagon` | 6 | | `Nonagon` | 9 |
| `Heptagon` | 7 | | `Decagon` | 10 |

`Rotation` is applied about the centre; zero places the first vertex along `+X`.

### Fitting

| Type | Notes |
| --- | --- |
| `LineOfBestFit` | Static entry point. `Fit(points, primaryPower = null)` returns a `PolynomialFit`; `Calculate(...)` returns an evaluable `CalculatedLine` directly. Accepts a `ReadOnlySpan<ReadOnlyPoint>` or a `Spline`. With `primaryPower: null` it searches degrees `0, 1, -1, 2, -2, …` so simpler equations win ties, using Householder QR with rank-deficiency and rounding-error guards, and pools scratch buffers for large inputs. |
| `PolynomialFit` | Result of a fit: `Coefficients`, `Length`, `IsValid`, `ToCalculatedLine()`. |

### Enumerations

| Type | Values |
| --- | --- |
| `CircleLineRelation` | `Tangent`, `Secant`, `Disjoint` |
| `Quadrant` (`[Flags]`) | `None`, `Bottom`, `Top`, `Left`, `Right`, `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight` |

---

## Extension surface

Because the members live on generic `extension` blocks, everything below works on **any** implementer
of the interface — including your own types.

| Class | Applies to | Highlights |
| --- | --- | --- |
| `Points` | `IPoint<T>` | `DistanceTo`, `Dot`, `Magnitude`, `AngleBetween`, `Reverse`, `Round`, `Floor`, `Abs`, arithmetic helpers. |
| `Sizes` | `ISize<T>` | `IsPortrait`, `IsLandscape`, `Reverse`, `Round`, `Floor`, arithmetic helpers. |
| `Lines` | `ILine<T>` | `Slope`, `Center`, `WithStart`, `WithEnd`, `Round`, `Floor`, arithmetic helpers. |
| `Rectangles` | `IRectangle<T>` | `TopLeft`/`TopRight`/`BottomLeft`/`BottomRight`, the four sides as `ReadOnlyLine`, `Center`, `Centroid`, `Area`, `Perimeter`, `DiagonalLength`, `BoundingBox`, `Union`, `SharedArea`, `IntersectsWith`, `DoesLineIntersect`, `Contains`/`ContainsAny`/`ContainsAll`, `IsAtLeast`, `Scale`, `Grow`, `Translate`. |
| `Circles` | `ICircle<T>` | `Area`, `Circumference`, `Perimeter`, `Diameter`, `BoundingBox`, `Contains`, `Intersects`, `Encloses`, `PointAt`, `PerimeterPoints`, `RadiusLine`, `DiameterLine` (and `CalculatedLine` variants), `GetLineRelation`, `IsTangent`/`IsSecant`/`IsDisjoint`, `Intersections`, `ToPolygon`, `Scale`, `Grow`, `Translate`, `MoveTo`, `Rotate`. |
| `Ellipses` | `IEllipse<T>` | `SemiMajorAxis`/`SemiMinorAxis`, `MajorAxis`/`MinorAxis`, `Eccentricity`, `IsCircle`, `Area`, `Perimeter`, `PointAt`, `Contains`, `Intersects`, transforms. |
| `Triangles` | `ITriangle<T>` | `Ab`/`Bc`/`Ca`, `Abc`/`Bac`/`Cab` angles, `Area`, `Perimeter`, `Centroid`, `BoundingBox`, `Contains`, `Intersects`, `InscribedCircle`, `CircumscribedCircle`, transforms. |
| `Quadrilaterals` | `IQuadrilateral<T>` | Edges `Ab`/`Bc`/`Cd`/`Da`, diagonals `Ac`/`Bd`, `Area`, `Perimeter`, `Centroid`, `BoundingBox`, the classification predicates, `Contains`, `Intersects`, transforms. |
| `RegularPolygons` | `IRegularPolygon<T>` | `Vertices`, `Edges`, `VertexCount`, `DiagonalCount`, `SideLength(s)`, `Apothem`, `DiagonalLengths`, `InteriorAngle`, `ExteriorAngle`, `CentralAngle`, `Angles`, `Area`, `Perimeter`, `InscribedCircle`, `CircumscribedCircle`, `ToPolygon`, transforms. |
| `Splines` | `ISpline<T>` | `Center`, plus the shared validity helpers. |
| `Shapes` | arrays | `Create` helpers for shape arrays. |

Every extension block also carries the common validity set: `IsValid`, `IsNaN`, `IsFinite`,
`IsInfinity`, `IsInteger`, `IsNegative`, `IsPositive`, `IsZero`, plus `Abs`, `Round`, `Floor`,
`Deconstruct`, and a `ToString(format)` accepting `"json"`, `","`, `"-"`, or `null`.

---

## Usage

```csharp
using Jakar.Shapes;

// Points, sizes and rectangles interop with System.Drawing and with scalars.
ReadOnlyPoint     a    = new(3, 4);
ReadOnlyPoint     b    = 10;                         // (10, 10)
double            dist = a.DistanceTo(b);
ReadOnlyRectangle rect = ReadOnlyRectangle.Create(0, 0, 100, 50);
bool              hit  = rect.Contains(a);
ReadOnlyLine      top  = rect.TopSide;

// Circles.
Circle circle = Circle.Create(new ReadOnlyPoint(0, 0), 5);
double area   = circle.Area;
Polygon approx = circle.ToPolygon(64);
CircleLineRelation relation = circle.GetLineRelation(top);

// Regular polygons — rotation in radians, angles reported in degrees.
Hexagon hex     = Hexagon.Create(ReadOnlyPoint.Zero, circumradius: 10, rotation: Radians.Zero);
Degrees interior = hex.InteriorAngle;                // 120°
Circle  inner    = hex.InscribedCircle;

// Quadrilaterals classify on demand.
Square square = Square.Create(new(0, 0), new(4, 0), new(4, 4), new(0, 4));
bool   real   = square.IsSquare;

// Curve fitting.
Spline         points = Spline.Create(new ReadOnlyPoint(0, 1), new(1, 3), new(2, 7), new(3, 13));
PolynomialFit  fit    = LineOfBestFit.Fit(points.Span);
CalculatedLine line   = fit.ToCalculatedLine();
ReadOnlyPoint  at5    = line.Get(5);

// JSON round-trip.
string json  = circle.ToJson();
Circle again = Circle.FromJson(json);
```

---

## Repository layout

```
Jakar.Shapes/
  Enumerations/   CircleLineRelation, Quadrant
  Interfaces/     IShape and friends, RefSelect delegates
  Geometry/       the concrete models
  Extensions/     the generic extension blocks
Jakar.Shapes.Tests/          unit tests
Jakar.Shapes.Experiments/    scratch console project
```

## License

MIT — see [LICENSE.txt](./LICENSE.txt).

Contributions are welcome; please open an issue or submit a pull request on GitHub.
