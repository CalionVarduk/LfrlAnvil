using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableRootTests;

public partial class CollectionVariableRootTests
{
    [Fact]
    public void Restore_ShouldDoNothing_WhenRootIsReadOnly()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.Remove( element.Key );
        sut.SetReadOnly( true );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Restore( element.Key );

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.Elements.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ element.Key ] ),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Restore_ShouldDoNothing_WhenElementIsDisposed()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        element.Dispose();

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Restore( element.Key );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ element.Key ] ),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Restore_ShouldDoNothing_WhenInitialElementIsNotRemoved()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Restore( element.Key );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Restore_ShouldDoNothing_WhenElementExistsAndIsNotInitial()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );
        sut.Add( element );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Restore( element.Key );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestSetEqual( [ element.Key ] ),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Restore_ShouldRestoreInitialElementAndPublishEvents_WhenElementCanBeRestored()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            new[] { element },
            CollectionVariableRootChanges<int, VariableMock>.Empty,
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Restore( element.Key );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.GetChildren().TestSetEqual( [ element ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e =>
                            Assertion.All(
                                "changeEvent",
                                e[0].Variable.TestRefEquals( sut ),
                                e[0].Source.TestEquals( VariableChangeSource.Change ),
                                e[0].PreviousState.TestEquals( VariableState.Changed ),
                                e[0].NewState.TestEquals( sut.State ),
                                e[0].AddedElements.TestEmpty(),
                                e[0].RemovedElements.TestEmpty(),
                                e[0].RestoredElements.TestSequence( [ element ] ),
                                e[0].SourceEvent.TestNull() ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e =>
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
    public void Restore_WithRange_ShouldDoNothing_WhenRootIsReadOnly()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.Remove( element.Key );
        sut.SetReadOnly( true );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Restore( new[] { element.Key } );

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.Elements.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ element.Key ] ),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Restore_WithRange_ShouldDoNothing_WhenRangeIsEmpty()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Restore( Array.Empty<int>() );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Restore_WithRange_ShouldDoNothing_WhenNoneOfTheElementsCanBeRestored()
    {
        var elements = Fixture.CreateMany<VariableMock>().ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( elements, keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Restore( elements.Select( e => e.Key ) );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.Values.TestSetEqual( elements ),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Restore_WithRange_ShouldRestoreInitialElementAndPublishEvents_WhenRangeContainsSingleElementThatCanBeRestored()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            new[] { element },
            CollectionVariableRootChanges<int, VariableMock>.Empty,
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Restore( new[] { element.Key } );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                sut.GetChildren().TestSetEqual( [ element ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e =>
                            Assertion.All(
                                "changeEvent",
                                e[0].Variable.TestRefEquals( sut ),
                                e[0].Source.TestEquals( VariableChangeSource.Change ),
                                e[0].PreviousState.TestEquals( VariableState.Changed ),
                                e[0].NewState.TestEquals( sut.State ),
                                e[0].AddedElements.TestEmpty(),
                                e[0].RemovedElements.TestEmpty(),
                                e[0].RestoredElements.TestSequence( [ element ] ),
                                e[0].SourceEvent.TestNull() ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e =>
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
    public void Restore_WithRange_ShouldRestoreInitialElementsAndPublishEvents_WhenRangeContainsMultipleElementsThatCanBeRestored()
    {
        var allElements = Fixture.CreateMany<VariableMock>( count: 5 ).ToList();

        var initialElements = new[] { allElements[0], allElements[1], allElements[2], allElements[3] };
        var keysToRestore = new[] { allElements[0].Key, allElements[1].Key, allElements[2].Key, allElements[4].Key, allElements[1].Key };

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            initialElements,
            new CollectionVariableRootChanges<int, VariableMock>( new[] { allElements[4] }, new[] { allElements[0].Key } ),
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Restore( keysToRestore );

        Assertion.All(
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ allElements[0], allElements[1], allElements[2], allElements[4] ] ),
                sut.Elements.AddedElementKeys.TestSetEqual( [ allElements[4].Key ] ),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ allElements[3].Key ] ),
                sut.GetChildren().TestSetEqual( [ allElements[0], allElements[1], allElements[2], allElements[3], allElements[4] ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e =>
                            Assertion.All(
                                "changeEvent",
                                e[0].Variable.TestRefEquals( sut ),
                                e[0].Source.TestEquals( VariableChangeSource.Change ),
                                e[0].PreviousState.TestEquals( VariableState.Changed ),
                                e[0].NewState.TestEquals( sut.State ),
                                e[0].AddedElements.TestEmpty(),
                                e[0].RemovedElements.TestEmpty(),
                                e[0].RestoredElements.TestSequence( [ allElements[1], allElements[2] ] ),
                                e[0].SourceEvent.TestNull() ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e =>
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
