using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableTests;

public partial class CollectionVariableTests
{
    [Fact]
    public void Refresh_WithKey_ShouldRefreshElementAndCollection_WhenKeyExists()
    {
        var key = Fixture.Create<int>();
        var (oldValue, value) = Fixture.CreateManyDistinct<string>( count: 2 );
        var initialElement = new TestElement( key, oldValue );
        var element = new TestElement( key, value );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateManyDistinct<string>( count: 4 );
        var errorsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( error );
        var warningsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( warning );
        var elementErrorsValidator = Validators<string>.Fail<TestElement>( elementError );
        var elementWarningsValidator = Validators<string>.Fail<TestElement>( elementWarning );
        var sut = CollectionVariable.Create(
            new[] { initialElement },
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator,
            elementErrorsValidator: elementErrorsValidator,
            elementWarningsValidator: elementWarningsValidator );

        sut.Replace( element );
        element.Value = oldValue;

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Refresh( element.Key );

        Assertion.All(
                sut.State.TestEquals( VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                sut.Errors.TestSequence( [ error ] ),
                sut.Warnings.TestSequence( [ warning ] ),
                sut.Elements.GetState( element.Key )
                    .TestEquals( CollectionVariableElementState.Invalid | CollectionVariableElementState.Warning ),
                sut.Elements.GetErrors( element.Key ).TestSequence( [ elementError ] ),
                sut.Elements.GetWarnings( element.Key ).TestSequence( [ elementWarning ] ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Source.TestEquals( VariableChangeSource.Refresh ),
                            e[0].RefreshedElements.Select( el => el.Element ).TestSequence( [ element ] ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0].Elements.Select( el => el.Element ).TestSequence( [ element ] ),
                            e[0].NewErrors.TestSequence( sut.Errors ),
                            e[0].NewWarnings.TestSequence( sut.Warnings ),
                            e[0].AssociatedChange.TestRefEquals( onChangeEvents.FirstOrDefault() ) ) ) )
            .Go();
    }

