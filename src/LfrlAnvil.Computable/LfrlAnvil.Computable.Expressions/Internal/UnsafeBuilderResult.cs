using System.Diagnostics;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Errors;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal readonly struct UnsafeBuilderResult<T>
{
    private UnsafeBuilderResult(T? result, Chain<ParsedExpressionBuilderError> errors)
    {
        Result = result;
        Errors = errors;
    }

    internal T? Result { get; }
    internal Chain<ParsedExpressionBuilderError> Errors { get; }
    internal bool IsOk => Errors.Count == 0;

    [Pure]
    internal UnsafeBuilderResult<TOther> CastErrorsTo<TOther>()
    {
        Debug.Assert( ! IsOk, "Result doesn't contain any errors." );
        return UnsafeBuilderResult<TOther>.CreateErrors( Errors );
    }

    [Pure]
    internal static UnsafeBuilderResult<T> CreateOk(T result)
    {
        return new UnsafeBuilderResult<T>( result, Chain<ParsedExpressionBuilderError>.Empty );
    }

    [Pure]
    internal static UnsafeBuilderResult<T> CreateErrors(ParsedExpressionBuilderError error)
    {
        return CreateErrors( Chain.Create( error ) );
    }

    [Pure]
    internal static UnsafeBuilderResult<T> CreateErrors(Chain<ParsedExpressionBuilderError> errors)
    {
        Debug.Assert( errors.Count > 0, "Errors chain cannot be empty." );
        return new UnsafeBuilderResult<T>( default, errors );
    }
}
