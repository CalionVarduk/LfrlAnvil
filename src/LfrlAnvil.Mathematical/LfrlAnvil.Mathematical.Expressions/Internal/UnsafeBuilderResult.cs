using System.Diagnostics;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mathematical.Expressions.Errors;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

internal readonly struct UnsafeBuilderResult<T>
{
    private UnsafeBuilderResult(T? result, Chain<MathExpressionBuilderError> errors)
    {
        Result = result;
        Errors = errors;
    }

    internal T? Result { get; }
    internal Chain<MathExpressionBuilderError> Errors { get; }
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
        return new UnsafeBuilderResult<T>( result, Chain<MathExpressionBuilderError>.Empty );
    }

    [Pure]
    internal static UnsafeBuilderResult<T> CreateErrors(MathExpressionBuilderError error)
    {
        return CreateErrors( Chain.Create( error ) );
    }

    [Pure]
    internal static UnsafeBuilderResult<T> CreateErrors(Chain<MathExpressionBuilderError> errors)
    {
        Debug.Assert( errors.Count > 0, "Errors chain cannot be empty." );
        return new UnsafeBuilderResult<T>( default, errors );
    }
}
