using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal.Delegates;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal static class ExpressionUsage
{
    private static readonly BitArray EmptyBitUsage = new BitArray( 0 );
    private static readonly IReadOnlySet<StringSegment> EmptyVariableUsage = new HashSet<StringSegment>();

    [Pure]
    internal static (BitArray DelegateUsage, IReadOnlySet<StringSegment> VariableUsage) FindDelegateAndVariableUsage(
        Expression expression,
        IReadOnlyList<InlineDelegateCollectionState.Result> delegates,
        LocalTermsCollection localTerms)
    {
        if ( delegates.Count == 0 )
        {
            if ( localTerms.VariableAssignments.Count == 0 )
                return (EmptyBitUsage, EmptyVariableUsage);

            var variableFinder = new VariableFinder( localTerms );
            variableFinder.Visit( expression );
            return (EmptyBitUsage, variableFinder.Usage);
        }

        if ( localTerms.VariableAssignments.Count == 0 )
        {
            var delegateFinder = new DelegateFinder( delegates );
            delegateFinder.Visit( expression );
            return (delegateFinder.Usage, EmptyVariableUsage);
        }

        var finder = new DelegateAndVariableFinder( delegates, localTerms );
        finder.Visit( expression );

        for ( var i = 0; i < finder.DelegateUsage.Length; ++i )
        {
            if ( ! finder.DelegateUsage[i] )
                continue;

            var @delegate = delegates[i];
            finder.VariableUsage.UnionWith( @delegate.UsedVariables );
        }

        return (finder.DelegateUsage, finder.VariableUsage);
    }

    [Pure]
    internal static BitArray FindArgumentUsage(Expression expression, LocalTermsCollection localTerms)
    {
        var finder = new ArgumentFinder( localTerms.ParameterExpression, localTerms.ArgumentIndexes.Count );
        finder.Visit( expression );
        return finder.Usage;
    }

    [Pure]
    internal static InlineDelegateCollectionState.Result[] GetUsedDelegates(
        IReadOnlyList<InlineDelegateCollectionState.Result> delegates,
        BitArray usage)
    {
        Assume.ContainsExactly( delegates, usage.Length, nameof( delegates ) );

        var count = 0;
        for ( var i = 0; i < usage.Length; ++i )
        {
            if ( usage[i] )
                ++count;
        }

        if ( count == 0 )
            return Array.Empty<InlineDelegateCollectionState.Result>();

        var index = 0;
        var result = new InlineDelegateCollectionState.Result[count];
        for ( var i = 0; i < usage.Length; ++i )
        {
            if ( usage[i] )
                result[index++] = delegates[i];
        }

        return result;
    }

    [Pure]
    internal static VariableAssignment[] GetUsedVariables(LocalTermsCollection localTerms, IReadOnlySet<StringSegment> usage)
    {
        if ( usage.Count == 0 )
            return Array.Empty<VariableAssignment>();

        var index = 0;
        var result = new VariableAssignment[usage.Count];
        foreach ( var name in usage )
        {
            localTerms.TryGetVariable( name, out var assignment );
            Assume.IsNotNull( assignment, nameof( assignment ) );
            result[index++] = assignment;
        }

        return result;
    }

    [Pure]
    internal static CompilableInlineDelegate[] GetCompilableDelegates(
        IReadOnlyList<InlineDelegateCollectionState.Result> rootDelegates,
        IReadOnlyList<VariableAssignment> assignments)
    {
        var count = rootDelegates.Count;
        foreach ( var assignment in assignments )
            count += assignment.Delegates.Count;

        if ( count == 0 )
            return Array.Empty<CompilableInlineDelegate>();

        var index = 0;
        var result = new CompilableInlineDelegate[count];
        foreach ( var @delegate in assignments.SelectMany( static a => a.Delegates ) )
            result[index++] = @delegate.Delegate;

        foreach ( var @delegate in rootDelegates )
            result[index++] = @delegate.Delegate;

        return result;
    }

    internal static void AddDelegateArgumentUsage(
        BitArray usage,
        IReadOnlyList<InlineDelegateCollectionState.Result> rootDelegates,
        IReadOnlyList<VariableAssignment> assignments)
    {
        foreach ( var index in assignments.SelectMany( static a => a.Delegates ).SelectMany( static d => d.UsedArgumentIndexes ) )
            usage[index] = true;

        foreach ( var index in rootDelegates.SelectMany( static d => d.UsedArgumentIndexes ) )
            usage[index] = true;
    }

    private sealed class DelegateAndVariableFinder : ExpressionVisitor
    {
        private readonly IReadOnlyList<InlineDelegateCollectionState.Result> _delegates;
        private readonly LocalTermsCollection _localTerms;

        internal DelegateAndVariableFinder(IReadOnlyList<InlineDelegateCollectionState.Result> delegates, LocalTermsCollection localTerms)
        {
            Assume.IsNotEmpty( delegates, nameof( delegates ) );
            Assume.IsNotEmpty( localTerms.VariableAssignments, nameof( localTerms.VariableAssignments ) );
            _delegates = delegates;
            _localTerms = localTerms;
            DelegateUsage = new BitArray( delegates.Count );
            VariableUsage = new HashSet<StringSegment>();
        }

        internal BitArray DelegateUsage { get; }
        internal HashSet<StringSegment> VariableUsage { get; }

        [return: NotNullIfNotNull( "node" )]
        public override Expression? Visit(Expression? node)
        {
            if ( DelegateFinder.TryFindDelegateUsage( node, _delegates, DelegateUsage ) )
                return base.Visit( node );

            VariableFinder.TryFindVariableUsage( node, _localTerms, VariableUsage );
            return base.Visit( node );
        }
    }

    private sealed class DelegateFinder : ExpressionVisitor
    {
        private readonly IReadOnlyList<InlineDelegateCollectionState.Result> _delegates;

        internal DelegateFinder(IReadOnlyList<InlineDelegateCollectionState.Result> delegates)
        {
            Assume.IsNotEmpty( delegates, nameof( delegates ) );
            _delegates = delegates;
            Usage = new BitArray( delegates.Count );
        }

        internal BitArray Usage { get; }

        [return: NotNullIfNotNull( "node" )]
        public override Expression? Visit(Expression? node)
        {
            TryFindDelegateUsage( node, _delegates, Usage );
            return base.Visit( node );
        }

        internal static bool TryFindDelegateUsage(
            [NotNullWhen( true )] Expression? node,
            IReadOnlyList<InlineDelegateCollectionState.Result> delegates,
            BitArray usage)
        {
            if ( ! ExpressionHelpers.IsLambdaPlaceholder( node ) )
                return false;

            for ( var i = 0; i < usage.Length; ++i )
            {
                if ( ! ReferenceEquals( delegates[i].Delegate.Placeholder, node ) )
                    continue;

                usage[i] = true;
                return true;
            }

            return false;
        }
    }

    private sealed class VariableFinder : ExpressionVisitor
    {
        private readonly LocalTermsCollection _localTerms;

        internal VariableFinder(LocalTermsCollection localTerms)
        {
            Assume.IsNotEmpty( localTerms.VariableAssignments, nameof( localTerms.VariableAssignments ) );
            _localTerms = localTerms;
            Usage = new HashSet<StringSegment>();
        }

        internal HashSet<StringSegment> Usage { get; }

        [return: NotNullIfNotNull( "node" )]
        public override Expression? Visit(Expression? node)
        {
            TryFindVariableUsage( node, _localTerms, Usage );
            return base.Visit( node );
        }

        internal static void TryFindVariableUsage(Expression? node, LocalTermsCollection localTerms, HashSet<StringSegment> usage)
        {
            if ( node is not ParameterExpression parameter || parameter.Name is null )
                return;

            var variableName = parameter.Name;
            if ( ! localTerms.TryGetVariable( variableName, out var assignment ) )
                return;

            if ( ReferenceEquals( parameter, assignment.Variable ) )
                usage.Add( variableName );
        }
    }

    private sealed class ArgumentFinder : ExpressionVisitor
    {
        private readonly ParameterExpression _rootParameter;

        internal ArgumentFinder(ParameterExpression rootParameter, int argumentCount)
        {
            Assume.IsGreaterThan( argumentCount, 0, nameof( argumentCount ) );
            _rootParameter = rootParameter;
            Usage = new BitArray( argumentCount );
        }

        internal BitArray Usage { get; }

        [return: NotNullIfNotNull( "node" )]
        public override Expression? Visit(Expression? node)
        {
            if ( node.TryGetArgumentAccessIndex( _rootParameter, Usage.Length, out var index ) )
                Usage[index] = true;

            return base.Visit( node );
        }
    }
}
