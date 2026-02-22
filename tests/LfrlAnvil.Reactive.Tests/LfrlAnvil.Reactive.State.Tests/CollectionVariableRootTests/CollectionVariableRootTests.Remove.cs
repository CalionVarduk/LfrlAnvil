using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableRootTests;

public partial class CollectionVariableRootTests
{
    [Fact]
    public void Remove_ShouldDoNothing_WhenRootIsReadOnly()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.SetReadOnly( true );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Remove( element.Key );

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenElementKeyDoesNotExist()
    {
        var key = Fixture.Create<int>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Remove( key );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveElementAndPublishEvents_WhenElementKeyIsInitialAndCanBeRemoved()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            new[] { element },
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Remove( element.Key );

        Assertion.All(
                element.State.TestEquals( VariableState.Default ),
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.GetChildren().TestSetEqual( [ element ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].Source.TestEquals( VariableChangeSource.Change ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestSequence( [ element ] ),
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
    public void Remove_ShouldRemoveElementAndDisposeItAndPublishEvents_WhenElementKeyIsNotInitialAndCanBeRemoved()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            Array.Empty<VariableMock>(),
            new CollectionVariableRootChanges<int, VariableMock>( new[] { element }, Array.Empty<int>() ),
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Remove( element.Key );

        Assertion.All(
                element.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Dirty ),
                sut.Elements.Values.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.GetChildren().TestEmpty(),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].Source.TestEquals( VariableChangeSource.Change ),
                            e[0].PreviousState.TestEquals( VariableState.Changed ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestSequence( [ element ] ),
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
    public void Remove_WithRange_ShouldDoNothing_WhenRootIsReadOnly()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.SetReadOnly( true );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Remove( new[] { element.Key } );

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.AddedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Remove_WithRange_ShouldDoNothing_WhenRangeIsEmpty()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Remove( Array.Empty<int>() );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Remove_WithRange_ShouldDoNothing_WhenNoneOfTheElementsCanBeRemoved()
    {
        var keys = Fixture.CreateMany<int>().ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Remove( keys );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Remove_WithRange_ShouldRemoveElementAndPublishEvents_WhenRangeContainsSingleElementKeyThatCanBeRemoved()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            new[] { element },
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Remove( new[] { element.Key } );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.GetChildren().TestSetEqual( [ element ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].Source.TestEquals( VariableChangeSource.Change ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestSequence( [ element ] ),
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
    public void Remove_WithRange_ShouldRemoveElementsAndPublishEvents_WhenRangeContainsMultipleElementsKeysThatCanBeRemoved()
    {
        var allElements = Fixture.CreateMany<VariableMock>( count: 5 ).ToList();

        var initialElements = new[] { allElements[0], allElements[1], allElements[2] };
        var keysToRemove = new[] { allElements[2].Key, allElements[0].Key, allElements[3].Key, allElements[3].Key, allElements[0].Key };

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            initialElements,
            new CollectionVariableRootChanges<int, VariableMock>(
                new[] { allElements[3], allElements[4] },
                new[] { allElements[0].Key, allElements[1].Key } ),
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Remove( keysToRemove );

        Assertion.All(
                allElements[0].State.TestEquals( VariableState.Default ),
                allElements[2].State.TestEquals( VariableState.Default ),
                allElements[3].State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                result.TestEquals( VariableChangeResult.Changed ),
                sut.Elements.Values.TestSetEqual( [ allElements[1], allElements[4] ] ),
                sut.Elements.AddedElementKeys.TestSetEqual( [ allElements[4].Key ] ),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ allElements[0].Key, allElements[2].Key ] ),
                sut.GetChildren().TestSetEqual( [ allElements[0], allElements[1], allElements[2], allElements[4] ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].Source.TestEquals( VariableChangeSource.Change ),
                            e[0].PreviousState.TestEquals( VariableState.Changed ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestSequence( [ allElements[0], allElements[3] ] ),
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
}
