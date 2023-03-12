using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableTests;

public partial class VariableTests
{
    [Fact]
    public void Reset_ShouldUpdateInitialValueAndValueAndResetFlagsAndValidation_WhenNewInitialValueIsNotEqualToNewValue()
    {
        var (initialValue, value, newInitialValue, newValue) = Fixture.CreateDistinctCollection<int>( count: 4 );
        var (error, warning) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var errorsValidator = Validators<string>.Fail<int>( error );
        var warningsValidator = Validators<string>.Fail<int>( warning );
        var sut = Variable.Create( initialValue, value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
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
        var sut = Variable.Create( initialValue, value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
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
}
