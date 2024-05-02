using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections.Extensions;

/// <summary>
/// Contains <see cref="ITreeDictionaryNode{TKey,TValue}"/> extension methods.
/// </summary>
public static class TreeDictionaryNodeExtensions
{
    /// <summary>
    /// Finds the root node.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>Root node or the provided <paramref name="node"/> if it is a root node.</returns>
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

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all ancestors of the provided <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<ITreeDictionaryNode<TKey, TValue>> VisitAncestors<TKey, TValue>(
        this ITreeDictionaryNode<TKey, TValue> node)
    {
        return node.Visit( static n => n.Parent );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all descendants of the provided <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<ITreeDictionaryNode<TKey, TValue>> VisitDescendants<TKey, TValue>(
        this ITreeDictionaryNode<TKey, TValue> node)
    {
        return node.VisitMany( static n => n.Children );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all descendants of the provided <paramref name="node"/>
    /// with a <paramref name="stopPredicate"/>.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <param name="stopPredicate">Predicate that stops the traversal for the given sub-tree, when it returns <b>true</b>.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<ITreeDictionaryNode<TKey, TValue>> VisitDescendants<TKey, TValue>(
        this ITreeDictionaryNode<TKey, TValue> node,
        Func<ITreeDictionaryNode<TKey, TValue>, bool> stopPredicate)
    {
        return node.VisitMany( static n => n.Children, stopPredicate );
    }

    /// <summary>
    /// Finds the root node.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>Root node or the provided <paramref name="node"/> if it is a root node.</returns>
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

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all ancestors of the provided <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<TreeDictionaryNode<TKey, TValue>> VisitAncestors<TKey, TValue>(
        this TreeDictionaryNode<TKey, TValue> node)
        where TKey : notnull
    {
        return node.Visit( static n => n.Parent );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all descendants of the provided <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<TreeDictionaryNode<TKey, TValue>> VisitDescendants<TKey, TValue>(
        this TreeDictionaryNode<TKey, TValue> node)
        where TKey : notnull
    {
        return node.VisitMany( static n => n.Children );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all descendants of the provided <paramref name="node"/>
    /// with a <paramref name="stopPredicate"/>.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <param name="stopPredicate">Predicate that stops the traversal for the given sub-tree, when it returns <b>true</b>.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<TreeDictionaryNode<TKey, TValue>> VisitDescendants<TKey, TValue>(
        this TreeDictionaryNode<TKey, TValue> node,
        Func<TreeDictionaryNode<TKey, TValue>, bool> stopPredicate)
        where TKey : notnull
    {
        return node.VisitMany( static n => n.Children, stopPredicate );
    }

    /// <summary>
    /// Creates a new <see cref="TreeDictionary{TKey,TValue}"/> instance equivalent to the sub-tree
    /// with the provided <paramref name="node"/> as its root node.
    /// </summary>
    /// <param name="node">Root node.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="TreeDictionary{TKey,TValue}"/> instance.</returns>
    [Pure]
    public static TreeDictionary<TKey, TValue> CreateTree<TKey, TValue>(this TreeDictionaryNode<TKey, TValue> node)
        where TKey : notnull
    {
        var result = new TreeDictionary<TKey, TValue>( node.Tree?.Comparer ?? EqualityComparer<TKey>.Default );

        result.SetRoot( node.Key, node.Value );
        foreach ( var descendant in node.VisitDescendants() )
        {
            Assume.IsNotNull( descendant.Parent );
            result.AddTo( descendant.Parent.Key, descendant.Key, descendant.Value );
        }

        return result;
    }
}
