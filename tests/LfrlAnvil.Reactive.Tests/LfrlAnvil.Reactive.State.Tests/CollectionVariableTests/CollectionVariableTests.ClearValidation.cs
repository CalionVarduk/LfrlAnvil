using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableTests;

public partial class CollectionVariableTests
{
    [Fact]
    public void ClearValidation_WithKey_ShouldClearElementAndCollectionValidation_WhenKeyExistsAndHasErrorsOrWarnings()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateDistinctCollection<string>( count: 4 );
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

        sut.RefreshValidation( element.Key );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.ClearValidation( element.Key );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.Elements.GetState( element.Key ).Should().Be( CollectionVariableElementState.Default );
            sut.Elements.GetErrors( element.Key ).Should().BeEmpty();
            sut.Elements.GetWarnings( element.Key ).Should().BeEmpty();

            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( element );
            validateEvent.PreviousErrors.Should().BeSequentiallyEqualTo( error );
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
            validateEvent.PreviousWarnings.Should().BeSequentiallyEqualTo( warning );
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
        }
    }

    [Fact]
    public void ClearValidation_WithKey_ShouldDoNothing_WhenKeyExistsAndDoesNotHaveErrorsOrWarnings()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateDistinctCollection<string>( count: 4 );
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

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.ClearValidation( element.Key );

        onValidateEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearValidation_WithKey_ShouldDoNothing_WhenKeyDoesNotExist()
    {
        var (element, other) = Fixture.CreateDistinctCollection<TestElement>( count: 2 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateDistinctCollection<string>( count: 4 );
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

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.ClearValidation( other.Key );

        onValidateEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearValidation_WithKey_ShouldDoNothing_WhenVariableIsDisposed()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateDistinctCollection<string>( count: 4 );
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

        sut.RefreshValidation( element.Key );

        sut.Dispose();

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.ClearValidation( element.Key );

        onValidateEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearValidation_WithKeyRange_ShouldUpdateElementsAndCollectionValidationWithoutDuplicates()
    {
        var (element, otherElement, nonExistingElement) = Fixture.CreateDistinctCollection<TestElement>( count: 3 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateDistinctCollection<string>( count: 4 );
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

        sut.RefreshValidation();

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.ClearValidation( new[] { element.Key, otherElement.Key, element.Key, otherElement.Key, nonExistingElement.Key } );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();

            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( element, otherElement );
            validateEvent.PreviousErrors.Should().BeSequentiallyEqualTo( error );
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
            validateEvent.PreviousWarnings.Should().BeSequentiallyEqualTo( warning );
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.AssociatedChange.Should().BeNull();
        }
    }

    [Fact]
    public void ClearValidation_WithKeyRange_ShouldUpdateCollectionValidation_WhenKeysAreEmpty()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateDistinctCollection<string>( count: 4 );
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

        sut.RefreshValidation( Array.Empty<int>() );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.ClearValidation( Array.Empty<int>() );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();

            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Should().BeEmpty();
            validateEvent.PreviousErrors.Should().BeSequentiallyEqualTo( error );
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
            validateEvent.PreviousWarnings.Should().BeSequentiallyEqualTo( warning );
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
        }
    }

    [Fact]
    public void ClearValidation_WithKeyRange_ShouldUpdateCollectionValidation_WhenNoneOfTheKeysExist()
    {
        var (element, other1, other2) = Fixture.CreateDistinctCollection<TestElement>( count: 3 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateDistinctCollection<string>( count: 4 );
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

        sut.RefreshValidation( Array.Empty<int>() );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.ClearValidation( new[] { other1.Key, other2.Key } );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();

            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Should().BeEmpty();
            validateEvent.PreviousErrors.Should().BeSequentiallyEqualTo( error );
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
            validateEvent.PreviousWarnings.Should().BeSequentiallyEqualTo( warning );
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
        }
    }

    [Fact]
    public void ClearValidation_WithKeyRange_ShouldUpdateElementsAndCollectionValidation_WhenFirstKeyDoesNotExist()
    {
        var (element, otherElement, nonExistingElement) = Fixture.CreateDistinctCollection<TestElement>( count: 3 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateDistinctCollection<string>( count: 4 );
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

        sut.RefreshValidation();

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.ClearValidation( new[] { nonExistingElement.Key, element.Key, otherElement.Key } );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();

            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( element, otherElement );
            validateEvent.PreviousErrors.Should().BeSequentiallyEqualTo( error );
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
            validateEvent.PreviousWarnings.Should().BeSequentiallyEqualTo( warning );
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
        }
    }

    [Fact]
    public void ClearValidation_WithKeyRange_ShouldDoNothing_WhenVariableIsDisposed()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateDistinctCollection<string>( count: 4 );
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

        sut.RefreshValidation();
        sut.Dispose();

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.ClearValidation( new[] { element.Key } );

        onValidateEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearValidation_ShouldUpdateAllElementsAndCollectionValidation()
    {
        var (element, otherElement) = Fixture.CreateDistinctCollection<TestElement>( count: 2 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateDistinctCollection<string>( count: 4 );
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

        sut.RefreshValidation();

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.ClearValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();

            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( element, otherElement );
            validateEvent.PreviousErrors.Should().BeSequentiallyEqualTo( error );
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
            validateEvent.PreviousWarnings.Should().BeSequentiallyEqualTo( warning );
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
        }
    }
}
