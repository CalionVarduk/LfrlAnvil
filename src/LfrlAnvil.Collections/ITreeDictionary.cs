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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic tree data structure with the ability to identify nodes by keys.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public interface ITreeDictionary<TKey, TValue> : IReadOnlyTreeDictionary<TKey, TValue>, IDictionary<TKey, TValue>
    where TKey : notnull
{
    /// <inheritdoc cref="ICollection{T}.Count" />
    new int Count { get; }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.Keys" />
    new IEnumerable<TKey> Keys { get; }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.Values" />
    new IEnumerable<TValue> Values { get; }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.this" />
    new TValue this[TKey key] { get; set; }

    /// <summary>
    /// Adds the provided (<paramref name="key"/>, <paramref name="value"/>) pair to this tree as a root node.
    /// </summary>
    /// <param name="key">Root's key.</param>
    /// <param name="value">Root's value.</param>
    /// <returns>Created <see cref="ITreeDictionaryNode{TKey,TValue}"/> instance.</returns>
    /// <exception cref="ArgumentException">When node's key already exists in this tree.</exception>
    /// <remarks>Old root node, if exists, will be attached to the new root node as its child.</remarks>
    ITreeDictionaryNode<TKey, TValue> SetRoot(TKey key, TValue value);

    /// <summary>
    /// Adds the provided <paramref name="node"/> to this tree as a root node.
    /// </summary>
    /// <param name="node">Node to set as root.</param>
    /// <exception cref="InvalidOperationException">When node already belongs to any tree.</exception>
    /// <exception cref="ArgumentException">When node's key already exists in this tree.</exception>
    /// <remarks>Old root node, if exists, will be attached to the new root node as its child.</remarks>
    void SetRoot(ITreeDictionaryNode<TKey, TValue> node);

    /// <summary>
    /// Adds the provided (<paramref name="key"/>, <paramref name="value"/>) pair to this tree.
    /// </summary>
    /// <param name="key">Node's key.</param>
    /// <param name="value">Node's value.</param>
    /// <returns>Created <see cref="ITreeDictionaryNode{TKey,TValue}"/> instance.</returns>
    /// <exception cref="ArgumentException">When node's key already exists in this tree.</exception>
    /// <remarks>
    /// Created node will become this tree's root if it was empty, otherwise it will be attached to the current root node as its child.
    /// </remarks>
    new ITreeDictionaryNode<TKey, TValue> Add(TKey key, TValue value);

    /// <summary>
    /// Adds the provided <paramref name="node"/> to this tree.
    /// </summary>
    /// <param name="node">Node to add.</param>
    /// <exception cref="InvalidOperationException">When node already belongs to any tree.</exception>
    /// <exception cref="ArgumentException">When node's key already exists in this tree.</exception>
    /// <remarks>
    /// Created node will become this tree's root if it was empty, otherwise it will be attached to the current root node as its child.
    /// </remarks>
    void Add(ITreeDictionaryNode<TKey, TValue> node);

    /// <summary>
    /// Adds the provided (<paramref name="key"/>, <paramref name="value"/>) pair to this tree
    /// and attaches it to the specified parent node as its child.
    /// </summary>
    /// <param name="parentKey">Parent node's key.</param>
    /// <param name="key">Node's key.</param>
    /// <param name="value">Node's value.</param>
    /// <returns>Created <see cref="ITreeDictionaryNode{TKey,TValue}"/> instance.</returns>
    /// <exception cref="KeyNotFoundException">When parent node's key does not exist in this tree.</exception>
    /// <exception cref="ArgumentException">When node's key already exists in this tree.</exception>
    ITreeDictionaryNode<TKey, TValue> AddTo(TKey parentKey, TKey key, TValue value);

    /// <summary>
    /// Adds the provided <paramref name="node"/> to this tree and attaches it to the specified parent node as its child.
    /// </summary>
    /// <param name="parentKey">Parent node's key.</param>
    /// <param name="node">Node to add.</param>
    /// <exception cref="InvalidOperationException">When node already belongs to any tree.</exception>
    /// <exception cref="KeyNotFoundException">When parent node's key does not exist in this tree.</exception>
    /// <exception cref="ArgumentException">When node's key already exists in this tree.</exception>
    void AddTo(TKey parentKey, ITreeDictionaryNode<TKey, TValue> node);

    /// <summary>
    /// Adds the provided (<paramref name="key"/>, <paramref name="value"/>) pair to this tree
    /// and attaches it to the specified parent node as its child.
    /// </summary>
    /// <param name="parent">Parent node.</param>
    /// <param name="key">Node's key.</param>
    /// <param name="value">Node's value.</param>
    /// <returns>Created <see cref="ITreeDictionaryNode{TKey,TValue}"/> instance.</returns>
    /// <exception cref="InvalidOperationException">When parent node does not belong to this tree.</exception>
    /// <exception cref="ArgumentException">When node's key already exists in this tree.</exception>
    ITreeDictionaryNode<TKey, TValue> AddTo(ITreeDictionaryNode<TKey, TValue> parent, TKey key, TValue value);

    /// <summary>
    /// Adds the provided <paramref name="node"/> to this tree and attaches it to the specified parent node as its child.
    /// </summary>
    /// <param name="parent">Parent node.</param>
    /// <param name="node">Node to add.</param>
    /// <exception cref="InvalidOperationException">
    /// When parent node does not belong to this tree or when node already belongs to any tree.
    /// </exception>
    /// <exception cref="ArgumentException">When node's key already exists in this tree.</exception>
    void AddTo(ITreeDictionaryNode<TKey, TValue> parent, ITreeDictionaryNode<TKey, TValue> node);

    /// <summary>
    /// Adds a sub-tree with its root at the provided <paramref name="node"/> to this tree.
    /// </summary>
    /// <param name="node">Root of the sub-tree to add.</param>
    /// <returns>Created <see cref="ITreeDictionaryNode{TKey,TValue}"/> instance of the added sub-tree's root node.</returns>
    /// <exception cref="InvalidOperationException">When node belongs to this tree.</exception>
    /// <exception cref="ArgumentException">When any of the sub-tree's node's keys already exist in this tree.</exception>
    /// <remarks>
    /// Created sub-tree's root node will become this tree's root if it was empty,
    /// otherwise it will be attached to the current root node as its child.
    /// </remarks>
    ITreeDictionaryNode<TKey, TValue> AddSubtree(ITreeDictionaryNode<TKey, TValue> node);

    /// <summary>
    /// Adds a sub-tree with its root at the provided <paramref name="node"/> to this tree
    /// and attaches it to the specified parent node as its child.
    /// </summary>
    /// <param name="parentKey">Parent node's key.</param>
    /// <param name="node">Root of the sub-tree to add.</param>
    /// <exception cref="KeyNotFoundException">When parent node's key does not exist in this tree.</exception>
    /// <exception cref="InvalidOperationException">When node belongs to this tree.</exception>
    /// <exception cref="ArgumentException">When any of the sub-tree's node's keys already exist in this tree.</exception>
    ITreeDictionaryNode<TKey, TValue> AddSubtreeTo(TKey parentKey, ITreeDictionaryNode<TKey, TValue> node);

    /// <summary>
    /// Adds a sub-tree with its root at the provided <paramref name="node"/> to this tree
    /// and attaches it to the specified parent node as its child.
    /// </summary>
    /// <param name="parent">Parent node.</param>
    /// <param name="node">Root of the sub-tree to add.</param>
    /// <exception cref="InvalidOperationException">When node belongs to this tree.</exception>
    /// <exception cref="ArgumentException">
    /// When parent node does not belong to this tree
    /// or when any of the sub-tree's node's keys already exist in this tree.
    /// </exception>
    ITreeDictionaryNode<TKey, TValue> AddSubtreeTo(
        ITreeDictionaryNode<TKey, TValue> parent,
        ITreeDictionaryNode<TKey, TValue> node);

    /// <summary>
    /// Removes the provided <paramref name="node"/> from this tree.
    /// </summary>
    /// <param name="node">Node to remove.</param>
    /// <exception cref="InvalidOperationException">When node does not belong to this tree.</exception>
    /// <remarks>
    /// When the removed node is the root node, then its first child becomes the new root node
    /// and the following children become that new root node's children,
    /// otherwise removed node's children become its parent's children.
    /// </remarks>
    void Remove(ITreeDictionaryNode<TKey, TValue> node);

    /// <summary>
    /// Removes a node associated with the provided <paramref name="key"/> from this tree.
    /// </summary>
    /// <param name="key">Key of the node to remove.</param>
    /// <param name="removed"><b>out</b> parameter that returns removed node's value.</param>
    /// <returns><b>true</b> when the node has been removed, otherwise <b>false</b>.</returns>
    /// <remarks>
    /// When the removed node is the root node, then its first child becomes the new root node
    /// and the following children become that new root node's children,
    /// otherwise removed node's children become its parent's children.
    /// </remarks>
    bool Remove(TKey key, [MaybeNullWhen( false )] out TValue removed);

    /// <summary>
    /// Removes the node associated with the provided <paramref name="key"/> and all of its descendants from this tree.
    /// </summary>
    /// <param name="key">Kee of the root node of the sub-tree to remove.</param>
    /// <returns>Number of removed nodes.</returns>
    int RemoveSubtree(TKey key);

    /// <summary>
    /// Removes the provided <paramref name="node"/> and all of its descendants from this tree.
    /// </summary>
    /// <param name="node">Root node of the sub-tree to remove.</param>
    /// <returns>Number of removed nodes.</returns>
    /// <exception cref="InvalidOperationException">When node does not belong to this tree.</exception>
    int RemoveSubtree(ITreeDictionaryNode<TKey, TValue> node);

    /// <summary>
    /// Swaps position of two nodes of this tree.
    /// </summary>
    /// <param name="firstKey">First node's key.</param>
    /// <param name="secondKey">Second node's key.</param>
    /// <exception cref="KeyNotFoundException">
    /// When <paramref name="firstKey"/> or <paramref name="secondKey"/> do not exist in this tree.
    /// </exception>
    /// <remarks>Positions of parents and children of swapped nodes are not modified.</remarks>
    void Swap(TKey firstKey, TKey secondKey);

    /// <summary>
    /// Swaps position of two nodes of this tree.
    /// </summary>
    /// <param name="firstNode">First node.</param>
    /// <param name="secondNode">Second node.</param>
    /// <exception cref="InvalidOperationException">
    /// When <paramref name="firstNode"/> or <paramref name="secondNode"/> do not belong to this tree.
    /// </exception>
    /// <remarks>Positions of parents and children of swapped nodes are not modified.</remarks>
    void Swap(ITreeDictionaryNode<TKey, TValue> firstNode, ITreeDictionaryNode<TKey, TValue> secondNode);

    /// <summary>
    /// Moves the node associated with the provided <paramref name="key"/> to a different parent node.
    /// </summary>
    /// <param name="parentKey">Parent node's key.</param>
    /// <param name="key">Key of the node to move.</param>
    /// <returns>Node associated with the provided <paramref name="key"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// When <paramref name="parentKey"/> or <paramref name="key"/> does not exist in this tree.
    /// </exception>
    /// <exception cref="InvalidOperationException">When node is moved to itself.</exception>
    /// <remarks>This operation is equivalent to removing the node from this tree and re-adding it to the specified parent node.</remarks>
    ITreeDictionaryNode<TKey, TValue> MoveTo(TKey parentKey, TKey key);

    /// <summary>
    /// Moves the provided <paramref name="node"/> to a different parent node.
    /// </summary>
    /// <param name="parentKey">Parent node's key.</param>
    /// <param name="node">Node to move.</param>
    /// <exception cref="KeyNotFoundException">When <paramref name="parentKey"/> does not exist in this tree.</exception>
    /// <exception cref="InvalidOperationException">When node does not belong to this tree or it's moved to itself.</exception>
    /// <remarks>This operation is equivalent to removing the node from this tree and re-adding it to the specified parent node.</remarks>
    void MoveTo(TKey parentKey, ITreeDictionaryNode<TKey, TValue> node);

    /// <summary>
    /// Moves the node associated with the provided <paramref name="key"/> to a different parent node.
    /// </summary>
    /// <param name="parent">Parent node.</param>
    /// <param name="key">Key of the node to move.</param>
    /// <returns>Node associated with the provided <paramref name="key"/>.</returns>
    /// <exception cref="KeyNotFoundException">When <paramref name="key"/> does not exist in this tree.</exception>
    /// <exception cref="InvalidOperationException">When parent node does not belong to this tree or node is moved to itself.</exception>
    /// <remarks>This operation is equivalent to removing the node from this tree and re-adding it to the specified parent node.</remarks>
    ITreeDictionaryNode<TKey, TValue> MoveTo(ITreeDictionaryNode<TKey, TValue> parent, TKey key);

    /// <summary>
    /// Moves the provided <paramref name="node"/> to a different parent node.
    /// </summary>
    /// <param name="parent">Parent node.</param>
    /// <param name="node">Node to move.</param>
    /// <exception cref="InvalidOperationException">
    /// When parent node or node do not belong to this tree or node is moved to itself.
    /// </exception>
    /// <remarks>This operation is equivalent to removing the node from this tree and re-adding it to the specified parent node.</remarks>
    void MoveTo(ITreeDictionaryNode<TKey, TValue> parent, ITreeDictionaryNode<TKey, TValue> node);

    /// <summary>
    /// Moves the sub-tree with root node associated with the provided <paramref name="key"/> to a different parent node.
    /// </summary>
    /// <param name="parentKey">Parent node's key.</param>
    /// <param name="key">Key of the sub-tree's root node to move.</param>
    /// <returns>Node associated with the provided <paramref name="key"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// When <paramref name="parentKey"/> or <paramref name="key"/> does not exist in this tree.
    /// </exception>
    /// <exception cref="InvalidOperationException">When node is moved to itself or to one of its descendants.</exception>
    /// <remarks>
    /// This operation is equivalent to removing the sub-tree from this tree and re-adding it to the specified parent node.
    /// </remarks>
    ITreeDictionaryNode<TKey, TValue> MoveSubtreeTo(TKey parentKey, TKey key);

    /// <summary>
    /// Moves the sub-tree with the provided root <paramref name="node"/> to a different parent node.
    /// </summary>
    /// <param name="parentKey">Parent node's key.</param>
    /// <param name="node">Sub-tree's root node to move.</param>
    /// <exception cref="KeyNotFoundException">When <paramref name="parentKey"/> does not exist in this tree.</exception>
    /// <exception cref="InvalidOperationException">
    /// When node does not belong to this tree or it's moved to itself or to one of its descendants.
    /// </exception>
    /// <remarks>
    /// This operation is equivalent to removing the sub-tree from this tree and re-adding it to the specified parent node.
    /// </remarks>
    void MoveSubtreeTo(TKey parentKey, ITreeDictionaryNode<TKey, TValue> node);

    /// <summary>
    /// Moves the sub-tree with root node associated with the provided <paramref name="key"/> to a different parent node.
    /// </summary>
    /// <param name="parent">Parent node.</param>
    /// <param name="key">Key of the sub-tree's root node to move.</param>
    /// <returns>Node associated with the provided <paramref name="key"/>.</returns>
    /// <exception cref="KeyNotFoundException">When <paramref name="key"/> does not exist in this tree.</exception>
    /// <exception cref="InvalidOperationException">
    /// When parent node does not belong to this tree or node is moved to itself or to one of its descendants.
    /// </exception>
    /// <remarks>
    /// This operation is equivalent to removing the sub-tree from this tree and re-adding it to the specified parent node.
    /// </remarks>
    ITreeDictionaryNode<TKey, TValue> MoveSubtreeTo(ITreeDictionaryNode<TKey, TValue> parent, TKey key);

    /// <summary>
    /// Moves the sub-tree with the provided root <paramref name="node"/> to a different parent node.
    /// </summary>
    /// <param name="parent">Parent node.</param>
    /// <param name="node">Sub-tree's root node to move.</param>
    /// <exception cref="InvalidOperationException">
    /// When parent node or node do not belong to this tree or node is moved to itself or to one of its descendants.
    /// </exception>
    /// <remarks>
    /// This operation is equivalent to removing the sub-tree from this tree and re-adding it to the specified parent node.
    /// </remarks>
    void MoveSubtreeTo(ITreeDictionaryNode<TKey, TValue> parent, ITreeDictionaryNode<TKey, TValue> node);

    /// <inheritdoc cref="IDictionary{TKey,TValue}.TryGetValue(TKey,out TValue)" />
    new bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result);

    /// <inheritdoc cref="IDictionary{TKey,TValue}.ContainsKey(TKey)" />
    [Pure]
    new bool ContainsKey(TKey key);
}
