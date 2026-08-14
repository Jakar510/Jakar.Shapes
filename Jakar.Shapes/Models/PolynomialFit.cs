// Jakar.Extensions :: Jakar.Shapes
// 08/13/2026

using System.Text;



namespace Jakar.Shapes;


/// <summary>
/// The result of a <see cref="LineOfBestFit"/> regression: the fitted coefficients, the degree that was used, and the
/// residual error. Coefficients are stored in ASCENDING order, so <c> Coefficients[k] </c> multiplies <c> x^k </c>
/// (or <c> x^-k </c> when <see cref="Degree"/> is negative).
/// </summary>
[DefaultValue(nameof(Invalid))]
public readonly struct PolynomialFit
{
    public static readonly PolynomialFit Invalid = new();
    private readonly       double[]?     __coefficients;

    /// <summary> Highest power in the fitted equation. Negative values denote a Laurent fit in <c> x^-k </c>. </summary>
    public readonly sbyte Degree;

    /// <summary> Residual sum of squared error over the points that were fitted. </summary>
    public readonly double SumOfSquaredError;


    /// <summary> Ascending coefficients: index <c> k </c> multiplies <c> x^k </c>, or <c> x^-k </c> when <see cref="Degree"/> is negative. </summary>
    public ReadOnlySpan<double> Coefficients => __coefficients;

    public bool IsValid => __coefficients is { Length: > 0 };

    /// <summary> Number of terms in the equation, always <c> |Degree| + 1 </c>. </summary>
    public int Length => __coefficients?.Length ?? 0;


    public PolynomialFit() : this(null, 0, double.PositiveInfinity) { }
    internal PolynomialFit( double[]? coefficients, sbyte degree, double sumOfSquaredError )
    {
        __coefficients    = coefficients;
        Degree            = degree;
        SumOfSquaredError = sumOfSquaredError;
    }


    /// <summary> Evaluates the fitted equation at <paramref name="x"/> via Horner's method. </summary>
    public double this[ double x ]
    {
        [Pure] get
        {
            double[]? coefficients = __coefficients;
            if ( coefficients is null || coefficients.Length is 0 ) { return double.NaN; }

            double t = Degree < 0
                           ? 1.0 / x
                           : x;

            double result = coefficients[^1];
            for ( int i = coefficients.Length - 2; i >= 0; i-- ) { result = result * t + coefficients[i]; }

            return result;
        }
    }


    [Pure] public CalculatedLine ToCalculatedLine()
    {
        if ( !IsValid ) { return CalculatedLine.Invalid; }

        PolynomialFit self = this;
        return CalculatedLine.Create(x => self[x]);
    }
    public static implicit operator CalculatedLine( PolynomialFit fit ) => fit.ToCalculatedLine();


    /// <summary> Renders the equation in descending order, e.g. <c> 2x^3 - 5x^2 + 3x + 7 </c>. </summary>
    public override string ToString()
    {
        double[]? coefficients = __coefficients;
        if ( coefficients is null || coefficients.Length is 0 ) { return $"{nameof(PolynomialFit)}<{nameof(Invalid)}>"; }

        StringBuilder sb      = new();
        bool          negated = Degree < 0;

        for ( int k = coefficients.Length - 1; k >= 0; k-- )
        {
            double value = coefficients[k];
            if ( value is 0 ) { continue; }

            if ( sb.Length > 0 )
            {
                sb.Append(value < 0
                              ? " - "
                              : " + ");
            }
            else if ( value < 0 ) { sb.Append('-'); }

            double magnitude = Math.Abs(value);
            if ( magnitude is not 1 || k is 0 ) { sb.Append(magnitude.ToString("G6")); }

            if ( k is 0 ) { continue; }

            sb.Append('x');

            switch ( k )
            {
                case 1 when !negated:
                    break;

                case 1:
                    sb.Append("^-1");
                    break;

                default:
                    sb.Append("^")
                      .Append(negated
                                  ? -k
                                  : k);

                    break;
            }
        }

        return sb.Length is 0
                   ? "0"
                   : sb.ToString();
    }
}
