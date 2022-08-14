using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Errors;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class ExpressionBuilderRootState : ExpressionBuilderState
{
    internal ExpressionBuilderRootState(
        Type argumentType,
        ParsedExpressionFactoryInternalConfiguration configuration,
        IParsedExpressionNumberParser numberParser)
        : base(
            parameterExpression: Expression.Parameter( argumentType.MakeArrayType(), "args" ),
            configuration: configuration,
            numberParser: numberParser )
    {
        ActiveState = this;
    }

    internal ExpressionBuilderState ActiveState { get; set; }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Chain<ParsedExpressionBuilderError> HandleToken(IntermediateToken token)
    {
        var errors = ActiveState.HandleTokenInternal( token );
        if ( errors.Count > 0 )
            errors = CollectChildStateErrors( ActiveState, errors );

        return errors;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal UnsafeBuilderResult<ExpressionBuilderResult> GetResult(Type outputType)
    {
        var errors = HandleExpressionEnd();
        if ( ! ActiveState.IsRoot )
        {
            var parentState = ((ExpressionBuilderChildState)ActiveState).ParentState;
            Assume.IsNotNull( parentState.LastHandledToken, nameof( parentState.LastHandledToken ) );

            var missingClosingSymbolError =
                ParsedExpressionBuilderError.CreateMissingSubExpressionClosingSymbol( parentState.LastHandledToken.Value );

            errors = errors.Extend( CollectChildStateErrors( parentState, Chain.Create( missingClosingSymbolError ) ) );
        }

        if ( errors.Count > 0 )
            return UnsafeBuilderResult<ExpressionBuilderResult>.CreateErrors( errors );

        var typeCastResult = ConvertResultToOutputType( outputType );
        if ( ! typeCastResult.IsOk )
            return typeCastResult.CastErrorsTo<ExpressionBuilderResult>();

        var result = new ExpressionBuilderResult( typeCastResult.Result!, ParameterExpression, ArgumentIndexes );
        return UnsafeBuilderResult<ExpressionBuilderResult>.CreateOk( result );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Chain<ParsedExpressionBuilderError> CollectChildStateErrors(
        ExpressionBuilderState state,
        Chain<ParsedExpressionBuilderError> errors)
    {
        while ( ! state.IsRoot )
        {
            state = ((ExpressionBuilderChildState)state).ParentState;
            Assume.IsNotNull( state.LastHandledToken, nameof( state.LastHandledToken ) );
            errors = Chain.Create( ParsedExpressionBuilderError.CreateNestedExpressionFailure( state.LastHandledToken.Value, errors ) );
        }

        return errors;
    }
}
