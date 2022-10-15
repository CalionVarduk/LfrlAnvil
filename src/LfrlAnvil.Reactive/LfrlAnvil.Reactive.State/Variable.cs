﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Reactive.State.Internal;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

public class Variable<TValue, TValidationResult> : VariableNode, IVariable<TValue, TValidationResult>, IMutableVariableNode, IDisposable
{
    private readonly EventPublisher<VariableValueChangeEvent<TValue, TValidationResult>> _onChange;
    private readonly EventPublisher<VariableValidationEvent<TValue, TValidationResult>> _onValidate;
    private VariableState _state;

    public Variable(
        TValue initialValue,
        TValue value,
        IEqualityComparer<TValue>? comparer = null,
        IValidator<TValue, TValidationResult>? errorsValidator = null,
        IValidator<TValue, TValidationResult>? warningsValidator = null)
    {
        InitialValue = initialValue;
        Value = value;
        Comparer = comparer ?? EqualityComparer<TValue>.Default;
        ErrorsValidator = errorsValidator ?? Validators<TValidationResult>.Pass<TValue>();
        WarningsValidator = warningsValidator ?? Validators<TValidationResult>.Pass<TValue>();
        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        _onChange = new EventPublisher<VariableValueChangeEvent<TValue, TValidationResult>>();
        _onValidate = new EventPublisher<VariableValidationEvent<TValue, TValidationResult>>();
        _state = CreateState( VariableState.Default, VariableState.Changed, ! Comparer.Equals( InitialValue, Value ) );
    }

    public Variable(
        TValue initialValue,
        IEqualityComparer<TValue>? comparer = null,
        IValidator<TValue, TValidationResult>? errorsValidator = null,
        IValidator<TValue, TValidationResult>? warningsValidator = null)
        : this( initialValue, initialValue, comparer, errorsValidator, warningsValidator ) { }

    public TValue Value { get; private set; }
    public TValue InitialValue { get; private set; }
    public IEqualityComparer<TValue> Comparer { get; }
    public Chain<TValidationResult> Errors { get; private set; }
    public Chain<TValidationResult> Warnings { get; private set; }
    public IValidator<TValue, TValidationResult> ErrorsValidator { get; }
    public IValidator<TValue, TValidationResult> WarningsValidator { get; }
    public sealed override VariableState State => _state;
    public sealed override IEventStream<VariableValueChangeEvent<TValue, TValidationResult>> OnChange => _onChange;
    public sealed override IEventStream<VariableValidationEvent<TValue, TValidationResult>> OnValidate => _onValidate;

    IEventStream<IVariableValueChangeEvent<TValue>> IReadOnlyVariable<TValue>.OnChange => _onChange;

    Type IReadOnlyVariable.ValueType => typeof( TValue );
    Type IReadOnlyVariable.ValidationResultType => typeof( TValidationResult );
    object? IReadOnlyVariable.Value => Value;
    object? IReadOnlyVariable.InitialValue => InitialValue;
    IEnumerable IReadOnlyVariable.Errors => Errors;
    IEnumerable IReadOnlyVariable.Warnings => Warnings;

    IEventStream<IVariableValueChangeEvent> IReadOnlyVariable.OnChange => _onChange;
    IEventStream<IVariableValidationEvent> IReadOnlyVariable.OnValidate => _onValidate;

    IEventStream<IVariableNodeEvent> IVariableNode.OnChange => _onChange;
    IEventStream<IVariableNodeEvent> IVariableNode.OnValidate => _onValidate;

    [Pure]
    public override string ToString()
    {
        return $"{nameof( Value )}: '{Value}', {nameof( State )}: {_state}";
    }

    public virtual void Dispose()
    {
        _state |= VariableState.ReadOnly | VariableState.Disposed;
        _onChange.Dispose();
        _onValidate.Dispose();
    }

