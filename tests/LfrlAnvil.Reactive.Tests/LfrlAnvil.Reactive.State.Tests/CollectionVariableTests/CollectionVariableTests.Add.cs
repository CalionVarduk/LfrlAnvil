using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableTests;

public partial class CollectionVariableTests
{
    [Fact]
    public void Add_ShouldAddNewElementAndUpdateChangedFlag_WhenNewElementDoesNotExist()
    {
        var (initialElement, element) = Fixture.CreateManyDistinct<TestElement>( count: 2 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { initialElement }, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Add( element );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ initialElement, element ] ),
                sut.Elements.ModifiedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.GetState( element.Key ).TestEquals( CollectionVariableElementState.Added ),
                sut.Elements.GetErrors( element.Key ).TestEmpty(),
                sut.Elements.GetWarnings( element.Key ).TestEmpty(),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].PreviousState.TestEquals( VariableState.Default ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].Source.TestEquals( VariableChangeSource.Change ),
                        e[0].RemovedElements.TestEmpty(),
                        e[0].RefreshedElements.TestEmpty(),
                        e[0].ReplacedElements.TestEmpty(),
                        e[0]
                            .AddedElements.TestCount( count => count.TestEquals( 1 ) )
                            .Then( s => Assertion.All(
                                "elementSnapshot",
                                s[0].Element.TestRefEquals( element ),
                                s[0].PreviousState.TestEquals( CollectionVariableElementState.NotFound ),
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
                                el[0].TestRefEquals( onChangeEvents.FirstOrDefault()?.AddedElements.FirstOrDefault() ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldUpdateErrorsAndWarnings_WhenNewElementIsAdded()
    {
        var (initialElement, element) = Fixture.CreateManyDistinct<TestElement>( count: 2 );
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

        var result = sut.Add( element );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                sut.Errors.TestSequence( [ error ] ),
                sut.Warnings.TestSequence( [ warning ] ),
                sut.Elements.Values.TestSetEqual( [ initialElement, element ] ),
                sut.Elements.ModifiedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.InvalidElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.WarningElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.GetState( element.Key )
                    .TestEquals(
                        CollectionVariableElementState.Added
                        | CollectionVariableElementState.Invalid
                        | CollectionVariableElementState.Warning ),
                sut.Elements.GetErrors( element.Key ).TestSequence( [ elementError ] ),
                sut.Elements.GetWarnings( element.Key ).TestSequence( [ elementWarning ] ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].PreviousState.TestEquals( VariableState.Default ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].Source.TestEquals( VariableChangeSource.Change ),
                        e[0].RemovedElements.TestEmpty(),
                        e[0].RefreshedElements.TestEmpty(),
                        e[0].ReplacedElements.TestEmpty(),
                        e[0]
                            .AddedElements.TestCount( count => count.TestEquals( 1 ) )
                            .Then( s => Assertion.All(
                                "elementSnapshot",
                                s[0].Element.TestRefEquals( element ),
                                s[0].PreviousState.TestEquals( CollectionVariableElementState.NotFound ),
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
                                el[0].TestRefEquals( onChangeEvents.FirstOrDefault()?.AddedElements.FirstOrDefault() ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddInitialElementCorrectlyAfterItWasRemoved()
    {
        var initialElement = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { initialElement }, keySelector );
        sut.Remove( initialElement.Key );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Add( initialElement );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ initialElement ] ),
                sut.Elements.ModifiedElementKeys.TestEmpty(),
                sut.Elements.GetState( initialElement.Key ).TestEquals( CollectionVariableElementState.Default ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].PreviousState.TestEquals( VariableState.Changed | VariableState.Dirty ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].Source.TestEquals( VariableChangeSource.Change ),
                        e[0].RemovedElements.TestEmpty(),
                        e[0].RefreshedElements.TestEmpty(),
                        e[0].ReplacedElements.TestEmpty(),
                        e[0]
                            .AddedElements.TestCount( count => count.TestEquals( 1 ) )
                            .Then( s => Assertion.All(
                                "elementSnapshot",
                                s[0].Element.TestRefEquals( initialElement ),
                                s[0].PreviousState.TestEquals( CollectionVariableElementState.Removed ),
                                s[0].NewState.TestEquals( sut.Elements.GetState( initialElement.Key ) ),
                                s[0].PreviousErrors.TestEmpty(),
                                s[0].NewErrors.TestSequence( sut.Elements.GetErrors( initialElement.Key ) ),
                                s[0].PreviousWarnings.TestEmpty(),
                                s[0].NewWarnings.TestSequence( sut.Elements.GetWarnings( initialElement.Key ) ) ) ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
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
                            .Then( el =>
                                el[0].TestRefEquals( onChangeEvents.FirstOrDefault()?.AddedElements.FirstOrDefault() ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddInitialElementCorrectlyAfterItWasRemoved_WithChangedFlag()
    {
        var key = Fixture.Create<int>();
        var (oldValue, value) = Fixture.CreateManyDistinct<string>( count: 2 );
        var initialElement = new TestElement( key, oldValue );
        var element = new TestElement( key, value );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { initialElement }, keySelector );
        sut.Remove( initialElement.Key );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Add( element );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.ModifiedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.GetState( element.Key ).TestEquals( CollectionVariableElementState.Changed ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].PreviousState.TestEquals( VariableState.Changed | VariableState.Dirty ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].Source.TestEquals( VariableChangeSource.Change ),
                        e[0].RemovedElements.TestEmpty(),
                        e[0].RefreshedElements.TestEmpty(),
                        e[0].ReplacedElements.TestEmpty(),
                        e[0]
                            .AddedElements.TestCount( count => count.TestEquals( 1 ) )
                            .Then( s => Assertion.All(
                                "elementSnapshot",
                                s[0].Element.TestRefEquals( element ),
                                s[0].PreviousState.TestEquals( CollectionVariableElementState.Removed ),
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
                        e[0].PreviousState.TestEquals( VariableState.Changed | VariableState.Dirty ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].PreviousWarnings.TestEmpty(),
                        e[0].NewWarnings.TestSequence( sut.Warnings ),
                        e[0].PreviousErrors.TestEmpty(),
                        e[0].NewErrors.TestSequence( sut.Errors ),
                        e[0]
                            .Elements.TestCount( count => count.TestEquals( 1 ) )
                            .Then( el =>
                                el[0].TestRefEquals( onChangeEvents.FirstOrDefault()?.AddedElements.FirstOrDefault() ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldDoNothing_WhenElementKeyAlreadyExists()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { element }, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Add( element );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_ShouldDoNothing_WhenStateContainsReadOnlyFlag()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Add( element );

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.State.TestEquals( VariableState.ReadOnly ),
                sut.Elements.Values.TestEmpty(),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_WithRange_ShouldFilterElementsToAddCorrectlyAndAddOnlyThoseThatDoNotExistAndDoNotRepeat()
    {
        var allElements = Fixture.CreateManyDistinct<TestElement>( count: 5 );
        var initialElements = new[] { allElements[0], allElements[1], allElements[2] };
        var elements = new[]
        {
            allElements[0],
            allElements[1],
            new TestElement( allElements[2].Key, Fixture.Create<string>() ),
            allElements[3],
            allElements[4],
            allElements[0],
            allElements[3],
            allElements[4]
        };

        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( initialElements, keySelector );
        sut.Remove( allElements[1].Key );
        sut.Remove( allElements[2].Key );
        sut.Add( allElements[3] );
        sut.Remove( allElements[3].Key );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Add( elements );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ allElements[0], allElements[1], elements[2], allElements[3], allElements[4] ] ),
                sut.Elements.ModifiedElementKeys.TestSetEqual( [ allElements[2].Key, allElements[3].Key, allElements[4].Key ] ),
                sut.Elements.GetState( allElements[0].Key ).TestEquals( CollectionVariableElementState.Default ),
                sut.Elements.GetState( allElements[1].Key ).TestEquals( CollectionVariableElementState.Default ),
                sut.Elements.GetState( allElements[2].Key ).TestEquals( CollectionVariableElementState.Changed ),
                sut.Elements.GetState( allElements[3].Key ).TestEquals( CollectionVariableElementState.Added ),
                sut.Elements.GetState( allElements[4].Key ).TestEquals( CollectionVariableElementState.Added ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => e[0]
                        .AddedElements.Select( el => el.Element )
                        .TestSequence( [ elements[1], elements[2], elements[3], elements[4] ] ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "validateEvent",
                        e[0].AssociatedChange.TestRefEquals( onChangeEvents.FirstOrDefault() ),
                        e[0]
                            .Elements.Select( el => el.Element )
                            .TestSequence( [ elements[1], elements[2], elements[3], elements[4] ] ) ) ) )
            .Go();
    }

    [Fact]
    public void Add_WithRange_ShouldAddSingleElementCorrectly_WhenItDoesNotExist()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Add( new[] { element } );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => e[0].AddedElements.Select( el => el.Element ).TestSequence( [ element ] ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => e[0].Elements.Select( el => el.Element ).TestSequence( [ element ] ) ) )
            .Go();
    }

    [Fact]
    public void Add_WithRange_ShouldDoNothing_WhenAllElementsAlreadyExist()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { element }, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Add( new[] { element, element } );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_WithRange_ShouldDoNothing_WhenElementsToAddAreEmpty()
    {
        var elements = new[] { Fixture.Create<TestElement>() };
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( elements, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Add( Array.Empty<TestElement>() );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.Values.TestSetEqual( elements.AsEnumerable() ),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_WithRange_ShouldDoNothing_WhenStateContainsReadOnlyFlag()
    {
        var elements = new[] { Fixture.Create<TestElement>() };
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.Add( elements );

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.State.TestEquals( VariableState.ReadOnly ),
                sut.Elements.Values.TestEmpty(),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }
}
