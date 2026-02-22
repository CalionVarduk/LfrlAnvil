using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableTests;

public partial class CollectionVariableTests
{
    [Fact]
    public void Reset_WithInitialElementsOnly_ShouldUpdateCollectionCorrectlyAndResetValidationAndDirtyState()
    {
        // 0: initial => initial
        // 1: initial => not found
        // 2: initial changed => initial
        // 3: initial changed => not found
        // 4: initial removed => initial
        // 5: added => initial
        // 6: added => not found
        // 7: not found => initial

        var allElements = Fixture.CreateManyDistinct<TestElement>( count: 8 );
        var initialElements = new[] { allElements[0], allElements[1], allElements[2], allElements[3], allElements[4] };
        var elements = new[]
        {
            allElements[0],
            allElements[1],
            new TestElement( allElements[2].Key, Fixture.Create<string>() ),
            new TestElement( allElements[3].Key, Fixture.Create<string>() ),
            allElements[5],
            allElements[6]
        };

        var newElements = new[] { allElements[0], allElements[2], allElements[4], allElements[5], allElements[7] };

        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( initialElements, elements, keySelector );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Reset( newElements );

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly ),
                sut.InitialElements.Values.TestSetEqual( newElements.AsEnumerable() ),
                sut.Elements.Values.TestSetEqual( newElements.AsEnumerable() ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Source.TestEquals( VariableChangeSource.Reset ),
                        e[0].RefreshedElements.TestEmpty(),
                        e[0].RemovedElements.Select( el => el.Element ).TestSequence( [ elements[1], elements[3], elements[5] ] ),
                        e[0].AddedElements.Select( el => el.Element ).TestSequence( [ allElements[4], allElements[7] ] ),
                        e[0]
                            .ReplacedElements.Select( el => (el.Element, el.PreviousElement) )
                            .TestSequence(
                                [ (newElements[0], elements[0]), (newElements[1], elements[2]), (newElements[3], elements[4]) ] ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "validateEvent",
                        e[0]
                            .Elements.Select( el => el.Element )
                            .TestSequence(
                            [
                                newElements[0], newElements[1], newElements[2], newElements[3], newElements[4], elements[1],
                                elements[3],
                                elements[5]
                            ] ),
                        e[0].AssociatedChange.TestRefEquals( onChangeEvents.FirstOrDefault() ) ) ) )
            .Go();
    }

    [Fact]
    public void Reset_WithInitialElementsOnly_ShouldDoNothing_WhenVariableIsDisposed()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        sut.Dispose();

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Reset( new[] { element } );

        Assertion.All(
                sut.InitialElements.TestEmpty(),
                sut.Elements.TestEmpty(),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Reset_ShouldUpdateCollectionCorrectlyAndResetValidationAndDirtyState()
    {
        var allElements = Fixture.CreateManyDistinct<TestElement>( count: 10 );
        var initialElements = new[] { allElements[0], allElements[1], allElements[2], allElements[3], allElements[4], allElements[5] };
        var elements = new[]
        {
            allElements[0],
            allElements[1],
            new TestElement( allElements[2].Key, Fixture.Create<string>() ),
            new TestElement( allElements[3].Key, Fixture.Create<string>() ),
            allElements[6],
            allElements[7]
        };

        var newInitialElements = new[] { allElements[0], allElements[1], allElements[2], allElements[6], allElements[7], allElements[8] };
        var newElements = new[]
        {
            allElements[0],
            new TestElement( allElements[1].Key, Fixture.Create<string>() ),
            allElements[4],
            new TestElement( allElements[6].Key, Fixture.Create<string>() ),
            allElements[8],
            allElements[9]
        };

        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( initialElements, elements, keySelector );
        sut.Refresh();

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Reset( newInitialElements, newElements );

        Assertion.All(
                sut.State.TestEquals( VariableState.Changed ),
                sut.InitialElements.Values.TestSetEqual( newInitialElements.AsEnumerable() ),
                sut.Elements.Values.TestSetEqual( newElements.AsEnumerable() ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Source.TestEquals( VariableChangeSource.Reset ),
                        e[0].RefreshedElements.TestEmpty(),
                        e[0].RemovedElements.Select( el => el.Element ).TestSequence( [ elements[2], elements[3], elements[5] ] ),
                        e[0]
                            .AddedElements.Select( el => el.Element )
                            .TestSequence( [ allElements[4], allElements[8], allElements[9] ] ),
                        e[0]
                            .ReplacedElements.Select( el => (el.Element, el.PreviousElement) )
                            .TestSequence(
                                [ (newElements[0], elements[0]), (newElements[1], elements[1]), (newElements[3], elements[4]) ] ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "validateEvent",
                        e[0]
                            .Elements.Select( el => el.Element )
                            .TestSequence(
                            [
                                newElements[0], newElements[1], newElements[2], newElements[3], newElements[4], newElements[5],
                                elements[2],
                                elements[3], elements[5]
                            ] ),
                        e[0].AssociatedChange.TestRefEquals( onChangeEvents.FirstOrDefault() ) ) ) )
            .Go();
    }

    [Fact]
    public void Reset_ShouldDoNothing_WhenVariableIsDisposed()
    {
        var (element, addedElement) = Fixture.CreateManyDistinct<TestElement>( count: 2 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        sut.Dispose();

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Reset( new[] { element }, new[] { element, addedElement } );

        Assertion.All(
                sut.InitialElements.TestEmpty(),
                sut.Elements.TestEmpty(),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }
}
