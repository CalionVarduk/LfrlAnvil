using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableTests;

public partial class CollectionVariableTests
{
    [Fact]
    public void Replace_ShouldReplaceExistingElementAndUpdateChangedFlag_WhenElementExistsAndNewValueIsNotEqualToOldValue()
    {
        var key = Fixture.Create<int>();
        var (oldValue, value) = Fixture.CreateManyDistinct<string>( count: 2 );
        var initialElement = new TestElement( key, oldValue );
        var element = new TestElement( key, value );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { initialElement }, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( element );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.ModifiedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.GetState( element.Key ).TestEquals( CollectionVariableElementState.Changed ),
                sut.Elements.GetErrors( element.Key ).TestEmpty(),
                sut.Elements.GetWarnings( element.Key ).TestEmpty(),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].Source.TestEquals( VariableChangeSource.Change ),
                            e[0].RemovedElements.TestEmpty(),
                            e[0].RefreshedElements.TestEmpty(),
                            e[0].AddedElements.TestEmpty(),
                            e[0]
                                .ReplacedElements.TestCount( count => count.TestEquals( 1 ) )
                                .Then(
                                    s => Assertion.All(
                                        "elementSnapshot",
                                        s[0].Element.TestRefEquals( element ),
                                        s[0].PreviousElement.TestRefEquals( initialElement ),
                                        s[0].PreviousState.TestEquals( CollectionVariableElementState.Default ),
                                        s[0].NewState.TestEquals( sut.Elements.GetState( element.Key ) ),
                                        s[0].PreviousErrors.TestEmpty(),
                                        s[0].NewErrors.TestSequence( sut.Elements.GetErrors( element.Key ) ),
                                        s[0].PreviousWarnings.TestEmpty(),
                                        s[0].NewWarnings.TestSequence( sut.Elements.GetWarnings( element.Key ) ) ) ) ) ),
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
                            e[0].NewErrors.TestSequence( sut.Errors ),
                            e[0].Elements.Count.TestEquals( 1 ),
                            e[0]
                                .Elements.TestCount( count => count.TestEquals( 1 ) )
                                .Then(
                                    el =>
                                        el[0].TestRefEquals( onChangeEvents.FirstOrDefault()?.ReplacedElements.FirstOrDefault() ) ) ) ) )
            .Go();
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.ModifiedElementKeys.TestEmpty(),
                sut.Elements.GetState( element.Key ).TestEquals( CollectionVariableElementState.Default ),
                sut.Elements.GetErrors( element.Key ).TestEmpty(),
                sut.Elements.GetWarnings( element.Key ).TestEmpty(),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].Source.TestEquals( VariableChangeSource.Change ),
                            e[0].RemovedElements.TestEmpty(),
                            e[0].RefreshedElements.TestEmpty(),
                            e[0].AddedElements.TestEmpty(),
                            e[0]
                                .ReplacedElements.TestCount( count => count.TestEquals( 1 ) )
                                .Then(
                                    s => Assertion.All(
                                        "elementSnapshot",
                                        s[0].Element.TestRefEquals( element ),
                                        s[0].PreviousElement.TestRefEquals( initialElement ),
                                        s[0].PreviousState.TestEquals( CollectionVariableElementState.Default ),
                                        s[0].NewState.TestEquals( sut.Elements.GetState( element.Key ) ),
                                        s[0].PreviousErrors.TestEmpty(),
                                        s[0].NewErrors.TestSequence( sut.Elements.GetErrors( element.Key ) ),
                                        s[0].PreviousWarnings.TestEmpty(),
                                        s[0].NewWarnings.TestSequence( sut.Elements.GetWarnings( element.Key ) ) ) ) ) ),
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
                            e[0].NewErrors.TestSequence( sut.Errors ),
                            e[0]
                                .Elements.TestCount( count => count.TestEquals( 1 ) )
                                .Then(
                                    el =>
                                        el[0].TestRefEquals( onChangeEvents.FirstOrDefault()?.ReplacedElements.FirstOrDefault() ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Replace_ShouldUpdateErrorsAndWarnings_WhenElementIsReplaced()
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

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( element );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                sut.Errors.TestSequence( [ error ] ),
                sut.Warnings.TestSequence( [ warning ] ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.ModifiedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.InvalidElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.WarningElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.GetState( element.Key )
                    .TestEquals(
                        CollectionVariableElementState.Changed
                        | CollectionVariableElementState.Invalid
                        | CollectionVariableElementState.Warning ),
                sut.Elements.GetErrors( element.Key ).TestSequence( [ elementError ] ),
                sut.Elements.GetWarnings( element.Key ).TestSequence( [ elementWarning ] ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].Source.TestEquals( VariableChangeSource.Change ),
                            e[0].RemovedElements.TestEmpty(),
                            e[0].RefreshedElements.TestEmpty(),
                            e[0].AddedElements.TestEmpty(),
                            e[0]
                                .ReplacedElements.TestCount( count => count.TestEquals( 1 ) )
                                .Then(
                                    s => Assertion.All(
                                        "elementSnapshot",
                                        s[0].Element.TestRefEquals( element ),
                                        s[0].PreviousElement.TestRefEquals( initialElement ),
                                        s[0].PreviousState.TestEquals( CollectionVariableElementState.Default ),
                                        s[0].NewState.TestEquals( sut.Elements.GetState( element.Key ) ),
                                        s[0].PreviousErrors.TestEmpty(),
                                        s[0].NewErrors.TestSequence( sut.Elements.GetErrors( element.Key ) ),
                                        s[0].PreviousWarnings.TestEmpty(),
                                        s[0].NewWarnings.TestSequence( sut.Elements.GetWarnings( element.Key ) ) ) ) ) ),
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
                            e[0].NewErrors.TestSequence( sut.Errors ),
                            e[0]
                                .Elements.TestCount( count => count.TestEquals( 1 ) )
                                .Then(
                                    el =>
                                        el[0].TestRefEquals( onChangeEvents.FirstOrDefault()?.ReplacedElements.FirstOrDefault() ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Replace_ShouldUpdateNonInitialAddedElementCorrectly()
    {
        var key = Fixture.Create<int>();
        var (oldValue, value) = Fixture.CreateManyDistinct<string>( count: 2 );
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.ModifiedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.GetState( element.Key ).TestEquals( CollectionVariableElementState.Added ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Changed | VariableState.Dirty ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].Source.TestEquals( VariableChangeSource.Change ),
                            e[0].RemovedElements.TestEmpty(),
                            e[0].RefreshedElements.TestEmpty(),
                            e[0].AddedElements.TestEmpty(),
                            e[0]
                                .ReplacedElements.TestCount( count => count.TestEquals( 1 ) )
                                .Then(
                                    s => Assertion.All(
                                        "elementSnapshot",
                                        s[0].Element.TestRefEquals( element ),
                                        s[0].PreviousElement.TestRefEquals( oldElement ),
                                        s[0].PreviousState.TestEquals( CollectionVariableElementState.Added ),
                                        s[0].NewState.TestEquals( sut.Elements.GetState( element.Key ) ),
                                        s[0].PreviousErrors.TestEmpty(),
                                        s[0].NewErrors.TestSequence( sut.Elements.GetErrors( element.Key ) ),
                                        s[0].PreviousWarnings.TestEmpty(),
                                        s[0].NewWarnings.TestSequence( sut.Elements.GetWarnings( element.Key ) ) ) ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].AssociatedChange.TestRefEquals( onChangeEvents.FirstOrDefault() ),
                            e[0].PreviousState.TestEquals( VariableState.Changed | VariableState.Dirty ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestSequence( sut.Warnings ),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestSequence( sut.Errors ),
                            e[0]
                                .Elements.TestCount( count => count.TestEquals( 1 ) )
                                .Then(
                                    el =>
                                        el[0].TestRefEquals( onChangeEvents.FirstOrDefault()?.ReplacedElements.FirstOrDefault() ) ) ) ) )
            .Go();
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.Values.TestEmpty(),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Replace_ShouldDoNothing_WhenStateContainsReadOnlyFlag()
    {
        var key = Fixture.Create<int>();
        var (oldValue, value) = Fixture.CreateManyDistinct<string>( count: 2 );
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.State.TestEquals( VariableState.ReadOnly ),
                sut.Elements.Values.TestSetEqual( [ oldElement ] ),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Replace_WithRange_ShouldFilterElementsToReplaceCorrectlyAndReplaceOnlyThoseThatExistAndDoNotRepeat()
    {
        var allElements = Fixture.CreateManyDistinct<TestElement>( count: 8 );
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ allElements[0], elements[1], elements[2], allElements[4], elements[7], elements[8] ] ),
                sut.Elements.ModifiedElementKeys.TestSetEqual(
                    [ allElements[2].Key, allElements[3].Key, allElements[4].Key, allElements[5].Key, allElements[6].Key ] ),
                sut.Elements.GetState( allElements[0].Key ).TestEquals( CollectionVariableElementState.Default ),
                sut.Elements.GetState( allElements[1].Key ).TestEquals( CollectionVariableElementState.Default ),
                sut.Elements.GetState( allElements[2].Key ).TestEquals( CollectionVariableElementState.Changed ),
                sut.Elements.GetState( allElements[3].Key ).TestEquals( CollectionVariableElementState.Removed ),
                sut.Elements.GetState( allElements[4].Key ).TestEquals( CollectionVariableElementState.Added ),
                sut.Elements.GetState( allElements[5].Key ).TestEquals( CollectionVariableElementState.Added ),
                sut.Elements.GetState( allElements[6].Key ).TestEquals( CollectionVariableElementState.Added ),
                sut.Elements.GetState( allElements[7].Key ).TestEquals( CollectionVariableElementState.NotFound ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => e[0]
                            .ReplacedElements.Select( el => (el.Element, el.PreviousElement) )
                            .TestSequence(
                            [
                                (elements[1], allElements[1]), (elements[2], allElements[2]), (elements[7], allElements[5]),
                                (elements[8], allElements[6])
                            ] ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0].Elements.Select( el => el.Element ).TestSequence( [ elements[1], elements[2], elements[7], elements[8] ] ),
                            e[0].AssociatedChange.TestRefEquals( onChangeEvents.FirstOrDefault() ) ) ) )
            .Go();
    }

    [Fact]
    public void Replace_WithRange_ShouldReplaceSingleElementCorrectly_WhenItExists()
    {
        var key = Fixture.Create<int>();
        var (oldValue, value) = Fixture.CreateManyDistinct<string>( count: 2 );
        var initialElement = new TestElement( key, oldValue );
        var element = new TestElement( key, value );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { initialElement }, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Replace( new[] { element } );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => e[0]
                            .ReplacedElements.Select( el => (el.Element, el.PreviousElement) )
                            .TestSequence( [ (element, initialElement) ] ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => e[0].Elements.Select( el => el.Element ).TestSequence( [ element ] ) ) )
            .Go();
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.Values.TestEmpty(),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.Values.TestSetEqual( elements.AsEnumerable() ),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Replace_WithRange_ShouldDoNothing_WhenStateContainsReadOnlyFlag()
    {
        var key = Fixture.Create<int>();
        var (oldValue, value) = Fixture.CreateManyDistinct<string>( count: 2 );
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.State.TestEquals( VariableState.ReadOnly ),
                sut.Elements.Values.TestSetEqual( [ initialElement ] ),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }
}
