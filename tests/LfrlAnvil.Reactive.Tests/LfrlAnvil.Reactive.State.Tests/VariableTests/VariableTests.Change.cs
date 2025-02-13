using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableTests;

public partial class VariableTests
{
    [Fact]
    public void Change_ShouldUpdateValueAndChangedFlag_WhenNewValueIsNotEqualToCurrentValue()
    {
        var (value, changeValue) = Fixture.CreateManyDistinct<int>( count: 2 );
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        var result = sut.Change( changeValue );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Value.TestEquals( changeValue ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].PreviousValue.TestEquals( value ),
                            e[0].NewValue.TestEquals( changeValue ),
                            e[0].Source.TestEquals( VariableChangeSource.Change ) ) ),
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( expectedBaseState | VariableState.Invalid | VariableState.Dirty ),
                sut.Value.TestEquals( changeValue ),
                sut.Errors.TestSequence( [ error ] ),
                sut.Warnings.TestEmpty(),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].PreviousValue.TestEquals( value ),
                            e[0].NewValue.TestEquals( changeValue ),
                            e[0].Source.TestEquals( VariableChangeSource.Change ) ) ),
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( expectedBaseState | VariableState.Warning | VariableState.Dirty ),
                sut.Value.TestEquals( changeValue ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestSequence( [ warning ] ),
                onChangeEvents.Count.TestEquals( 1 ),
                onValidateEvents.Count.TestEquals( 1 ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].PreviousValue.TestEquals( value ),
                            e[0].NewValue.TestEquals( changeValue ),
                            e[0].Source.TestEquals( VariableChangeSource.Change ) ) ),
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
    public void Change_ShouldDoNothing_WhenStateContainsReadOnlyFlag()
    {
        var (value, changeValue) = Fixture.CreateManyDistinct<int>( count: 2 );
        var sut = Variable.WithoutValidators<string>.Create( value );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        var result = sut.Change( changeValue );

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.State.TestEquals( VariableState.ReadOnly ),
                sut.Value.TestEquals( value ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }
}
