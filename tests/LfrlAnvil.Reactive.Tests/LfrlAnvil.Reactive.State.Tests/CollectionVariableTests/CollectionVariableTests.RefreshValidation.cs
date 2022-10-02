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
    public void RefreshValidation_WithKey_ShouldRefreshElementAndCollectionValidation_WhenKeyExists()
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

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.RefreshValidation( element.Key );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid | VariableState.Warning );
            sut.Errors.Should().BeSequentiallyEqualTo( error );
            sut.Warnings.Should().BeSequentiallyEqualTo( warning );
            sut.Elements.GetState( element.Key )
                .Should()
                .Be( CollectionVariableElementState.Invalid | CollectionVariableElementState.Warning );

            sut.Elements.GetErrors( element.Key ).Should().BeSequentiallyEqualTo( elementError );
            sut.Elements.GetWarnings( element.Key ).Should().BeSequentiallyEqualTo( elementWarning );

            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( element );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
        }
    }

    [Fact]
    public void RefreshValidation_WithKey_ShouldDoNothing_WhenKeyDoesNotExist()
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

        sut.RefreshValidation( other.Key );

        onValidateEvents.Should().BeEmpty();
    }

    [Fact]
    public void RefreshValidation_WithKey_ShouldDoNothing_WhenVariableIsDisposed()
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

        sut.Dispose();

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.RefreshValidation( element.Key );

        onValidateEvents.Should().BeEmpty();
    }

    [Fact]
    public void RefreshValidation_WithKeyRange_ShouldUpdateElementsAndCollectionValidationWithoutDuplicates()
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

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.RefreshValidation( new[] { element.Key, otherElement.Key, element.Key, otherElement.Key, nonExistingElement.Key } );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid | VariableState.Warning );
            sut.Errors.Should().BeSequentiallyEqualTo( error );
            sut.Warnings.Should().BeSequentiallyEqualTo( warning );

            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( element, otherElement );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.AssociatedChange.Should().BeNull();
        }
    }

    [Fact]
    public void RefreshValidation_WithKeyRange_ShouldUpdateCollectionValidation_WhenKeysAreEmpty()
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

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.RefreshValidation( Array.Empty<int>() );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid | VariableState.Warning );
            sut.Errors.Should().BeSequentiallyEqualTo( error );
            sut.Warnings.Should().BeSequentiallyEqualTo( warning );

            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Should().BeEmpty();
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
        }
    }

    [Fact]
    public void RefreshValidation_WithKeyRange_ShouldUpdateCollectionValidation_WhenNoneOfTheKeysExist()
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

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.RefreshValidation( new[] { other1.Key, other2.Key } );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid | VariableState.Warning );
            sut.Errors.Should().BeSequentiallyEqualTo( error );
            sut.Warnings.Should().BeSequentiallyEqualTo( warning );

            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Should().BeEmpty();
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
        }
    }

    [Fact]
    public void RefreshValidation_WithKeyRange_ShouldUpdateElementsAndCollectionValidation_WhenFirstKeyDoesNotExist()
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

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.RefreshValidation( new[] { nonExistingElement.Key, element.Key, otherElement.Key } );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid | VariableState.Warning );
            sut.Errors.Should().BeSequentiallyEqualTo( error );
            sut.Warnings.Should().BeSequentiallyEqualTo( warning );

            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( element, otherElement );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
        }
    }

    [Fact]
    public void RefreshValidation_WithKeyRange_ShouldDoNothing_WhenVariableIsDisposed()
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

        sut.Dispose();

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.RefreshValidation( new[] { element.Key } );

        onValidateEvents.Should().BeEmpty();
    }

    [Fact]
    public void RefreshValidation_ShouldUpdateAllElementsAndCollectionValidation()
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

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.RefreshValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid | VariableState.Warning );
            sut.Errors.Should().BeSequentiallyEqualTo( error );
            sut.Warnings.Should().BeSequentiallyEqualTo( warning );

            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( element, otherElement );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
        }
    }
}
