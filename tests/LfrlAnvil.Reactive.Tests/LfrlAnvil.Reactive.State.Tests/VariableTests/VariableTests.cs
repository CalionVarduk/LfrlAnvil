using System.Collections.Generic;
using FluentAssertions.Execution;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableTests;

public class VariableTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = new Variable<int, string>( initialValue, value, comparer, errorsValidator, warningsValidator );

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( value );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );

            ((IReadOnlyVariable<int, string>)sut).OnValidate.Should().BeSameAs( sut.OnValidate );
            ((IReadOnlyVariable<int>)sut).OnChange.Should().BeSameAs( sut.OnChange );
            ((IReadOnlyVariable)sut).ValueType.Should().Be( typeof( int ) );
            ((IReadOnlyVariable)sut).ValidationResultType.Should().Be( typeof( string ) );
            ((IReadOnlyVariable)sut).InitialValue.Should().Be( sut.InitialValue );
            ((IReadOnlyVariable)sut).Value.Should().Be( sut.Value );
            ((IReadOnlyVariable)sut).Errors.Should().BeSequentiallyEqualTo( sut.Errors );
            ((IReadOnlyVariable)sut).Warnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            ((IReadOnlyVariable)sut).OnValidate.Should().Be( sut.OnValidate );
            ((IReadOnlyVariable)sut).OnChange.Should().Be( sut.OnChange );
            ((IVariableNode)sut).OnValidate.Should().Be( sut.OnValidate );
            ((IVariableNode)sut).OnChange.Should().Be( sut.OnChange );
            ((IVariableNode)sut).GetChildren().Should().BeEmpty();
        }
    }

    [Fact]
    public void Ctor_ShouldSetStateToDefault_WhenInitialValueEqualsValue()
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );
        sut.State.Should().Be( VariableState.Default );
    }

    [Fact]
    public void Ctor_ShouldNotInvokeErrorsValidator()
    {
        var value = Fixture.Create<int>();
        var errorsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );

        var sut = Variable.Create( value, errorsValidator );

        sut.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_ShouldNotInvokeWarningsValidator()
    {
        var value = Fixture.Create<int>();
        var warningsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );

        var sut = Variable.Create( value, Validators<string>.Pass<int>(), warningsValidator );

        sut.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void ToString_ShouldReturnInformationAboutValueAndState()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var expected = $"Value: '{value}', State: Changed, ReadOnly";
        var sut = Variable.WithoutValidators<string>.Create( initialValue, value );
        sut.SetReadOnly( true );

        var result = sut.ToString();

        result.Should().Be( expected );
    }

    [Fact]
    public void Dispose_ShouldDisposeUnderlyingOnChangeAndOnValidateEventPublishersAndAddReadOnlyAndDisposedState()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = Variable.WithoutValidators<string>.Create( initialValue, value );

        var onChange = sut.OnChange.Listen( Substitute.For<IEventListener<VariableValueChangeEvent<int, string>>>() );
        var onValidate = sut.OnValidate.Listen( Substitute.For<IEventListener<VariableValidationEvent<int, string>>>() );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            onChange.IsDisposed.Should().BeTrue();
            onValidate.IsDisposed.Should().BeTrue();
            sut.State.Should().Be( VariableState.Changed | VariableState.ReadOnly | VariableState.Disposed );
        }
    }

    [Fact]
    public void Change_ShouldUpdateValueAndChangedFlag_WhenNewValueIsNotEqualToCurrentValue()
    {
        var (value, changeValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        var result = sut.Change( changeValue );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Value.Should().Be( changeValue );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.PreviousValue.Should().Be( value );
            changeEvent.NewValue.Should().Be( changeValue );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.PreviousState.Should().Be( VariableState.Default );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
        }
    }

    [Theory]
    [InlineData( 1, 1, VariableState.Default )]
    [InlineData( 1, 2, VariableState.Changed )]
    public void Change_ShouldUpdateValueAndErrorsBasedOnNewValue_WhenErrorsValidatorReturnsNonEmptyChain(
        int value,
        int changeValue,
        VariableState expectedBaseState)
    {
        var error = Fixture.Create<string>();
        var sut = Variable.Create( value, Validators<string>.IfTrue( v => v == changeValue, Validators<string>.Fail<int>( error ) ) );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        var result = sut.Change( changeValue );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( expectedBaseState | VariableState.Invalid | VariableState.Dirty );
            sut.Value.Should().Be( changeValue );
            sut.Errors.Should().BeSequentiallyEqualTo( error );
            sut.Warnings.Should().BeEmpty();
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.PreviousValue.Should().Be( value );
            changeEvent.NewValue.Should().Be( changeValue );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.PreviousState.Should().Be( VariableState.Default );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
        }
    }

    [Theory]
    [InlineData( 1, 1, VariableState.Default )]
    [InlineData( 1, 2, VariableState.Changed )]
    public void Change_ShouldUpdateValueAndWarningsBasedOnNewValue_WhenWarningsValidatorReturnsNonEmptyChain(
        int value,
        int changeValue,
        VariableState expectedBaseState)
    {
        var warning = Fixture.Create<string>();
        var sut = Variable.Create(
            value,
            Validators<string>.Pass<int>(),
            Validators<string>.IfTrue( v => v == changeValue, Validators<string>.Fail<int>( warning ) ) );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        var result = sut.Change( changeValue );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( expectedBaseState | VariableState.Warning | VariableState.Dirty );
            sut.Value.Should().Be( changeValue );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeSequentiallyEqualTo( warning );
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.PreviousValue.Should().Be( value );
            changeEvent.NewValue.Should().Be( changeValue );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.PreviousState.Should().Be( VariableState.Default );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
        }
    }

    [Fact]
    public void Change_ShouldDoNothing_WhenStateContainsReadOnlyFlag()
    {
        var (value, changeValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = Variable.WithoutValidators<string>.Create( value );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        var result = sut.Change( changeValue );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.State.Should().Be( VariableState.ReadOnly );
            sut.Value.Should().Be( value );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void TryChange_ShouldUpdateValueAndChangedFlag_WhenNewValueIsNotEqualToCurrentValue()
    {
        var (value, changeValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        var result = sut.TryChange( changeValue );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Value.Should().Be( changeValue );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.PreviousValue.Should().Be( value );
            changeEvent.NewValue.Should().Be( changeValue );
            changeEvent.Source.Should().Be( VariableChangeSource.TryChange );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.PreviousState.Should().Be( VariableState.Default );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
        }
    }

    [Fact]
    public void TryChange_ShouldUpdateValueAndErrorsBasedOnNewValue_WhenErrorsValidatorReturnsNonEmptyChain()
    {
        var (value, changeValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var error = Fixture.Create<string>();
        var sut = Variable.Create( value, Validators<string>.IfTrue( v => v == changeValue, Validators<string>.Fail<int>( error ) ) );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        var result = sut.TryChange( changeValue );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Invalid | VariableState.Dirty );
            sut.Value.Should().Be( changeValue );
            sut.Errors.Should().BeSequentiallyEqualTo( error );
            sut.Warnings.Should().BeEmpty();
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.PreviousValue.Should().Be( value );
            changeEvent.NewValue.Should().Be( changeValue );
            changeEvent.Source.Should().Be( VariableChangeSource.TryChange );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.PreviousState.Should().Be( VariableState.Default );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
        }
    }

    [Fact]
    public void TryChange_ShouldUpdateValueAndWarningsBasedOnNewValue_WhenWarningsValidatorReturnsNonEmptyChain()
    {
        var (value, changeValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var warning = Fixture.Create<string>();
        var sut = Variable.Create(
            value,
            Validators<string>.Pass<int>(),
            Validators<string>.IfTrue( v => v == changeValue, Validators<string>.Fail<int>( warning ) ) );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        var result = sut.TryChange( changeValue );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Warning | VariableState.Dirty );
            sut.Value.Should().Be( changeValue );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeSequentiallyEqualTo( warning );
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.PreviousValue.Should().Be( value );
            changeEvent.NewValue.Should().Be( changeValue );
            changeEvent.Source.Should().Be( VariableChangeSource.TryChange );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.PreviousState.Should().Be( VariableState.Default );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
        }
    }

    [Fact]
    public void TryChange_ShouldDoNothing_WhenNewValueIsEqualToCurrentValue()
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        var result = sut.TryChange( value );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.State.Should().Be( VariableState.Default );
            sut.Value.Should().Be( value );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void TryChange_ShouldDoNothing_WhenStateContainsReadOnlyFlag()
    {
        var (value, changeValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = Variable.WithoutValidators<string>.Create( value );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        var result = sut.TryChange( changeValue );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.State.Should().Be( VariableState.ReadOnly );
            sut.Value.Should().Be( value );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void Refresh_ShouldUpdateChangedFlagAndErrorsAndWarningsForCurrentValue()
    {
        var value = new List<int>();
        var (error, warning) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var comparer = EqualityComparerFactory<List<int>>.Create( (a, b) => a?.Count == b?.Count );
        var errorsValidator = Validators<string>.Empty<int>( error );
        var warningsValidator = Validators<string>.Empty<int>( warning );
        var sut = Variable.Create( new List<int>(), value, comparer, errorsValidator, warningsValidator );

        var onChangeEvents = new List<VariableValueChangeEvent<List<int>, string>>();
        var onValidateEvents = new List<VariableValidationEvent<List<int>, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<List<int>, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<List<int>, string>>( onValidateEvents.Add ) );

        value.Add( Fixture.Create<int>() );
        sut.Refresh();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty );
            sut.Value.Should().BeSameAs( value );
            sut.Errors.Should().BeSequentiallyEqualTo( error );
            sut.Warnings.Should().BeSequentiallyEqualTo( warning );
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.PreviousValue.Should().BeSameAs( value );
            changeEvent.NewValue.Should().BeSameAs( value );
            changeEvent.Source.Should().Be( VariableChangeSource.Refresh );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.PreviousState.Should().Be( VariableState.Default );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
        }
    }

    [Fact]
    public void Refresh_ShouldPerformUpdates_EvenWhenReadOnlyFlagIsSet()
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.Refresh();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly | VariableState.Dirty );
            sut.Value.Should().Be( value );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.ReadOnly );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.PreviousValue.Should().Be( value );
            changeEvent.NewValue.Should().Be( value );
            changeEvent.Source.Should().Be( VariableChangeSource.Refresh );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.PreviousState.Should().Be( VariableState.ReadOnly );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
        }
    }

    [Fact]
    public void Refresh_ShouldDoNothing_WhenVariableIsDisposed()
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.Dispose();
        sut.Refresh();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            sut.Value.Should().Be( value );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void RefreshValidation_ShouldUpdateErrorsAndWarningsForCurrentValue()
    {
        var value = Fixture.Create<int>();
        var (error, warning) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var errorsValidator = Validators<string>.Fail<int>( error );
        var warningsValidator = Validators<string>.Fail<int>( warning );
        var sut = Variable.Create( value, errorsValidator, warningsValidator );

        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.RefreshValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid | VariableState.Warning );
            sut.Value.Should().Be( value );
            sut.Errors.Should().BeSequentiallyEqualTo( error );
            sut.Warnings.Should().BeSequentiallyEqualTo( warning );
            onValidateEvents.Should().HaveCount( 1 );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeNull();
            validateEvent.PreviousState.Should().Be( VariableState.Default );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
        }
    }

    [Fact]
    public void RefreshValidation_ShouldUpdateErrorsAndWarningsForCurrentValue_EvenWhenReadOnlyFlagIsSet()
    {
        var value = Fixture.Create<int>();
        var (error, warning) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var errorsValidator = Validators<string>.Fail<int>( error );
        var warningsValidator = Validators<string>.Fail<int>( warning );
        var sut = Variable.Create( value, errorsValidator, warningsValidator );
        sut.SetReadOnly( true );

        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.RefreshValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid | VariableState.Warning | VariableState.ReadOnly );
            sut.Value.Should().Be( value );
            sut.Errors.Should().BeSequentiallyEqualTo( error );
            sut.Warnings.Should().BeSequentiallyEqualTo( warning );
            onValidateEvents.Should().HaveCount( 1 );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeNull();
            validateEvent.PreviousState.Should().Be( VariableState.ReadOnly );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
        }
    }

    [Fact]
    public void RefreshValidation_ShouldDoNothing_WhenVariableIsDisposed()
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.Dispose();
        sut.RefreshValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            sut.Value.Should().Be( value );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void ClearValidation_ShouldResetErrorsAndWarningsToEmpty_WhenVariableHasAnyErrorsOrWarnings()
    {
        var value = Fixture.Create<int>();
        var (error, warning) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var errorsValidator = Validators<string>.Fail<int>( error );
        var warningsValidator = Validators<string>.Fail<int>( warning );
        var sut = Variable.Create( value, errorsValidator, warningsValidator );
        sut.Refresh();

        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.ClearValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Dirty );
            sut.Value.Should().Be( value );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onValidateEvents.Should().HaveCount( 1 );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeNull();
            validateEvent.PreviousState.Should().Be( VariableState.Invalid | VariableState.Warning | VariableState.Dirty );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeSequentiallyEqualTo( warning );
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeSequentiallyEqualTo( error );
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
        }
    }

    [Fact]
    public void ClearValidation_ShouldDoNothing_WhenVariableDoesNotHaveAnyErrorsOrWarnings()
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.ClearValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Value.Should().Be( value );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void ClearValidation_ShouldDoNothing_WhenVariableIsDisposed()
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.Dispose();
        sut.ClearValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            sut.Value.Should().Be( value );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void Reset_ShouldUpdateInitialValueAndValueAndResetFlagsAndValidation_WhenNewInitialValueIsNotEqualToNewValue()
    {
        var (initialValue, value, newInitialValue, newValue) = Fixture.CreateDistinctCollection<int>( count: 4 );
        var (error, warning) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var errorsValidator = Validators<string>.Fail<int>( error );
        var warningsValidator = Validators<string>.Fail<int>( warning );
        var sut = Variable.Create( initialValue, value, errorsValidator, warningsValidator );
        sut.Refresh();

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.Reset( newInitialValue, newValue );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed );
            sut.InitialValue.Should().Be( newInitialValue );
            sut.Value.Should().Be( newValue );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should()
                .Be( VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty );

            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.PreviousValue.Should().Be( value );
            changeEvent.NewValue.Should().Be( newValue );
            changeEvent.Source.Should().Be( VariableChangeSource.Reset );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.PreviousState.Should()
                .Be( VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty );

            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeSequentiallyEqualTo( warning );
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeSequentiallyEqualTo( error );
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
        }
    }

    [Fact]
    public void Reset_ShouldUpdateInitialValueAndValueAndResetFlagsAndValidation_WhenNewInitialValueIsEqualToNewValue()
    {
        var (initialValue, value, newValue) = Fixture.CreateDistinctCollection<int>( count: 3 );
        var (error, warning) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var errorsValidator = Validators<string>.Fail<int>( error );
        var warningsValidator = Validators<string>.Fail<int>( warning );
        var sut = Variable.Create( initialValue, value, errorsValidator, warningsValidator );
        sut.Refresh();

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.Reset( newValue, newValue );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.InitialValue.Should().Be( newValue );
            sut.Value.Should().Be( newValue );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should()
                .Be( VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty );

            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.PreviousValue.Should().Be( value );
            changeEvent.NewValue.Should().Be( newValue );
            changeEvent.Source.Should().Be( VariableChangeSource.Reset );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.PreviousState.Should()
                .Be( VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty );

            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeSequentiallyEqualTo( warning );
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeSequentiallyEqualTo( error );
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
        }
    }

    [Fact]
    public void Reset_ShouldPreserveReadOnlyFlag_WhenItIsEnabled()
    {
        var (initialValue, value, newValue) = Fixture.CreateDistinctCollection<int>( count: 3 );
        var sut = Variable.WithoutValidators<string>.Create( initialValue, value );
        sut.SetReadOnly( true );

        sut.Reset( newValue, newValue );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly );
            sut.InitialValue.Should().Be( newValue );
            sut.Value.Should().Be( newValue );
        }
    }

    [Fact]
    public void Reset_ShouldDoNothing_WhenVariableIsDisposed()
    {
        var (value, newInitialValue, newValue) = Fixture.CreateDistinctCollection<int>( count: 3 );
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.Dispose();
        sut.Reset( newInitialValue, newValue );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            sut.InitialValue.Should().Be( value );
            sut.Value.Should().Be( value );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetReadOnly_ShouldEnableReadOnlyFlag_WhenNewValueIsTrueAndCurrentValueIsFalse()
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );

        sut.SetReadOnly( true );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly );
            onChangeEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.PreviousValue.Should().Be( value );
            changeEvent.NewValue.Should().Be( value );
            changeEvent.Source.Should().Be( VariableChangeSource.SetReadOnly );
        }
    }

    [Fact]
    public void SetReadOnly_ShouldDisableReadOnlyFlag_WhenNewValueIsFalseAndCurrentValueIsTrue()
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );

        sut.SetReadOnly( false );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            onChangeEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.ReadOnly );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.PreviousValue.Should().Be( value );
            changeEvent.NewValue.Should().Be( value );
            changeEvent.Source.Should().Be( VariableChangeSource.SetReadOnly );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetReadOnly_ShouldDoNothing_WhenReadOnlyFlagIsEqualToNewValue(bool enabled)
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );
        sut.SetReadOnly( enabled );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );

        sut.SetReadOnly( enabled );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( enabled ? VariableState.ReadOnly : VariableState.Default );
            onChangeEvents.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetReadOnly_ShouldDoNothing_WhenVariableIsDisposed(bool enabled)
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );

        sut.Dispose();
        sut.SetReadOnly( enabled );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            onChangeEvents.Should().BeEmpty();
        }
    }
}
