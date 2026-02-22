using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableRootTests;

public partial class CollectionVariableRootTests
{
    [Fact]
    public void Clear_ShouldDoNothing_WhenRootIsReadOnly()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.SetReadOnly( true );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Clear();

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
    public void Clear_ShouldDoNothing_WhenOldElementsAreEmpty()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Clear();

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
    public void Clear_ShouldRemoveAllElementsAndPublishEvents_WhenOldElementsAreNotEmpty()
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

        var result = sut.Clear();

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
}
