// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections.Extensions;

/// <summary>
/// Contains <see cref="ITreeNode{T}"/> extension methods.
/// </summary>
public static class TreeNodeExtensions
{
    /// <summary>
    /// Checks whether or not the provided <paramref name="node"/> is a root node.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns><b>true</b> when the node is a root node (does not have a parent node), otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsRoot<T>(this ITreeNode<T> node)
    {
        return node.Parent is null;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="node"/> is a leaf node.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns><b>true</b> when the node is a leaf node (does not have any children nodes), otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsLeaf<T>(this ITreeNode<T> node)
    {
        return node.Children.Count == 0;
    }

    /// <summary>
    /// Finds a 0-based index of the provided <paramref name="node"/> in the <see cref="ITreeNode{T}.Children"/> collection
    /// of the specified <paramref name="parent"/>.
    /// </summary>
    /// <param name="parent">Source parent node.</param>
    /// <param name="node">Node to check.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>0-based index of the provided <paramref name="node"/> if it exists, otherwise <b>-1</b>.</returns>
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

    /// <summary>
    /// Checks whether or not the provided <paramref name="node"/> is a direct child node of the specified <paramref name="parent"/>.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <param name="parent">Parent node.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// <b>true</b> when the provided <paramref name="node"/> is a child node of <paramref name="parent"/>, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsChildOf<T>(this ITreeNode<T> node, ITreeNode<T> parent)
    {
        return ReferenceEquals( node.Parent, parent );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="parent"/> is a parent node of the specified <paramref name="node"/>.
    /// </summary>
    /// <param name="parent">Parent node.</param>
    /// <param name="node">Node to check.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// <b>true</b> when the provided <paramref name="parent"/> is a parent node of <paramref name="node"/>, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsParentOf<T>(this ITreeNode<T> parent, ITreeNode<T> node)
    {
        return ReferenceEquals( parent, node.Parent );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="ancestor"/> is an ancestor node of the specified <paramref name="node"/>.
    /// </summary>
    /// <param name="ancestor">Ancestor node.</param>
    /// <param name="node">Node to check.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// <b>true</b> when the provided <paramref name="ancestor"/> is an ancestor node of <paramref name="node"/>, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsAncestorOf<T>(this ITreeNode<T> ancestor, ITreeNode<T> node)
    {
        return node.IsDescendantOf( ancestor );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="node"/> is a descendant node of the specified <paramref name="ancestor"/>.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <param name="ancestor">Ancestor node.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// <b>true</b> when the provided <paramref name="node"/> is a descendant node of <paramref name="ancestor"/>, otherwise <b>false</b>.
    /// </returns>
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

    /// <summary>
    /// Calculates the depth of the provided <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Node's depth. <b>0</b> means that the node is a root node.</returns>
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

    /// <summary>
    /// Calculates the depth of the provided <paramref name="node"/> relative to the specified <paramref name="root"/>.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <param name="root">Explicit root node.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// Node's depth relative to the <paramref name="root"/> node. <b>0</b> means that the node is the <paramref name="root"/> node.
    /// <b>-1</b> means that the node is not a descendant of the <paramref name="root"/> node.
    /// </returns>
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

    /// <summary>
    /// Finds the root node.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Root node or the provided <paramref name="node"/> if it is a root node.</returns>
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

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all ancestors of the provided <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<ITreeNode<T>> VisitAncestors<T>(this ITreeNode<T> node)
    {
        return node.Visit( static n => n.Parent );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all descendants of the provided <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<ITreeNode<T>> VisitDescendants<T>(this ITreeNode<T> node)
    {
        return node.VisitMany( static n => n.Children );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all descendants of the provided <paramref name="node"/>
    /// with a <paramref name="stopPredicate"/>.
    /// </summary>
    /// <param name="node">Source node.</param>
    /// <param name="stopPredicate">Predicate that stops the traversal for the given sub-tree, when it returns <b>true</b>.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<ITreeNode<T>> VisitDescendants<T>(this ITreeNode<T> node, Func<ITreeNode<T>, bool> stopPredicate)
    {
        return node.VisitMany( static n => n.Children, stopPredicate );
    }
}
