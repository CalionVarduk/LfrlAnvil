using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableRootTests;

public partial class CollectionVariableRootTests
{
    [Fact]
    public void Change_ShouldDoNothing_WhenRootIsReadOnly()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.SetReadOnly( true );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Change( CollectionVariableRootChanges<int, VariableMock>.Empty );

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Change_ShouldDoNothing_WhenOldAndNewElementsAreEmpty()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Change( CollectionVariableRootChanges<int, VariableMock>.Empty );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.Values.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Change_ShouldDoNothing_WhenOldAndNewElementsAreNotEmptyAndEqual()
    {
        var allElements = Fixture.CreateMany<VariableMock>( count: 3 ).ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create(
            new[] { allElements[0], allElements[1] },
            new CollectionVariableRootChanges<int, VariableMock>( new[] { allElements[2] }, new[] { allElements[0].Key } ),
            keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Change(
            new CollectionVariableRootChanges<int, VariableMock>(
                Array.Empty<VariableMock>(),
                new[] { allElements[0].Key, allElements[2].Key } ) );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.Values.TestSetEqual( [ allElements[0], allElements[2] ] ),
                sut.Elements.AddedElementKeys.TestSetEqual( [ allElements[2].Key ] ),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ allElements[1].Key ] ),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Change_ShouldRemoveAllElementsAndPublishEvents_WhenOldElementsAreNotEmptyAndNewElementsAreEmpty()
    {
        var elements = Fixture.CreateMany<VariableMock>( count: 3 ).ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            new[] { elements[0], elements[1] },
            new CollectionVariableRootChanges<int, VariableMock>( new[] { elements[2] }, new[] { elements[0].Key, elements[1].Key } ),
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Change( CollectionVariableRootChanges<int, VariableMock>.Empty );

        Assertion.All(
                elements[0].State.TestEquals( VariableState.Default ),
                elements[1].State.TestEquals( VariableState.Default ),
                elements[2].State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ elements[0].Key, elements[1].Key ] ),
                sut.GetChildren().TestSetEqual( [ elements[0], elements[1] ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].Source.TestEquals( VariableChangeSource.Change ),
                            e[0].PreviousState.TestEquals( VariableState.Changed ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestSequence( [ elements[0], elements[1], elements[2] ] ),
                            e[0].RestoredElements.TestEmpty(),
                            e[0].SourceEvent.TestNull() ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].AssociatedChange.TestRefEquals( onChange.FirstOrDefault() ),
                            e[0].SourceEvent.TestNull() ) ),
                errorsValidator.TestReceivedCalls( v => { _ = v.Validate( sut.Elements ); }, count: 1 ),
                warningsValidator.TestReceivedCalls( v => { _ = v.Validate( sut.Elements ); }, count: 1 ) )
            .Go();
    }

    [Fact]
    public void Change_ShouldChangeElementsAndPublishEvents_WhenParameterContainsCollectionChanges()
    {
        var allElements = Fixture.CreateMany<VariableMock>( count: 13 ).ToList();
        allElements.Add( new VariableMock( allElements[5].Key, Fixture.Create<string>() ) );
        allElements.Add( new VariableMock( allElements[8].Key, Fixture.Create<string>() ) );
        allElements.Add( new VariableMock( allElements[9].Key, Fixture.Create<string>() ) );
        allElements[12].Dispose();
        allElements[15].Dispose();

        var initialElements = new[] { allElements[0], allElements[1], allElements[2] };
        var addedElements = new[]
        {
            allElements[3], allElements[4], allElements[5], allElements[6], allElements[7], allElements[8], allElements[9]
        };

        var elementsToAdd = new[]
        {
            allElements[10],
            allElements[10],
            allElements[11],
            allElements[1],
            allElements[11],
            allElements[12],
            allElements[6],
            allElements[7],
            allElements[7],
            allElements[13],
            allElements[14],
            allElements[15]
        };

        var keysToRestore = new[]
        {
            allElements[0].Key,
            allElements[1].Key,
            allElements[1].Key,
            allElements[3].Key,
            allElements[7].Key,
            allElements[8].Key,
            allElements[0].Key,
            allElements[9].Key,
            Fixture.Create<int>()
        };

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            initialElements,
            new CollectionVariableRootChanges<int, VariableMock>( addedElements, new[] { initialElements[0].Key, initialElements[2].Key } ),
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Change( new CollectionVariableRootChanges<int, VariableMock>( elementsToAdd, keysToRestore ) );

        Assertion.All(
                allElements[3].State.TestEquals( VariableState.Default ),
                allElements[4].State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                allElements[5].State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                allElements[6].State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                allElements[7].State.TestEquals( VariableState.Default ),
                allElements[8].State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                allElements[9].State.TestEquals( VariableState.Default ),
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual(
                [
                    allElements[0],
                    allElements[1],
                    allElements[3],
                    allElements[13],
                    allElements[7],
                    allElements[14],
                    allElements[9],
                    allElements[10],
                    allElements[11]
                ] ),
                sut.Elements.AddedElementKeys.TestSetEqual(
                [
                    allElements[3].Key,
                    allElements[13].Key,
                    allElements[7].Key,
                    allElements[14].Key,
                    allElements[9].Key,
                    allElements[10].Key,
                    allElements[11].Key
                ] ),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ allElements[2].Key ] ),
                sut.GetChildren()
                    .TestSetEqual(
                    [
                        allElements[0],
                        allElements[1],
                        allElements[2],
                        allElements[3],
                        allElements[13],
                        allElements[7],
                        allElements[14],
                        allElements[9],
                        allElements[10],
                        allElements[11]
                    ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].Source.TestEquals( VariableChangeSource.Change ),
                            e[0].PreviousState.TestEquals( VariableState.Changed ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].AddedElements.TestSequence( [ allElements[10], allElements[11], allElements[13], allElements[14] ] ),
                            e[0]
                                .RemovedElements.TestSequence(
                                    [ allElements[2], allElements[4], allElements[5], allElements[6], allElements[8] ] ),
                            e[0].RestoredElements.TestSequence( [ allElements[1] ] ),
                            e[0].SourceEvent.TestNull() ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].AssociatedChange.TestRefEquals( onChange.FirstOrDefault() ),
                            e[0].SourceEvent.TestNull() ) ),
                errorsValidator.TestReceivedCalls( v => { _ = v.Validate( sut.Elements ); }, count: 1 ),
                warningsValidator.TestReceivedCalls( v => { _ = v.Validate( sut.Elements ); }, count: 1 ) )
            .Go();
    }
}
