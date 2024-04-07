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

public class CollectionVariable<TKey, TElement, TValidationResult>
    : VariableNode, ICollectionVariable<TKey, TElement, TValidationResult>, IMutableVariableNode, IDisposable
    where TKey : notnull
    where TElement : notnull
{
    private readonly EventPublisher<CollectionVariableChangeEvent<TKey, TElement, TValidationResult>> _onChange;
    private readonly EventPublisher<CollectionVariableValidationEvent<TKey, TElement, TValidationResult>> _onValidate;
    private ElementsCollection _elements;
    private VariableState _state;

    public CollectionVariable(
        IEnumerable<TElement> initialElements,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TElement>? elementComparer = null,
        IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult>? errorsValidator = null,
        IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult>? warningsValidator = null,
        IValidator<TElement, TValidationResult>? elementErrorsValidator = null,
        IValidator<TElement, TValidationResult>? elementWarningsValidator = null)
    {
        InitialElements = initialElements.ToDictionary( keySelector, keyComparer );

        _elements = new ElementsCollection(
            InitialElements,
            keyComparer,
            elementComparer,
            elementErrorsValidator,
            elementWarningsValidator );

        KeySelector = keySelector;

        ErrorsValidator = errorsValidator
            ?? Validators<TValidationResult>.Pass<ICollectionVariableElements<TKey, TElement, TValidationResult>>();

        WarningsValidator = warningsValidator
            ?? Validators<TValidationResult>.Pass<ICollectionVariableElements<TKey, TElement, TValidationResult>>();

        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        _state = _elements.Modified.Count == 0 ? VariableState.Default : VariableState.Changed;
        _onChange = new EventPublisher<CollectionVariableChangeEvent<TKey, TElement, TValidationResult>>();
        _onValidate = new EventPublisher<CollectionVariableValidationEvent<TKey, TElement, TValidationResult>>();
    }

    public CollectionVariable(
        IEnumerable<TElement> initialElements,
        IEnumerable<TElement> elements,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TElement>? elementComparer = null,
        IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult>? errorsValidator = null,
        IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult>? warningsValidator = null,
        IValidator<TElement, TValidationResult>? elementErrorsValidator = null,
        IValidator<TElement, TValidationResult>? elementWarningsValidator = null)
    {
        InitialElements = initialElements.ToDictionary( keySelector, keyComparer );

        _elements = new ElementsCollection(
            InitialElements,
            elements,
            keySelector,
            keyComparer,
            elementComparer,
            elementErrorsValidator,
            elementWarningsValidator );

        KeySelector = keySelector;

        ErrorsValidator = errorsValidator
            ?? Validators<TValidationResult>.Pass<ICollectionVariableElements<TKey, TElement, TValidationResult>>();

        WarningsValidator = warningsValidator
            ?? Validators<TValidationResult>.Pass<ICollectionVariableElements<TKey, TElement, TValidationResult>>();

        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        _state = _elements.Modified.Count == 0 ? VariableState.Default : VariableState.Changed;
        _onChange = new EventPublisher<CollectionVariableChangeEvent<TKey, TElement, TValidationResult>>();
        _onValidate = new EventPublisher<CollectionVariableValidationEvent<TKey, TElement, TValidationResult>>();
    }

    public Func<TElement, TKey> KeySelector { get; }
    public Chain<TValidationResult> Errors { get; private set; }
    public Chain<TValidationResult> Warnings { get; private set; }
    public IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult> ErrorsValidator { get; }
    public IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult> WarningsValidator { get; }
    public IReadOnlyDictionary<TKey, TElement> InitialElements { get; private set; }
    public ICollectionVariableElements<TKey, TElement, TValidationResult> Elements => _elements;
    public sealed override VariableState State => _state;
    public sealed override IEventStream<CollectionVariableChangeEvent<TKey, TElement, TValidationResult>> OnChange => _onChange;
    public sealed override IEventStream<CollectionVariableValidationEvent<TKey, TElement, TValidationResult>> OnValidate => _onValidate;

    ICollectionVariableElements<TKey, TElement> IReadOnlyCollectionVariable<TKey, TElement>.Elements => _elements;
    IEventStream<ICollectionVariableChangeEvent<TKey, TElement>> IReadOnlyCollectionVariable<TKey, TElement>.OnChange => _onChange;

    Type IReadOnlyCollectionVariable.KeyType => typeof( TKey );
    Type IReadOnlyCollectionVariable.ElementType => typeof( TElement );
    Type IReadOnlyCollectionVariable.ValidationResultType => typeof( TValidationResult );
    ICollectionVariableElements IReadOnlyCollectionVariable.Elements => _elements;
    IEnumerable IReadOnlyCollectionVariable.InitialElements => InitialElements.Values;
    IEnumerable IReadOnlyCollectionVariable.Errors => Errors;
    IEnumerable IReadOnlyCollectionVariable.Warnings => Warnings;
    IEventStream<ICollectionVariableChangeEvent> IReadOnlyCollectionVariable.OnChange => _onChange;
    IEventStream<ICollectionVariableValidationEvent> IReadOnlyCollectionVariable.OnValidate => _onValidate;

    [Pure]
    public override string ToString()
    {
        return $"{nameof( Elements )}: {_elements.Count}, {nameof( State )}: {_state}";
    }

    public virtual void Dispose()
    {
        _state |= VariableState.ReadOnly | VariableState.Disposed;
        _onChange.Dispose();
        _onValidate.Dispose();
    }

    public VariableChangeResult TryChange(IEnumerable<TElement> elements)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var elementsToChange = FindElementsToTryChange( ModifyChangeInput( elements ) );
        if ( elementsToChange is null )
            return VariableChangeResult.NotChanged;

        UpdateAndPublishEvents( elementsToChange.Value, VariableChangeSource.TryChange );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult Change(IEnumerable<TElement> elements)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var elementsToChange = FindElementsToChange( ModifyChangeInput( elements ) );
        if ( elementsToChange is null )
            return VariableChangeResult.NotChanged;

        UpdateAndPublishEvents( elementsToChange.Value, VariableChangeSource.Change );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult Add(TElement element)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var key = KeySelector( element );
        if ( ! CanAdd( key, element ) )
            return VariableChangeResult.NotChanged;

        var elementSnapshot = new[] { AddElementAndCreateSnapshot( key, element ) };
        UpdateAndPublishAdditionEvents( elementSnapshot, VariableChangeSource.Change );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult Add(IEnumerable<TElement> elements)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var elementsToAdd = FindElementsToAdd( elements );
        if ( elementsToAdd is null )
            return VariableChangeResult.NotChanged;

        var spanOfElementsToAdd = CollectionsMarshal.AsSpan( elementsToAdd );
        var elementSnapshots = new CollectionVariableElementSnapshot<TElement, TValidationResult>[spanOfElementsToAdd.Length];

        for ( var i = 0; i < spanOfElementsToAdd.Length; ++i )
        {
            var (key, element) = spanOfElementsToAdd[i];
            var elementSnapshot = AddElementAndCreateSnapshot( key, element );
            elementSnapshots[i] = elementSnapshot;
        }

        UpdateAndPublishAdditionEvents( elementSnapshots, VariableChangeSource.Change );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult TryReplace(TElement element)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var key = KeySelector( element );
        if ( ! CanTryReplace( key, element, out var current ) )
            return VariableChangeResult.NotChanged;

        var elementSnapshot = new[] { ReplaceElementAndCreateSnapshot( key, element, current ) };
        UpdateAndPublishReplacementEvents( elementSnapshot, VariableChangeSource.TryChange );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult TryReplace(IEnumerable<TElement> elements)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var elementsToReplace = FindElementsToTryReplace( elements );
        if ( elementsToReplace is null )
            return VariableChangeResult.NotChanged;

        var spanOfElementsToReplace = CollectionsMarshal.AsSpan( elementsToReplace );
        var elementSnapshots = new CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>[spanOfElementsToReplace.Length];

        for ( var i = 0; i < spanOfElementsToReplace.Length; ++i )
        {
            var (key, element, replacedElement) = spanOfElementsToReplace[i];
            var elementSnapshot = ReplaceElementAndCreateSnapshot( key, element, replacedElement );
            elementSnapshots[i] = elementSnapshot;
        }

        UpdateAndPublishReplacementEvents( elementSnapshots, VariableChangeSource.TryChange );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult Replace(TElement element)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var key = KeySelector( element );
        if ( ! CanReplace( key, element, out var current ) )
            return VariableChangeResult.NotChanged;

        var elementSnapshot = new[] { ReplaceElementAndCreateSnapshot( key, element, current ) };
        UpdateAndPublishReplacementEvents( elementSnapshot, VariableChangeSource.Change );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult Replace(IEnumerable<TElement> elements)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var elementsToReplace = FindElementsToReplace( elements );
        if ( elementsToReplace is null )
            return VariableChangeResult.NotChanged;

        var spanOfElementsToReplace = CollectionsMarshal.AsSpan( elementsToReplace );
        var elementSnapshots = new CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>[spanOfElementsToReplace.Length];

        for ( var i = 0; i < spanOfElementsToReplace.Length; ++i )
        {
            var (key, element, replacedElement) = spanOfElementsToReplace[i];
            var elementSnapshot = ReplaceElementAndCreateSnapshot( key, element, replacedElement );
            elementSnapshots[i] = elementSnapshot;
        }

        UpdateAndPublishReplacementEvents( elementSnapshots, VariableChangeSource.Change );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult AddOrTryReplace(TElement element)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var key = KeySelector( element );
        if ( ! CanAddOrTryReplace( key, element, out var changeDto ) )
            return VariableChangeResult.NotChanged;

        if ( changeDto.ChangeType == ElementChangeType.Add )
        {
            var addedElementSnapshot = new[] { AddElementAndCreateSnapshot( key, element ) };
            UpdateAndPublishAdditionEvents( addedElementSnapshot, VariableChangeSource.TryChange );
        }
        else
        {
            var replacedElementSnapshot = new[] { ReplaceElementAndCreateSnapshot( key, element, changeDto.PreviousElement! ) };
            UpdateAndPublishReplacementEvents( replacedElementSnapshot, VariableChangeSource.TryChange );
        }

        return VariableChangeResult.Changed;
    }

    public VariableChangeResult AddOrTryReplace(IEnumerable<TElement> elements)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var elementsToAddOrReplace = FindElementsToAddOrTryReplace( elements );
        if ( elementsToAddOrReplace is null )
            return VariableChangeResult.NotChanged;

        UpdateAndPublishEvents( elementsToAddOrReplace.Value, VariableChangeSource.TryChange );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult AddOrReplace(TElement element)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var key = KeySelector( element );
        if ( ! CanAddOrReplace( key, element, out var changeDto ) )
            return VariableChangeResult.NotChanged;

        if ( changeDto.ChangeType == ElementChangeType.Add )
        {
            var addedElementSnapshot = new[] { AddElementAndCreateSnapshot( key, element ) };
            UpdateAndPublishAdditionEvents( addedElementSnapshot, VariableChangeSource.Change );
        }
        else
        {
            var replacedElementSnapshot = new[] { ReplaceElementAndCreateSnapshot( key, element, changeDto.PreviousElement! ) };
            UpdateAndPublishReplacementEvents( replacedElementSnapshot, VariableChangeSource.Change );
        }

        return VariableChangeResult.Changed;
    }

    public VariableChangeResult AddOrReplace(IEnumerable<TElement> elements)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var elementsToAddOrReplace = FindElementsToAddOrReplace( elements );
        if ( elementsToAddOrReplace is null )
            return VariableChangeResult.NotChanged;

        UpdateAndPublishEvents( elementsToAddOrReplace.Value, VariableChangeSource.Change );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult Remove(TKey key)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        if ( ! CanRemove( key, out var element ) )
            return VariableChangeResult.NotChanged;

        var elementSnapshot = new[] { RemoveElementAndCreateSnapshot( key, element ) };
        UpdateAndPublishRemovalEvents( elementSnapshot );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult Remove(IEnumerable<TKey> keys)
    {
        if ( (_state & VariableState.ReadOnly) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        var elementsToRemove = FindElementsToRemove( keys );
        if ( elementsToRemove is null )
            return VariableChangeResult.NotChanged;

        var spanOfElementsToRemove = CollectionsMarshal.AsSpan( elementsToRemove );
        var elementSnapshots = new CollectionVariableElementSnapshot<TElement, TValidationResult>[spanOfElementsToRemove.Length];

        for ( var i = 0; i < spanOfElementsToRemove.Length; ++i )
        {
            var (key, element) = spanOfElementsToRemove[i];
            var elementSnapshot = RemoveElementAndCreateSnapshot( key, element );
            elementSnapshots[i] = elementSnapshot;
        }

        UpdateAndPublishRemovalEvents( elementSnapshots );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult Clear()
    {
        return Change( Array.Empty<TElement>() );
    }

    public void Refresh()
    {
        Refresh( _elements.Elements.Keys );
    }

    public void Refresh(TKey key)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        if ( ! CanRefresh( key, out var element ) )
            return;

        var elementSnapshot = new[] { RefreshElementAndCreateSnapshot( key, element ) };
        UpdateAndPublishRefreshmentEvents( elementSnapshot );
    }

    public void Refresh(IEnumerable<TKey> keys)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        var elementsToRefresh = FindElementsToRefresh( keys );
        if ( elementsToRefresh is null )
        {
            RefreshValidationAndPublishEvent( Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>() );
            return;
        }

        var spanOfElementsToRefresh = CollectionsMarshal.AsSpan( elementsToRefresh );
        var elementSnapshots = new CollectionVariableElementSnapshot<TElement, TValidationResult>[spanOfElementsToRefresh.Length];

        for ( var i = 0; i < spanOfElementsToRefresh.Length; ++i )
        {
            var (key, element) = spanOfElementsToRefresh[i];
            var elementSnapshot = RefreshElementAndCreateSnapshot( key, element );
            elementSnapshots[i] = elementSnapshot;
        }

        UpdateAndPublishRefreshmentEvents( elementSnapshots );
    }

    public void RefreshValidation()
    {
        RefreshValidation( _elements.Elements.Keys );
    }

    public void RefreshValidation(TKey key)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        if ( ! CanRefresh( key, out var element ) )
            return;

        var elementSnapshot = new[] { RefreshElementValidationAndCreateSnapshot( key, element ) };
        RefreshValidationAndPublishEvent( elementSnapshot );
    }

    public void RefreshValidation(IEnumerable<TKey> keys)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        var elementsToRefresh = FindElementsToRefresh( keys );
        if ( elementsToRefresh is null )
        {
            RefreshValidationAndPublishEvent( Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>() );
            return;
        }

        var spanOfElementsToRefresh = CollectionsMarshal.AsSpan( elementsToRefresh );
        var elementSnapshots = new CollectionVariableElementSnapshot<TElement, TValidationResult>[spanOfElementsToRefresh.Length];

        for ( var i = 0; i < spanOfElementsToRefresh.Length; ++i )
        {
            var (key, element) = spanOfElementsToRefresh[i];
            var elementSnapshot = RefreshElementValidationAndCreateSnapshot( key, element );
            elementSnapshots[i] = elementSnapshot;
        }

        RefreshValidationAndPublishEvent( elementSnapshots );
    }

    public void ClearValidation()
    {
        ClearValidation( _elements.Elements.Keys );
    }

    public void ClearValidation(TKey key)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        if ( ! CanClearValidation( key, out var element ) )
            return;

        var elementSnapshot = new[] { ClearElementValidationAndCreateSnapshot( key, element ) };
        ClearValidationAndPublishEvent( elementSnapshot );
    }

    public void ClearValidation(IEnumerable<TKey> keys)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        var elementsToClear = FindElementsToClearValidation( keys );
        if ( elementsToClear is null )
        {
            ClearValidationAndPublishEvent( Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>() );
            return;
        }

        var spanOfElementsToClear = CollectionsMarshal.AsSpan( elementsToClear );
        var elementSnapshots = new CollectionVariableElementSnapshot<TElement, TValidationResult>[spanOfElementsToClear.Length];

        for ( var i = 0; i < spanOfElementsToClear.Length; ++i )
        {
            var (key, element) = spanOfElementsToClear[i];
            var elementSnapshot = ClearElementValidationAndCreateSnapshot( key, element );
            elementSnapshots[i] = elementSnapshot;
        }

        ClearValidationAndPublishEvent( elementSnapshots );
    }

    public void Reset(IEnumerable<TElement> initialElements)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        var newInitialElements = initialElements.ToDictionary( KeySelector, _elements.KeyComparer );
        var newElements = new ElementsCollection(
            newInitialElements,
            _elements.KeyComparer,
            _elements.ElementComparer,
            _elements.ErrorsValidator,
            _elements.WarningsValidator );

        ResetAndPublishEvents( newInitialElements, newElements );
    }

    public void Reset(IEnumerable<TElement> initialElements, IEnumerable<TElement> elements)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        var newInitialElements = initialElements.ToDictionary( KeySelector, _elements.KeyComparer );
        var newElements = new ElementsCollection(
            newInitialElements,
            elements,
            KeySelector,
            _elements.KeyComparer,
            _elements.ElementComparer,
            _elements.ErrorsValidator,
            _elements.WarningsValidator );

        ResetAndPublishEvents( newInitialElements, newElements );
    }

    public void SetReadOnly(bool enabled)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        if ( (_state & VariableState.ReadOnly) == (enabled ? VariableState.ReadOnly : VariableState.Default) )
            return;

        var previousState = _state;
        UpdateReadOnly( enabled );

        var changeEvent = new CollectionVariableChangeEvent<TKey, TElement, TValidationResult>(
            this,
            previousState,
            Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>(),
            Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>(),
            Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>(),
            Array.Empty<CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>>(),
            VariableChangeSource.SetReadOnly );

        OnPublishChangeEvent( changeEvent );
    }

    [Pure]
    public override IEnumerable<IVariableNode> GetChildren()
    {
        return Array.Empty<IVariableNode>();
    }

    [Pure]
    protected virtual bool ContinueElementAddition(TElement element)
    {
        return true;
    }

    [Pure]
    protected virtual bool ContinueElementRemoval(TElement element)
    {
        return true;
    }

    [Pure]
    protected virtual bool ContinueElementReplacement(TElement element, TElement replacement)
    {
        return true;
    }

    [Pure]
    protected virtual IEnumerable<TElement> ModifyChangeInput(IEnumerable<TElement> elements)
    {
        return elements;
    }

    protected virtual void Update()
    {
        Errors = ErrorsValidator.Validate( _elements );
        Warnings = WarningsValidator.Validate( _elements );
        _state = CreateState( _state, VariableState.Changed, _elements.Modified.Count > 0 );
        _state = CreateState( _state, VariableState.Invalid, Errors.Count > 0 || _elements.Invalid.Count > 0 );
        _state = CreateState( _state, VariableState.Warning, Warnings.Count > 0 || _elements.Warning.Count > 0 );
        _state |= VariableState.Dirty;
    }

    protected virtual void UpdateReadOnly(bool enabled)
    {
        _state = CreateState( _state, VariableState.ReadOnly, enabled );
    }

    protected virtual void OnPublishChangeEvent(CollectionVariableChangeEvent<TKey, TElement, TValidationResult> @event)
    {
        _onChange.Publish( @event );
    }

    protected virtual void OnPublishValidationEvent(CollectionVariableValidationEvent<TKey, TElement, TValidationResult> @event)
    {
        _onValidate.Publish( @event );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanAdd(TKey key, TElement element)
    {
        return ! _elements.Elements.ContainsKey( key ) && ContinueElementAddition( element );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanRemove(TKey key, [MaybeNullWhen( false )] out TElement element)
    {
        return _elements.Elements.TryGetValue( key, out element ) && ContinueElementRemoval( element );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanReplace(TKey key, TElement element, [MaybeNullWhen( false )] out TElement current)
    {
        return _elements.Elements.TryGetValue( key, out current ) && ContinueElementReplacement( current, element );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanTryReplace(TKey key, TElement element, [MaybeNullWhen( false )] out TElement current)
    {
        return _elements.Elements.TryGetValue( key, out current )
            && ! _elements.ElementComparer.Equals( current, element )
            && ContinueElementReplacement( current, element );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanAddOrReplace(TKey key, TElement element, out ElementChangeDto changeDto)
    {
        changeDto = default;
        if ( _elements.Elements.TryGetValue( key, out var current ) )
        {
            if ( ! ContinueElementReplacement( current, element ) )
                return false;

            changeDto = new ElementChangeDto( ElementChangeType.Replace, key, element, current );
            return true;
        }

        if ( ! ContinueElementAddition( element ) )
            return false;

        changeDto = new ElementChangeDto( ElementChangeType.Add, key, element );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanAddOrTryReplace(TKey key, TElement element, out ElementChangeDto changeDto)
    {
        changeDto = default;
        if ( _elements.Elements.TryGetValue( key, out var current ) )
        {
            if ( _elements.ElementComparer.Equals( current, element ) || ! ContinueElementReplacement( current, element ) )
                return false;

            changeDto = new ElementChangeDto( ElementChangeType.Replace, key, element, current );
            return true;
        }

        if ( ! ContinueElementAddition( element ) )
            return false;

        changeDto = new ElementChangeDto( ElementChangeType.Add, key, element );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanRefresh(TKey key, [MaybeNullWhen( false )] out TElement element)
    {
        return _elements.Elements.TryGetValue( key, out element );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanClearValidation(TKey key, [MaybeNullWhen( false )] out TElement element)
    {
        if ( ! _elements.Elements.TryGetValue( key, out element ) )
            return false;

        var info = _elements.Info[key];
        return info.Errors.Count != 0 || info.Warnings.Count != 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ElementChangeDto CreateChange(TKey key, TElement element)
    {
        return _elements.Elements.TryGetValue( key, out var current )
            ? new ElementChangeDto( ElementChangeType.Replace, key, element, current )
            : new ElementChangeDto( ElementChangeType.Add, key, element );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ElementChangeDto CreateChangeAttempt(TKey key, TElement element)
    {
        if ( ! _elements.Elements.TryGetValue( key, out var current ) )
            return new ElementChangeDto( ElementChangeType.Add, key, element );

        if ( _elements.ElementComparer.Equals( current, element ) )
            return new ElementChangeDto( ElementChangeType.Refresh, key, current );

        return new ElementChangeDto( ElementChangeType.Replace, key, element, current );
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

    private bool TryFindFirstElementToReplace(
        IEnumerator<TElement> enumerator,
        [MaybeNullWhen( false )] out TKey key,
        [MaybeNullWhen( false )] out TElement element,
        [MaybeNullWhen( false )] out TElement replacedElement)
    {
        if ( ! enumerator.MoveNext() )
        {
            key = default;
            element = default;
            replacedElement = default;
            return false;
        }

        element = enumerator.Current;
        key = KeySelector( element );
        if ( CanReplace( key, element, out replacedElement ) )
            return true;

        while ( enumerator.MoveNext() )
        {
            element = enumerator.Current;
            key = KeySelector( element );
            if ( CanReplace( key, element, out replacedElement ) )
                return true;
        }

        return false;
    }

    [Pure]
    private List<(TKey Key, TElement Element, TElement ReplacedElement)>? FindElementsToReplace(IEnumerable<TElement> elements)
    {
        using var enumerator = elements.GetEnumerator();
        if ( ! TryFindFirstElementToReplace( enumerator, out var key, out var element, out var replacedElement ) )
            return null;

        var handledKeys = new HashSet<TKey>( _elements.KeyComparer ) { key };
        var result = new List<(TKey, TElement, TElement)> { (key, element, replacedElement) };

        while ( enumerator.MoveNext() )
        {
            element = enumerator.Current;
            key = KeySelector( element );
            if ( ! handledKeys.Contains( key ) && CanReplace( key, element, out replacedElement ) )
            {
                handledKeys.Add( key );
                result.Add( (key, element, replacedElement) );
            }
        }

        return result;
    }

    private bool TryFindFirstElementToTryReplace(
        IEnumerator<TElement> enumerator,
        [MaybeNullWhen( false )] out TKey key,
        [MaybeNullWhen( false )] out TElement element,
        [MaybeNullWhen( false )] out TElement replacedElement)
    {
        if ( ! enumerator.MoveNext() )
        {
            key = default;
            element = default;
            replacedElement = default;
            return false;
        }

        element = enumerator.Current;
        key = KeySelector( element );
        if ( CanTryReplace( key, element, out replacedElement ) )
            return true;

        while ( enumerator.MoveNext() )
        {
            element = enumerator.Current;
            key = KeySelector( element );
            if ( CanTryReplace( key, element, out replacedElement ) )
                return true;
        }

        return false;
    }

    [Pure]
    private List<(TKey Key, TElement Element, TElement ReplacedElement)>? FindElementsToTryReplace(IEnumerable<TElement> elements)
    {
        using var enumerator = elements.GetEnumerator();
        if ( ! TryFindFirstElementToTryReplace( enumerator, out var key, out var element, out var replacedElement ) )
            return null;

        var handledKeys = new HashSet<TKey>( _elements.KeyComparer ) { key };
        var result = new List<(TKey, TElement, TElement)> { (key, element, replacedElement) };

        while ( enumerator.MoveNext() )
        {
            element = enumerator.Current;
            key = KeySelector( element );
            if ( ! handledKeys.Contains( key ) && CanTryReplace( key, element, out replacedElement ) )
            {
                handledKeys.Add( key );
                result.Add( (key, element, replacedElement) );
            }
        }

        return result;
    }

    private bool TryFindFirstElementToAddOrReplace(IEnumerator<TElement> enumerator, out ElementChangeDto changeDto)
    {
        if ( ! enumerator.MoveNext() )
        {
            changeDto = default;
            return false;
        }

        var element = enumerator.Current;
        var key = KeySelector( element );
        if ( CanAddOrReplace( key, element, out changeDto ) )
            return true;

        while ( enumerator.MoveNext() )
        {
            element = enumerator.Current;
            key = KeySelector( element );
            if ( CanAddOrReplace( key, element, out changeDto ) )
                return true;
        }

        changeDto = default;
        return false;
    }

    [Pure]
    private ChangesDto? FindElementsToAddOrReplace(IEnumerable<TElement> elements)
    {
        using var enumerator = elements.GetEnumerator();
        if ( ! TryFindFirstElementToAddOrReplace( enumerator, out var changeDto ) )
            return null;

        var handledKeys = new HashSet<TKey>( _elements.KeyComparer ) { changeDto.Key };
        var result = new ChangesDto( changeDto );

        while ( enumerator.MoveNext() )
        {
            var element = enumerator.Current;
            var key = KeySelector( element );
            if ( ! handledKeys.Contains( key ) && CanAddOrReplace( key, element, out changeDto ) )
            {
                handledKeys.Add( key );
                result.Add( changeDto );
            }
        }

        return result;
    }

    private bool TryFindFirstElementToAddOrTryReplace(
        IEnumerator<TElement> enumerator,
        out ElementChangeDto changeDto)
    {
        if ( ! enumerator.MoveNext() )
        {
            changeDto = default;
            return false;
        }

        var element = enumerator.Current;
        var key = KeySelector( element );
        if ( CanAddOrTryReplace( key, element, out changeDto ) )
            return true;

        while ( enumerator.MoveNext() )
        {
            element = enumerator.Current;
            key = KeySelector( element );
            if ( CanAddOrTryReplace( key, element, out changeDto ) )
                return true;
        }

        changeDto = default;
        return false;
    }

    [Pure]
    private ChangesDto? FindElementsToAddOrTryReplace(IEnumerable<TElement> elements)
    {
        using var enumerator = elements.GetEnumerator();
        if ( ! TryFindFirstElementToAddOrTryReplace( enumerator, out var changeDto ) )
            return null;

        var handledKeys = new HashSet<TKey>( _elements.KeyComparer ) { changeDto.Key };
        var result = new ChangesDto( changeDto );

        while ( enumerator.MoveNext() )
        {
            var element = enumerator.Current;
            var key = KeySelector( element );
            if ( ! handledKeys.Contains( key ) && CanAddOrTryReplace( key, element, out changeDto ) )
            {
                handledKeys.Add( key );
                result.Add( changeDto );
            }
        }

        return result;
    }

    [Pure]
    private ChangesDto? CreateRemoveAllChange()
    {
        using var enumerator = _elements.Elements.GetEnumerator();
        if ( ! enumerator.MoveNext() )
            return null;

        var entry = enumerator.Current;
        var changeDto = new ElementChangeDto( ElementChangeType.Remove, entry.Key, entry.Value );
        var result = new ChangesDto( changeDto );

        while ( enumerator.MoveNext() )
        {
            entry = enumerator.Current;
            changeDto = new ElementChangeDto( ElementChangeType.Remove, entry.Key, entry.Value );
            result.Add( changeDto );
        }

        return result;
    }

    [Pure]
    private ChangesDto? FindElementsToChange(IEnumerable<TElement> elements)
    {
        using var enumerator = elements.GetEnumerator();
        if ( ! enumerator.MoveNext() )
            return CreateRemoveAllChange();

        var element = enumerator.Current;
        var key = KeySelector( element );
        var changeDto = CreateChange( key, element );
        var handledKeys = new HashSet<TKey>( _elements.KeyComparer ) { changeDto.Key };
        var result = new ChangesDto( changeDto );

        while ( enumerator.MoveNext() )
        {
            element = enumerator.Current;
            key = KeySelector( element );
            if ( ! handledKeys.Add( key ) )
                continue;

            changeDto = CreateChange( key, element );
            result.Add( changeDto );
        }

        foreach ( var entry in _elements.Elements )
        {
            if ( ! handledKeys.Contains( entry.Key ) )
                result.Add( new ElementChangeDto( ElementChangeType.Remove, entry.Key, entry.Value ) );
        }

        return result;
    }

    [Pure]
    private ChangesDto? FindElementsToTryChange(IEnumerable<TElement> elements)
    {
        using var enumerator = elements.GetEnumerator();
        if ( ! enumerator.MoveNext() )
            return CreateRemoveAllChange();

        var element = enumerator.Current;
        var key = KeySelector( element );
        var changeDto = CreateChangeAttempt( key, element );
        var handledKeys = new HashSet<TKey>( _elements.KeyComparer ) { changeDto.Key };
        var result = new ChangesDto( changeDto );

        while ( enumerator.MoveNext() )
        {
            element = enumerator.Current;
            key = KeySelector( element );
            if ( ! handledKeys.Add( key ) )
                continue;

            changeDto = CreateChangeAttempt( key, element );
            result.Add( changeDto );
        }

        foreach ( var entry in _elements.Elements )
        {
            if ( ! handledKeys.Contains( entry.Key ) )
                result.Add( new ElementChangeDto( ElementChangeType.Remove, entry.Key, entry.Value ) );
        }

        return result;
    }

    private bool TryFindFirstElementToRefresh(
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
        if ( CanRefresh( key, out element ) )
            return true;

        while ( enumerator.MoveNext() )
        {
            key = enumerator.Current;
            if ( CanRefresh( key, out element ) )
                return true;
        }

        return false;
    }

    [Pure]
    private List<(TKey Key, TElement Element)>? FindElementsToRefresh(IEnumerable<TKey> keys)
    {
        using var enumerator = keys.GetEnumerator();
        if ( ! TryFindFirstElementToRefresh( enumerator, out var key, out var element ) )
            return null;

        var handledKeys = new HashSet<TKey>( _elements.KeyComparer ) { key };
        var result = new List<(TKey, TElement)> { (key, element) };

        while ( enumerator.MoveNext() )
        {
            key = enumerator.Current;
            if ( ! handledKeys.Contains( key ) && CanRefresh( key, out element ) )
            {
                handledKeys.Add( key );
                result.Add( (key, element) );
            }
        }

        return result;
    }

    private bool TryFindFirstElementToClearValidation(
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
        if ( CanClearValidation( key, out element ) )
            return true;

        while ( enumerator.MoveNext() )
        {
            key = enumerator.Current;
            if ( CanClearValidation( key, out element ) )
                return true;
        }

        return false;
    }

    [Pure]
    private List<(TKey Key, TElement Element)>? FindElementsToClearValidation(IEnumerable<TKey> keys)
    {
        using var enumerator = keys.GetEnumerator();
        if ( ! TryFindFirstElementToClearValidation( enumerator, out var key, out var element ) )
            return null;

        var handledKeys = new HashSet<TKey>( _elements.KeyComparer ) { key };
        var result = new List<(TKey, TElement)> { (key, element) };

        while ( enumerator.MoveNext() )
        {
            key = enumerator.Current;
            if ( ! handledKeys.Contains( key ) && CanClearValidation( key, out element ) )
            {
                handledKeys.Add( key );
                result.Add( (key, element) );
            }
        }

        return result;
    }

    private CollectionVariableElementSnapshot<TElement, TValidationResult> AddElementAndCreateSnapshot(TKey key, TElement element)
    {
        if ( _elements.Info.TryGetValue( key, out var oldInfo ) )
        {
            var updatedElementInfo = GetUpdatedElementInfo( key, element );
            _elements.AddElement( key, element, updatedElementInfo );

            var updatedSnapshot = new CollectionVariableElementSnapshot<TElement, TValidationResult>(
                element: element,
                previousState: oldInfo.State,
                newState: updatedElementInfo.State,
                previousErrors: oldInfo.Errors,
                newErrors: updatedElementInfo.Errors,
                previousWarnings: oldInfo.Warnings,
                newWarnings: updatedElementInfo.Warnings );

            return updatedSnapshot;
        }

        var elementInfo = GetUpdatedElementInfo( key, element );
        _elements.AddElement( key, element, elementInfo );

        var snapshot = new CollectionVariableElementSnapshot<TElement, TValidationResult>(
            element: element,
            previousState: CollectionVariableElementState.NotFound,
            newState: elementInfo.State,
            previousErrors: Chain<TValidationResult>.Empty,
            newErrors: elementInfo.Errors,
            previousWarnings: Chain<TValidationResult>.Empty,
            newWarnings: elementInfo.Warnings );

        return snapshot;
    }

    private CollectionVariableReplacedElementSnapshot<TElement, TValidationResult> ReplaceElementAndCreateSnapshot(
        TKey key,
        TElement element,
        TElement previous)
    {
        var previousInfo = _elements.Info[key];
        var elementInfo = GetUpdatedElementInfo( key, element );
        _elements.ReplaceElement( key, element, elementInfo );

        var snapshot = new CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>(
            previousElement: previous,
            element: element,
            previousState: previousInfo.State,
            newState: elementInfo.State,
            previousErrors: previousInfo.Errors,
            newErrors: elementInfo.Errors,
            previousWarnings: previousInfo.Warnings,
            newWarnings: elementInfo.Warnings );

        return snapshot;
    }

    private CollectionVariableElementSnapshot<TElement, TValidationResult> RemoveElementAndCreateSnapshot(TKey key, TElement element)
    {
        var previousInfo = _elements.Info[key];
        var newInfo = GetRemovedElementInfo( key );
        _elements.RemoveElement( key, newInfo );

        var snapshot = new CollectionVariableElementSnapshot<TElement, TValidationResult>(
            element: element,
            previousState: previousInfo.State,
            newState: newInfo.State,
            previousErrors: previousInfo.Errors,
            newErrors: newInfo.Errors,
            previousWarnings: previousInfo.Warnings,
            newWarnings: newInfo.Warnings );

        return snapshot;
    }

    private CollectionVariableElementSnapshot<TElement, TValidationResult> RefreshElementAndCreateSnapshot(TKey key, TElement element)
    {
        var previousInfo = _elements.Info[key];
        var newInfo = GetUpdatedElementInfo( key, element );
        _elements.RefreshElement( key, newInfo );

        var snapshot = new CollectionVariableElementSnapshot<TElement, TValidationResult>(
            element: element,
            previousState: previousInfo.State,
            newState: newInfo.State,
            previousErrors: previousInfo.Errors,
            newErrors: newInfo.Errors,
            previousWarnings: previousInfo.Warnings,
            newWarnings: newInfo.Warnings );

        return snapshot;
    }

    private CollectionVariableElementSnapshot<TElement, TValidationResult> ClearElementValidationAndCreateSnapshot(
        TKey key,
        TElement element)
    {
        var previousInfo = _elements.Info[key];
        var newInfo = GetElementInfoWithClearedValidation( previousInfo );
        _elements.RefreshElement( key, newInfo );

        var snapshot = new CollectionVariableElementSnapshot<TElement, TValidationResult>(
            element: element,
            previousState: previousInfo.State,
            newState: newInfo.State,
            previousErrors: previousInfo.Errors,
            newErrors: newInfo.Errors,
            previousWarnings: previousInfo.Warnings,
            newWarnings: newInfo.Warnings );

        return snapshot;
    }

    private CollectionVariableElementSnapshot<TElement, TValidationResult> RefreshElementValidationAndCreateSnapshot(
        TKey key,
        TElement element)
    {
        var previousInfo = _elements.Info[key];
        var newInfo = GetElementInfoWithRefreshedValidation( element, previousInfo );
        _elements.RefreshElement( key, newInfo );

        var snapshot = new CollectionVariableElementSnapshot<TElement, TValidationResult>(
            element: element,
            previousState: previousInfo.State,
            newState: newInfo.State,
            previousErrors: previousInfo.Errors,
            newErrors: newInfo.Errors,
            previousWarnings: previousInfo.Warnings,
            newWarnings: newInfo.Warnings );

        return snapshot;
    }

    [Pure]
    private ElementsCollection.ElementInfo GetUpdatedElementInfo(TKey key, TElement element)
    {
        var errors = _elements.ErrorsValidator.Validate( element );
        var warnings = _elements.WarningsValidator.Validate( element );

        var state = CollectionVariableElementState.Default;

        if ( errors.Count > 0 )
            state |= CollectionVariableElementState.Invalid;

        if ( warnings.Count > 0 )
            state |= CollectionVariableElementState.Warning;

        if ( ! InitialElements.TryGetValue( key, out var initial ) )
            state |= CollectionVariableElementState.Added;
        else if ( ! _elements.ElementComparer.Equals( initial, element ) )
            state |= CollectionVariableElementState.Changed;

        var result = ElementsCollection.ElementInfo.Create( state, errors, warnings );
        return result;
    }

    [Pure]
    private ElementsCollection.ElementInfo GetRemovedElementInfo(TKey key)
    {
        var state = InitialElements.ContainsKey( key ) ? CollectionVariableElementState.Removed : CollectionVariableElementState.NotFound;
        var result = ElementsCollection.ElementInfo.Create( state );
        return result;
    }

    [Pure]
    private static ElementsCollection.ElementInfo GetElementInfoWithClearedValidation(ElementsCollection.ElementInfo previousInfo)
    {
        var state = previousInfo.State & ~(CollectionVariableElementState.Invalid | CollectionVariableElementState.Warning);
        var result = ElementsCollection.ElementInfo.Create( state );
        return result;
    }

    [Pure]
    private ElementsCollection.ElementInfo GetElementInfoWithRefreshedValidation(
        TElement element,
        ElementsCollection.ElementInfo previousInfo)
    {
        var errors = _elements.ErrorsValidator.Validate( element );
        var warnings = _elements.WarningsValidator.Validate( element );
        var state = previousInfo.State & ~(CollectionVariableElementState.Invalid | CollectionVariableElementState.Warning);

        if ( errors.Count > 0 )
            state |= CollectionVariableElementState.Invalid;

        if ( warnings.Count > 0 )
            state |= CollectionVariableElementState.Warning;

        var result = ElementsCollection.ElementInfo.Create( state, errors, warnings );
        return result;
    }

    private void UpdateAndPublishAdditionEvents(
        IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> addedElements,
        VariableChangeSource changeSource)
    {
        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;

        Update();

        PublishEvents(
            previousState: previousState,
            addedElements: addedElements,
            removedElements: Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>(),
            refreshedElements: Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>(),
            replacedElements: Array.Empty<CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>>(),
            previousErrors: previousErrors,
            previousWarnings: previousWarnings,
            elementValidations: addedElements,
            changeSource: changeSource );
    }

    private void UpdateAndPublishRemovalEvents(
        IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> removedElements)
    {
        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;

        Update();

        PublishEvents(
            previousState: previousState,
            addedElements: Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>(),
            removedElements: removedElements,
            refreshedElements: Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>(),
            replacedElements: Array.Empty<CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>>(),
            previousErrors: previousErrors,
            previousWarnings: previousWarnings,
            elementValidations: removedElements,
            changeSource: VariableChangeSource.Change );
    }

    private void UpdateAndPublishReplacementEvents(
        IReadOnlyList<CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>> replacedElements,
        VariableChangeSource changeSource)
    {
        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;

        Update();

        PublishEvents(
            previousState: previousState,
            addedElements: Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>(),
            removedElements: Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>(),
            refreshedElements: Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>(),
            replacedElements: replacedElements,
            previousErrors: previousErrors,
            previousWarnings: previousWarnings,
            elementValidations: replacedElements,
            changeSource: changeSource );
    }

    private void UpdateAndPublishRefreshmentEvents(
        IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> refreshedElements)
    {
        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;

        Update();

        PublishEvents(
            previousState: previousState,
            addedElements: Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>(),
            removedElements: Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>(),
            refreshedElements: refreshedElements,
            replacedElements: Array.Empty<CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>>(),
            previousErrors: previousErrors,
            previousWarnings: previousWarnings,
            elementValidations: refreshedElements,
            changeSource: VariableChangeSource.Refresh );
    }

    private void ClearValidationAndPublishEvent(IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> elements)
    {
        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;

        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        _state = CreateState( _state, VariableState.Invalid, _elements.Invalid.Count > 0 );
        _state = CreateState( _state, VariableState.Warning, _elements.Warning.Count > 0 );

        var validationEvent = new CollectionVariableValidationEvent<TKey, TElement, TValidationResult>(
            this,
            previousErrors,
            previousWarnings,
            previousState,
            elements,
            associatedChange: null );

        OnPublishValidationEvent( validationEvent );
    }

    private void RefreshValidationAndPublishEvent(IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> elements)
    {
        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;

        Errors = ErrorsValidator.Validate( _elements );
        Warnings = WarningsValidator.Validate( _elements );
        _state = CreateState( _state, VariableState.Invalid, Errors.Count > 0 || _elements.Invalid.Count > 0 );
        _state = CreateState( _state, VariableState.Warning, Warnings.Count > 0 || _elements.Warning.Count > 0 );

        var validationEvent = new CollectionVariableValidationEvent<TKey, TElement, TValidationResult>(
            this,
            previousErrors,
            previousWarnings,
            previousState,
            elements,
            associatedChange: null );

        OnPublishValidationEvent( validationEvent );
    }

    private void UpdateAndPublishEvents(ChangesDto changes, VariableChangeSource changeSource)
    {
        var spanOfElementsToChange = CollectionsMarshal.AsSpan( changes.Elements );
        var elementSnapshots = new CollectionVariableElementSnapshot<TElement, TValidationResult>[spanOfElementsToChange.Length];

        var addIndex = 0;
        var addedElements = changes.CreateAddedElementsContainer();

        var removeIndex = 0;
        var removedElements = changes.CreateRemovedElementsContainer();

        var replaceIndex = 0;
        var replacedElements = changes.CreateReplacedElementsContainer();

        var refreshIndex = 0;
        var refreshedElements = changes.CreateRefreshedElementsContainer();

        for ( var i = 0; i < spanOfElementsToChange.Length; ++i )
        {
            var dto = spanOfElementsToChange[i];

            switch ( dto.ChangeType )
            {
                case ElementChangeType.Add:
                    addedElements[addIndex] = AddElementAndCreateSnapshot( dto.Key, dto.Element );
                    elementSnapshots[i] = addedElements[addIndex++];
                    break;
                case ElementChangeType.Remove:
                    removedElements[removeIndex] = RemoveElementAndCreateSnapshot( dto.Key, dto.Element );
                    elementSnapshots[i] = removedElements[removeIndex++];
                    break;
                case ElementChangeType.Replace:
                    replacedElements[replaceIndex] = ReplaceElementAndCreateSnapshot( dto.Key, dto.Element, dto.PreviousElement! );
                    elementSnapshots[i] = replacedElements[replaceIndex++];
                    break;
                default:
                    refreshedElements[refreshIndex] = RefreshElementAndCreateSnapshot( dto.Key, dto.Element );
                    elementSnapshots[i] = refreshedElements[refreshIndex++];
                    break;
            }
        }

        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;

        Update();

        PublishEvents(
            previousState: previousState,
            addedElements: addedElements,
            removedElements: removedElements,
            refreshedElements: refreshedElements,
            replacedElements: replacedElements,
            previousErrors: previousErrors,
            previousWarnings: previousWarnings,
            elementValidations: elementSnapshots,
            changeSource: changeSource );
    }

    [Pure]
    private List<(ElementChangeType ChangeType, CollectionVariableElementSnapshot<TElement, TValidationResult> Snapshot)>
        GetResetElementChanges(ElementsCollection newElements)
    {
        var result = new List<(ElementChangeType, CollectionVariableElementSnapshot<TElement, TValidationResult>)>();

        foreach ( var (key, element) in newElements.Elements )
        {
            var info = newElements.Info[key];
            if ( ! _elements.Info.TryGetValue( key, out var oldInfo ) )
                oldInfo = ElementsCollection.ElementInfo.Create( CollectionVariableElementState.NotFound );

            if ( _elements.Elements.TryGetValue( key, out var oldElement ) )
            {
                result.Add(
                    (ElementChangeType.Replace, new CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>(
                        oldElement,
                        element,
                        oldInfo.State,
                        info.State,
                        oldInfo.Errors,
                        info.Errors,
                        oldInfo.Warnings,
                        info.Warnings )) );

                continue;
            }

            result.Add(
                (ElementChangeType.Add, new CollectionVariableElementSnapshot<TElement, TValidationResult>(
                    element,
                    oldInfo.State,
                    info.State,
                    oldInfo.Errors,
                    info.Errors,
                    oldInfo.Warnings,
                    info.Warnings )) );
        }

        foreach ( var (key, oldElement) in _elements.Elements )
        {
            if ( newElements.Elements.ContainsKey( key ) )
                continue;

            var oldInfo = _elements.Info[key];
            if ( ! newElements.Info.TryGetValue( key, out var info ) )
                info = ElementsCollection.ElementInfo.Create( CollectionVariableElementState.NotFound );

            result.Add(
                (ElementChangeType.Remove, new CollectionVariableElementSnapshot<TElement, TValidationResult>(
                    oldElement,
                    oldInfo.State,
                    info.State,
                    oldInfo.Errors,
                    info.Errors,
                    oldInfo.Warnings,
                    info.Warnings )) );
        }

        return result;
    }

    private void ResetAndPublishEvents(IReadOnlyDictionary<TKey, TElement> initialElements, ElementsCollection elements)
    {
        var changes = GetResetElementChanges( elements );

        var spanOfChanges = CollectionsMarshal.AsSpan( changes );
        var elementSnapshots = spanOfChanges.Length == 0
            ? Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>()
            : new CollectionVariableElementSnapshot<TElement, TValidationResult>[spanOfChanges.Length];

        var toAddCount = 0;
        var toReplaceCount = 0;
        var toRemoveCount = 0;

        for ( var i = 0; i < spanOfChanges.Length; ++i )
        {
            var (changeType, snapshot) = spanOfChanges[i];
            elementSnapshots[i] = snapshot;
            switch ( changeType )
            {
                case ElementChangeType.Add:
                    ++toAddCount;
                    break;
                case ElementChangeType.Replace:
                    ++toReplaceCount;
                    break;
                default:
                    ++toRemoveCount;
                    break;
            }
        }

        var addIndex = 0;
        var addedElements = toAddCount == 0
            ? Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>()
            : new CollectionVariableElementSnapshot<TElement, TValidationResult>[toAddCount];

        var removeIndex = 0;
        var removedElements = toRemoveCount == 0
            ? Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>()
            : new CollectionVariableElementSnapshot<TElement, TValidationResult>[toRemoveCount];

        var replaceIndex = 0;
        var replacedElements = toReplaceCount == 0
            ? Array.Empty<CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>>()
            : new CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>[toReplaceCount];

        for ( var i = 0; i < spanOfChanges.Length; ++i )
        {
            var (changeType, snapshot) = spanOfChanges[i];
            switch ( changeType )
            {
                case ElementChangeType.Add:
                    addedElements[addIndex++] = snapshot;
                    break;
                case ElementChangeType.Remove:
                    removedElements[removeIndex++] = snapshot;
                    break;
                default:
                    replacedElements[replaceIndex++] =
                        ReinterpretCast.To<CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>>( snapshot );

                    break;
            }
        }

        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;

        InitialElements = initialElements;
        _elements = elements;
        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        _state = CreateState( _state & VariableState.ReadOnly, VariableState.Changed, _elements.Modified.Count > 0 );

        PublishEvents(
            previousState: previousState,
            addedElements: addedElements,
            removedElements: removedElements,
            refreshedElements: Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>(),
            replacedElements: replacedElements,
            previousErrors: previousErrors,
            previousWarnings: previousWarnings,
            elementValidations: elementSnapshots,
            changeSource: VariableChangeSource.Reset );
    }

    private void PublishEvents(
        VariableState previousState,
        IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> addedElements,
        IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> removedElements,
        IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> refreshedElements,
        IReadOnlyList<CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>> replacedElements,
        Chain<TValidationResult> previousErrors,
        Chain<TValidationResult> previousWarnings,
        IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> elementValidations,
        VariableChangeSource changeSource)
    {
        var changeEvent = new CollectionVariableChangeEvent<TKey, TElement, TValidationResult>(
            this,
            previousState,
            addedElements,
            removedElements,
            refreshedElements,
            replacedElements,
            changeSource );

        OnPublishChangeEvent( changeEvent );

        var validationEvent = new CollectionVariableValidationEvent<TKey, TElement, TValidationResult>(
            this,
            previousErrors,
            previousWarnings,
            previousState,
            elementValidations,
            changeEvent );

        OnPublishValidationEvent( validationEvent );
    }

    internal sealed class ElementsCollection : ICollectionVariableElements<TKey, TElement, TValidationResult>
    {
        internal readonly Dictionary<TKey, TElement> Elements;
        internal readonly Dictionary<TKey, ElementInfo> Info;
        internal readonly HashSet<TKey> Invalid;
        internal readonly HashSet<TKey> Warning;
        internal readonly HashSet<TKey> Modified;

        internal ElementsCollection(
            IReadOnlyDictionary<TKey, TElement> initialElements,
            IEqualityComparer<TKey>? keyComparer = null,
            IEqualityComparer<TElement>? elementComparer = null,
            IValidator<TElement, TValidationResult>? errorsValidator = null,
            IValidator<TElement, TValidationResult>? warningsValidator = null)
        {
            ElementComparer = elementComparer ?? EqualityComparer<TElement>.Default;
            ErrorsValidator = errorsValidator ?? Validators<TValidationResult>.Pass<TElement>();
            WarningsValidator = warningsValidator ?? Validators<TValidationResult>.Pass<TElement>();

            Elements = new Dictionary<TKey, TElement>( keyComparer );
            Info = new Dictionary<TKey, ElementInfo>( Elements.Comparer );
            Invalid = new HashSet<TKey>( Elements.Comparer );
            Warning = new HashSet<TKey>( Elements.Comparer );
            Modified = new HashSet<TKey>( Elements.Comparer );

            foreach ( var (key, element) in initialElements )
            {
                Elements.Add( key, element );

                if ( ElementComparer.Equals( element, element ) )
                {
                    Info.Add( key, ElementInfo.Create( CollectionVariableElementState.Default ) );
                    continue;
                }

                Modified.Add( key );
                Info.Add( key, ElementInfo.Create( CollectionVariableElementState.Changed ) );
            }
        }

        internal ElementsCollection(
            IReadOnlyDictionary<TKey, TElement> initialElements,
            IEnumerable<TElement> elements,
            Func<TElement, TKey> keySelector,
            IEqualityComparer<TKey>? keyComparer = null,
            IEqualityComparer<TElement>? elementComparer = null,
            IValidator<TElement, TValidationResult>? errorsValidator = null,
            IValidator<TElement, TValidationResult>? warningsValidator = null)
        {
            ElementComparer = elementComparer ?? EqualityComparer<TElement>.Default;
            ErrorsValidator = errorsValidator ?? Validators<TValidationResult>.Pass<TElement>();
            WarningsValidator = warningsValidator ?? Validators<TValidationResult>.Pass<TElement>();

            Elements = new Dictionary<TKey, TElement>( keyComparer );
            Info = new Dictionary<TKey, ElementInfo>( Elements.Comparer );
            Invalid = new HashSet<TKey>( Elements.Comparer );
            Warning = new HashSet<TKey>( Elements.Comparer );
            Modified = new HashSet<TKey>( Elements.Comparer );

            foreach ( var element in elements )
            {
                var key = keySelector( element );
                Elements.Add( key, element );

                if ( ! initialElements.TryGetValue( key, out var initial ) )
                {
                    Modified.Add( key );
                    Info.Add( key, ElementInfo.Create( CollectionVariableElementState.Added ) );
                    continue;
                }

                if ( ElementComparer.Equals( initial, element ) )
                {
                    Info.Add( key, ElementInfo.Create( CollectionVariableElementState.Default ) );
                    continue;
                }

                Modified.Add( key );
                Info.Add( key, ElementInfo.Create( CollectionVariableElementState.Changed ) );
            }

            foreach ( var initial in initialElements.Values )
            {
                var key = keySelector( initial );
                if ( Elements.ContainsKey( key ) )
                    continue;

                Modified.Add( key );
                Info.Add( key, ElementInfo.Create( CollectionVariableElementState.Removed ) );
            }
        }

        public IEqualityComparer<TElement> ElementComparer { get; }
        public IValidator<TElement, TValidationResult> ErrorsValidator { get; }
        public IValidator<TElement, TValidationResult> WarningsValidator { get; }
        public int Count => Elements.Count;
        public IReadOnlyCollection<TKey> Keys => Elements.Keys;
        public IReadOnlyCollection<TElement> Values => Elements.Values;
        public IReadOnlySet<TKey> InvalidElementKeys => Invalid;
        public IReadOnlySet<TKey> WarningElementKeys => Warning;
        public IReadOnlySet<TKey> ModifiedElementKeys => Modified;
        public IEqualityComparer<TKey> KeyComparer => Elements.Comparer;
        public TElement this[TKey key] => Elements[key];

        IEnumerable ICollectionVariableElements.Keys => Keys;
        IEnumerable ICollectionVariableElements.Values => Values;
        IEnumerable ICollectionVariableElements.InvalidElementKeys => Invalid;
        IEnumerable ICollectionVariableElements.WarningElementKeys => Warning;
        IEnumerable ICollectionVariableElements.ModifiedElementKeys => Modified;
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
        public CollectionVariableElementState GetState(TKey key)
        {
            return Info.TryGetValue( key, out var info ) ? info.State : CollectionVariableElementState.NotFound;
        }

        [Pure]
        public Chain<TValidationResult> GetErrors(TKey key)
        {
            return Info.TryGetValue( key, out var info ) ? info.Errors : Chain<TValidationResult>.Empty;
        }

        [Pure]
        public Chain<TValidationResult> GetWarnings(TKey key)
        {
            return Info.TryGetValue( key, out var info ) ? info.Warnings : Chain<TValidationResult>.Empty;
        }

        [Pure]
        public IEnumerator<KeyValuePair<TKey, TElement>> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        internal void AddElement(TKey key, TElement element, ElementInfo elementInfo)
        {
            Elements.Add( key, element );
            Info[key] = elementInfo;

            if ( elementInfo.HasState( CollectionVariableElementState.Invalid ) )
                Invalid.Add( key );

            if ( elementInfo.HasState( CollectionVariableElementState.Warning ) )
                Warning.Add( key );

            if ( elementInfo.HasState( CollectionVariableElementState.Added | CollectionVariableElementState.Changed ) )
                Modified.Add( key );
            else
                Modified.Remove( key );
        }

        internal void RemoveElement(TKey key, ElementInfo elementInfo)
        {
            Elements.Remove( key );
            Invalid.Remove( key );
            Warning.Remove( key );

            if ( elementInfo.HasState( CollectionVariableElementState.Removed ) )
            {
                Modified.Add( key );
                Info[key] = elementInfo;
                return;
            }

            Modified.Remove( key );
            Info.Remove( key );
        }

        internal void ReplaceElement(TKey key, TElement element, ElementInfo elementInfo)
        {
            Elements[key] = element;
            RefreshElement( key, elementInfo );
        }

        internal void RefreshElement(TKey key, ElementInfo elementInfo)
        {
            Info[key] = elementInfo;

            if ( elementInfo.HasState( CollectionVariableElementState.Invalid ) )
                Invalid.Add( key );
            else
                Invalid.Remove( key );

            if ( elementInfo.HasState( CollectionVariableElementState.Warning ) )
                Warning.Add( key );
            else
                Warning.Remove( key );

            if ( elementInfo.HasState( CollectionVariableElementState.Added | CollectionVariableElementState.Changed ) )
                Modified.Add( key );
            else
                Modified.Remove( key );
        }

        [Pure]
        IEnumerable ICollectionVariableElements<TKey, TElement>.GetErrors(TKey key)
        {
            return GetErrors( key );
        }

        [Pure]
        IEnumerable ICollectionVariableElements<TKey, TElement>.GetWarnings(TKey key)
        {
            return GetWarnings( key );
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal readonly struct ElementInfo
        {
            internal readonly CollectionVariableElementState State;
            internal readonly Chain<TValidationResult> Errors;
            internal readonly Chain<TValidationResult> Warnings;

            private ElementInfo(CollectionVariableElementState state, Chain<TValidationResult> errors, Chain<TValidationResult> warnings)
            {
                State = state;
                Errors = errors;
                Warnings = warnings;
            }

            [Pure]
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal static ElementInfo Create(CollectionVariableElementState state)
            {
                return new ElementInfo( state, Chain<TValidationResult>.Empty, Chain<TValidationResult>.Empty );
            }

            [Pure]
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal static ElementInfo Create(
                CollectionVariableElementState state,
                Chain<TValidationResult> errors,
                Chain<TValidationResult> warnings)
            {
                return new ElementInfo( state, errors, warnings );
            }

            [Pure]
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal bool HasState(CollectionVariableElementState state)
            {
                return (State & state) != CollectionVariableElementState.Default;
            }
        }
    }

    private struct ChangesDto
    {
        internal readonly List<ElementChangeDto> Elements;
        internal int ToAddCount;
        internal int ToReplaceCount;
        internal int ToRemoveCount;
        internal int ToRefreshCount;

        internal ChangesDto(ElementChangeDto changeDto)
        {
            ToAddCount = 0;
            ToReplaceCount = 0;
            ToRemoveCount = 0;
            ToRefreshCount = 0;
            Elements = new List<ElementChangeDto>();
            Add( changeDto );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Add(ElementChangeDto changeDto)
        {
            Elements.Add( changeDto );
            switch ( changeDto.ChangeType )
            {
                case ElementChangeType.Add:
                    ++ToAddCount;
                    break;
                case ElementChangeType.Remove:
                    ++ToRemoveCount;
                    break;
                case ElementChangeType.Replace:
                    ++ToReplaceCount;
                    break;
                default:
                    ++ToRefreshCount;
                    break;
            }
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal CollectionVariableElementSnapshot<TElement, TValidationResult>[] CreateAddedElementsContainer()
        {
            return ToAddCount == 0
                ? Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>()
                : new CollectionVariableElementSnapshot<TElement, TValidationResult>[ToAddCount];
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal CollectionVariableElementSnapshot<TElement, TValidationResult>[] CreateRemovedElementsContainer()
        {
            return ToRemoveCount == 0
                ? Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>()
                : new CollectionVariableElementSnapshot<TElement, TValidationResult>[ToRemoveCount];
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal CollectionVariableElementSnapshot<TElement, TValidationResult>[] CreateRefreshedElementsContainer()
        {
            return ToRefreshCount == 0
                ? Array.Empty<CollectionVariableElementSnapshot<TElement, TValidationResult>>()
                : new CollectionVariableElementSnapshot<TElement, TValidationResult>[ToRefreshCount];
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>[] CreateReplacedElementsContainer()
        {
            return ToReplaceCount == 0
                ? Array.Empty<CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>>()
                : new CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>[ToReplaceCount];
        }
    }

    private readonly struct ElementChangeDto
    {
        internal readonly ElementChangeType ChangeType;
        internal readonly TKey Key;
        internal readonly TElement Element;
        internal readonly TElement? PreviousElement;

        internal ElementChangeDto(ElementChangeType changeType, TKey key, TElement element, TElement? previousElement = default)
        {
            ChangeType = changeType;
            Key = key;
            Element = element;
            PreviousElement = previousElement;
        }
    }

    private enum ElementChangeType : byte
    {
        Add = 0,
        Remove = 1,
        Refresh = 2,
        Replace = 3
    }
}
