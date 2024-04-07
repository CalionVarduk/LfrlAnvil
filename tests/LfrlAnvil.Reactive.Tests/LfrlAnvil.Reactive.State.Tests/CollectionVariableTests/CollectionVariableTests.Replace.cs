using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableTests;

public partial class CollectionVariableTests
{
    [Fact]
    public void Replace_ShouldReplaceExistingElementAndUpdateChangedFlag_WhenElementExistsAndNewValueIsNotEqualToOldValue()
    {
        var key = Fixture.Create<int>();
        var (oldValue, value) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var initialElement = new TestElement( key, oldValue );
        var element = new TestElement( key, value );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { initialElement }, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( element );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.ModifiedElementKeys.Should().BeEquivalentTo( element.Key );
            sut.Elements.GetState( element.Key ).Should().Be( CollectionVariableElementState.Changed );
            sut.Elements.GetErrors( element.Key ).Should().BeEmpty();
            sut.Elements.GetWarnings( element.Key ).Should().BeEmpty();
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.RemovedElements.Should().BeEmpty();
            changeEvent.RefreshedElements.Should().BeEmpty();
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.ReplacedElements.Should().HaveCount( 1 );

            var elementSnapshot = changeEvent.ReplacedElements[0];
            elementSnapshot.Element.Should().BeSameAs( element );
            elementSnapshot.PreviousElement.Should().BeSameAs( initialElement );
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
    public void Replace_ShouldReplaceExistingElementAndUpdateChangedFlag_WhenElementExistsAndNewValueIsEqualToOldValue()
    {
        var initialElement = Fixture.Create<TestElement>();
        var element = new TestElement( initialElement.Key, initialElement.Value );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { initialElement }, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( element );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Dirty );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.ModifiedElementKeys.Should().BeEmpty();
            sut.Elements.GetState( element.Key ).Should().Be( CollectionVariableElementState.Default );
            sut.Elements.GetErrors( element.Key ).Should().BeEmpty();
            sut.Elements.GetWarnings( element.Key ).Should().BeEmpty();
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.RemovedElements.Should().BeEmpty();
            changeEvent.RefreshedElements.Should().BeEmpty();
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.ReplacedElements.Should().HaveCount( 1 );

            var elementSnapshot = changeEvent.ReplacedElements[0];
            elementSnapshot.Element.Should().BeSameAs( element );
            elementSnapshot.PreviousElement.Should().BeSameAs( initialElement );
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
    public void Replace_ShouldUpdateErrorsAndWarnings_WhenElementIsReplaced()
    {
        var key = Fixture.Create<int>();
        var (oldValue, value) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var initialElement = new TestElement( key, oldValue );
        var element = new TestElement( key, value );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateDistinctCollection<string>( count: 4 );
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

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( element );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty );
            sut.Errors.Should().BeSequentiallyEqualTo( error );
            sut.Warnings.Should().BeSequentiallyEqualTo( warning );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.ModifiedElementKeys.Should().BeEquivalentTo( element.Key );
            sut.Elements.InvalidElementKeys.Should().BeEquivalentTo( element.Key );
            sut.Elements.WarningElementKeys.Should().BeEquivalentTo( element.Key );
            sut.Elements.GetState( element.Key )
                .Should()
                .Be(
                    CollectionVariableElementState.Changed
                    | CollectionVariableElementState.Invalid
                    | CollectionVariableElementState.Warning );

            sut.Elements.GetErrors( element.Key ).Should().BeSequentiallyEqualTo( elementError );
            sut.Elements.GetWarnings( element.Key ).Should().BeSequentiallyEqualTo( elementWarning );

            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.RemovedElements.Should().BeEmpty();
            changeEvent.RefreshedElements.Should().BeEmpty();
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.ReplacedElements.Should().HaveCount( 1 );

            var elementSnapshot = changeEvent.ReplacedElements[0];
            elementSnapshot.Element.Should().BeSameAs( element );
            elementSnapshot.PreviousElement.Should().BeSameAs( initialElement );
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
    public void Replace_ShouldUpdateNonInitialAddedElementCorrectly()
    {
        var key = Fixture.Create<int>();
        var (oldValue, value) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var oldElement = new TestElement( key, oldValue );
        var element = new TestElement( key, value );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );
        sut.Add( oldElement );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( element );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.ModifiedElementKeys.Should().BeEquivalentTo( element.Key );
            sut.Elements.GetState( element.Key ).Should().Be( CollectionVariableElementState.Added );
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Changed | VariableState.Dirty );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.RemovedElements.Should().BeEmpty();
            changeEvent.RefreshedElements.Should().BeEmpty();
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.ReplacedElements.Should().HaveCount( 1 );

            var elementSnapshot = changeEvent.ReplacedElements[0];
            elementSnapshot.Element.Should().BeSameAs( element );
            elementSnapshot.PreviousElement.Should().BeSameAs( oldElement );
            elementSnapshot.PreviousState.Should().Be( CollectionVariableElementState.Added );
            elementSnapshot.NewState.Should().Be( sut.Elements.GetState( element.Key ) );
            elementSnapshot.PreviousErrors.Should().BeEmpty();
            elementSnapshot.NewErrors.Should().BeSequentiallyEqualTo( sut.Elements.GetErrors( element.Key ) );
            elementSnapshot.PreviousWarnings.Should().BeEmpty();
            elementSnapshot.NewWarnings.Should().BeSequentiallyEqualTo( sut.Elements.GetWarnings( element.Key ) );

