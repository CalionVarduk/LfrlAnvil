using System.Collections.Generic;

namespace LfrlAnvil.Collections;

public interface ITreeNode<out T>
{
    T Value { get; }
    ITreeNode<T>? Parent { get; }
    IReadOnlyList<ITreeNode<T>> Children { get; }
}
