using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableRootTests;

public partial class CollectionVariableRootTests
{
    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementBecomesChanged()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        VariableValueChangeEvent<string, string>? onVariableChange = null;
        VariableValidationEvent<string, string>? onVariableValidate = null;
        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        element.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<string, string>>( e => onVariableChange = e ) );
        element.OnValidate.Listen( EventListener.Create<VariableValidationEvent<string, string>>( e => onVariableValidate = e ) );
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        element.Change( Fixture.Create<string>() );

        Assertion.All(
                sut.State.TestEquals( VariableState.Changed ),
                sut.Elements.ChangedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].SourceEvent.TestRefEquals( onVariableChange ),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestEmpty(),
                            e[0].RestoredElements.TestEmpty(),
                            e[0].Source.TestEquals( VariableChangeSource.ChildNode ) ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( sut.State ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].SourceEvent.TestRefEquals( onVariableValidate ),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestEmpty(),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestEmpty(),
                            e[0].AssociatedChange.TestNull() ) ) )
            .Go();
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementBecomesInvalid()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), validator, Validators<string>.Pass<string>() );
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        VariableValueChangeEvent<string, string>? onVariableChange = null;
        VariableValidationEvent<string, string>? onVariableValidate = null;
        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        element.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<string, string>>( e => onVariableChange = e ) );
        element.OnValidate.Listen( EventListener.Create<VariableValidationEvent<string, string>>( e => onVariableValidate = e ) );
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        element.Change( element.Value );

        Assertion.All(
                sut.State.TestEquals( VariableState.Invalid ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( VariableState.Default ),
                            e[0].SourceEvent.TestRefEquals( onVariableChange ),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestEmpty(),
                            e[0].RestoredElements.TestEmpty(),
                            e[0].Source.TestEquals( VariableChangeSource.ChildNode ) ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].SourceEvent.TestRefEquals( onVariableValidate ),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestEmpty(),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestEmpty(),
                            e[0].AssociatedChange.TestNull() ) ) )
            .Go();
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementBecomesWarning()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), Validators<string>.Pass<string>(), validator );
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        VariableValueChangeEvent<string, string>? onVariableChange = null;
        VariableValidationEvent<string, string>? onVariableValidate = null;
        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        element.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<string, string>>( e => onVariableChange = e ) );
        element.OnValidate.Listen( EventListener.Create<VariableValidationEvent<string, string>>( e => onVariableValidate = e ) );
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        element.Change( element.Value );

        Assertion.All(
                sut.State.TestEquals( VariableState.Warning ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( VariableState.Default ),
                            e[0].SourceEvent.TestRefEquals( onVariableChange ),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestEmpty(),
                            e[0].RestoredElements.TestEmpty(),
                            e[0].Source.TestEquals( VariableChangeSource.ChildNode ) ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].SourceEvent.TestRefEquals( onVariableValidate ),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestEmpty(),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestEmpty(),
                            e[0].AssociatedChange.TestNull() ) ) )
            .Go();
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementStopsBeingChanged()
    {
        var element = Fixture.Create<VariableMock>();
        var oldValue = element.Value;
        element.Change( Fixture.Create<string>() );

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        element.Change( oldValue );

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty() )
            .Go();
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementStopsBeingInvalid()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), validator, Validators<string>.Pass<string>() );
        element.RefreshValidation();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        element.ClearValidation();

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty() )
            .Go();
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementStopsBeingWarning()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), Validators<string>.Pass<string>(), validator );
        element.RefreshValidation();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        element.ClearValidation();

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty() )
            .Go();
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenOneOfElementsStopsBeingChanged()
    {
        var (element1, element2) = Fixture.CreateMany<VariableMock>( count: 2 ).ToList();
        var oldValue = element1.Value;
        element1.Change( Fixture.Create<string>() );
        element2.Change( Fixture.Create<string>() );

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element1, element2 }, keySelector );

        element1.Change( oldValue );

        Assertion.All(
                sut.State.TestEquals( VariableState.Changed ),
                sut.Elements.ChangedElementKeys.TestSetEqual( [ element2.Key ] ),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty() )
            .Go();
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenOneOfElementsStopsBeingInvalid()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element1 = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), validator, Validators<string>.Pass<string>() );
        var element2 = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), validator, Validators<string>.Pass<string>() );
        element1.RefreshValidation();
        element2.RefreshValidation();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element1, element2 }, keySelector );

        element1.ClearValidation();

        Assertion.All(
                sut.State.TestEquals( VariableState.Invalid ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestSetEqual( [ element2.Key ] ),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty() )
            .Go();
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenOneOfElementsStopsBeingWarning()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element1 = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), Validators<string>.Pass<string>(), validator );
        var element2 = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), Validators<string>.Pass<string>(), validator );
        element1.RefreshValidation();
        element2.RefreshValidation();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element1, element2 }, keySelector );

        element1.ClearValidation();

        Assertion.All(
                sut.State.TestEquals( VariableState.Warning ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestSetEqual( [ element2.Key ] ),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty() )
            .Go();
    }

    [Fact]
    public void ElementChange_ShouldNotBeListenedToByRoot_WhenRemovedInitialElementChanges()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), validator, validator );
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create(
            new[] { element },
            CollectionVariableRootChanges<int, VariableMock>.Empty,
            keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        element.Change( Fixture.Create<string>() );

        Assertion.All(
                sut.State.TestEquals( VariableState.Changed ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ element.Key ] ),
                onChange.TestEmpty(),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementGetsDisposed()
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

        element.Dispose();

        Assertion.All(
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.GetChildren().TestSetEqual( [ element ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].SourceEvent.TestNull(),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestSequence( [ element ] ),
                            e[0].RestoredElements.TestEmpty(),
                            e[0].Source.TestEquals( VariableChangeSource.Change ) ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].SourceEvent.TestNull(),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestEmpty(),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestEmpty(),
                            e[0].AssociatedChange.TestRefEquals( onChange.FirstOrDefault() ) ) ),
                errorsValidator.TestReceivedCalls( v => { _ = v.Validate( sut.Elements ); }, count: 1 ),
                warningsValidator.TestReceivedCalls( v => { _ = v.Validate( sut.Elements ); }, count: 1 ) )
            .Go();
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenNonInitialElementGetsDisposed()
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

        element.Dispose();

        Assertion.All(
                sut.State.TestEquals( VariableState.Dirty ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                sut.GetChildren().TestEmpty(),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Changed ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].SourceEvent.TestNull(),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestSequence( [ element ] ),
                            e[0].RestoredElements.TestEmpty(),
                            e[0].Source.TestEquals( VariableChangeSource.Change ) ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "validateEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Changed ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].SourceEvent.TestNull(),
                            e[0].PreviousErrors.TestEmpty(),
                            e[0].NewErrors.TestEmpty(),
                            e[0].PreviousWarnings.TestEmpty(),
                            e[0].NewWarnings.TestEmpty(),
                            e[0].AssociatedChange.TestRefEquals( onChange.FirstOrDefault() ) ) ),
                errorsValidator.TestReceivedCalls( v => { _ = v.Validate( sut.Elements ); }, count: 1 ),
                warningsValidator.TestReceivedCalls( v => { _ = v.Validate( sut.Elements ); }, count: 1 ) )
            .Go();
    }
}
