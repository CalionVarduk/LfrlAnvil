using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.State.Internal;

internal sealed class CollectionVariableRootElements<TKey, TElement> : ICollectionVariableRootElements<TKey, TElement>
    where TKey : notnull
    where TElement : VariableNode
{
    internal readonly Dictionary<TKey, TElement> Elements;
    internal readonly Dictionary<TKey, TElement> Owned;
    internal readonly Dictionary<TKey, (IEventSubscriber OnChange, IEventSubscriber OnValidate)> Subscribers;
    internal readonly HashSet<TKey> Changed;
    internal readonly HashSet<TKey> Invalid;
    internal readonly HashSet<TKey> Warning;
    internal readonly HashSet<TKey> Removed;
    internal readonly HashSet<TKey> Added;

    internal CollectionVariableRootElements(IEqualityComparer<TKey>? keyComparer = null)
    {
        Elements = new Dictionary<TKey, TElement>( keyComparer );
        Owned = new Dictionary<TKey, TElement>( Elements.Comparer );
        Subscribers = new Dictionary<TKey, (IEventSubscriber, IEventSubscriber)>( Elements.Comparer );
        Changed = new HashSet<TKey>( Elements.Comparer );
        Invalid = new HashSet<TKey>( Elements.Comparer );
        Warning = new HashSet<TKey>( Elements.Comparer );
        Removed = new HashSet<TKey>( Elements.Comparer );
        Added = new HashSet<TKey>( Elements.Comparer );
    }

    public int Count => Elements.Count;
    public IReadOnlyCollection<TKey> Keys => Elements.Keys;
    public IReadOnlyCollection<TElement> Values => Elements.Values;
    public IReadOnlySet<TKey> InvalidElementKeys => Invalid;
    public IReadOnlySet<TKey> WarningElementKeys => Warning;
    public IReadOnlySet<TKey> AddedElementKeys => Added;
    public IReadOnlySet<TKey> RemovedElementKeys => Removed;
    public IReadOnlySet<TKey> ChangedElementKeys => Changed;
    public IEqualityComparer<TKey> KeyComparer => Elements.Comparer;
    public TElement this[TKey key] => Elements[key];

    IEnumerable ICollectionVariableRootElements.Keys => Keys;
    IReadOnlyCollection<IVariableNode> ICollectionVariableRootElements.Values => Values;
    IEnumerable ICollectionVariableRootElements.InvalidElementKeys => Invalid;
    IEnumerable ICollectionVariableRootElements.WarningElementKeys => Warning;
    IEnumerable ICollectionVariableRootElements.AddedElementKeys => Added;
    IEnumerable ICollectionVariableRootElements.RemovedElementKeys => Removed;
    IEnumerable ICollectionVariableRootElements.ChangedElementKeys => Changed;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TElement>.Keys => Keys;
    IEnumerable<TElement> IReadOnlyDictionary<TKey, TElement>.Values => Values;

    [Pure]
    public bool ContainsKey(TKey key)
    {
        return Elements.ContainsKey( key );
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TElement value)
    {
        return Elements.TryGetValue( key, out value );
    }

    [Pure]
    public IEnumerator<KeyValuePair<TKey, TElement>> GetEnumerator()
    {
        return Elements.GetEnumerator();
    }

    internal void AddInitialElement(TKey key, TElement element)
    {
        Owned.Add( key, element );
        AddOwnedElement( key, element );
    }

    internal void AddInitialElementAsRemoved(TKey key, TElement element)
    {
        Owned.Add( key, element );
        Removed.Add( key );
    }

    internal bool TryAddInitialElementAsAdded(TKey key, TElement element)
    {
        if ( ! Owned.TryAdd( key, element ) )
            return false;

        Added.Add( key );
        AddOwnedElement( key, element );
        return true;
    }

    internal void AddElement(TKey key, TElement element)
    {
        Owned.Add( key, element );
        Added.Add( key );
        AddOwnedElement( key, element );
    }

    internal void RestoreElement(TKey key, TElement element)
    {
        Removed.Remove( key );
        AddOwnedElement( key, element );
    }

    internal void RemoveElement(TKey key, TElement element)
    {
        Elements.Remove( key );
        Changed.Remove( key );
        Invalid.Remove( key );
        Warning.Remove( key );

        if ( ! Added.Remove( key ) )
        {
            Removed.Add( key );
            return;
        }

        Owned.Remove( key );
        if ( element is IDisposable disposable )
            disposable.Dispose();
    }

    private void AddOwnedElement(TKey key, TElement element)
    {
        Elements.Add( key, element );

        if ( (element.State & VariableState.Changed) != VariableState.Default )
            Changed.Add( key );

        if ( (element.State & VariableState.Invalid) != VariableState.Default )
            Invalid.Add( key );

        if ( (element.State & VariableState.Warning) != VariableState.Default )
            Warning.Add( key );
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
