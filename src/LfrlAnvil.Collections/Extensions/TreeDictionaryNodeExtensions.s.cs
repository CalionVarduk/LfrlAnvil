using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections.Extensions;

public static class TreeDictionaryNodeExtensions
{
    [Pure]
    public static ITreeDictionaryNode<TKey, TValue> GetRoot<TKey, TValue>(this ITreeDictionaryNode<TKey, TValue> node)
    {
        if ( node.Parent is null )
            return node;

        var ancestor = node.Parent;

        while ( ancestor.Parent is not null )
            ancestor = ancestor.Parent;

        return ancestor;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<ITreeDictionaryNode<TKey, TValue>> VisitAncestors<TKey, TValue>(
        this ITreeDictionaryNode<TKey, TValue> node)
    {
        return node.Visit( n => n.Parent );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<ITreeDictionaryNode<TKey, TValue>> VisitDescendants<TKey, TValue>(
        this ITreeDictionaryNode<TKey, TValue> node)
    {
        return node.VisitMany( n => n.Children );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<ITreeDictionaryNode<TKey, TValue>> VisitDescendants<TKey, TValue>(
        this ITreeDictionaryNode<TKey, TValue> node,
        Func<ITreeDictionaryNode<TKey, TValue>, bool> stopPredicate)
    {
        return node.VisitMany( n => n.Children, stopPredicate );
    }

    [Pure]
    public static TreeDictionaryNode<TKey, TValue> GetRoot<TKey, TValue>(this TreeDictionaryNode<TKey, TValue> node)
        where TKey : notnull
    {
        if ( node.Parent is null )
            return node;

        var ancestor = node.Parent;

        while ( ancestor.Parent is not null )
            ancestor = ancestor.Parent;

        return ancestor;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<TreeDictionaryNode<TKey, TValue>> VisitAncestors<TKey, TValue>(
        this TreeDictionaryNode<TKey, TValue> node)
        where TKey : notnull
    {
        return node.Visit( n => n.Parent );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<TreeDictionaryNode<TKey, TValue>> VisitDescendants<TKey, TValue>(
        this TreeDictionaryNode<TKey, TValue> node)
        where TKey : notnull
    {
        return node.VisitMany( n => n.Children );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<TreeDictionaryNode<TKey, TValue>> VisitDescendants<TKey, TValue>(
        this TreeDictionaryNode<TKey, TValue> node,
        Func<TreeDictionaryNode<TKey, TValue>, bool> stopPredicate)
        where TKey : notnull
    {
        return node.VisitMany( n => n.Children, stopPredicate );
    }

    [Pure]
    public static TreeDictionary<TKey, TValue> CreateTree<TKey, TValue>(this TreeDictionaryNode<TKey, TValue> node)
        where TKey : notnull
    {
        var result = new TreeDictionary<TKey, TValue>( node.Tree?.Comparer ?? EqualityComparer<TKey>.Default );

        result.SetRoot( node.Key, node.Value );
        foreach ( var descendant in node.VisitDescendants() )
            result.AddTo( descendant.Parent!.Key, descendant.Key, descendant.Value );

        return result;
    }
}
