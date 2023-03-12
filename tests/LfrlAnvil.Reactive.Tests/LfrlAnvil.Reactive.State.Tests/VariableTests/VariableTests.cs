using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableTests;

public partial class VariableTests : TestsBase
{
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
        var sut = Variable.Create( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );

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
        var sut = Variable.Create( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
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
        var sut = Variable.Create( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
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
