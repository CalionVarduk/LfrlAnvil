using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Reactive.State.Internal;
using LfrlAnvil.Reactive.State.Tests.VariableRootTests;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableRootTests;

public partial class CollectionVariableRootTests
{
    [Fact]
    public void Add_ShouldDoNothing_WhenRootIsReadOnly()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );
        sut.SetReadOnly( true );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Add( element );

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.Elements.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_ShouldDoNothing_WhenElementIsDisposed()
    {
        var element = Fixture.Create<VariableMock>();
        element.Dispose();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Add( element );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_ShouldDoNothing_WhenElementAlreadyHasParent()
    {
        var parent = new VariableRootMock();
        var element = Fixture.Create<VariableMock>();
        parent.ExposedRegisterNode( Fixture.Create<string>(), element );

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Add( element );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_ShouldDoNothing_WhenElementIsRootItself()
    {
        var keySelector = Lambda.Of( (VariableNode e) => e );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableNode>(), keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<VariableNode, VariableNode, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<VariableNode, VariableNode, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<VariableNode, VariableNode, string>>( onChange.Add ) );
        sut.OnValidate.Listen(
            EventListener.Create<CollectionVariableRootValidationEvent<VariableNode, VariableNode, string>>( onValidate.Add ) );

        var result = sut.Add( sut );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_ShouldDoNothing_WhenKeyExistsInInitialElements()
    {
        var initialElement = Fixture.Create<VariableMock>();
        var element = new VariableMock( initialElement.Key, Fixture.Create<string>() );

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { initialElement }, keySelector );
        sut.Remove( initialElement.Key );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Add( element );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewElementAndPublishEvents_WhenElementCanBeAdded()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            Array.Empty<VariableMock>(),
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Add( element );

        Assertion.All(
                element.Parent.TestRefEquals( sut ),
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.AddedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.GetChildren().TestSetEqual( [ element ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e =>
                            Assertion.All(
                                "changeEvent",
                                e[0].Variable.TestRefEquals( sut ),
                                e[0].Source.TestEquals( VariableChangeSource.Change ),
                                e[0].PreviousState.TestEquals( VariableState.Default ),
                                e[0].NewState.TestEquals( sut.State ),
                                e[0].AddedElements.TestSequence( [ element ] ),
                                e[0].RemovedElements.TestEmpty(),
                                e[0].RestoredElements.TestEmpty(),
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
    public void Add_WithRange_ShouldDoNothing_WhenRootIsReadOnly()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );
        sut.SetReadOnly( true );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Add( new[] { element } );

        Assertion.All(
                result.TestEquals( VariableChangeResult.ReadOnly ),
                sut.Elements.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_WithRange_ShouldDoNothing_WhenRangeIsEmpty()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Add( Array.Empty<VariableMock>() );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_WithRange_ShouldDoNothing_WhenNoneOfTheElementsCanBeAdded()
    {
        var elements = Fixture.CreateMany<VariableMock>().ToList();
        foreach ( var e in elements )
            e.Dispose();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Add( elements );

        Assertion.All(
                result.TestEquals( VariableChangeResult.NotChanged ),
                sut.Elements.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_WithRange_ShouldAddNewElementAndPublishEvents_WhenRangeContainsSingleElementThatCanBeAdded()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            Array.Empty<VariableMock>(),
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Add( new[] { element } );

        Assertion.All(
                element.Parent.TestRefEquals( sut ),
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.AddedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.GetChildren().TestSetEqual( [ element ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e =>
                            Assertion.All(
                                "changeEvent",
                                e[0].Variable.TestRefEquals( sut ),
                                e[0].Source.TestEquals( VariableChangeSource.Change ),
                                e[0].PreviousState.TestEquals( VariableState.Default ),
                                e[0].NewState.TestEquals( sut.State ),
                                e[0].AddedElements.TestSequence( [ element ] ),
                                e[0].RemovedElements.TestEmpty(),
                                e[0].RestoredElements.TestEmpty(),
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
    public void Add_WithRange_ShouldAddNewElementsAndPublishEvents_WhenRangeContainsMultipleElementsThatCanBeAdded()
    {
        var allElements = Fixture.CreateMany<VariableMock>( count: 6 ).ToList();
        allElements[3].Dispose();

        var initialElements = new[] { allElements[0], allElements[1] };
        var elementsToAdd = new[] { allElements[3], allElements[4], allElements[3], allElements[4], allElements[5], allElements[4] };

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            initialElements,
            new CollectionVariableRootChanges<int, VariableMock>( new[] { allElements[2] }, new[] { allElements[0].Key } ),
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Add( elementsToAdd );

        Assertion.All(
                allElements[4].Parent.TestRefEquals( sut ),
                allElements[5].Parent.TestRefEquals( sut ),
                result.TestEquals( VariableChangeResult.Changed ),
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.Values.TestSetEqual( [ allElements[0], allElements[2], allElements[4], allElements[5] ] ),
                sut.Elements.AddedElementKeys.TestSetEqual( [ allElements[2].Key, allElements[4].Key, allElements[5].Key ] ),
                sut.GetChildren().TestSetEqual( [ allElements[0], allElements[1], allElements[2], allElements[4], allElements[5] ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e =>
                            Assertion.All(
                                "changeEvent",
                                e[0].Variable.TestRefEquals( sut ),
                                e[0].Source.TestEquals( VariableChangeSource.Change ),
                                e[0].PreviousState.TestEquals( VariableState.Changed ),
                                e[0].NewState.TestEquals( sut.State ),
                                e[0].AddedElements.TestSequence( [ allElements[4], allElements[5] ] ),
                                e[0].RemovedElements.TestEmpty(),
                                e[0].RestoredElements.TestEmpty(),
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
