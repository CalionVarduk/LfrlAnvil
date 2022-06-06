using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections
{
    public sealed class TreeDictionaryNode<TKey, TValue> : ITreeDictionaryNode<TKey, TValue>
        where TKey : notnull
    {
        private List<TreeDictionaryNode<TKey, TValue>> _children;

        public TreeDictionaryNode(TKey key, TValue value)
        {
            Key = key;
            Value = value;
            _children = new List<TreeDictionaryNode<TKey, TValue>>( capacity: 1 );
        }

        public TKey Key { get; }
        public TValue Value { get; set; }
        public TreeDictionaryNode<TKey, TValue>? Parent { get; private set; }
        public TreeDictionary<TKey, TValue>? Tree { get; private set; }
        public IReadOnlyList<TreeDictionaryNode<TKey, TValue>> Children => _children;

        ITreeDictionaryNode<TKey, TValue>? ITreeDictionaryNode<TKey, TValue>.Parent => Parent;
        IReadOnlyList<ITreeDictionaryNode<TKey, TValue>> ITreeDictionaryNode<TKey, TValue>.Children => _children;
        ITreeNode<TValue>? ITreeNode<TValue>.Parent => Parent;
        IReadOnlyList<ITreeNode<TValue>> ITreeNode<TValue>.Children => _children;

        [Pure]
        public override string ToString()
        {
            return $"{Key} => {Value}";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SetTree(TreeDictionary<TKey, TValue> tree)
        {
            Debug.Assert( Tree is null, "tree assigned to node should be null" );
            Debug.Assert( Parent is null, "parent assigned to node should be null" );
            Debug.Assert( _children.Count == 0, "children assigned to node should be empty" );

            Tree = tree;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SetParent(TreeDictionaryNode<TKey, TValue> parent)
        {
            Debug.Assert( Tree is not null, "tree assigned to node should not be null" );

            Parent = parent;
            Parent._children.Add( this );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ClearParent()
        {
            Debug.Assert( Tree is not null, "tree assigned to node should not be null" );
            Debug.Assert( Parent is not null, "parent assigned to node should not be null" );

            Parent = null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void RemoveFromParent()
        {
            Debug.Assert( Tree is not null, "tree assigned to node should not be null" );
            Debug.Assert( Parent is not null, "parent assigned to node should not be null" );

            Parent._children.Remove( this );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Clear()
        {
            Debug.Assert( Tree is not null, "tree assigned to node should not be null" );

            Tree = null;
            Parent = null;
            _children.Clear();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SwapParentWith(TreeDictionaryNode<TKey, TValue> node)
        {
            Debug.Assert( Tree is not null, "tree assigned to node should not be null" );
            Debug.Assert( node.Tree is not null, "tree assigned to node should not be null" );

            (Parent, node.Parent) = (node.Parent, Parent);
        }

        internal void SwapChildrenWith(TreeDictionaryNode<TKey, TValue> node)
        {
            Debug.Assert( Tree is not null, "tree assigned to node should not be null" );
            Debug.Assert( node.Tree is not null, "tree assigned to node should not be null" );

            (_children, node._children) = (node._children, _children);

            foreach ( var child in _children )
                child.Parent = this;

            foreach ( var child in node._children )
                child.Parent = node;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ReplaceChildAt(int index, TreeDictionaryNode<TKey, TValue> newChild)
        {
            Debug.Assert( Tree is not null, "tree assigned to node should not be null" );
            Debug.Assert( index >= 0 && index < _children.Count, "index is out of bounds" );

            _children[index] = newChild;
        }

        internal void InheritChildrenFrom(TreeDictionaryNode<TKey, TValue> node)
        {
            Debug.Assert( Tree is not null, "tree assigned to node should not be null" );
            Debug.Assert( node.Tree is not null, "tree assigned to node should not be null" );

            _children.AddRange( node._children );

            foreach ( var child in node._children )
                child.Parent = this;

            node._children.Clear();
        }
    }
}
