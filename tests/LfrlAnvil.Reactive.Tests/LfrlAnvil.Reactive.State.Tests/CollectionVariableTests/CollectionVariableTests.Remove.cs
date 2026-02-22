using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestEmpty(),
                sut.Elements.ModifiedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.GetState( element.Key ).TestEquals( CollectionVariableElementState.Removed ),
                sut.Elements.GetErrors( element.Key ).TestEmpty(),
                sut.Elements.GetWarnings( element.Key ).TestEmpty(),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].PreviousState.TestEquals( VariableState.Default ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].Source.TestEquals( VariableChangeSource.Change ),
                        e[0].AddedElements.TestEmpty(),
                        e[0].RefreshedElements.TestEmpty(),
                        e[0].ReplacedElements.TestEmpty(),
                        e[0]
                            .RemovedElements.TestCount( count => count.TestEquals( 1 ) )
                            .Then( s => Assertion.All(
                                "elementSnapshot",
                                s[0].Element.TestRefEquals( element ),
                                s[0].PreviousState.TestEquals( CollectionVariableElementState.Default ),
                                s[0].NewState.TestEquals( sut.Elements.GetState( element.Key ) ),
                                s[0].PreviousErrors.TestEmpty(),
                                s[0].NewErrors.TestSequence( sut.Elements.GetErrors( element.Key ) ),
                                s[0].PreviousWarnings.TestEmpty(),
                                s[0].NewWarnings.TestSequence( sut.Elements.GetWarnings( element.Key ) ) ) ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
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
                            .Then( el =>
                                el[0].TestRefEquals( onChangeEvents.FirstOrDefault()?.RemovedElements.FirstOrDefault() ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldUpdateErrorsAndWarnings_WhenElementIsRemoved()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var (error, warning, elementError, elementWarning) = Fixture.CreateManyDistinct<string>( count: 4 );
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                sut.Errors.TestSequence( [ error ] ),
                sut.Warnings.TestSequence( [ warning ] ),
                sut.Elements.Values.TestEmpty(),
                sut.Elements.ModifiedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.GetState( element.Key ).TestEquals( CollectionVariableElementState.NotFound ),
                sut.Elements.GetErrors( element.Key ).TestEmpty(),
                sut.Elements.GetWarnings( element.Key ).TestEmpty(),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0]
                            .PreviousState.TestEquals(
                                VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].Source.TestEquals( VariableChangeSource.Change ),
                        e[0].AddedElements.TestEmpty(),
                        e[0].RefreshedElements.TestEmpty(),
                        e[0].ReplacedElements.TestEmpty(),
                        e[0]
                            .RemovedElements.TestCount( count => count.TestEquals( 1 ) )
                            .Then( s => Assertion.All(
                                "elementSnapshot",
                                s[0].Element.TestRefEquals( element ),
                                s[0]
                                    .PreviousState.TestEquals(
                                        CollectionVariableElementState.Added
                                        | CollectionVariableElementState.Invalid
                                        | CollectionVariableElementState.Warning ),
                                s[0].NewState.TestEquals( sut.Elements.GetState( element.Key ) ),
                                s[0].PreviousErrors.TestSequence( [ elementError ] ),
                                s[0].NewErrors.TestSequence( sut.Elements.GetErrors( element.Key ) ),
                                s[0].PreviousWarnings.TestSequence( [ elementWarning ] ),
                                s[0].NewWarnings.TestSequence( sut.Elements.GetWarnings( element.Key ) ) ) ) ) ),
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
                        e[0].NewErrors.TestSequence( sut.Errors ),
                        e[0].Elements.Count.TestEquals( 1 ),
                        e[0]
                            .Elements.TestCount( count => count.TestEquals( 1 ) )
                            .Then( el =>
                                el[0].TestRefEquals( onChangeEvents.FirstOrDefault()?.RemovedElements.FirstOrDefault() ) ) ) ) )
            .Go();
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.Values.TestEmpty(),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.State.TestEquals( VariableState.ReadOnly ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Remove_WithRange_ShouldFilterKeysToRemoveCorrectlyAndRemoveOnlyThoseThatExistAndDoNotRepeat()
    {
        var allElements = Fixture.CreateManyDistinct<TestElement>( count: 4 );
        var initialElements = new[] { allElements[0], allElements[1] };
        var keys = new[]
        {
            allElements[3].Key, allElements[1].Key, allElements[2].Key, allElements[1].Key, allElements[2].Key, allElements[3].Key
        };

        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( initialElements, keySelector );
        sut.Add( allElements[2] );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Remove( keys );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ allElements[0] ] ),
                sut.Elements.ModifiedElementKeys.TestSetEqual( [ allElements[1].Key ] ),
                sut.Elements.GetState( allElements[0].Key ).TestEquals( CollectionVariableElementState.Default ),
                sut.Elements.GetState( allElements[1].Key ).TestEquals( CollectionVariableElementState.Removed ),
                sut.Elements.GetState( allElements[2].Key ).TestEquals( CollectionVariableElementState.NotFound ),
                sut.Elements.GetState( allElements[3].Key ).TestEquals( CollectionVariableElementState.NotFound ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => e[0].RemovedElements.Select( el => el.Element ).TestSequence( [ allElements[1], allElements[2] ] ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "validateEvent",
                        e[0].Elements.Select( el => el.Element ).TestSequence( [ allElements[1], allElements[2] ] ),
                        e[0].AssociatedChange.TestRefEquals( onChangeEvents.FirstOrDefault() ) ) ) )
            .Go();
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestEmpty(),
                onChangeEvents.Count.TestEquals( 1 ),
                onChangeEvents[0].RemovedElements.Select( e => e.Element ).TestSequence( [ element ] ),
                onValidateEvents.Count.TestEquals( 1 ),
                onValidateEvents[0].Elements.Select( e => e.Element ).TestSequence( [ element ] ) )
            .Go();
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.Values.TestEmpty(),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.Values.TestSetEqual( elements.AsEnumerable() ),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
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

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.State.TestEquals( VariableState.ReadOnly ),
                sut.Elements.Values.TestSetEqual( elements.AsEnumerable() ),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }
}