    [Fact]
    public void Refresh_WithKey_ShouldDoNothing_WhenKeyDoesNotExist()
    {
        var (element, other) = Fixture.CreateManyDistinct<TestElement>( count: 2 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateManyDistinct<string>( count: 4 );
        var errorsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( error );
        var warningsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( warning );
        var elementErrorsValidator = Validators<string>.Fail<TestElement>( elementError );
        var elementWarningsValidator = Validators<string>.Fail<TestElement>( elementWarning );
        var sut = CollectionVariable.Create(
            new[] { element },
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator,
            elementErrorsValidator: elementErrorsValidator,
            elementWarningsValidator: elementWarningsValidator );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Refresh( other.Key );

        Assertion.All(
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Refresh_WithKey_ShouldDoNothing_WhenVariableIsDisposed()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateManyDistinct<string>( count: 4 );
        var errorsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( error );
        var warningsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( warning );
        var elementErrorsValidator = Validators<string>.Fail<TestElement>( elementError );
        var elementWarningsValidator = Validators<string>.Fail<TestElement>( elementWarning );
        var sut = CollectionVariable.Create(
            new[] { element },
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator,
            elementErrorsValidator: elementErrorsValidator,
            elementWarningsValidator: elementWarningsValidator );

        sut.Dispose();

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Refresh( element.Key );

        Assertion.All(
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Refresh_WithKeyRange_ShouldUpdateElementsAndCollectionWithoutDuplicates()
    {
        var (element, otherElement, nonExistingElement) = Fixture.CreateManyDistinct<TestElement>( count: 3 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateManyDistinct<string>( count: 4 );
        var errorsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( error );
        var warningsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( warning );
        var elementErrorsValidator = Validators<string>.Fail<TestElement>( elementError );
        var elementWarningsValidator = Validators<string>.Fail<TestElement>( elementWarning );
        var sut = CollectionVariable.Create(
            new[] { element, otherElement },
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator,
            elementErrorsValidator: elementErrorsValidator,
            elementWarningsValidator: elementWarningsValidator );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Refresh( new[] { element.Key, otherElement.Key, element.Key, otherElement.Key, nonExistingElement.Key } );

        Assertion.All(
                sut.State.TestEquals( VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                sut.Errors.TestSequence( [ error ] ),
                sut.Warnings.TestSequence( [ warning ] ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Source.TestEquals( VariableChangeSource.Refresh ),
                            e[0].RefreshedElements.Select( el => el.Element ).TestSequence( [ element, otherElement ] ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0].Elements.Select( el => el.Element ).TestSequence( [ element, otherElement ] ),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestSequence( sut.Errors ),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestSequence( sut.Warnings ),
                            e[0].AssociatedChange.TestRefEquals( onChangeEvents.FirstOrDefault() ) ) ) )
            .Go();
    }

    [Fact]
    public void Refresh_WithKeyRange_ShouldUpdateCollectionValidation_WhenKeysAreEmpty()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateManyDistinct<string>( count: 4 );
        var errorsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( error );
        var warningsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( warning );
        var elementErrorsValidator = Validators<string>.Fail<TestElement>( elementError );
        var elementWarningsValidator = Validators<string>.Fail<TestElement>( elementWarning );
        var sut = CollectionVariable.Create(
            new[] { element },
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator,
            elementErrorsValidator: elementErrorsValidator,
            elementWarningsValidator: elementWarningsValidator );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Refresh( Array.Empty<int>() );

        Assertion.All(
                sut.State.TestEquals( VariableState.Invalid | VariableState.Warning ),
                sut.Errors.TestSequence( [ error ] ),
                sut.Warnings.TestSequence( [ warning ] ),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0].Elements.TestEmpty(),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestSequence( sut.Errors ),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestSequence( sut.Warnings ) ) ) )
            .Go();
    }

    [Fact]
    public void Refresh_WithKeyRange_ShouldUpdateCollectionValidation_WhenNoneOfTheKeysExist()
    {
        var (element, other1, other2) = Fixture.CreateManyDistinct<TestElement>( count: 3 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateManyDistinct<string>( count: 4 );
        var errorsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( error );
        var warningsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( warning );
        var elementErrorsValidator = Validators<string>.Fail<TestElement>( elementError );
        var elementWarningsValidator = Validators<string>.Fail<TestElement>( elementWarning );
        var sut = CollectionVariable.Create(
            new[] { element },
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator,
            elementErrorsValidator: elementErrorsValidator,
            elementWarningsValidator: elementWarningsValidator );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Refresh( new[] { other1.Key, other2.Key } );

        Assertion.All(
                sut.State.TestEquals( VariableState.Invalid | VariableState.Warning ),
                sut.Errors.TestSequence( [ error ] ),
                sut.Warnings.TestSequence( [ warning ] ),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0].Elements.TestEmpty(),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestSequence( sut.Errors ),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestSequence( sut.Warnings ) ) ) )
            .Go();
    }

    [Fact]
    public void Refresh_WithKeyRange_ShouldUpdateElementsAndCollection_WhenFirstKeyDoesNotExist()
    {
        var (element, otherElement, nonExistingElement) = Fixture.CreateManyDistinct<TestElement>( count: 3 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateManyDistinct<string>( count: 4 );
        var errorsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( error );
        var warningsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( warning );
        var elementErrorsValidator = Validators<string>.Fail<TestElement>( elementError );
        var elementWarningsValidator = Validators<string>.Fail<TestElement>( elementWarning );
        var sut = CollectionVariable.Create(
            new[] { element, otherElement },
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator,
            elementErrorsValidator: elementErrorsValidator,
            elementWarningsValidator: elementWarningsValidator );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Refresh( new[] { nonExistingElement.Key, element.Key, otherElement.Key } );

        Assertion.All(
                sut.State.TestEquals( VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                sut.Errors.TestSequence( [ error ] ),
                sut.Warnings.TestSequence( [ warning ] ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Source.TestEquals( VariableChangeSource.Refresh ),
                            e[0].RefreshedElements.Select( el => el.Element ).TestSequence( [ element, otherElement ] ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0].Elements.Select( el => el.Element ).TestSequence( [ element, otherElement ] ),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestSequence( sut.Errors ),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestSequence( sut.Warnings ) ) ) )
            .Go();
    }

    [Fact]
    public void Refresh_WithKeyRange_ShouldDoNothing_WhenVariableIsDisposed()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateManyDistinct<string>( count: 4 );
        var errorsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( error );
        var warningsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( warning );
        var elementErrorsValidator = Validators<string>.Fail<TestElement>( elementError );
        var elementWarningsValidator = Validators<string>.Fail<TestElement>( elementWarning );
        var sut = CollectionVariable.Create(
            new[] { element },
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator,
            elementErrorsValidator: elementErrorsValidator,
            elementWarningsValidator: elementWarningsValidator );

        sut.Dispose();

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Refresh( new[] { element.Key } );

        Assertion.All(
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Refresh_ShouldUpdateAllElementsAndCollection()
    {
        var (element, otherElement) = Fixture.CreateManyDistinct<TestElement>( count: 2 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateManyDistinct<string>( count: 4 );
        var errorsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( error );
        var warningsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( warning );
        var elementErrorsValidator = Validators<string>.Fail<TestElement>( elementError );
        var elementWarningsValidator = Validators<string>.Fail<TestElement>( elementWarning );
        var sut = CollectionVariable.Create(
            new[] { element, otherElement },
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator,
            elementErrorsValidator: elementErrorsValidator,
            elementWarningsValidator: elementWarningsValidator );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Refresh();

        Assertion.All(
                sut.State.TestEquals( VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                sut.Errors.TestSequence( [ error ] ),
                sut.Warnings.TestSequence( [ warning ] ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Source.TestEquals( VariableChangeSource.Refresh ),
                            e[0].RefreshedElements.Select( el => el.Element ).TestSequence( [ element, otherElement ] ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0].Elements.Select( el => el.Element ).TestSequence( [ element, otherElement ] ),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestSequence( sut.Errors ),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestSequence( sut.Warnings ) ) ) )
            .Go();
    }
}
