using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

public class Variable<TValue, TValidationResult> : IVariable<TValue, TValidationResult>, IDisposable
{
    private readonly EventPublisher<VariableValueChangedEvent<TValue, TValidationResult>> _onChange;
    private readonly EventPublisher<VariableValidationEvent<TValue, TValidationResult>> _onValidate;

    public Variable(
        TValue originalValue,
        TValue value,
        IEqualityComparer<TValue> comparer,
        IValidator<TValue, TValidationResult> errorsValidator,
        IValidator<TValue, TValidationResult> warningsValidator)
    {
        OriginalValue = originalValue;
        Value = value;
        Comparer = comparer;
        ErrorsValidator = errorsValidator;
        WarningsValidator = warningsValidator;
        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        _onChange = new EventPublisher<VariableValueChangedEvent<TValue, TValidationResult>>();
        _onValidate = new EventPublisher<VariableValidationEvent<TValue, TValidationResult>>();
        State = UpdateState( VariableState.Default, VariableState.Changed, ! Comparer.Equals( OriginalValue, Value ) );
    }

    public TValue Value { get; private set; }
    public TValue OriginalValue { get; private set; }
    public VariableState State { get; private set; }
    public IEqualityComparer<TValue> Comparer { get; }
    public Chain<TValidationResult> Errors { get; private set; }
    public Chain<TValidationResult> Warnings { get; private set; }
    public IValidator<TValue, TValidationResult> ErrorsValidator { get; }
    public IValidator<TValue, TValidationResult> WarningsValidator { get; }
    public IEventStream<VariableValueChangedEvent<TValue, TValidationResult>> OnChange => _onChange;
    public IEventStream<VariableValidationEvent<TValue, TValidationResult>> OnValidate => _onValidate;

    IEventStream<IVariableValueChangedEvent<TValue>> IReadOnlyVariable<TValue>.OnChange => _onChange;

    Type IReadOnlyVariable.ValueType => typeof( TValue );
    Type IReadOnlyVariable.ValidationResultType => typeof( TValidationResult );
    object? IReadOnlyVariable.Value => Value;
    object? IReadOnlyVariable.OriginalValue => OriginalValue;

    IReadOnlyCollection<object?> IReadOnlyVariable.Errors => (IReadOnlyCollection<object?>)(IReadOnlyCollection<TValidationResult>)Errors;

    IReadOnlyCollection<object?> IReadOnlyVariable.Warnings =>
        (IReadOnlyCollection<object?>)(IReadOnlyCollection<TValidationResult>)Warnings;

    IEventStream<IVariableValueChangedEvent> IReadOnlyVariable.OnChange => _onChange;
    IEventStream<IVariableValidationEvent> IReadOnlyVariable.OnValidate => _onValidate;

    [Pure]
    public override string ToString()
    {
        return $"{nameof( Value )}: '{Value}', {nameof( State )}: {State}";
    }

    public virtual void Dispose()
    {
        _onChange.Dispose();
        _onValidate.Dispose();
        State |= VariableState.ReadOnly | VariableState.Disposed;
    }

    public VariableChangeResult TryChange(TValue value)
    {
        if ( Comparer.Equals( Value, value ) )
            return VariableChangeResult.NotChanged;

        if ( (State & (VariableState.ReadOnly | VariableState.Disposed)) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        ChangeInternal( value, VariableChangeSource.TryChange );
        return VariableChangeResult.Changed;
    }

    public VariableChangeResult Change(TValue value)
    {
        if ( (State & (VariableState.ReadOnly | VariableState.Disposed)) != VariableState.Default )
            return VariableChangeResult.ReadOnly;

        ChangeInternal( value, VariableChangeSource.Change );
        return VariableChangeResult.Changed;
    }

    public void Refresh()
    {
        if ( (State & VariableState.Disposed) != VariableState.Default )
            return;

        ChangeInternal( Value, VariableChangeSource.Refresh );
    }

    public void ClearValidation()
    {
        if ( (State & VariableState.Disposed) != VariableState.Default )
            return;

        if ( (State & (VariableState.Invalid | VariableState.Warning)) == VariableState.Default )
            return;

        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = State;
        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        State &= ~(VariableState.Invalid | VariableState.Warning);

        var validationEvent = new VariableValidationEvent<TValue, TValidationResult>(
            this,
            previousErrors,
            previousWarnings,
            previousState,
            associatedChange: null );

        OnPublishValidationEvent( validationEvent );
    }

    public void Reset(TValue originalValue, TValue value)
    {
        if ( (State & VariableState.Disposed) != VariableState.Default )
            return;

        var previousValue = Value;
        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = State;

        OriginalValue = originalValue;
        Value = value;
        Errors = Chain<TValidationResult>.Empty;
        Warnings = Chain<TValidationResult>.Empty;
        State = UpdateState( State & VariableState.ReadOnly, VariableState.Changed, ! Comparer.Equals( OriginalValue, Value ) );

        PublishEvents( previousState, previousValue, previousErrors, previousWarnings, VariableChangeSource.Reset );
    }

    public void SetReadOnly(bool enabled)
    {
        if ( (State & VariableState.Disposed) != VariableState.Default )
            return;

        if ( (State & VariableState.ReadOnly) == (enabled ? VariableState.ReadOnly : VariableState.Default) )
            return;

        UpdateReadOnly( enabled );
    }

    protected virtual void OnPublishChangeEvent(VariableValueChangedEvent<TValue, TValidationResult> @event)
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
        State = UpdateState( State, VariableState.Changed, ! Comparer.Equals( OriginalValue, Value ) );
        State = UpdateState( State, VariableState.Invalid, Errors.Count > 0 );
        State = UpdateState( State, VariableState.Warning, Warnings.Count > 0 );
    }

    protected virtual void UpdateReadOnly(bool enabled)
    {
        State = UpdateState( State, VariableState.ReadOnly, enabled );
    }

    private void ChangeInternal(TValue value, VariableChangeSource changeSource)
    {
        var previousValue = Value;
        var previousErrors = Errors;
        var previousWarnings = Warnings;
        var previousState = State;

        Update( value );
        State |= VariableState.Dirty;

        PublishEvents( previousState, previousValue, previousErrors, previousWarnings, changeSource );
    }

    private void PublishEvents(
        VariableState previousState,
        TValue previousValue,
        Chain<TValidationResult> previousErrors,
        Chain<TValidationResult> previousWarnings,
        VariableChangeSource changeSource)
    {
        var changeEvent = new VariableValueChangedEvent<TValue, TValidationResult>( this, previousValue, previousState, changeSource );
        OnPublishChangeEvent( changeEvent );

        var validationEvent = new VariableValidationEvent<TValue, TValidationResult>(
            this,
            previousErrors,
            previousWarnings,
            previousState,
            changeEvent );

        OnPublishValidationEvent( validationEvent );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static VariableState UpdateState(VariableState current, VariableState value, bool enabled)
    {
        return enabled ? current | value : current & ~value;
    }
}
