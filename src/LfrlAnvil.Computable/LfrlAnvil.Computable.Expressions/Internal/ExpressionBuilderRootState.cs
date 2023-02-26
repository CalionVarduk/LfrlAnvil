using System;
using System.Collections;
using System.Collections.Generic;
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
            ? DiscardUnusedDelegatesAndVariablesAndArguments( typeCastResult.Result )
            : DiscardUnusedDelegatesAndVariables( typeCastResult.Result );

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
    internal IReadOnlyList<InlineDelegateCollectionState.Result> GetCompilableDelegates()
    {
        return _compilableDelegates;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ClearCompilableDelegates()
    {
        _compilableDelegates.Clear();
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

    private ExpressionBuilderResult DiscardUnusedDelegatesAndVariablesAndArguments(Expression expression)
    {
        if ( LocalTerms.ArgumentIndexes.Count == 0 )
            return DiscardUnusedDelegatesAndVariables( expression );

        var (delegateUsage, variableUsage) = ExpressionUsage.FindDelegateAndVariableUsage( expression, _compilableDelegates, LocalTerms );
        var usedDelegates = ExpressionUsage.GetUsedDelegates( _compilableDelegates, delegateUsage );
        var usedVariables = ExpressionUsage.GetUsedVariables( LocalTerms, variableUsage );
        MarkUsedVariableAssignments( usedVariables );

        var usedAssignments = LocalTerms.GetUsedVariableAssignments();
        var compilableDelegates = ExpressionUsage.GetCompilableDelegates( usedDelegates, usedAssignments );
        expression = ExpressionHelpers.IncludeVariables( expression, usedAssignments );

        var argumentUsage = ExpressionUsage.FindArgumentUsage( expression, LocalTerms );
        ExpressionUsage.AddDelegateArgumentUsage( argumentUsage, usedDelegates, usedAssignments );
        var (unusedArgumentCount, requiresArgumentReorganizing) = GetArgumentUsageInfo( argumentUsage );

        if ( unusedArgumentCount == 0 )
            return CreateResultWithoutAnyDiscardedArguments( expression, compilableDelegates );

        if ( unusedArgumentCount == LocalTerms.ArgumentIndexes.Count )
            return CreateResultWithAllArgumentsDiscarded( expression, compilableDelegates );

        return requiresArgumentReorganizing
            ? CreateResultWithSomeDiscardedArguments( expression, compilableDelegates, argumentUsage )
            : CreateResultWithSomeDiscardedArgumentsOnlyAtTheEnd( expression, compilableDelegates, unusedArgumentCount );
    }

    [Pure]
    private ExpressionBuilderResult DiscardUnusedDelegatesAndVariables(Expression expression)
    {
        var (delegateUsage, variableUsage) = ExpressionUsage.FindDelegateAndVariableUsage( expression, _compilableDelegates, LocalTerms );
        var usedDelegates = ExpressionUsage.GetUsedDelegates( _compilableDelegates, delegateUsage );
        var usedVariables = ExpressionUsage.GetUsedVariables( LocalTerms, variableUsage );
        MarkUsedVariableAssignments( usedVariables );

        var usedAssignments = LocalTerms.GetUsedVariableAssignments();
        var compilableDelegates = ExpressionUsage.GetCompilableDelegates( usedDelegates, usedAssignments );
        expression = ExpressionHelpers.IncludeVariables( expression, usedAssignments );

        return CreateResultWithoutAnyDiscardedArguments( expression, compilableDelegates );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void MarkUsedVariableAssignments(VariableAssignment[] assignments)
    {
        foreach ( var assignment in assignments )
            assignment.MarkAsUsed();
    }

    [Pure]
    private (int DiscardedCount, bool RequiresReorganizing) GetArgumentUsageInfo(BitArray usage)
    {
        Assume.IsNotEmpty( LocalTerms.ArgumentIndexes, nameof( LocalTerms.ArgumentIndexes ) );
        Assume.ContainsExactly( LocalTerms.ArgumentIndexes, usage.Length, nameof( LocalTerms.ArgumentIndexes ) );

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
            LocalTerms.ParameterExpression,
            delegates,
            LocalTerms.ArgumentIndexes,
            discardedArguments: new HashSet<StringSlice>() );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExpressionBuilderResult CreateResultWithAllArgumentsDiscarded(Expression expression, CompilableInlineDelegate[] delegates)
    {
        var discardedArguments = new HashSet<StringSlice>();
        foreach ( var (name, _) in LocalTerms.ArgumentIndexes )
            discardedArguments.Add( name );

        LocalTerms.ArgumentIndexes.Clear();
        LocalTerms.ArgumentIndexes.TrimExcess();
        return new ExpressionBuilderResult(
            expression,
            LocalTerms.ParameterExpression,
            delegates,
            LocalTerms.ArgumentIndexes,
            discardedArguments );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExpressionBuilderResult CreateResultWithSomeDiscardedArgumentsOnlyAtTheEnd(
        Expression expression,
        CompilableInlineDelegate[] delegates,
        int unusedArgumentCount)
    {
        var i = 0;
        var firstIndexToRemove = LocalTerms.ArgumentIndexes.Count - unusedArgumentCount;
        var keysToRemove = new StringSlice[unusedArgumentCount];

        foreach ( var (key, index) in LocalTerms.ArgumentIndexes )
        {
            if ( index < firstIndexToRemove )
                continue;

            keysToRemove[i++] = key;
        }

        var discardedArguments = new HashSet<StringSlice>();
        foreach ( var key in keysToRemove )
        {
            discardedArguments.Add( key );
            LocalTerms.ArgumentIndexes.Remove( key );
        }

        LocalTerms.ArgumentIndexes.TrimExcess();
        return new ExpressionBuilderResult(
            expression,
            LocalTerms.ParameterExpression,
            delegates,
            LocalTerms.ArgumentIndexes,
            discardedArguments );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExpressionBuilderResult CreateResultWithSomeDiscardedArguments(
        Expression expression,
        CompilableInlineDelegate[] delegates,
        BitArray usage)
    {
        Assume.IsNotEmpty( LocalTerms.ArgumentIndexes, nameof( LocalTerms.ArgumentIndexes ) );
        Assume.ContainsExactly( LocalTerms.ArgumentIndexes, usage.Length, nameof( LocalTerms.ArgumentIndexes ) );

        var argumentNames = LocalTerms.ArgumentIndexes.ToDictionary( static kv => kv.Value, static kv => kv.Key );
        var argumentAccessExpressions = new Dictionary<int, Expression>();
        var discardedArguments = new HashSet<StringSlice>();
        LocalTerms.ArgumentIndexes.Clear();

        for ( var i = 0; i < usage.Length; ++i )
        {
            var argumentName = argumentNames[i];
            if ( ! usage[i] )
            {
                discardedArguments.Add( argumentName );
                continue;
            }

            var newIndex = LocalTerms.ArgumentIndexes.Count;
            var argumentExpression = i == newIndex
                ? LocalTerms.GetArgumentAccess( i )
                : LocalTerms.ParameterExpression.CreateArgumentAccess( newIndex );

            LocalTerms.ArgumentIndexes.Add( argumentName, newIndex );
            argumentAccessExpressions.Add( i, argumentExpression );
        }

        LocalTerms.ArgumentIndexes.TrimExcess();
        var argumentAccessReorganizer = new ArgumentAccessReorganizer(
            LocalTerms.ParameterExpression,
            argumentAccessExpressions,
            usage.Length );

        foreach ( var @delegate in delegates )
            @delegate.ReorganizeArgumentAccess( argumentAccessReorganizer );

        expression = argumentAccessReorganizer.Visit( expression );
        return new ExpressionBuilderResult(
            expression,
            LocalTerms.ParameterExpression,
            delegates,
            LocalTerms.ArgumentIndexes,
            discardedArguments );
    }
}
