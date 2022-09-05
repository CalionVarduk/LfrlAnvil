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
        IParsedExpressionNumberParser numberParser,
        IReadOnlyDictionary<StringSlice, ConstantExpression>? boundArguments)
        : base(
            parameterExpression: Expression.Parameter( argumentType.MakeArrayType(), "args" ),
            configuration: configuration,
            numberParser: numberParser,
            boundArguments: boundArguments )
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

        errors = errors.Extend( HandleExpressionEnd( parentToken: null ) );

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
        
        var result = Configuration.DiscardUnusedArguments
            ? DiscardUnusedDelegatesAndArguments( typeCastResult.Result )
            : DiscardUnusedDelegates( typeCastResult.Result );

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

    private ExpressionBuilderResult DiscardUnusedDelegatesAndArguments(Expression expression)
    {
        if ( ArgumentIndexes.Count == 0 )
            return DiscardUnusedDelegates( expression );

        var usageValidator = new ExpressionUsageValidator( ParameterExpression, _compilableDelegates, ArgumentIndexes.Count );
        usageValidator.Visit( expression );

        var compilableDelegates = GetUsedCompilableDelegates( usageValidator.DelegateUsage );
        var (unusedArgumentCount, requiresArgumentReorganizing) = GetArgumentUsageInfo( usageValidator.ArgumentUsage );

        if ( unusedArgumentCount == 0 )
            return CreateResultWithoutAnyDiscardedArguments( expression, compilableDelegates );

        if ( unusedArgumentCount == ArgumentIndexes.Count )
            return CreateResultWithAllArgumentsDiscarded( expression, compilableDelegates );

        return requiresArgumentReorganizing
            ? CreateResultWithSomeDiscardedArguments( expression, compilableDelegates, usageValidator.ArgumentUsage )
            : CreateResultWithSomeDiscardedArgumentsOnlyAtTheEnd( expression, compilableDelegates, unusedArgumentCount );
    }

    [Pure]
    private ExpressionBuilderResult DiscardUnusedDelegates(Expression expression)
    {
        if ( _compilableDelegates.Count == 0 )
            return CreateResultWithoutAnyDiscardedArguments( expression, Array.Empty<CompilableInlineDelegate>() );

        var usageValidator = new DelegateUsageValidator( _compilableDelegates );
        usageValidator.Visit( expression );

        var compilableDelegates = GetUsedCompilableDelegates( usageValidator.DelegateUsage );
        return CreateResultWithoutAnyDiscardedArguments( expression, compilableDelegates );
    }

    [Pure]
    private CompilableInlineDelegate[] GetUsedCompilableDelegates(BitArray usage)
    {
        Assume.ContainsExactly( _compilableDelegates, usage.Length, nameof( _compilableDelegates ) );

        var count = 0;
        for ( var i = 0; i < usage.Length; ++i )
        {
            if ( usage[i] )
                ++count;
        }

        var index = 0;
        var result = count == 0 ? Array.Empty<CompilableInlineDelegate>() : new CompilableInlineDelegate[count];
        for ( var i = 0; i < usage.Length; ++i )
        {
            if ( usage[i] )
                result[index++] = _compilableDelegates[i].Delegate;
        }

        return result;
    }

    [Pure]
    private (int DiscardedCount, bool RequiresReorganizing) GetArgumentUsageInfo(BitArray usage)
    {
        Assume.IsNotEmpty( ArgumentIndexes, nameof( ArgumentIndexes ) );
        Assume.ContainsExactly( ArgumentIndexes, usage.Length, nameof( ArgumentIndexes ) );

        var discardedCount = usage[0] ? 0 : 1;
        var requiresReorganizing = false;

        for ( var i = 1; i < usage.Length; ++i )
        {
            if ( usage[i] )
            {
                if ( ! requiresReorganizing )
                    requiresReorganizing = ! usage[i - 1];

                continue;
            }

            ++discardedCount;
        }

        return (discardedCount, requiresReorganizing);
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExpressionBuilderResult CreateResultWithoutAnyDiscardedArguments(Expression expression, CompilableInlineDelegate[] delegates)
    {
        return new ExpressionBuilderResult(
            expression,
            ParameterExpression,
            delegates,
            ArgumentIndexes,
            discardedArguments: new HashSet<StringSlice>() );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExpressionBuilderResult CreateResultWithAllArgumentsDiscarded(Expression expression, CompilableInlineDelegate[] delegates)
    {
        var discardedArguments = new HashSet<StringSlice>();
        foreach ( var (name, _) in ArgumentIndexes )
            discardedArguments.Add( name );

        ArgumentIndexes.Clear();
        ArgumentIndexes.TrimExcess();
        return new ExpressionBuilderResult( expression, ParameterExpression, delegates, ArgumentIndexes, discardedArguments );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExpressionBuilderResult CreateResultWithSomeDiscardedArgumentsOnlyAtTheEnd(
        Expression expression,
        CompilableInlineDelegate[] delegates,
        int unusedArgumentCount)
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

        var discardedArguments = new HashSet<StringSlice>();
        foreach ( var key in keysToRemove )
        {
            discardedArguments.Add( key );
            ArgumentIndexes.Remove( key );
        }

        ArgumentIndexes.TrimExcess();
        return new ExpressionBuilderResult( expression, ParameterExpression, delegates, ArgumentIndexes, discardedArguments );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExpressionBuilderResult CreateResultWithSomeDiscardedArguments(
        Expression expression,
        CompilableInlineDelegate[] delegates,
        BitArray usage)
    {
        Assume.IsNotEmpty( ArgumentIndexes, nameof( ArgumentIndexes ) );
        Assume.ContainsExactly( ArgumentIndexes, usage.Length, nameof( ArgumentIndexes ) );

        var argumentNames = ArgumentIndexes.ToDictionary( kv => kv.Value, kv => kv.Key );
        var argumentAccessExpressions = new Dictionary<int, Expression>();
        var discardedArguments = new HashSet<StringSlice>();
        ArgumentIndexes.Clear();

        for ( var i = 0; i < usage.Length; ++i )
        {
            var argumentName = argumentNames[i];
            if ( ! usage[i] )
            {
                discardedArguments.Add( argumentName );
                continue;
            }

            var newIndex = ArgumentIndexes.Count;
            var argumentExpression = i == newIndex ? GetArgumentAccess( i ) : ParameterExpression.CreateArgumentAccess( newIndex );
            ArgumentIndexes.Add( argumentName, newIndex );
            argumentAccessExpressions.Add( i, argumentExpression );
        }

        ArgumentIndexes.TrimExcess();
        var argumentAccessReorganizer = new ArgumentAccessReorganizer( ParameterExpression, argumentAccessExpressions, usage.Length );
        foreach ( var @delegate in delegates )
            @delegate.ReorganizeArgumentAccess( argumentAccessReorganizer );

        expression = argumentAccessReorganizer.Visit( expression );
        return new ExpressionBuilderResult( expression, ParameterExpression, delegates, ArgumentIndexes, discardedArguments );
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
            Assume.IsGreaterThan( argumentCount, 0, nameof( argumentCount ) );

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

    private sealed class DelegateUsageValidator : ExpressionVisitor
    {
        private readonly List<InlineDelegateCollectionState.Result> _compilableDelegates;

        internal DelegateUsageValidator(List<InlineDelegateCollectionState.Result> compilableDelegates)
        {
            Assume.IsNotEmpty( compilableDelegates, nameof( compilableDelegates ) );
            DelegateUsage = new BitArray( compilableDelegates.Count );
            _compilableDelegates = compilableDelegates;
        }

        public BitArray DelegateUsage { get; }

        [return: NotNullIfNotNull( "node" )]
        public override Expression? Visit(Expression? node)
        {
            if ( ExpressionHelpers.IsLambdaPlaceholder( node ) )
            {
                var lambdaIndex = _compilableDelegates.FindIndex( x => ReferenceEquals( x.Delegate.Placeholder, node ) );
                if ( lambdaIndex >= 0 )
                {
                    DelegateUsage[lambdaIndex] = true;
                    return node;
                }
            }

            return base.Visit( node );
        }
    }
}
