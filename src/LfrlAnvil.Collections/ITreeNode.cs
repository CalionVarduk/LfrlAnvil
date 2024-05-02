using System.Collections.Generic;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a tree data structure's node.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public interface ITreeNode<out T>
{
    /// <summary>
    /// Underlying value.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Optional parent node.
    /// </summary>
    ITreeNode<T>? Parent { get; }

    /// <summary>
    /// Collection of children nodes.
    /// </summary>
    IReadOnlyList<ITreeNode<T>> Children { get; }
}
