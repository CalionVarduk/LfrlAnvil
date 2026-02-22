using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableTests;

public partial class VariableTests
{
    [Fact]
    public void Reset_ShouldUpdateInitialValueAndValueAndResetFlagsAndValidation_WhenNewInitialValueIsNotEqualToNewValue()
    {
        var (initialValue, value, newInitialValue, newValue) = Fixture.CreateManyDistinct<int>( count: 4 );
        var (error, warning) = Fixture.CreateManyDistinct<string>( count: 2 );
        var errorsValidator = Validators<string>.Fail<int>( error );
        var warningsValidator = Validators<string>.Fail<int>( warning );
        var sut = Variable.Create( initialValue, value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
        sut.Refresh();

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.Reset( newInitialValue, newValue );

        Assertion.All(
                sut.State.TestEquals( VariableState.Changed ),
                sut.InitialValue.TestEquals( newInitialValue ),
                sut.Value.TestEquals( newValue ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0]
                            .PreviousState.TestEquals(
                                VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].PreviousValue.TestEquals( value ),
                        e[0].NewValue.TestEquals( newValue ),
                        e[0].Source.TestEquals( VariableChangeSource.Reset ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "validateEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].AssociatedChange.TestRefEquals( onChangeEvents.FirstOrDefault() ),
                        e[0]
                            .PreviousState.TestEquals(
                                VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].PreviousWarnings.TestSequence( [ warning ] ),
                        e[0].NewWarnings.TestSequence( sut.Warnings ),
                        e[0].PreviousErrors.TestSequence( [ error ] ),
                        e[0].NewErrors.TestSequence( sut.Errors ) ) ) )
            .Go();
    }

    [Fact]
    public void Reset_ShouldUpdateInitialValueAndValueAndResetFlagsAndValidation_WhenNewInitialValueIsEqualToNewValue()
    {
        var (initialValue, value, newValue) = Fixture.CreateManyDistinct<int>( count: 3 );
        var (error, warning) = Fixture.CreateManyDistinct<string>( count: 2 );
        var errorsValidator = Validators<string>.Fail<int>( error );
        var warningsValidator = Validators<string>.Fail<int>( warning );
        var sut = Variable.Create( initialValue, value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
        sut.Refresh();

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.Reset( newValue, newValue );

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                sut.InitialValue.TestEquals( newValue ),
                sut.Value.TestEquals( newValue ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0]
                            .PreviousState.TestEquals(
                                VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].PreviousValue.TestEquals( value ),
                        e[0].NewValue.TestEquals( newValue ),
                        e[0].Source.TestEquals( VariableChangeSource.Reset ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "validateEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].AssociatedChange.TestRefEquals( onChangeEvents.FirstOrDefault() ),
                        e[0]
                            .PreviousState.TestEquals(
                                VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].PreviousWarnings.TestSequence( [ warning ] ),
                        e[0].NewWarnings.TestSequence( sut.Warnings ),
                        e[0].PreviousErrors.TestSequence( [ error ] ),
                        e[0].NewErrors.TestSequence( sut.Errors ) ) ) )
            .Go();
    }

    [Fact]
    public void Reset_ShouldPreserveReadOnlyFlag_WhenItIsEnabled()
    {
        var (initialValue, value, newValue) = Fixture.CreateManyDistinct<int>( count: 3 );
        var sut = Variable.WithoutValidators<string>.Create( initialValue, value );
        sut.SetReadOnly( true );

        sut.Reset( newValue, newValue );

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly ),
                sut.InitialValue.TestEquals( newValue ),
                sut.Value.TestEquals( newValue ) )
            .Go();
    }

    [Fact]
    public void Reset_ShouldDoNothing_WhenVariableIsDisposed()
    {
        var (value, newInitialValue, newValue) = Fixture.CreateManyDistinct<int>( count: 3 );
        var sut = Variable.WithoutValidators<string>.Create( value );

        var onChangeEvents = new List<VariableValueChangeEvent<int, string>>();
        var onValidateEvents = new List<VariableValidationEvent<int, string>>();
        sut.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( onChangeEvents.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( onValidateEvents.Add ) );

        sut.Dispose();
        sut.Reset( newInitialValue, newValue );

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                sut.InitialValue.TestEquals( value ),
                sut.Value.TestEquals( value ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }
}
