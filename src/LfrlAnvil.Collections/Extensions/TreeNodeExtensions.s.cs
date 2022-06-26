using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections.Extensions;

public static class TreeNodeExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsRoot<T>(this ITreeNode<T> node)
    {
        return node.Parent is null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsLeaf<T>(this ITreeNode<T> node)
    {
        return node.Children.Count == 0;
    }

    [Pure]
    public static int GetChildIndex<T>(this ITreeNode<T> parent, ITreeNode<T> node)
    {
        for ( var i = 0; i < parent.Children.Count; ++i )
        {
            if ( ReferenceEquals( parent.Children[i], node ) )
                return i;
        }

        return -1;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsChildOf<T>(this ITreeNode<T> node, ITreeNode<T> parent)
    {
        return ReferenceEquals( node.Parent, parent );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsParentOf<T>(this ITreeNode<T> parent, ITreeNode<T> node)
    {
        return ReferenceEquals( parent, node.Parent );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsAncestorOf<T>(this ITreeNode<T> ancestor, ITreeNode<T> node)
    {
        return node.IsDescendantOf( ancestor );
    }

    [Pure]
    public static bool IsDescendantOf<T>(this ITreeNode<T> node, ITreeNode<T> ancestor)
    {
        var current = node.Parent;

        while ( current is not null )
        {
            if ( ReferenceEquals( current, ancestor ) )
                return true;

            current = current.Parent;
        }

        return false;
    }

    [Pure]
    public static int GetLevel<T>(this ITreeNode<T> node)
    {
        var result = 0;
        var ancestor = node.Parent;

        while ( ancestor is not null )
        {
            ++result;
            ancestor = ancestor.Parent;
        }

        return result;
    }

    [Pure]
    public static int GetLevel<T>(this ITreeNode<T> node, ITreeNode<T> root)
    {
        if ( ReferenceEquals( node, root ) )
            return 0;

        var result = 0;
        var ancestor = node.Parent;

        while ( ancestor is not null )
        {
            ++result;
            if ( ReferenceEquals( ancestor, root ) )
                return result;

            ancestor = ancestor.Parent;
        }

        return -1;
    }

    [Pure]
    public static ITreeNode<T> GetRoot<T>(this ITreeNode<T> node)
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
    public static IEnumerable<ITreeNode<T>> VisitAncestors<T>(this ITreeNode<T> node)
    {
        return node.Visit( n => n.Parent );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<ITreeNode<T>> VisitDescendants<T>(this ITreeNode<T> node)
    {
        return node.VisitMany( n => n.Children );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<ITreeNode<T>> VisitDescendants<T>(this ITreeNode<T> node, Func<ITreeNode<T>, bool> stopPredicate)
    {
        return node.VisitMany( n => n.Children, stopPredicate );
    }
}