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
    public void Remove_ShouldRemoveExistingElementAndUpdateChangedFlag_WhenElementKeyExists()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { element }, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Remove( element.Key );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEmpty();
            sut.Elements.ModifiedKeys.Should().BeEquivalentTo( element.Key );
            sut.Elements.GetState( element.Key ).Should().Be( CollectionVariableElementState.Removed );
            sut.Elements.GetErrors( element.Key ).Should().BeEmpty();
            sut.Elements.GetWarnings( element.Key ).Should().BeEmpty();
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RefreshedElements.Should().BeEmpty();
            changeEvent.ReplacedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().HaveCount( 1 );

            var elementSnapshot = changeEvent.RemovedElements[0];
            elementSnapshot.Element.Should().BeSameAs( element );
            elementSnapshot.PreviousState.Should().Be( CollectionVariableElementState.Default );
            elementSnapshot.NewState.Should().Be( sut.Elements.GetState( element.Key ) );
            elementSnapshot.PreviousErrors.Should().BeEmpty();
            elementSnapshot.NewErrors.Should().BeSequentiallyEqualTo( sut.Elements.GetErrors( element.Key ) );
            elementSnapshot.PreviousWarnings.Should().BeEmpty();
            elementSnapshot.NewWarnings.Should().BeSequentiallyEqualTo( sut.Elements.GetWarnings( element.Key ) );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.PreviousState.Should().Be( VariableState.Default );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeSequentiallyEqualTo( sut.Warnings );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeSequentiallyEqualTo( sut.Errors );
            validateEvent.Elements.Should().HaveCount( 1 );
            validateEvent.Elements[0].Should().BeSameAs( elementSnapshot );
        }
    }

    [Fact]
    public void Remove_ShouldUpdateErrorsAndWarnings_WhenElementIsRemoved()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateDistinctCollection<string>( count: 4 );
        var errorsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( error );
        var warningsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( warning );
        var elementErrorsValidator = Validators<string>.Fail<TestElement>( elementError );
        var elementWarningsValidator = Validators<string>.Fail<TestElement>( elementWarning );
        var sut = CollectionVariable.Create(
            Array.Empty<TestElement>(),
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator,
            elementErrorsValidator: elementErrorsValidator,
            elementWarningsValidator: elementWarningsValidator );

        sut.Add( element );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Remove( element.Key );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Invalid | VariableState.Warning | VariableState.Dirty );
            sut.Errors.Should().BeSequentiallyEqualTo( error );
            sut.Warnings.Should().BeSequentiallyEqualTo( warning );
            sut.Elements.Values.Should().BeEmpty();
            sut.Elements.ModifiedKeys.Should().BeEmpty();
            sut.Elements.InvalidKeys.Should().BeEmpty();
            sut.Elements.WarningKeys.Should().BeEmpty();
            sut.Elements.GetState( element.Key ).Should().Be( CollectionVariableElementState.NotFound );
            sut.Elements.GetErrors( element.Key ).Should().BeEmpty();
            sut.Elements.GetWarnings( element.Key ).Should().BeEmpty();
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should()
                .Be( VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty );

            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RefreshedElements.Should().BeEmpty();
            changeEvent.ReplacedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().HaveCount( 1 );

            var elementSnapshot = changeEvent.RemovedElements[0];
            elementSnapshot.Element.Should().BeSameAs( element );
            elementSnapshot.PreviousState.Should()
                .Be(
                    CollectionVariableElementState.Added |
                    CollectionVariableElementState.Invalid |
                    CollectionVariableElementState.Warning );

            elementSnapshot.NewState.Should().Be( sut.Elements.GetState( element.Key ) );
            elementSnapshot.PreviousErrors.Should().BeSequentiallyEqualTo( elementError );
            elementSnapshot.NewErrors.Should().BeSequentiallyEqualTo( sut.Elements.GetErrors( element.Key ) );
            elementSnapshot.PreviousWarnings.Should().BeSequentiallyEqualTo( elementWarning );
            elementSnapshot.NewWarnings.Should().BeSequentiallyEqualTo( sut.Elements.GetWarnings( element.Key ) );

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
            validateEvent.Elements.Should().HaveCount( 1 );
            validateEvent.Elements[0].Should().BeSameAs( elementSnapshot );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenElementKeyDoesNotExist()
    {
        var key = Fixture.Create<int>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Remove( key );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.State.Should().Be( VariableState.Default );
            sut.Elements.Values.Should().BeEmpty();
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenStateContainsReadOnlyFlag()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Remove( element.Key );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.State.Should().Be( VariableState.ReadOnly );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void Remove_WithRange_ShouldFilterKeysToRemoveCorrectlyAndRemoveOnlyThoseThatExistAndDoNotRepeat()
    {
        var allElements = Fixture.CreateDistinctCollection<TestElement>( count: 4 );
        var initialElements = new[] { allElements[0], allElements[1] };
        var keys = new[]
        {
            allElements[3].Key,
            allElements[1].Key,
            allElements[2].Key,
            allElements[1].Key,
            allElements[2].Key,
            allElements[3].Key
        };

        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( initialElements, keySelector );
        sut.Add( allElements[2] );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Remove( keys );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEquivalentTo( allElements[0] );
            sut.Elements.ModifiedKeys.Should().BeEquivalentTo( allElements[1].Key );
            sut.Elements.GetState( allElements[0].Key ).Should().Be( CollectionVariableElementState.Default );
            sut.Elements.GetState( allElements[1].Key ).Should().Be( CollectionVariableElementState.Removed );
            sut.Elements.GetState( allElements[2].Key ).Should().Be( CollectionVariableElementState.NotFound );
            sut.Elements.GetState( allElements[3].Key ).Should().Be( CollectionVariableElementState.NotFound );
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.RemovedElements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( allElements[1], allElements[2] );

            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( allElements[1], allElements[2] );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
        }
    }

    [Fact]
    public void Remove_WithRange_ShouldRemoveSingleKeyCorrectly_WhenItExists()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { element }, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Remove( new[] { element.Key } );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEmpty();
            onChangeEvents.Should().HaveCount( 1 );
            onChangeEvents[0].RemovedElements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( element );
            onValidateEvents.Should().HaveCount( 1 );
            onValidateEvents[0].Elements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( element );
        }
    }

    [Fact]
    public void Remove_WithRange_ShouldDoNothing_WhenNoneOfTheKeysExist()
    {
        var key = Fixture.Create<int>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Remove( new[] { key, key } );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.State.Should().Be( VariableState.Default );
            sut.Elements.Values.Should().BeEmpty();
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void Remove_WithRange_ShouldDoNothing_WhenElementsToAddAreEmpty()
    {
        var elements = new[] { Fixture.Create<TestElement>() };
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( elements, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Remove( Array.Empty<int>() );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.State.Should().Be( VariableState.Default );
            sut.Elements.Values.Should().BeEquivalentTo( elements.AsEnumerable() );
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void Remove_WithRange_ShouldDoNothing_WhenStateContainsReadOnlyFlag()
    {
        var elements = new[] { Fixture.Create<TestElement>() };
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( elements, keySelector );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Remove( elements.Select( e => e.Key ) );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.State.Should().Be( VariableState.ReadOnly );
            sut.Elements.Values.Should().BeEquivalentTo( elements.AsEnumerable() );
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }
}
