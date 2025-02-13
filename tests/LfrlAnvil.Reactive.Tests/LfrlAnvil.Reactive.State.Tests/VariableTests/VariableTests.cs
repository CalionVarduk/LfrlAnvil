using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableTests;

public partial class VariableTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnInformationAboutValueAndState()
    {
        var (initialValue, value) = Fixture.CreateManyDistinct<int>( count: 2 );
        var expected = $"Value: '{value}', State: Changed, ReadOnly";
        var sut = Variable.WithoutValidators<string>.Create( initialValue, value );
        sut.SetReadOnly( true );

        var result = sut.ToString();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Dispose_ShouldDisposeUnderlyingOnChangeAndOnValidateEventPublishersAndAddReadOnlyAndDisposedState()
    {
        var (initialValue, value) = Fixture.CreateManyDistinct<int>( count: 2 );
        var sut = Variable.WithoutValidators<string>.Create( initialValue, value );

        var onChange = sut.OnChange.Listen( Substitute.For<IEventListener<VariableValueChangeEvent<int, string>>>() );
        var onValidate = sut.OnValidate.Listen( Substitute.For<IEventListener<VariableValidationEvent<int, string>>>() );

        sut.Dispose();

        Assertion.All(
                onChange.IsDisposed.TestTrue(),
                onValidate.IsDisposed.TestTrue(),
                sut.State.TestEquals( VariableState.Changed | VariableState.ReadOnly | VariableState.Disposed ) )
            .Go();
    }

    [Fact]
    public void Refresh_ShouldUpdateChangedFlagAndErrorsAndWarningsForCurrentValue()
    {
        var value = new List<int>();
        var (error, warning) = Fixture.CreateManyDistinct<string>( count: 2 );
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

        Assertion.All(
                sut.State.TestEquals( VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                sut.Value.TestRefEquals( value ),
                sut.Errors.TestSequence( [ error ] ),
                sut.Warnings.TestSequence( [ warning ] ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].PreviousValue.TestEquals( value ),
                            e[0].NewValue.TestEquals( value ),
                            e[0].Source.TestEquals( VariableChangeSource.Refresh ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].AssociatedChange.TestRefEquals( onChangeEvents.FirstOrDefault() ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestSequence( sut.Warnings ),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestSequence( sut.Errors ) ) ) )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly | VariableState.Dirty ),
                sut.Value.TestEquals( value ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.ReadOnly ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].PreviousValue.TestEquals( value ),
                            e[0].NewValue.TestEquals( value ),
                            e[0].Source.TestEquals( VariableChangeSource.Refresh ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].AssociatedChange.TestRefEquals( onChangeEvents.FirstOrDefault() ),
                            e[0].PreviousState.TestEquals( VariableState.ReadOnly ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestSequence( sut.Warnings ),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestSequence( sut.Errors ) ) ) )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                sut.Value.TestEquals( value ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void RefreshValidation_ShouldUpdateErrorsAndWarningsForCurrentValue()
    {
        var value = Fixture.Create<int>();
        var (error, warning) = Fixture.CreateManyDistinct<string>( count: 2 );
        var errorsValidator = Validators<string>.Fail<int>( error );
        var warningsValidator = Validators<string>.Fail<int>( warning );
        var sut = Variable.Create( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );

        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.RefreshValidation();

        Assertion.All(
                sut.State.TestEquals( VariableState.Invalid | VariableState.Warning ),
                sut.Value.TestEquals( value ),
                sut.Errors.TestSequence( [ error ] ),
                sut.Warnings.TestSequence( [ warning ] ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].AssociatedChange.TestNull(),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestSequence( sut.Warnings ),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestSequence( sut.Errors ) ) ) )
            .Go();
    }

    [Fact]
    public void RefreshValidation_ShouldUpdateErrorsAndWarningsForCurrentValue_EvenWhenReadOnlyFlagIsSet()
    {
        var value = Fixture.Create<int>();
        var (error, warning) = Fixture.CreateManyDistinct<string>( count: 2 );
        var errorsValidator = Validators<string>.Fail<int>( error );
        var warningsValidator = Validators<string>.Fail<int>( warning );
        var sut = Variable.Create( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
        sut.SetReadOnly( true );

        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.RefreshValidation();

        Assertion.All(
                sut.State.TestEquals( VariableState.Invalid | VariableState.Warning | VariableState.ReadOnly ),
                sut.Value.TestEquals( value ),
                sut.Errors.TestSequence( [ error ] ),
                sut.Warnings.TestSequence( [ warning ] ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].AssociatedChange.TestNull(),
                            e[0].PreviousState.TestEquals( VariableState.ReadOnly ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestSequence( sut.Warnings ),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestSequence( sut.Errors ) ) ) )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                sut.Value.TestEquals( value ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void ClearValidation_ShouldResetErrorsAndWarningsToEmpty_WhenVariableHasAnyErrorsOrWarnings()
    {
        var value = Fixture.Create<int>();
        var (error, warning) = Fixture.CreateManyDistinct<string>( count: 2 );
        var errorsValidator = Validators<string>.Fail<int>( error );
        var warningsValidator = Validators<string>.Fail<int>( warning );
        var sut = Variable.Create( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
        sut.Refresh();

        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.ClearValidation();

        Assertion.All(
                sut.State.TestEquals( VariableState.Dirty ),
                sut.Value.TestEquals( value ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].AssociatedChange.TestNull(),
                            e[0].PreviousState.TestEquals( VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].PreviousWarnings.TestSequence( [ warning ] ),
                            e[0].NewWarnings.TestSequence( sut.Warnings ),
                            e[0].PreviousErrors.TestSequence( [ error ] ),
                            e[0].NewErrors.TestSequence( sut.Errors ) ) ) )
            .Go();
    }

    [Fact]
    public void ClearValidation_ShouldDoNothing_WhenVariableDoesNotHaveAnyErrorsOrWarnings()
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.ClearValidation();

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                sut.Value.TestEquals( value ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                sut.Value.TestEquals( value ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetReadOnly_ShouldEnableReadOnlyFlag_WhenNewValueIsTrueAndCurrentValueIsFalse()
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );

        sut.SetReadOnly( true );

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].PreviousValue.TestEquals( value ),
                            e[0].NewValue.TestEquals( value ),
                            e[0].Source.TestEquals( VariableChangeSource.SetReadOnly ) ) ) )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.ReadOnly ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].PreviousValue.TestEquals( value ),
                            e[0].NewValue.TestEquals( value ),
                            e[0].Source.TestEquals( VariableChangeSource.SetReadOnly ) ) ) )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( enabled ? VariableState.ReadOnly : VariableState.Default ),
                onChangeEvents.TestEmpty() )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                onChangeEvents.TestEmpty() )
            .Go();
    }
}
