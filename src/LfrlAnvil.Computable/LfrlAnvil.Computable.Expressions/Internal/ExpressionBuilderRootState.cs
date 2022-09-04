using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Errors;
using LfrlAnvil.Computable.Expressions.Internal.Delegates;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class ExpressionBuilderRootState : ExpressionBuilderState
{
    private int _nextStateId;
    private readonly List<InlineDelegateCollectionState.Result> _compilableDelegates;

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
        _nextStateId = Id + 1;
        _compilableDelegates = new List<InlineDelegateCollectionState.Result>();
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
        var errors = ActiveState.IsRoot
            ? Chain<ParsedExpressionBuilderError>.Empty
            : ActiveState.TryHandleExpressionEndAsInlineDelegate();

        errors = errors.Extend( HandleExpressionEnd() );

        if ( ! ActiveState.IsRoot )
        {
            var parentState = ReinterpretCast.To<ExpressionBuilderChildState>( ActiveState ).ParentState;
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

        var result = RemoveUnusedArguments( typeCastResult.Result );
        return UnsafeBuilderResult<ExpressionBuilderResult>.CreateOk( result );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal int GetNextStateId()
    {
        return _nextStateId++;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddCompilableDelegate(InlineDelegateCollectionState.Result? result)
    {
        if ( result is not null )
            _compilableDelegates.Add( result.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Chain<ParsedExpressionBuilderError> CollectChildStateErrors(
        ExpressionBuilderState state,
        Chain<ParsedExpressionBuilderError> errors)
    {
        while ( ! state.IsRoot )
        {
            state = ReinterpretCast.To<ExpressionBuilderChildState>( state ).ParentState;
            Assume.IsNotNull( state.LastHandledToken, nameof( state.LastHandledToken ) );
            errors = Chain.Create( ParsedExpressionBuilderError.CreateNestedExpressionFailure( state.LastHandledToken.Value, errors ) );
        }

        return errors;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExpressionBuilderResult RemoveUnusedArguments(Expression expression)
    {
        if ( ArgumentIndexes.Count == 0 && _compilableDelegates.Count == 0 )
            return new ExpressionBuilderResult( ParameterExpression, expression, Array.Empty<CompilableInlineDelegate>(), ArgumentIndexes );

        var argumentUsageValidator = new ExpressionUsageValidator( ParameterExpression, _compilableDelegates, ArgumentIndexes.Count );
        argumentUsageValidator.Visit( expression );

        var usedDelegateCount = 0;
        for ( var i = 0; i < argumentUsageValidator.DelegateUsage.Length; ++i )
        {
            var isUsed = argumentUsageValidator.DelegateUsage[i];
            if ( isUsed )
                ++usedDelegateCount;
        }

        var delegateIndex = 0;
        var compilableDelegates = usedDelegateCount == 0
            ? Array.Empty<CompilableInlineDelegate>()
            : new CompilableInlineDelegate[usedDelegateCount];

        for ( var i = 0; i < argumentUsageValidator.DelegateUsage.Length; ++i )
        {
            var isUsed = argumentUsageValidator.DelegateUsage[i];
            if ( isUsed )
                compilableDelegates[delegateIndex++] = _compilableDelegates[i].Delegate;
        }

        var unusedArgumentCount = argumentUsageValidator.ArgumentUsage.Count == 0 || argumentUsageValidator.ArgumentUsage[0] ? 0 : 1;
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
            return new ExpressionBuilderResult( ParameterExpression, expression, compilableDelegates, ArgumentIndexes );

        if ( unusedArgumentCount == ArgumentIndexes.Count )
        {
            ArgumentIndexes.Clear();
            ArgumentIndexes.TrimExcess();
            return new ExpressionBuilderResult( ParameterExpression, expression, compilableDelegates, ArgumentIndexes );
        }

        if ( ! requiresArgumentReorganizing )
        {
            var i = 0;
            var firstIndexToRemove = ArgumentIndexes.Count - unusedArgumentCount;
            var keysToRemove = new StringSlice[unusedArgumentCount];

            foreach ( var (key, index) in ArgumentIndexes )
            {
                if ( index < firstIndexToRemove )
                    continue;

                keysToRemove[i++] = key;
            }

            foreach ( var key in keysToRemove )
                ArgumentIndexes.Remove( key );

            ArgumentIndexes.TrimExcess();
            return new ExpressionBuilderResult( ParameterExpression, expression, compilableDelegates, ArgumentIndexes );
        }

        var argumentNames = ArgumentIndexes.ToDictionary( kv => kv.Value, kv => kv.Key );
        var argumentAccessExpressions = new Dictionary<int, Expression>();
        ArgumentIndexes.Clear();

        for ( var i = 0; i < argumentUsageValidator.ArgumentUsage.Length; ++i )
        {
            var isUsed = argumentUsageValidator.ArgumentUsage[i];
            if ( ! isUsed )
                continue;

            var argumentName = argumentNames[i];
            var newIndex = ArgumentIndexes.Count;
            var argumentExpression = i == newIndex ? GetArgumentAccess( i ) : ParameterExpression.CreateArgumentAccess( newIndex );
            ArgumentIndexes.Add( argumentName, newIndex );
            argumentAccessExpressions.Add( i, argumentExpression );
        }

        ArgumentIndexes.TrimExcess();
        var argumentAccessReorganizer = new ArgumentAccessReorganizer(
            ParameterExpression,
            argumentAccessExpressions,
            argumentUsageValidator.ArgumentUsage.Length );

        foreach ( var @delegate in compilableDelegates )
            @delegate.ReorganizeArgumentAccess( argumentAccessReorganizer );

        expression = argumentAccessReorganizer.Visit( expression );
        return new ExpressionBuilderResult( ParameterExpression, expression, compilableDelegates, ArgumentIndexes );
    }

    private sealed class ExpressionUsageValidator : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;
        private readonly List<InlineDelegateCollectionState.Result> _compilableDelegates;

        internal ExpressionUsageValidator(
            ParameterExpression parameter,
            List<InlineDelegateCollectionState.Result> compilableDelegates,
            int argumentCount)
        {
            ArgumentUsage = new BitArray( argumentCount );
            DelegateUsage = new BitArray( compilableDelegates.Count );
            _parameter = parameter;
            _compilableDelegates = compilableDelegates;
        }

        public BitArray ArgumentUsage { get; }
        public BitArray DelegateUsage { get; }

        [return: NotNullIfNotNull( "node" )]
        public override Expression? Visit(Expression? node)
        {
            if ( node.TryGetArgumentAccessIndex( _parameter, ArgumentUsage.Length, out var index ) )
                ArgumentUsage[index] = true;
            else if ( ExpressionHelpers.IsLambdaPlaceholder( node ) )
            {
                var lambdaIndex = _compilableDelegates.FindIndex( x => ReferenceEquals( x.Delegate.Placeholder, node ) );
                if ( lambdaIndex >= 0 )
                {
                    DelegateUsage[lambdaIndex] = true;

                    var delegateArgumentIndexes = _compilableDelegates[lambdaIndex].UsedArgumentIndexes;
                    foreach ( var i in delegateArgumentIndexes )
                        ArgumentUsage[i] = true;

                    return node;
                }
            }

            return base.Visit( node );
        }
    }
}