    public VariableChangeResult TryChange(TValue value)
    {
        if ( Comparer.Equals( Value, value ) )
            return VariableChangeResult.NotChanged;

        if ( (_state & (VariableState.ReadOnly | VariableState.Disposed)) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        ChangeInternal( value, VariableChangeSource.TryChange );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult Change(TValue value)
    {
        if ( (_state & (VariableState.ReadOnly | VariableState.Disposed)) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        ChangeInternal( value, VariableChangeSource.Change );
        return VariableChangeResult.Changed;
    }

    public void Refresh()
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        ChangeInternal( Value, VariableChangeSource.Refresh );
    }

    public void RefreshValidation()
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;
        Errors = ErrorsValidator.Validate( Value );
        Warnings = WarningsValidator.Validate( Value );
        _state = CreateState( _state, VariableState.Invalid, Errors.Count > 0 );
        _state = CreateState( _state, VariableState.Warning, Warnings.Count > 0 );

        var validationEvent = new VariableValidationEvent<TValue, TValidationResult>(
            this,
            previousErrors,
            previousWarnings,
            previousState,
            associatedChange: null );

        OnPublishValidationEvent( validationEvent );
    }

    public void ClearValidation()
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        if ( (_state & (VariableState.Invalid | VariableState.Warning)) == VariableState.Default )
            return;

        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;
        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        _state &= ~(VariableState.Invalid | VariableState.Warning);

        var validationEvent = new VariableValidationEvent<TValue, TValidationResult>(
            this,
            previousErrors,
            previousWarnings,
            previousState,
            associatedChange: null );

        OnPublishValidationEvent( validationEvent );
    }

    public void Reset(TValue initialValue, TValue value)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        var previousValue = Value;
        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;

        InitialValue = initialValue;
        Value = value;
        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        _state = CreateState( _state & VariableState.ReadOnly, VariableState.Changed, ! Comparer.Equals( InitialValue, Value ) );

        PublishEvents( previousState, previousValue, previousErrors, previousWarnings, VariableChangeSource.Reset );
    }

    public void SetReadOnly(bool enabled)
    {
        if ( (_state & VariableState.Disposed) != VariableState.Default )
            return;

        if ( (_state & VariableState.ReadOnly) == (enabled ? VariableState.ReadOnly : VariableState.Default) )
            return;

        var previousState = _state;
        UpdateReadOnly( enabled );

        var changeEvent = new VariableValueChangeEvent<TValue, TValidationResult>(
            this,
            Value,
            previousState,
            VariableChangeSource.SetReadOnly );

        OnPublishChangeEvent( changeEvent );
    }

    [Pure]
    public override IEnumerable<IVariableNode> GetChildren()
    {
        return Array.Empty<IVariableNode>();
    }

    protected virtual void OnPublishChangeEvent(VariableValueChangeEvent<TValue, TValidationResult> @event)
    {
        _onChange.Publish( @event );
    }

    protected virtual void OnPublishValidationEvent(VariableValidationEvent<TValue, TValidationResult> @event)
    {
        _onValidate.Publish( @event );
    }

    protected virtual void Update(TValue value)
    {
        Value = value;
        Errors = ErrorsValidator.Validate( Value );
        Warnings = WarningsValidator.Validate( Value );
        _state = CreateState( _state, VariableState.Changed, ! Comparer.Equals( InitialValue, Value ) );
        _state = CreateState( _state, VariableState.Invalid, Errors.Count > 0 );
        _state = CreateState( _state, VariableState.Warning, Warnings.Count > 0 );
        _state |= VariableState.Dirty;
    }

    protected virtual void UpdateReadOnly(bool enabled)
    {
        _state = CreateState( _state, VariableState.ReadOnly, enabled );
    }

    private void ChangeInternal(TValue value, VariableChangeSource changeSource)
    {
        var previousValue = Value;
        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = _state;

        Update( value );
        PublishEvents( previousState, previousValue, previousErrors, previousWarnings, changeSource );
    }

    private void PublishEvents(
        VariableState previousState,
        TValue previousValue,
        Chain<TValidationResult> previousErrors,
        Chain<TValidationResult> previousWarnings,
        VariableChangeSource changeSource)
    {
        var changeEvent = new VariableValueChangeEvent<TValue, TValidationResult>( this, previousValue, previousState, changeSource );
        OnPublishChangeEvent( changeEvent );

        var validationEvent = new VariableValidationEvent<TValue, TValidationResult>(
            this,
            previousErrors,
            previousWarnings,
            previousState,
            changeEvent );

        OnPublishValidationEvent( validationEvent );
    }
}