            var validateEvent = onValidateEvents[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.PreviousState.Should().Be( VariableState.Changed | VariableState.Dirty );
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
    public void Replace_ShouldDoNothing_WhenElementDoesNotExist()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( element );

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
    public void Replace_ShouldDoNothing_WhenStateContainsReadOnlyFlag()
    {
        var key = Fixture.Create<int>();
        var (oldValue, value) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var oldElement = new TestElement( key, oldValue );
        var element = new TestElement( key, value );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { oldElement }, keySelector );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( element );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.State.Should().Be( VariableState.ReadOnly );
            sut.Elements.Values.Should().BeEquivalentTo( oldElement );
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void Replace_WithRange_ShouldFilterElementsToReplaceCorrectlyAndReplaceOnlyThoseThatExistAndDoNotRepeat()
    {
        var allElements = Fixture.CreateDistinctCollection<TestElement>( count: 8 );
        var initialElements = new[] { allElements[0], allElements[1], allElements[2], allElements[3] };
        var elements = new[]
        {
            allElements[7],
            new TestElement( allElements[1].Key, allElements[1].Value ),
            new TestElement( allElements[2].Key, Fixture.Create<string>() ),
            allElements[3],
            allElements[2],
            allElements[1],
            allElements[2],
            new TestElement( allElements[5].Key, allElements[5].Value ),
            new TestElement( allElements[6].Key, Fixture.Create<string>() ),
            allElements[7]
        };

        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( initialElements, keySelector );
        sut.Add( allElements[4] );
        sut.Add( allElements[5] );
        sut.Add( allElements[6] );
        sut.Remove( allElements[3].Key );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( elements );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should()
                .BeEquivalentTo( allElements[0], elements[1], elements[2], allElements[4], elements[7], elements[8] );

            sut.Elements.ModifiedElementKeys.Should()
                .BeEquivalentTo( allElements[2].Key, allElements[3].Key, allElements[4].Key, allElements[5].Key, allElements[6].Key );

            sut.Elements.GetState( allElements[0].Key ).Should().Be( CollectionVariableElementState.Default );
            sut.Elements.GetState( allElements[1].Key ).Should().Be( CollectionVariableElementState.Default );
            sut.Elements.GetState( allElements[2].Key ).Should().Be( CollectionVariableElementState.Changed );
            sut.Elements.GetState( allElements[3].Key ).Should().Be( CollectionVariableElementState.Removed );
            sut.Elements.GetState( allElements[4].Key ).Should().Be( CollectionVariableElementState.Added );
            sut.Elements.GetState( allElements[5].Key ).Should().Be( CollectionVariableElementState.Added );
            sut.Elements.GetState( allElements[6].Key ).Should().Be( CollectionVariableElementState.Added );
            sut.Elements.GetState( allElements[7].Key ).Should().Be( CollectionVariableElementState.NotFound );
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.ReplacedElements.Select( e => (e.Element, e.PreviousElement) )
                .Should()
                .BeSequentiallyEqualTo(
                    (elements[1], allElements[1]),
                    (elements[2], allElements[2]),
                    (elements[7], allElements[5]),
                    (elements[8], allElements[6]) );

            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Select( e => e.Element )
                .Should()
                .BeSequentiallyEqualTo( elements[1], elements[2], elements[7], elements[8] );

            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
        }
    }

    [Fact]
    public void Replace_WithRange_ShouldReplaceSingleElementCorrectly_WhenItExists()
    {
        var key = Fixture.Create<int>();
        var (oldValue, value) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var initialElement = new TestElement( key, oldValue );
        var element = new TestElement( key, value );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { initialElement }, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( new[] { element } );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            onChangeEvents.Should().HaveCount( 1 );
            onChangeEvents[0]
                .ReplacedElements.Select( e => (e.Element, e.PreviousElement) )
                .Should()
                .BeSequentiallyEqualTo( (element, initialElement) );

            onValidateEvents.Should().HaveCount( 1 );
            onValidateEvents[0].Elements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( element );
        }
    }

    [Fact]
    public void Replace_WithRange_ShouldDoNothing_WhenAllElementsDoNotExist()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( new[] { element, element } );

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
    public void Replace_WithRange_ShouldDoNothing_WhenElementsToReplaceAreEmpty()
    {
        var elements = new[] { Fixture.Create<TestElement>() };
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( elements, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( Array.Empty<TestElement>() );

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
    public void Replace_WithRange_ShouldDoNothing_WhenStateContainsReadOnlyFlag()
    {
        var key = Fixture.Create<int>();
        var (oldValue, value) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var initialElement = new TestElement( key, oldValue );
        var element = new TestElement( key, value );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { initialElement }, keySelector );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( new[] { element } );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.State.Should().Be( VariableState.ReadOnly );
            sut.Elements.Values.Should().BeEquivalentTo( initialElement );
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }
}
