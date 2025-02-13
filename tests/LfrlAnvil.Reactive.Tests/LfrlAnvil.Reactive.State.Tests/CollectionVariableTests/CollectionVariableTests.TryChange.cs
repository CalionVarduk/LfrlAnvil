using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableTests;

public partial class CollectionVariableTests
{
    [Fact]
    public void TryChange_ShouldUpdateElementsWithNewCollection()
    {
        var allElements = Fixture.CreateManyDistinct<TestElement>( count: 7 );
        var initialElements = new[] { allElements[0], allElements[1], allElements[2], allElements[3], allElements[4] };
        var elements = new[]
        {
            allElements[0],
            new TestElement( allElements[2].Key, allElements[2].Value ),
            new TestElement( allElements[3].Key, Fixture.Create<string>() ),
            allElements[4],
            allElements[0],
            allElements[4],
            allElements[4],
            allElements[6]
        };

        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( initialElements, keySelector );
        sut.Remove( allElements[4].Key );
        sut.Add( allElements[5] );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.TryChange( elements );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ elements[0], elements[1], elements[2], elements[3], elements[7] ] ),
                sut.Elements.ModifiedElementKeys.TestSetEqual( [ allElements[1].Key, allElements[3].Key, allElements[6].Key ] ),
                sut.Elements.GetState( allElements[0].Key ).TestEquals( CollectionVariableElementState.Default ),
                sut.Elements.GetState( allElements[1].Key ).TestEquals( CollectionVariableElementState.Removed ),
                sut.Elements.GetState( allElements[2].Key ).TestEquals( CollectionVariableElementState.Default ),
                sut.Elements.GetState( allElements[3].Key ).TestEquals( CollectionVariableElementState.Changed ),
                sut.Elements.GetState( allElements[4].Key ).TestEquals( CollectionVariableElementState.Default ),
                sut.Elements.GetState( allElements[5].Key ).TestEquals( CollectionVariableElementState.NotFound ),
                sut.Elements.GetState( allElements[6].Key ).TestEquals( CollectionVariableElementState.Added ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Source.TestEquals( VariableChangeSource.TryChange ),
                            e[0].RefreshedElements.Select( el => el.Element ).TestSequence( [ allElements[0], allElements[2] ] ),
                            e[0].AddedElements.Select( el => el.Element ).TestSequence( [ allElements[4], allElements[6] ] ),
                            e[0].RemovedElements.Select( el => el.Element ).TestSequence( [ allElements[1], allElements[5] ] ),
                            e[0]
                                .ReplacedElements.Select( el => (el.Element, el.PreviousElement) )
                                .TestSequence( [ (elements[2], allElements[3]) ] ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "validateEvent",
                            e[0]
                                .Elements.Select( el => el.Element )
                                .TestSequence(
                                    [ elements[0], elements[1], elements[2], elements[3], elements[7], allElements[1], allElements[5] ] ),
                            e[0].AssociatedChange.TestRefEquals( onChangeEvents.FirstOrDefault() ) ) ) )
            .Go();
    }

    [Fact]
    public void TryChange_ShouldRemoveAllElements_WhenCurrentElementsAreNotEmptyAndNewElementsAreEmpty()
    {
        var elements = Fixture.CreateManyDistinct<TestElement>( count: 2 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( elements, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.TryChange( Array.Empty<TestElement>() );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestEmpty(),
                onChangeEvents.Count.TestEquals( 1 ),
                onChangeEvents[0].RemovedElements.Select( e => e.Element ).TestSequence( elements ),
                onValidateEvents.Count.TestEquals( 1 ),
                onValidateEvents[0].AssociatedChange.TestRefEquals( onChangeEvents[0] ) )
            .Go();
    }

    [Fact]
    public void TryChange_ShouldDoNothing_WhenCurrentAndNewElementsAreEmpty()
    {
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.TryChange( Array.Empty<TestElement>() );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.Values.TestEmpty(),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }

    [Fact]
    public void TryChange_ShouldDoNothing_WhenStateContainsReadOnlyFlag()
    {
        var elements = new[] { Fixture.Create<TestElement>() };
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.TryChange( elements );

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.State.TestEquals( VariableState.ReadOnly ),
                sut.Elements.Values.TestEmpty(),
                onChangeEvents.TestEmpty(),
                onValidateEvents.TestEmpty() )
            .Go();
    }
}
