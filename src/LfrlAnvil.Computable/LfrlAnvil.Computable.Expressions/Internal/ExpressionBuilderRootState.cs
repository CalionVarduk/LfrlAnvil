using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
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

        var result = RemoveUnusedArguments( ParameterExpression, typeCastResult.Result!, ArgumentIndexes );
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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ExpressionBuilderResult RemoveUnusedArguments(
        ParameterExpression parameter,
        Expression expression,
        Dictionary<StringSlice, int> argumentIndexes)
    {
        if ( argumentIndexes.Count == 0 )
            return new ExpressionBuilderResult( parameter, expression, argumentIndexes );

        var argumentUsageValidator = new ArgumentUsageValidator( parameter, argumentIndexes.Count );
        argumentUsageValidator.Visit( expression );

        var unusedArgumentCount = argumentUsageValidator.ArgumentUsage[0] ? 0 : 1;
        var requiresArgumentReorganizing = false;

        for ( var i = 1; i < argumentUsageValidator.ArgumentUsage.Length; ++i )
        {
            var isUsed = argumentUsageValidator.ArgumentUsage[i];
            if ( isUsed )
            {
                if ( ! requiresArgumentReorganizing )
                {
                    var isPreviousUsed = argumentUsageValidator.ArgumentUsage[i - 1];
                    requiresArgumentReorganizing = ! isPreviousUsed;
                }

                continue;
            }

            ++unusedArgumentCount;
        }

        if ( unusedArgumentCount == 0 )
            return new ExpressionBuilderResult( parameter, expression, argumentIndexes );

        if ( unusedArgumentCount == argumentIndexes.Count )
        {
            argumentIndexes.Clear();
            argumentIndexes.TrimExcess();
            return new ExpressionBuilderResult( parameter, expression, argumentIndexes );
        }

        if ( ! requiresArgumentReorganizing )
        {
            var i = 0;
            var firstIndexToRemove = argumentIndexes.Count - unusedArgumentCount;
            var keysToRemove = new StringSlice[unusedArgumentCount];

            foreach ( var (key, index) in argumentIndexes )
            {
                if ( index < firstIndexToRemove )
                    continue;

                keysToRemove[i++] = key;
            }

            foreach ( var key in keysToRemove )
                argumentIndexes.Remove( key );

            argumentIndexes.TrimExcess();
            return new ExpressionBuilderResult( parameter, expression, argumentIndexes );
        }

        var argumentNames = argumentIndexes.ToDictionary( kv => kv.Value, kv => kv.Key );
        var argumentAccessExpressions = new Dictionary<int, Expression>();
        argumentIndexes.Clear();

        for ( var i = 0; i < argumentUsageValidator.ArgumentUsage.Length; ++i )
        {
            var isUsed = argumentUsageValidator.ArgumentUsage[i];
            if ( ! isUsed )
                continue;

            var argumentName = argumentNames[i];
            var newIndex = argumentIndexes.Count;
            argumentIndexes.Add( argumentName, newIndex );
            argumentAccessExpressions.Add( i, parameter.CreateArgumentAccess( newIndex ) );
        }

        argumentIndexes.TrimExcess();
        var argumentAccessReorganizer = new ArgumentAccessReorganizer(
            parameter,
            argumentAccessExpressions,
            argumentUsageValidator.ArgumentUsage.Length );

        expression = argumentAccessReorganizer.Visit( expression );
        return new ExpressionBuilderResult( parameter, expression, argumentIndexes );
    }

    private sealed class ArgumentUsageValidator : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        internal ArgumentUsageValidator(ParameterExpression parameter, int argumentCount)
        {
            Assume.IsGreaterThan( argumentCount, 0, nameof( argumentCount ) );
            ArgumentUsage = new bool[argumentCount];
            _parameter = parameter;
        }

        public bool[] ArgumentUsage { get; }

        [return: NotNullIfNotNull( "node" )]
        public override Expression? Visit(Expression? node)
        {
            if ( node.TryGetArgumentAccessIndex( _parameter, ArgumentUsage.Length, out var index ) )
                ArgumentUsage[index] = true;

            return base.Visit( node );
        }
    }

    private sealed class ArgumentAccessReorganizer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;
        private readonly IReadOnlyDictionary<int, Expression> _argumentAccessExpressions;
        private readonly int _oldArgumentCount;

        internal ArgumentAccessReorganizer(
            ParameterExpression parameter,
            IReadOnlyDictionary<int, Expression> argumentAccessExpressions,
            int oldArgumentCount)
        {
            _parameter = parameter;
            _argumentAccessExpressions = argumentAccessExpressions;
            _oldArgumentCount = oldArgumentCount;
        }

        [return: NotNullIfNotNull( "node" )]
        public override Expression? Visit(Expression? node)
        {
            if ( ! node.TryGetArgumentAccessIndex( _parameter, _oldArgumentCount, out var oldIndex ) )
                return base.Visit( node );

            var result = _argumentAccessExpressions[oldIndex];
            return result;
        }
    }
}
