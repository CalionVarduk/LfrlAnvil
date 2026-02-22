using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableRootTests;

public partial class CollectionVariableRootTests : TestsBase
{
    public CollectionVariableRootTests()
    {
        Fixture.Customize<VariableMock>( (_, _) => f => new VariableMock( f.Create<int>(), f.Create<string>() ) );
    }

    [Fact]
    public void ToString_ShouldReturnInformationAboutElementCountAndState()
    {
        var element = Fixture.Create<VariableMock>();
        element.Change( Fixture.Create<string>() );
        var expected = "Elements: 1, State: Changed, ReadOnly";
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.SetReadOnly( true );

        var result = sut.ToString();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Dispose_ShouldDisposeUnderlyingOnChangeAndOnValidateEventPublishersAndOwnedElementsAndAddReadOnlyAndDisposedState()
    {
        var allElements = Fixture.CreateMany<VariableMock>( count: 3 ).ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create(
            new[] { allElements[0], allElements[1] },
            new CollectionVariableRootChanges<int, VariableMock>( new[] { allElements[2] }, new[] { allElements[0].Key } ),
            keySelector );

        var onChange = sut.OnChange.Listen(
            Substitute.For<IEventListener<CollectionVariableRootChangeEvent<int, VariableMock, string>>>() );

        var onValidate = sut.OnValidate.Listen(
            Substitute.For<IEventListener<CollectionVariableRootValidationEvent<int, VariableMock, string>>>() );

        sut.Dispose();

        Assertion.All(
                onChange.IsDisposed.TestTrue(),
                onValidate.IsDisposed.TestTrue(),
                sut.State.TestEquals( VariableState.Changed | VariableState.ReadOnly | VariableState.Disposed ),
                allElements.TestAll( (e, _) => e.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenRootIsAlreadyDisposed()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );
        sut.Dispose();

        sut.Dispose();

        sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ).Go();
    }

    [Fact]
    public void Refresh_ShouldRefreshForAllElementsAndCollectionItself()
    {
        var allElements = Fixture.CreateMany<VariableMock>( count: 3 ).ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create(
            new[] { allElements[0], allElements[1] },
            new CollectionVariableRootChanges<int, VariableMock>(
                new[] { allElements[2] },
                new[] { allElements[0].Key } ),
            keySelector );

        var onChangeEvents = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnValidate.Listen(
            EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidateEvents.Add ) );

        sut.Refresh();

        Assertion.All(
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.Elements.AddedElementKeys.TestSetEqual( [ allElements[2].Key ] ),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ allElements[1].Key ] ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                allElements[0].State.TestEquals( VariableState.Dirty ),
                allElements[1].State.TestEquals( VariableState.Default ),
                allElements[2].State.TestEquals( VariableState.Dirty ),
                onChangeEvents.TestCount( count => count.TestEquals( 3 ) )
                    .Then( e =>
                        Assertion.All(
                            e.TestAny( (el, _) => el.SourceEvent.TestNotNull( source => source.Variable.TestEquals( allElements[0] ) ) ),
                            e.TestAny( (el, _) => el.SourceEvent.TestNotNull( source => source.Variable.TestEquals( allElements[2] ) ) ),
                            e[2].SourceEvent.TestNull(),
                            e[2].Source.TestEquals( VariableChangeSource.Refresh ) ) ),
                onValidateEvents.TestCount( count => count.TestEquals( 3 ) )
                    .Then( e =>
                        Assertion.All(
                            e.TestAny( (el, _) => el.SourceEvent.TestNotNull( source => source.Variable.TestEquals( allElements[0] ) ) ),
                            e.TestAny( (el, _) => el.SourceEvent.TestNotNull( source => source.Variable.TestEquals( allElements[2] ) ) ),
                            e[2].SourceEvent.TestNull(),
                            e[2].AssociatedChange.TestRefEquals( onChangeEvents[2] ) ) ) )
            .Go();
    }

    [Fact]
    public void Refresh_ShouldDoNothing_WhenRootIsAlreadyDisposed()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.Dispose();

        sut.Refresh();

        sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ).Go();
    }

    [Fact]
    public void RefreshValidation_ShouldRefreshValidationForAllElementsAndCollectionItself()
    {
        var validator = Validators<string>.Fail<ICollectionVariableRootElements<int, VariableMock>>( Fixture.Create<string>() );
        var elementValidator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var allElements = Enumerable.Range( 0, 3 )
            .Select( i => new VariableMock( i, Fixture.Create<string>(), elementValidator, elementValidator ) )
            .ToList();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.Create(
            new[] { allElements[0], allElements[1] },
            new CollectionVariableRootChanges<int, VariableMock>(
                new[] { allElements[2] },
                new[] { allElements[0].Key } ),
            keySelector,
            errorsValidator: validator,
            warningsValidator: validator );

        var onValidateEvents = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnValidate.Listen(
            EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidateEvents.Add ) );

        sut.RefreshValidation();

        Assertion.All(
                sut.State.TestEquals( VariableState.Changed | VariableState.Invalid | VariableState.Warning ),
                sut.Errors.Count.TestEquals( 1 ),
                sut.Warnings.Count.TestEquals( 1 ),
                sut.Elements.AddedElementKeys.TestSetEqual( [ allElements[2].Key ] ),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ allElements[1].Key ] ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestSetEqual( [ allElements[0].Key, allElements[2].Key ] ),
                sut.Elements.WarningElementKeys.TestSetEqual( [ allElements[0].Key, allElements[2].Key ] ),
                allElements[0].State.TestEquals( VariableState.Invalid | VariableState.Warning ),
                allElements[1].State.TestEquals( VariableState.Default ),
                allElements[2].State.TestEquals( VariableState.Invalid | VariableState.Warning ),
                onValidateEvents.TestCount( count => count.TestEquals( 3 ) )
                    .Then( e =>
                        Assertion.All(
                            e.TestAny( (el, _) => el.SourceEvent.TestNotNull( source => source.Variable.TestEquals( allElements[0] ) ) ),
                            e.TestAny( (el, _) => el.SourceEvent.TestNotNull( source => source.Variable.TestEquals( allElements[2] ) ) ),
                            e[2].SourceEvent.TestNull(),
                            e[2].AssociatedChange.TestNull() ) ) )
            .Go();
    }

    [Fact]
    public void RefreshValidation_ShouldDoNothing_WhenRootIsAlreadyDisposed()
    {
        var elementValidator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), elementValidator, elementValidator );
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.Dispose();

        sut.RefreshValidation();

        sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ).Go();
    }

    [Fact]
    public void ClearValidation_ShouldClearValidationForAllElementsAndCollectionItself()
    {
        var validator = Validators<string>.Fail<ICollectionVariableRootElements<int, VariableMock>>( Fixture.Create<string>() );
        var elementValidator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var allElements = Enumerable.Range( 0, 3 )
            .Select( i => new VariableMock( i, Fixture.Create<string>(), elementValidator, elementValidator ) )
            .ToList();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.Create(
            new[] { allElements[0], allElements[1] },
            new CollectionVariableRootChanges<int, VariableMock>(
                new[] { allElements[2] },
                new[] { allElements[0].Key } ),
            keySelector,
            errorsValidator: validator,
            warningsValidator: validator );

        sut.RefreshValidation();

        var onValidateEvents = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnValidate.Listen(
            EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidateEvents.Add ) );

        sut.ClearValidation();

        Assertion.All(
                sut.State.TestEquals( VariableState.Changed ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.Elements.AddedElementKeys.TestSetEqual( [ allElements[2].Key ] ),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ allElements[1].Key ] ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                allElements[0].State.TestEquals( VariableState.Default ),
                allElements[1].State.TestEquals( VariableState.Default ),
                allElements[2].State.TestEquals( VariableState.Default ),
                onValidateEvents.TestCount( count => count.TestEquals( 3 ) )
                    .Then( e =>
                        Assertion.All(
                            e.TestAny( (el, _) => el.SourceEvent.TestNotNull( source => source.Variable.TestEquals( allElements[0] ) ) ),
                            e.TestAny( (el, _) => el.SourceEvent.TestNotNull( source => source.Variable.TestEquals( allElements[2] ) ) ),
                            e[2].SourceEvent.TestNull(),
                            e[2].AssociatedChange.TestNull() ) ) )
            .Go();
    }

    [Fact]
    public void ClearValidation_ShouldNotPublishEventForCollection_WhenCollectionItselfDoesNotHaveAnyErrorsAndWarnings()
    {
        var elementValidator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var allElements = Enumerable.Range( 0, 3 )
            .Select( i => new VariableMock( i, Fixture.Create<string>(), elementValidator, elementValidator ) )
            .ToList();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create(
            new[] { allElements[0], allElements[1] },
            new CollectionVariableRootChanges<int, VariableMock>(
                new[] { allElements[2] },
                new[] { allElements[0].Key } ),
            keySelector );

        sut.RefreshValidation();

        var onValidateEvents = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnValidate.Listen(
            EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidateEvents.Add ) );

        sut.ClearValidation();

        Assertion.All(
                sut.State.TestEquals( VariableState.Changed ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.Elements.AddedElementKeys.TestSetEqual( [ allElements[2].Key ] ),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ allElements[1].Key ] ),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                allElements[0].State.TestEquals( VariableState.Default ),
                allElements[1].State.TestEquals( VariableState.Default ),
                allElements[2].State.TestEquals( VariableState.Default ),
                onValidateEvents.TestCount( count => count.TestEquals( 2 ) )
                    .Then( e =>
                        Assertion.All(
                            e.TestAny( (el, _) => el.SourceEvent.TestNotNull( source => source.Variable.TestEquals( allElements[0] ) ) ),
                            e.TestAny( (el, _) =>
                                el.SourceEvent.TestNotNull( source => source.Variable.TestEquals( allElements[2] ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void ClearValidation_ShouldDoNothing_WhenRootIsAlreadyDisposed()
    {
        var elementValidator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), elementValidator, elementValidator );
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.RefreshValidation();
        sut.Dispose();

        sut.ClearValidation();

        sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed | VariableState.Invalid | VariableState.Warning ).Go();
    }

    [Fact]
    public void SetReadOnly_ShouldEnableReadOnlyFlag_WhenNewValueIsTrueAndCurrentValueIsFalse()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChangeEvents = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChangeEvents.Add ) );

        sut.SetReadOnly( true );

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].Source.TestEquals( VariableChangeSource.SetReadOnly ),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestEmpty(),
                            e[0].RestoredElements.TestEmpty(),
                            e[0].SourceEvent.TestNull() ) ) )
            .Go();
    }

    [Fact]
    public void SetReadOnly_ShouldDisableReadOnlyFlag_WhenNewValueIsFalseAndCurrentValueIsTrue()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChangeEvents.Add ) );

        sut.SetReadOnly( false );

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e =>
                        Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.ReadOnly ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].Source.TestEquals( VariableChangeSource.SetReadOnly ),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestEmpty(),
                            e[0].RestoredElements.TestEmpty(),
                            e[0].SourceEvent.TestNull() ) ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetReadOnly_ShouldDoNothing_WhenReadOnlyFlagIsEqualToNewValue(bool enabled)
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );
        sut.SetReadOnly( enabled );

        var onChangeEvents = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChangeEvents.Add ) );

        sut.SetReadOnly( enabled );

        Assertion.All(
                sut.State.TestEquals( enabled ? VariableState.ReadOnly : VariableState.Default ),
                onChangeEvents.TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetReadOnly_ShouldDoNothing_WhenVariableIsDisposed(bool enabled)
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChangeEvents = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChangeEvents.Add ) );

        sut.Dispose();
        sut.SetReadOnly( enabled );

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                onChangeEvents.TestEmpty() )
            .Go();
    }
}

public sealed class VariableMock : Variable<string, string>
{
    public VariableMock(int key, string value)
        : base( value )
    {
        Key = key;
    }

    public VariableMock(int key, string value, IValidator<string, string> errorsValidator, IValidator<string, string> warningsValidator)
        : base( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator )
    {
        Key = key;
    }

    public int Key { get; }
}
