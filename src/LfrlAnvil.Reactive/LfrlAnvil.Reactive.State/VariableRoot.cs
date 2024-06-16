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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Reactive.State.Exceptions;
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State;

/// <inheritdoc cref="IVariableRoot{TKey}" />
public abstract class VariableRoot<TKey> : VariableNode, IVariableRoot<TKey>, IMutableVariableNode, IDisposable
    where TKey : notnull
{
    private readonly NodeCollection _nodes;
    private readonly EventPublisher<VariableRootChangeEvent<TKey>> _onChange;
    private readonly EventPublisher<VariableRootValidationEvent<TKey>> _onValidate;
    private VariableState _state;

    /// <summary>
    /// Creates a new <see cref="VariableRoot{TKey}"/> instance with <see cref="EqualityComparer{T}.Default"/> node's key equality comparer.
    /// </summary>
    protected VariableRoot()
        : this( EqualityComparer<TKey>.Default ) { }

    /// <summary>
    /// Creates a new <see cref="VariableRoot{TKey}"/> instance.
    /// </summary>
    /// <param name="comparer">Node's key equality comparer.</param>
    protected VariableRoot(IEqualityComparer<TKey> comparer)
    {
        _nodes = new NodeCollection( comparer );
        _onChange = new EventPublisher<VariableRootChangeEvent<TKey>>();
        _onValidate = new EventPublisher<VariableRootValidationEvent<TKey>>();
        _state = VariableState.ReadOnly;
    }

    /// <inheritdoc />
    public sealed override VariableState State => _state;

    /// <inheritdoc />
    public IVariableNodeCollection<TKey> Nodes => _nodes;

    /// <inheritdoc />
    public sealed override IEventStream<VariableRootChangeEvent<TKey>> OnChange => _onChange;

    /// <inheritdoc />
    public sealed override IEventStream<VariableRootValidationEvent<TKey>> OnValidate => _onValidate;

    IEventStream<IVariableRootEvent<TKey>> IReadOnlyVariableRoot<TKey>.OnChange => _onChange;
    IEventStream<IVariableRootEvent<TKey>> IReadOnlyVariableRoot<TKey>.OnValidate => _onValidate;

    IVariableNodeCollection IReadOnlyVariableRoot.Nodes => _nodes;
    IEventStream<IVariableRootEvent> IReadOnlyVariableRoot.OnChange => _onChange;
    IEventStream<IVariableRootEvent> IReadOnlyVariableRoot.OnValidate => _onValidate;

    IEventStream<IVariableNodeEvent> IVariableNode.OnChange => _onChange;
    IEventStream<IVariableNodeEvent> IVariableNode.OnValidate => _onValidate;

    /// <summary>
    /// Returns a string representation of this <see cref="VariableRoot{TKey}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Nodes )}: {_nodes.Count}, {nameof( State )}: {_state}";
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        _state |= VariableState.Disposed;

        foreach ( var node in Nodes.Values )
        {
            if ( node is IDisposable disposable )
                disposable.Dispose();
        }

        _onChange.Dispose();
        _onValidate.Dispose();
    }

    /// <inheritdoc cref="IMutableVariableNode.Refresh()" />
    public void Refresh()
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        foreach ( var node in Nodes.Values )
        {
            if ( node is IMutableVariableNode mutableNode )
                mutableNode.Refresh();
        }
    }

    /// <inheritdoc cref="IMutableVariableNode.RefreshValidation()" />
    public void RefreshValidation()
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        foreach ( var node in Nodes.Values )
        {
            if ( node is IMutableVariableNode mutableNode )
                mutableNode.RefreshValidation();
        }
    }

    /// <inheritdoc cref="IMutableVariableNode.ClearValidation()" />
    public void ClearValidation()
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        foreach ( var node in Nodes.Values )
        {
            if ( node is IMutableVariableNode mutableNode )
                mutableNode.ClearValidation();
        }
    }

    /// <summary>
    /// Changes the read-only state of this variable.
    /// </summary>
    /// <param name="enabled">Specifies whether or not the read-only state should be enabled.</param>
    public void SetReadOnly(bool enabled)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        foreach ( var node in Nodes.Values )
        {
            if ( node is IMutableVariableNode mutableNode )
                mutableNode.SetReadOnly( enabled );
        }
    }

    /// <inheritdoc />
    [Pure]
    public override IEnumerable<IVariableNode> GetChildren()
    {
        return _nodes.Values;
    }

    /// <summary>
    /// Emits the provided change <paramref name="event"/>.
    /// </summary>
    /// <param name="event">Event to publish.</param>
    protected virtual void OnPublishChangeEvent(VariableRootChangeEvent<TKey> @event)
    {
        _onChange.Publish( @event );
    }

    /// <summary>
    /// Emits the provided validation <paramref name="event"/>.
    /// </summary>
    /// <param name="event">Event to publish.</param>
    protected virtual void OnPublishValidationEvent(VariableRootValidationEvent<TKey> @event)
    {
        _onValidate.Publish( @event );
    }

    /// <summary>
    /// Registers the provided <paramref name="node"/> in this root's <see cref="Nodes"/> collection.
    /// </summary>
    /// <param name="key">Key to register the node under.</param>
    /// <param name="node">Node to register.</param>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <returns><paramref name="node"/>.</returns>
    /// <exception cref="VariableNodeRegistrationException">
    /// When this is disposed or has a parent
    /// or when <paramref name="node"/> is disposed or has a parent
    /// or when this and <paramref name="node"/> are the same.
    /// </exception>
    /// <exception cref="ArgumentException">When <paramref name="key"/> already exists.</exception>
    protected TNode RegisterNode<TNode>(TKey key, TNode node)
        where TNode : VariableNode
    {
        AssertNodeRegistration( node );

        _nodes.Nodes.Add( key, node );
        SetAsParentOf( node );
        SetupNodeEvents( key, node );

        return node;
    }

    private void AssertNodeRegistration(IVariableNode node)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            throw new VariableNodeRegistrationException( Resources.ParentNodeIsDisposed, this, node );

        if ( Parent is not null )
            throw new VariableNodeRegistrationException( Resources.CannotRegisterChildNodesInParentNodeThatAlreadyHasParent, this, node );

        if ( (node.State & VariableState.Disposed) != VariableState.Default )
            throw new VariableNodeRegistrationException( Resources.ChildNodeIsDisposed, this, node );

        if ( node.Parent is not null )
            throw new VariableNodeRegistrationException( Resources.ChildNodeAlreadyHasParent, this, node );

        if ( ReferenceEquals( this, node ) )
            throw new VariableNodeRegistrationException( Resources.ParentNodeCannotRegisterSelf, this, node );
    }

    private void SetupNodeEvents(TKey key, IVariableNode node)
    {
        UpdateState( _nodes.ChangedNodes, key, node.State, VariableState.Changed );
        UpdateState( _nodes.InvalidNodes, key, node.State, VariableState.Invalid );
        UpdateState( _nodes.WarningNodes, key, node.State, VariableState.Warning );
        UpdateState( _nodes.DirtyNodes, key, node.State, VariableState.Dirty );
        UpdateReadOnlyState( key, node.State );

        node.OnChange.Listen( new OnNodeChangeListener( this, key ) );
        node.OnValidate.Listen( new OnNodeValidationListener( this, key ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void UpdateState(HashSet<TKey> set, TKey nodeKey, VariableState nodeState, VariableState value)
    {
        if ( (nodeState & value) != VariableState.Default )
        {
            set.Add( nodeKey );
            _state |= value;
            return;
        }

        if ( set.Remove( nodeKey ) && set.Count == 0 )
            _state &= ~value;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void UpdateReadOnlyState(TKey nodeKey, VariableState nodeState)
    {
        if ( (nodeState & VariableState.ReadOnly) != VariableState.Default )
        {
            if ( _nodes.ReadOnlyNodes.Add( nodeKey ) && _nodes.ReadOnlyNodes.Count == _nodes.Count )
                _state |= VariableState.ReadOnly;

            return;
        }

        _nodes.ReadOnlyNodes.Remove( nodeKey );
        _state &= ~VariableState.ReadOnly;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void OnNodeChanged(TKey nodeKey, IVariableNodeEvent @event)
    {
        var previousState = _state;

        UpdateState( _nodes.ChangedNodes, nodeKey, @event.NewState, VariableState.Changed );
        UpdateState( _nodes.DirtyNodes, nodeKey, @event.NewState, VariableState.Dirty );
        UpdateReadOnlyState( nodeKey, @event.NewState );

        var changeEvent = new VariableRootChangeEvent<TKey>( this, nodeKey, @event, previousState );
        OnPublishChangeEvent( changeEvent );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void OnNodeValidated(TKey nodeKey, IVariableNodeEvent @event)
    {
        var previousState = _state;

        UpdateState( _nodes.InvalidNodes, nodeKey, @event.NewState, VariableState.Invalid );
        UpdateState( _nodes.WarningNodes, nodeKey, @event.NewState, VariableState.Warning );

        var validationEvent = new VariableRootValidationEvent<TKey>( this, nodeKey, @event, previousState );
        OnPublishValidationEvent( validationEvent );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void OnNodeDisposed(TKey nodeKey)
    {
        UpdateReadOnlyState( nodeKey, VariableState.ReadOnly );
    }

    private sealed class NodeCollection : IVariableNodeCollection<TKey>
    {
        internal readonly Dictionary<TKey, IVariableNode> Nodes;
        internal readonly HashSet<TKey> ChangedNodes;
        internal readonly HashSet<TKey> InvalidNodes;
        internal readonly HashSet<TKey> WarningNodes;
        internal readonly HashSet<TKey> ReadOnlyNodes;
        internal readonly HashSet<TKey> DirtyNodes;

        internal NodeCollection(IEqualityComparer<TKey> comparer)
        {
            Nodes = new Dictionary<TKey, IVariableNode>( comparer );
            ChangedNodes = new HashSet<TKey>( Nodes.Comparer );
            InvalidNodes = new HashSet<TKey>( Nodes.Comparer );
            WarningNodes = new HashSet<TKey>( Nodes.Comparer );
            ReadOnlyNodes = new HashSet<TKey>( Nodes.Comparer );
            DirtyNodes = new HashSet<TKey>( Nodes.Comparer );
        }

        public int Count => Nodes.Count;
        public IEqualityComparer<TKey> Comparer => Nodes.Comparer;
        public IVariableNode this[TKey key] => Nodes[key];
        public IReadOnlyCollection<IVariableNode> Values => Nodes.Values;
        public IReadOnlyCollection<TKey> Keys => Nodes.Keys;
        public IReadOnlySet<TKey> ChangedNodeKeys => ChangedNodes;
        public IReadOnlySet<TKey> InvalidNodeKeys => InvalidNodes;
        public IReadOnlySet<TKey> WarningNodeKeys => WarningNodes;
        public IReadOnlySet<TKey> ReadOnlyNodeKeys => ReadOnlyNodes;
        public IReadOnlySet<TKey> DirtyNodeKeys => DirtyNodes;

        IReadOnlyCollection<IVariableNode> IVariableNodeCollection.Values => Nodes.Values;
        IEnumerable IVariableNodeCollection.Keys => Keys;
        IEnumerable IVariableNodeCollection.ChangedNodeKeys => ChangedNodeKeys;
        IEnumerable IVariableNodeCollection.InvalidNodeKeys => InvalidNodeKeys;
        IEnumerable IVariableNodeCollection.WarningNodeKeys => WarningNodeKeys;
        IEnumerable IVariableNodeCollection.ReadOnlyNodeKeys => ReadOnlyNodeKeys;
        IEnumerable IVariableNodeCollection.DirtyNodeKeys => DirtyNodeKeys;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, IVariableNode>.Keys => Keys;
        IEnumerable<IVariableNode> IReadOnlyDictionary<TKey, IVariableNode>.Values => Values;

        [Pure]
        public bool ContainsKey(TKey key)
        {
            return Nodes.ContainsKey( key );
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out IVariableNode value)
        {
            return Nodes.TryGetValue( key, out value );
        }

        [Pure]
        public IEnumerator<KeyValuePair<TKey, IVariableNode>> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    private sealed class OnNodeChangeListener : EventListener<IVariableNodeEvent>
    {
        private readonly VariableRoot<TKey> _root;
        private readonly TKey _nodeKey;

        internal OnNodeChangeListener(VariableRoot<TKey> root, TKey nodeKey)
        {
            _root = root;
            _nodeKey = nodeKey;
        }

        public override void React(IVariableNodeEvent @event)
        {
            _root.OnNodeChanged( _nodeKey, @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _root.OnNodeDisposed( _nodeKey );
        }
    }

    private sealed class OnNodeValidationListener : EventListener<IVariableNodeEvent>
    {
        private readonly VariableRoot<TKey> _root;
        private readonly TKey _nodeKey;

        internal OnNodeValidationListener(VariableRoot<TKey> root, TKey nodeKey)
        {
            _root = root;
            _nodeKey = nodeKey;
        }

        public override void React(IVariableNodeEvent @event)
        {
            _root.OnNodeValidated( _nodeKey, @event );
        }

        public override void OnDispose(DisposalSource source) { }
    }
}
