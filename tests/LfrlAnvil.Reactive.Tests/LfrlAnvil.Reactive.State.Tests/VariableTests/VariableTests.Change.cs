﻿using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableTests;

public partial class VariableTests
{
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
        var sut = Variable.Create(
            value,
            errorsValidator: Validators<string>.IfTrue( v => v == changeValue, Validators<string>.Fail<int>( error ) ) );

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
            errorsValidator: Validators<string>.Pass<int>(),
            warningsValidator: Validators<string>.IfTrue( v => v == changeValue, Validators<string>.Fail<int>( warning ) ) );

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
}
