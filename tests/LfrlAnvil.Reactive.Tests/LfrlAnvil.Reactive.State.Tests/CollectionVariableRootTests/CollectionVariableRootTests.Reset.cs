using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableRootTests;

public partial class CollectionVariableRootTests
{
    [Fact]
    public void Reset_ShouldDoNothing_WhenRootIsDisposed()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.Dispose();

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        sut.Reset( Array.Empty<VariableMock>() );

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Reset_ShouldResetAllElementsAndPublishEvents_WhenRootIsNotDisposed()
    {
        var elements = Fixture.CreateMany<VariableMock>( count: 6 ).ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var validator = Validators<string>.Fail<ICollectionVariableRootElements<int, VariableMock>>( Fixture.Create<string>() );

        var sut = CollectionVariableRoot.Create(
            new[] { elements[0], elements[1] },
            new CollectionVariableRootChanges<int, VariableMock>( new[] { elements[2] }, new[] { elements[0].Key, elements[1].Key } ),
            keySelector,
            errorsValidator: validator,
            warningsValidator: validator );

        sut.Refresh();

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        sut.Reset( elements );

        Assertion.All(
                elements.Take( 3 )
                    .TestAll( (e, _) => e.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed | VariableState.Dirty ) ),
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.Values.TestSetEqual( elements.Skip( 3 ) ),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].Source.TestEquals( VariableChangeSource.Reset ),
                            e[0]
                                .PreviousState.TestEquals(
                                    VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].AddedElements.TestSequence( elements.Skip( 3 ) ),
                            e[0].RemovedElements.TestSetEqual( elements.Take( 3 ) ),
                            e[0].RestoredElements.TestEmpty(),
                            e[0].SourceEvent.TestNull() ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].AssociatedChange.TestRefEquals( onChange.FirstOrDefault() ),
                            e[0].SourceEvent.TestNull() ) ) )
            .Go();
    }

    [Fact]
    public void Reset_WithChanges_ShouldDoNothing_WhenRootIsDisposed()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.Dispose();

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        sut.Reset( Array.Empty<VariableMock>(), CollectionVariableRootChanges<int, VariableMock>.Empty );

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Reset_WithChanges_ShouldResetAllElementsAndPublishEvents_WhenRootIsNotDisposed()
    {
        var elements = Fixture.CreateMany<VariableMock>( count: 6 ).ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var validator = Validators<string>.Fail<ICollectionVariableRootElements<int, VariableMock>>( Fixture.Create<string>() );

        var sut = CollectionVariableRoot.Create(
            new[] { elements[0], elements[1] },
            new CollectionVariableRootChanges<int, VariableMock>( new[] { elements[2] }, new[] { elements[0].Key } ),
            keySelector,
            errorsValidator: validator,
            warningsValidator: validator );

        sut.SetReadOnly( true );
        sut.Refresh();

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        sut.Reset(
            new[] { elements[3], elements[4] },
            new CollectionVariableRootChanges<int, VariableMock>( new[] { elements[5] }, new[] { elements[3].Key } ) );

        Assertion.All(
                elements[0].State.TestEquals( VariableState.ReadOnly | VariableState.Disposed | VariableState.Dirty ),
                elements[1].State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                elements[2].State.TestEquals( VariableState.ReadOnly | VariableState.Disposed | VariableState.Dirty ),
                sut.State.TestEquals( VariableState.Changed | VariableState.ReadOnly ),
                sut.Elements.Values.TestSetEqual( [ elements[3], elements[5] ] ),
                sut.Elements.AddedElementKeys.TestSetEqual( [ elements[5].Key ] ),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ elements[4].Key ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].Source.TestEquals( VariableChangeSource.Reset ),
                            e[0]
                                .PreviousState.TestEquals(
                                    VariableState.Changed
                                    | VariableState.Invalid
                                    | VariableState.Warning
                                    | VariableState.ReadOnly
                                    | VariableState.Dirty ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].AddedElements.TestSequence( [ elements[3], elements[5] ] ),
                            e[0].RemovedElements.TestSetEqual( [ elements[0], elements[2] ] ),
                            e[0].RestoredElements.TestEmpty(),
                            e[0].SourceEvent.TestNull() ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].AssociatedChange.TestRefEquals( onChange.FirstOrDefault() ),
                            e[0].SourceEvent.TestNull() ) ) )
            .Go();
    }
}
