using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Reactive.State.Internal;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

/// <inheritdoc cref="ICollectionVariableRoot{TKey,TElement,TValidationResult}" />
public class CollectionVariableRoot<TKey, TElement, TValidationResult>
    : VariableNode, ICollectionVariableRoot<TKey, TElement, TValidationResult>, IMutableVariableNode, IDisposable
    where TKey : notnull
    where TElement : VariableNode
{
    private readonly EventPublisher<CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>> _onChange;
    private readonly EventPublisher<CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult>> _onValidate;
    private CollectionVariableRootElements<TKey, TElement> _elements;
    private VariableState _state;

    /// <summary>
    /// Creates a new <see cref="CollectionVariableRoot{TKey,TElement,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="elements">Initial collection of elements.</param>
    /// <param name="keySelector">Element's key selector.</param>
    /// <param name="keyComparer">Element key equality comparer.</param>
    /// <param name="errorsValidator">Collection of elements validator that marks result as errors.</param>
    /// <param name="warningsValidator">Collection of elements validator that marks result as warnings.</param>
    public CollectionVariableRoot(
        IEnumerable<TElement> elements,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult>? errorsValidator = null,
        IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult>? warningsValidator = null)
    {
        InitialElements = elements.Where( CanAddInitial ).ToDictionary( keySelector, keyComparer );
        _elements = CreateElementsCollection( InitialElements, keyComparer );

        KeySelector = keySelector;
        ErrorsValidator = errorsValidator ?? Validators<TValidationResult>.Pass<ICollectionVariableRootElements<TKey, TElement>>();
        WarningsValidator = warningsValidator ?? Validators<TValidationResult>.Pass<ICollectionVariableRootElements<TKey, TElement>>();
        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        _state = CreateInitialState( VariableState.Default );

        _onChange = new EventPublisher<CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>>();
        _onValidate = new EventPublisher<CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult>>();

        foreach ( var (key, element) in _elements.Elements )
            SetupElementEvents( key, element );
    }

    /// <summary>
    /// Creates a new <see cref="CollectionVariableRoot{TKey,TElement,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="elements">Initial collection of elements.</param>
    /// <param name="elementChanges">Element changes that define the collection of current elements.</param>
    /// <param name="keySelector">Element's key selector.</param>
    /// <param name="keyComparer">Element key equality comparer.</param>
    /// <param name="errorsValidator">Collection of elements validator that marks result as errors.</param>
    /// <param name="warningsValidator">Collection of elements validator that marks result as warnings.</param>
    public CollectionVariableRoot(
        IEnumerable<TElement> elements,
        CollectionVariableRootChanges<TKey, TElement> elementChanges,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult>? errorsValidator = null,
        IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult>? warningsValidator = null)
    {
        InitialElements = elements.Where( CanAddInitial ).ToDictionary( keySelector, keyComparer );
        _elements = CreateElementsCollection( InitialElements, elementChanges, keySelector, keyComparer );

        KeySelector = keySelector;
        ErrorsValidator = errorsValidator ?? Validators<TValidationResult>.Pass<ICollectionVariableRootElements<TKey, TElement>>();
        WarningsValidator = warningsValidator ?? Validators<TValidationResult>.Pass<ICollectionVariableRootElements<TKey, TElement>>();
        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        _state = CreateInitialState( VariableState.Default );

        _onChange = new EventPublisher<CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>>();
        _onValidate = new EventPublisher<CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult>>();

        foreach ( var (key, element) in _elements.Elements )
            SetupElementEvents( key, element );
    }

    /// <inheritdoc />
    public Func<TElement, TKey> KeySelector { get; }

    /// <inheritdoc />
    public Chain<TValidationResult> Errors { get; private set; }

    /// <inheritdoc />
    public Chain<TValidationResult> Warnings { get; private set; }

    /// <inheritdoc />
    public IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult> ErrorsValidator { get; }

    /// <inheritdoc />
    public IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult> WarningsValidator { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<TKey, TElement> InitialElements { get; private set; }

    /// <inheritdoc />
    public ICollectionVariableRootElements<TKey, TElement> Elements => _elements;

    /// <inheritdoc cref="IReadOnlyCollectionVariableRoot{TKey,TElement,TValidationResult}.OnChange" />
    public sealed override IEventStream<CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>> OnChange => _onChange;

    /// <inheritdoc cref="IReadOnlyCollectionVariableRoot{TKey,TElement,TValidationResult}.OnValidate" />
    public sealed override IEventStream<CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult>> OnValidate => _onValidate;

    /// <inheritdoc />
    public sealed override VariableState State => _state;

    IEventStream<ICollectionVariableRootChangeEvent<TKey, TElement>> IReadOnlyCollectionVariableRoot<TKey, TElement>.OnChange => _onChange;

    Type IReadOnlyCollectionVariableRoot.KeyType => typeof( TKey );
    Type IReadOnlyCollectionVariableRoot.ElementType => typeof( TElement );
    Type IReadOnlyCollectionVariableRoot.ValidationResultType => typeof( TValidationResult );
    IEnumerable IReadOnlyCollectionVariableRoot.Errors => Errors;
    IEnumerable IReadOnlyCollectionVariableRoot.Warnings => Warnings;
    IEnumerable IReadOnlyCollectionVariableRoot.InitialElements => InitialElements.Values;
    ICollectionVariableRootElements IReadOnlyCollectionVariableRoot.Elements => Elements;
    IEventStream<ICollectionVariableRootChangeEvent> IReadOnlyCollectionVariableRoot.OnChange => _onChange;
    IEventStream<ICollectionVariableRootValidationEvent> IReadOnlyCollectionVariableRoot.OnValidate => _onValidate;

    /// <summary>
    /// Returns a string representation of this <see cref="CollectionVariableRoot{TKey,TElement,TValidationResult}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Elements )}: {_elements.Count}, {nameof( State )}: {_state}";
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        _state |= VariableState.ReadOnly | VariableState.Disposed;
        DisposeElements();
        _onChange.Dispose();
        _onValidate.Dispose();
    }

    /// <inheritdoc />
    public VariableChangeResult Change(CollectionVariableRootChanges<TKey, TElement> changes)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var elementsToChange = FindElementsToChange( ModifyChangeInput( changes ) );
        if ( elementsToChange.Count == 0 )
            return VariableChangeResult.NotChanged;

        UpdateAndPublishChangeEvents( elementsToChange );
        return VariableChangeResult.Changed;
    }

    /// <inheritdoc />
    public VariableChangeResult Add(TElement element)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var key = KeySelector( element );
        if ( ! CanAdd( key, element ) )
            return VariableChangeResult.NotChanged;

        SetAsParentOf( element );
        _elements.AddElement( key, element );

        UpdateAndPublishEvents(
            addedElements: new[] { element },
            removedElements: Array.Empty<TElement>(),
            restoredElements: Array.Empty<TElement>(),
            changeSource: VariableChangeSource.Change );

        SetupElementEvents( key, element );

        return VariableChangeResult.Changed;
    }

    /// <inheritdoc />
    public VariableChangeResult Add(IEnumerable<TElement> elements)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var elementsToAdd = FindElementsToAdd( elements );
        if ( elementsToAdd is null )
            return VariableChangeResult.NotChanged;

        var spanOfElementsToAdd = CollectionsMarshal.AsSpan( elementsToAdd );
        var addedElements = new TElement[spanOfElementsToAdd.Length];

        for ( var i = 0; i < spanOfElementsToAdd.Length; ++i )
        {
            var (key, element) = spanOfElementsToAdd[i];
            SetAsParentOf( element );
            _elements.AddElement( key, element );
            addedElements[i] = element;
        }

        UpdateAndPublishEvents(
            addedElements: addedElements,
            removedElements: Array.Empty<TElement>(),
            restoredElements: Array.Empty<TElement>(),
            changeSource: VariableChangeSource.Change );

        for ( var i = 0; i < spanOfElementsToAdd.Length; ++i )
        {
            var (key, element) = spanOfElementsToAdd[i];
            SetupElementEvents( key, element );
        }

        return VariableChangeResult.Changed;
    }

    /// <inheritdoc />
    public VariableChangeResult Restore(TKey key)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        if ( ! CanRestore( key, out var element ) )
            return VariableChangeResult.NotChanged;

        _elements.RestoreElement( key, element );

        UpdateAndPublishEvents(
            addedElements: Array.Empty<TElement>(),
            removedElements: Array.Empty<TElement>(),
            restoredElements: new[] { element },
            changeSource: VariableChangeSource.Change );

        SetupElementEvents( key, element );

        return VariableChangeResult.Changed;
    }

    /// <inheritdoc />
    public VariableChangeResult Restore(IEnumerable<TKey> keys)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var elementsToRestore = FindElementsToRestore( keys );
        if ( elementsToRestore is null )
            return VariableChangeResult.NotChanged;

        var spanOfElementsToRestore = CollectionsMarshal.AsSpan( elementsToRestore );
        var restoredElements = new TElement[spanOfElementsToRestore.Length];

        for ( var i = 0; i < spanOfElementsToRestore.Length; ++i )
        {
            var (key, element) = spanOfElementsToRestore[i];
            _elements.RestoreElement( key, element );
            restoredElements[i] = element;
        }

        UpdateAndPublishEvents(
            addedElements: Array.Empty<TElement>(),
            removedElements: Array.Empty<TElement>(),
            restoredElements: restoredElements,
            changeSource: VariableChangeSource.Change );

        for ( var i = 0; i < spanOfElementsToRestore.Length; ++i )
        {
            var (key, element) = spanOfElementsToRestore[i];
            SetupElementEvents( key, element );
        }

        return VariableChangeResult.Changed;
    }

    /// <inheritdoc />
    public VariableChangeResult Remove(TKey key)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        if ( ! CanRemove( key, out var element ) )
            return VariableChangeResult.NotChanged;

        DisposeElementEvents( key );
        _elements.RemoveElement( key, element );

        UpdateAndPublishEvents(
            addedElements: Array.Empty<TElement>(),
            removedElements: new[] { element },
            restoredElements: Array.Empty<TElement>(),
            changeSource: VariableChangeSource.Change );

        return VariableChangeResult.Changed;
    }

    /// <inheritdoc />
    public VariableChangeResult Remove(IEnumerable<TKey> keys)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var elementsToRemove = FindElementsToRemove( keys );
        if ( elementsToRemove is null )
            return VariableChangeResult.NotChanged;

        var spanOfElementsToRemove = CollectionsMarshal.AsSpan( elementsToRemove );
        var removedElements = new TElement[spanOfElementsToRemove.Length];

        for ( var i = 0; i < spanOfElementsToRemove.Length; ++i )
        {
            var (key, element) = spanOfElementsToRemove[i];
            DisposeElementEvents( key );
            _elements.RemoveElement( key, element );
            removedElements[i] = element;
        }

        UpdateAndPublishEvents(
            addedElements: Array.Empty<TElement>(),
            removedElements: removedElements,
            restoredElements: Array.Empty<TElement>(),
            changeSource: VariableChangeSource.Change );

        return VariableChangeResult.Changed;
    }

    /// <inheritdoc />
    public VariableChangeResult Clear()
    {
        return Change( new CollectionVariableRootChanges<TKey, TElement>( Array.Empty<TElement>(), Array.Empty<TKey>() ) );
    }

    /// <inheritdoc cref="IMutableVariableNode.Refresh()" />
    public void Refresh()
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        foreach ( var node in _elements.Elements.Values )
        {
            if ( node is IMutableVariableNode mutableNode )
                mutableNode.Refresh();
        }

        UpdateAndPublishEvents(
            addedElements: Array.Empty<TElement>(),
            removedElements: Array.Empty<TElement>(),
            restoredElements: Array.Empty<TElement>(),
            changeSource: VariableChangeSource.Refresh );
    }

    /// <inheritdoc cref="IMutableVariableNode.RefreshValidation()" />
    public void RefreshValidation()
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        foreach ( var node in _elements.Elements.Values )
        {
            if ( node is IMutableVariableNode mutableNode )
                mutableNode.RefreshValidation();
        }

        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;

        Errors = ErrorsValidator.Validate( _elements );
        Warnings = WarningsValidator.Validate( _elements );
        _state = CreateState( _state, VariableState.Invalid, Errors.Count > 0 || _elements.Invalid.Count > 0 );
        _state = CreateState( _state, VariableState.Warning, Warnings.Count > 0 || _elements.Warning.Count > 0 );

        var validationEvent = new CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult>(
            this,
            previousState,
            previousErrors,
            previousWarnings,
            null,
            null );

        OnPublishValidationEvent( validationEvent );
    }

    /// <inheritdoc cref="IMutableVariableNode.ClearValidation()" />
    public void ClearValidation()
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        foreach ( var node in _elements.Elements.Values )
        {
            if ( node is IMutableVariableNode mutableNode )
                mutableNode.ClearValidation();
        }

        if ( Errors.Count == 0 && Warnings.Count == 0 )
            return;

        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;

        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        _state = CreateState( _state, VariableState.Invalid, _elements.Invalid.Count > 0 );
        _state = CreateState( _state, VariableState.Warning, _elements.Warning.Count > 0 );

        var validationEvent = new CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult>(
            this,
            previousState,
            previousErrors,
            previousWarnings,
            null,
            null );

        OnPublishValidationEvent( validationEvent );
    }

    /// <summary>
    /// Changes the read-only state of this variable.
    /// </summary>
    /// <param name="enabled">Specifies whether or not the read-only state should be enabled.</param>
    public void SetReadOnly(bool enabled)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        if ( (_state & VariableState.ReadOnly) == (enabled ? VariableState.ReadOnly : VariableState.Default) )
            return;

        var previousState = _state;
        UpdateReadOnly( enabled );

        var changeEvent = new CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>(
            this,
            previousState,
            Array.Empty<TElement>(),
            Array.Empty<TElement>(),
            Array.Empty<TElement>(),
            VariableChangeSource.SetReadOnly );

        OnPublishChangeEvent( changeEvent );
    }

    /// <summary>
    /// Removes all errors and warnings from this variable and resets <see cref="InitialElements"/> and <see cref="Elements"/>.
    /// </summary>
    /// <param name="elements">Initial elements to set.</param>
    public void Reset(IEnumerable<TElement> elements)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        var newInitialElements = elements.Where( CanAddInitial ).ToDictionary( KeySelector, _elements.KeyComparer );
        var newElements = CreateElementsCollection( newInitialElements, _elements.KeyComparer );

        ResetAndPublishEvents( newInitialElements, newElements );
    }

    /// <summary>
    /// Removes all errors and warnings from this variable and resets <see cref="InitialElements"/> and <see cref="Elements"/>.
    /// </summary>
    /// <param name="elements">Initial elements to set.</param>
    /// <param name="elementChanges">Element changes that define the collection of current elements.</param>
    public void Reset(IEnumerable<TElement> elements, CollectionVariableRootChanges<TKey, TElement> elementChanges)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        var newInitialElements = elements.Where( CanAddInitial ).ToDictionary( KeySelector, _elements.KeyComparer );
        var newElements = CreateElementsCollection( newInitialElements, elementChanges, KeySelector, _elements.KeyComparer );

        ResetAndPublishEvents( newInitialElements, newElements );
    }

    /// <inheritdoc />
    [Pure]
    public override IEnumerable<IVariableNode> GetChildren()
    {
        return _elements.Owned.Values;
    }

    /// <summary>
    /// Performs optional element addition validation.
    /// </summary>
    /// <param name="element">Element to add.</param>
    /// <returns><b>true</b> to continue element addition, otherwise <b>false</b>.</returns>
    [Pure]
    protected virtual bool ContinueElementAddition(TElement element)
    {
        return true;
    }

    /// <summary>
    /// Performs optional element removal validation.
    /// </summary>
    /// <param name="element">Element to remove.</param>
    /// <returns><b>true</b> to continue element removal, otherwise <b>false</b>.</returns>
    [Pure]
    protected virtual bool ContinueElementRemoval(TElement element)
    {
        return true;
    }

    /// <summary>
    /// Performs optional element restoration validation.
    /// </summary>
    /// <param name="element">Element to restore.</param>
    /// <returns><b>true</b> to continue element restoration, otherwise <b>false</b>.</returns>
    [Pure]
    protected virtual bool ContinueElementRestoration(TElement element)
    {
        return true;
    }

    /// <summary>
    /// Allows to modify element changes that define the collection of current elements.
    /// </summary>
    /// <param name="changes">Element changes that define the collection of current elements.</param>
    /// <returns>Actual element changes to perform.</returns>
    [Pure]
    protected virtual CollectionVariableRootChanges<TKey, TElement> ModifyChangeInput(CollectionVariableRootChanges<TKey, TElement> changes)
    {
        return changes;
    }

    /// <summary>
    /// Updates errors, warnings and state of this variable.
    /// </summary>
    protected virtual void Update()
    {
        Errors = ErrorsValidator.Validate( _elements );
        Warnings = WarningsValidator.Validate( _elements );

        _state = CreateState(
            _state,
            VariableState.Changed,
            _elements.Changed.Count + _elements.Added.Count + _elements.Removed.Count > 0 );

        _state = CreateState( _state, VariableState.Invalid, Errors.Count > 0 || _elements.Invalid.Count > 0 );
        _state = CreateState( _state, VariableState.Warning, Warnings.Count > 0 || _elements.Warning.Count > 0 );
        _state |= VariableState.Dirty;
    }

    /// <summary>
    /// Updates state of this variable due to read-only state change.
    /// </summary>
    /// <param name="enabled">Specifies whether or not the read-only state should be enabled.</param>
    protected virtual void UpdateReadOnly(bool enabled)
    {
        _state = CreateState( _state, VariableState.ReadOnly, enabled );
    }

    /// <summary>
    /// Emits the provided change <paramref name="event"/>.
    /// </summary>
    /// <param name="event">Event to publish.</param>
    protected virtual void OnPublishChangeEvent(CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult> @event)
    {
        _onChange.Publish( @event );
    }

    /// <summary>
    /// Emits the provided validation <paramref name="event"/>.
    /// </summary>
    /// <param name="event">Event to publish.</param>
    protected virtual void OnPublishValidationEvent(CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult> @event)
    {
        _onValidate.Publish( @event );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanAddInitial(TElement element)
    {
        return (element.State & VariableState.Disposed) == VariableState.Default
            && ! ReferenceEquals( element, this )
            && element.Parent is null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanAdd(TKey key, TElement element)
    {
        return IsAdditionCandidate( key, element ) && ContinueElementAddition( element );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanRestore(TKey key, [MaybeNullWhen( false )] out TElement element)
    {
        return IsRestorationCandidate( key, out element ) && ContinueElementRestoration( element );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanRemove(TKey key, [MaybeNullWhen( false )] out TElement element)
    {
        return _elements.Elements.TryGetValue( key, out element ) && ContinueElementRemoval( element );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool IsAdditionCandidate(TKey key, TElement element)
    {
        return ! _elements.Owned.ContainsKey( key )
            && (element.State & VariableState.Disposed) == VariableState.Default
            && ! ReferenceEquals( element, this )
            && (element.Parent is null || ReferenceEquals( element.Parent, this ));
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool IsRestorationCandidate(TKey key, [MaybeNullWhen( false )] out TElement element)
    {
        if ( ! _elements.Removed.Contains( key ) )
        {
            element = default;
            return false;
        }

        element = _elements.Owned[key];
        return (element.State & VariableState.Disposed) == VariableState.Default;
    }

    private bool TryFindFirstElementToAdd(
        IEnumerator<TElement> enumerator,
        [MaybeNullWhen( false )] out TKey key,
        [MaybeNullWhen( false )] out TElement element)
    {
        if ( ! enumerator.MoveNext() )
        {
            key = default;
            element = default;
            return false;
        }

        element = enumerator.Current;
        key = KeySelector( element );
        if ( CanAdd( key, element ) )
            return true;

        while ( enumerator.MoveNext() )
        {
            element = enumerator.Current;
            key = KeySelector( element );
            if ( CanAdd( key, element ) )
                return true;
        }

        return false;
    }

    [Pure]
    private List<(TKey Key, TElement Element)>? FindElementsToAdd(IEnumerable<TElement> elements)
    {
        using var enumerator = elements.GetEnumerator();
        if ( ! TryFindFirstElementToAdd( enumerator, out var key, out var element ) )
            return null;

        var handledKeys = new HashSet<TKey>( _elements.KeyComparer ) { key };
        var result = new List<(TKey, TElement)> { (key, element) };

        while ( enumerator.MoveNext() )
        {
            element = enumerator.Current;
            key = KeySelector( element );
            if ( ! handledKeys.Contains( key ) && CanAdd( key, element ) )
            {
                handledKeys.Add( key );
                result.Add( (key, element) );
            }
        }

        return result;
    }

    private bool TryFindFirstElementToRestore(
        IEnumerator<TKey> enumerator,
        [MaybeNullWhen( false )] out TKey key,
        [MaybeNullWhen( false )] out TElement element)
    {
        if ( ! enumerator.MoveNext() )
        {
            key = default;
            element = default;
            return false;
        }

        key = enumerator.Current;
        if ( CanRestore( key, out element ) )
            return true;

        while ( enumerator.MoveNext() )
        {
            key = enumerator.Current;
            if ( CanRestore( key, out element ) )
                return true;
        }

        return false;
    }

    [Pure]
    private List<(TKey Key, TElement Element)>? FindElementsToRestore(IEnumerable<TKey> keys)
    {
        using var enumerator = keys.GetEnumerator();
        if ( ! TryFindFirstElementToRestore( enumerator, out var key, out var element ) )
            return null;

        var handledKeys = new HashSet<TKey>( _elements.KeyComparer ) { key };
        var result = new List<(TKey, TElement)> { (key, element) };

        while ( enumerator.MoveNext() )
        {
            key = enumerator.Current;
            if ( ! handledKeys.Contains( key ) && CanRestore( key, out element ) )
            {
                handledKeys.Add( key );
                result.Add( (key, element) );
            }
        }

        return result;
    }

    private bool TryFindFirstElementToRemove(
        IEnumerator<TKey> enumerator,
        [MaybeNullWhen( false )] out TKey key,
        [MaybeNullWhen( false )] out TElement element)
    {
        if ( ! enumerator.MoveNext() )
        {
            key = default;
            element = default;
            return false;
        }

        key = enumerator.Current;
        if ( CanRemove( key, out element ) )
            return true;

        while ( enumerator.MoveNext() )
        {
            key = enumerator.Current;
            if ( CanRemove( key, out element ) )
                return true;
        }

        return false;
    }

    [Pure]
    private List<(TKey Key, TElement Element)>? FindElementsToRemove(IEnumerable<TKey> keys)
    {
        using var enumerator = keys.GetEnumerator();
        if ( ! TryFindFirstElementToRemove( enumerator, out var key, out var element ) )
            return null;

        var handledKeys = new HashSet<TKey>( _elements.KeyComparer ) { key };
        var result = new List<(TKey, TElement)> { (key, element) };

        while ( enumerator.MoveNext() )
        {
            key = enumerator.Current;
            if ( ! handledKeys.Contains( key ) && CanRemove( key, out element ) )
            {
                handledKeys.Add( key );
                result.Add( (key, element) );
            }
        }

        return result;
    }

    [Pure]
    private List<ElementChangeDto> FindElementsToChange(CollectionVariableRootChanges<TKey, TElement> changes)
    {
        var handledKeys = new Dictionary<TKey, ElementChangeType>( _elements.KeyComparer );
        var result = new List<ElementChangeDto>();

        foreach ( var key in changes.KeysToRestore )
        {
            if ( handledKeys.ContainsKey( key ) )
                continue;

            if ( _elements.Elements.ContainsKey( key ) )
            {
                handledKeys.Add( key, ElementChangeType.Restore );
                continue;
            }

            if ( IsRestorationCandidate( key, out var element ) )
            {
                handledKeys.Add( key, ElementChangeType.Restore );
                result.Add( new ElementChangeDto( ElementChangeType.Restore, key, element ) );
            }
        }

        foreach ( var (key, element) in _elements.Elements )
        {
            if ( handledKeys.TryAdd( key, ElementChangeType.Remove ) )
                result.Add( new ElementChangeDto( ElementChangeType.Remove, key, element ) );
        }

        foreach ( var element in changes.ElementsToAdd )
        {
            var key = KeySelector( element );

            if ( ! handledKeys.TryGetValue( key, out var change ) )
            {
                if ( IsAdditionCandidate( key, element ) )
                {
                    handledKeys.Add( key, ElementChangeType.Add );
                    result.Add( new ElementChangeDto( ElementChangeType.Add, key, element ) );
                }

                continue;
            }

            if ( change == ElementChangeType.Add || InitialElements.ContainsKey( key ) )
                continue;

            var currentElement = _elements.Elements[key];
            if ( ReferenceEquals( element, currentElement ) || ! CanAddInitial( element ) )
                continue;

            if ( change == ElementChangeType.Restore )
                result.Add( new ElementChangeDto( ElementChangeType.Remove, key, currentElement ) );

            handledKeys[key] = ElementChangeType.Add;
            result.Add( new ElementChangeDto( ElementChangeType.Add, key, element ) );
        }

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private VariableState CreateInitialState(VariableState @base)
    {
        var result = CreateState(
            @base,
            VariableState.Changed,
            _elements.Added.Count + _elements.Removed.Count + _elements.Changed.Count > 0 );

        result = CreateState( result, VariableState.Invalid, _elements.Invalid.Count > 0 );
        result = CreateState( result, VariableState.Warning, _elements.Warning.Count > 0 );

        return result;
    }

    private CollectionVariableRootElements<TKey, TElement> CreateElementsCollection(
        IReadOnlyDictionary<TKey, TElement> initialElements,
        IEqualityComparer<TKey>? keyComparer)
    {
        var result = new CollectionVariableRootElements<TKey, TElement>( keyComparer );

        foreach ( var (key, element) in initialElements )
        {
            SetAsParentOf( element );
            result.AddInitialElement( key, element );
        }

        return result;
    }

    private CollectionVariableRootElements<TKey, TElement> CreateElementsCollection(
        IReadOnlyDictionary<TKey, TElement> initialElements,
        CollectionVariableRootChanges<TKey, TElement> elementChanges,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer)
    {
        var result = new CollectionVariableRootElements<TKey, TElement>( keyComparer );
        var keysToRestore = elementChanges.KeysToRestore.ToHashSet( keyComparer );

        foreach ( var (key, element) in initialElements )
        {
            SetAsParentOf( element );
            if ( ! keysToRestore.Contains( key ) )
            {
                result.AddInitialElementAsRemoved( key, element );
                continue;
            }

            result.AddInitialElement( key, element );
        }

        foreach ( var element in elementChanges.ElementsToAdd )
        {
            if ( ! CanAddInitial( element ) )
                continue;

            var key = keySelector( element );
            if ( result.TryAddInitialElementAsAdded( key, element ) )
                SetAsParentOf( element );
        }

        return result;
    }

    private void DisposeElements()
    {
        foreach ( var (onChange, onValidate) in _elements.Subscribers.Values )
        {
            onChange.Dispose();
            onValidate.Dispose();
        }

        foreach ( var element in _elements.Owned.Values )
        {
            if ( element is IDisposable disposable )
                disposable.Dispose();
        }

        _elements.Subscribers.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void SetupElementEvents(TKey key, TElement element)
    {
        var onChangeSubscriber = element.OnChange.Listen( new OnElementChangeListener( this, key ) );
        var onValidateSubscriber = element.OnValidate.Listen( new OnElementValidationListener( this, key ) );
        _elements.Subscribers.Add( key, (onChangeSubscriber, onValidateSubscriber) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void DisposeElementEvents(TKey key)
    {
        if ( _elements.Subscribers.Remove( key, out var subscribers ) )
        {
            subscribers.OnChange.Dispose();
            subscribers.OnValidate.Dispose();
        }
    }

    private void UpdateAndPublishEvents(
        IReadOnlyList<TElement> addedElements,
        IReadOnlyList<TElement> removedElements,
        IReadOnlyList<TElement> restoredElements,
        VariableChangeSource changeSource)
    {
        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;

        Update();

        var changeEvent = new CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>(
            variable: this,
            previousState: previousState,
            addedElements: addedElements,
            removedElements: removedElements,
            restoredElements: restoredElements,
            source: changeSource );

        OnPublishChangeEvent( changeEvent );

        var validateEvent = new CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult>(
            variable: this,
            previousState: previousState,
            previousErrors: previousErrors,
            previousWarnings: previousWarnings,
            associatedChange: changeEvent,
            sourceEvent: null );

        OnPublishValidationEvent( validateEvent );
    }

    private void UpdateAndPublishChangeEvents(List<ElementChangeDto> changes)
    {
        var spanOfElementsToChange = CollectionsMarshal.AsSpan( changes );
        var toAddCount = 0;
        var toRemoveCount = 0;
        var toRestoreCount = 0;

        for ( var i = 0; i < spanOfElementsToChange.Length; ++i )
        {
            var dto = spanOfElementsToChange[i];

            switch ( dto.ChangeType )
            {
                case ElementChangeType.Add:
                    ++toAddCount;
                    break;
                case ElementChangeType.Remove:
                    ++toRemoveCount;
                    break;
                default:
                    ++toRestoreCount;
                    break;
            }
        }

        var addIndex = 0;
        var addedElements = toAddCount == 0 ? Array.Empty<TElement>() : new TElement[toAddCount];

        var removeIndex = 0;
        var removedElements = toRemoveCount == 0 ? Array.Empty<TElement>() : new TElement[toRemoveCount];

        var restoreIndex = 0;
        var restoredElements = toRestoreCount == 0 ? Array.Empty<TElement>() : new TElement[toRestoreCount];

        for ( var i = 0; i < spanOfElementsToChange.Length; ++i )
        {
            var dto = spanOfElementsToChange[i];

            switch ( dto.ChangeType )
            {
                case ElementChangeType.Add:
                    SetAsParentOf( dto.Element );
                    _elements.AddElement( dto.Key, dto.Element );
                    addedElements[addIndex++] = dto.Element;
                    break;
                case ElementChangeType.Remove:
                    DisposeElementEvents( dto.Key );
                    _elements.RemoveElement( dto.Key, dto.Element );
                    removedElements[removeIndex++] = dto.Element;
                    break;
                default:
                    _elements.RestoreElement( dto.Key, dto.Element );
                    restoredElements[restoreIndex++] = dto.Element;
                    break;
            }
        }

        UpdateAndPublishEvents(
            addedElements: addedElements,
            removedElements: removedElements,
            restoredElements: restoredElements,
            changeSource: VariableChangeSource.Change );

        for ( var i = 0; i < spanOfElementsToChange.Length; ++i )
        {
            var dto = spanOfElementsToChange[i];

            switch ( dto.ChangeType )
            {
                case ElementChangeType.Add:
                case ElementChangeType.Restore:
                    SetupElementEvents( dto.Key, dto.Element );
                    break;
            }
        }
    }

    private void ResetAndPublishEvents(
        IReadOnlyDictionary<TKey, TElement> initialElements,
        CollectionVariableRootElements<TKey, TElement> elements)
    {
        var index = 0;
        var removedElements = _elements.Count == 0 ? Array.Empty<TElement>() : new TElement[_elements.Count];
        foreach ( var element in _elements.Elements.Values )
            removedElements[index++] = element;

        index = 0;
        var addedElements = elements.Count == 0 ? Array.Empty<TElement>() : new TElement[elements.Count];
        foreach ( var element in elements.Elements.Values )
            addedElements[index++] = element;

        DisposeElements();

        var previousState = _state;
        var previousErrors = Errors;
        var previousWarnings = Warnings;

        InitialElements = initialElements;
        _elements = elements;
        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        _state = CreateInitialState( _state & VariableState.ReadOnly );

        var changeEvent = new CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>(
            variable: this,
            previousState: previousState,
            addedElements: addedElements,
            removedElements: removedElements,
            restoredElements: Array.Empty<TElement>(),
            source: VariableChangeSource.Reset );

        OnPublishChangeEvent( changeEvent );

        var validateEvent = new CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult>(
            variable: this,
            previousState: previousState,
            previousErrors: previousErrors,
            previousWarnings: previousWarnings,
            associatedChange: changeEvent,
            sourceEvent: null );

        OnPublishValidationEvent( validateEvent );

        foreach ( var (key, element) in _elements.Elements )
            SetupElementEvents( key, element );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void UpdateState(
        HashSet<TKey> set,
        TKey elementKey,
        VariableState elementState,
        VariableState value,
        bool isEnabledInCollectionOnly)
    {
        if ( (elementState & value) != VariableState.Default )
        {
            set.Add( elementKey );
            _state |= value;
            return;
        }

        if ( set.Remove( elementKey ) && set.Count == 0 )
            _state = CreateState( _state, value, isEnabledInCollectionOnly );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void OnElementChanged(TKey elementKey, IVariableNodeEvent @event)
    {
        var previousState = _state;

        UpdateState(
            _elements.Changed,
            elementKey,
            @event.NewState,
            VariableState.Changed,
            _elements.Added.Count + _elements.Removed.Count > 0 );

        var changeEvent = new CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>(
            variable: this,
            previousState: previousState,
            addedElements: Array.Empty<TElement>(),
            removedElements: Array.Empty<TElement>(),
            restoredElements: Array.Empty<TElement>(),
            sourceEvent: @event );

        OnPublishChangeEvent( changeEvent );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void OnElementValidated(TKey elementKey, IVariableNodeEvent @event)
    {
        var previousState = _state;

        UpdateState( _elements.Invalid, elementKey, @event.NewState, VariableState.Invalid, Errors.Count > 0 );
        UpdateState( _elements.Warning, elementKey, @event.NewState, VariableState.Warning, Warnings.Count > 0 );

        var validationEvent = new CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult>(
            variable: this,
            previousState: previousState,
            previousErrors: Errors,
            previousWarnings: Warnings,
            associatedChange: null,
            sourceEvent: @event );

        OnPublishValidationEvent( validationEvent );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void OnElementDisposed(TKey elementKey)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default || ! _elements.Elements.TryGetValue( elementKey, out var element ) )
            return;

        DisposeElementEvents( elementKey );
        _elements.RemoveElement( elementKey, element );

        UpdateAndPublishEvents(
            addedElements: Array.Empty<TElement>(),
            removedElements: new[] { element },
            restoredElements: Array.Empty<TElement>(),
            changeSource: VariableChangeSource.Change );
    }

    private sealed class OnElementChangeListener : EventListener<IVariableNodeEvent>
    {
        private readonly CollectionVariableRoot<TKey, TElement, TValidationResult> _root;
        private readonly TKey _elementKey;

        internal OnElementChangeListener(CollectionVariableRoot<TKey, TElement, TValidationResult> root, TKey elementKey)
        {
            _root = root;
            _elementKey = elementKey;
        }

        public override void React(IVariableNodeEvent @event)
        {
            _root.OnElementChanged( _elementKey, @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            if ( source == DisposalSource.EventSource )
                _root.OnElementDisposed( _elementKey );
        }
    }

    private sealed class OnElementValidationListener : EventListener<IVariableNodeEvent>
    {
        private readonly CollectionVariableRoot<TKey, TElement, TValidationResult> _root;
        private readonly TKey _elementKey;

        internal OnElementValidationListener(CollectionVariableRoot<TKey, TElement, TValidationResult> root, TKey elementKey)
        {
            _root = root;
            _elementKey = elementKey;
        }

        public override void React(IVariableNodeEvent @event)
        {
            _root.OnElementValidated( _elementKey, @event );
        }

        public override void OnDispose(DisposalSource source) { }
    }

    private readonly struct ElementChangeDto
    {
        internal readonly ElementChangeType ChangeType;
        internal readonly TKey Key;
        internal readonly TElement Element;

        internal ElementChangeDto(ElementChangeType changeType, TKey key, TElement element)
        {
            ChangeType = changeType;
            Key = key;
            Element = element;
        }
    }

    private enum ElementChangeType : byte
    {
        Add = 0,
        Remove = 1,
        Restore = 2
    }
}
