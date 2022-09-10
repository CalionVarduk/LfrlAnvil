using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderParameterCountError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderParameterCountError(
        ParsedExpressionBuilderErrorType type,
        StringSlice token,
        int actual,
        int expected)
        : base( type, token )
    {
        ActualParameterCount = actual;
        ExpectedParameterCount = expected;
    }

    public int ActualParameterCount { get; }
    public int ExpectedParameterCount { get; }

    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, actual: {ActualParameterCount}, expected: {ExpectedParameterCount}";
    }
}
