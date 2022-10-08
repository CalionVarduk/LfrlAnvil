using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableRootTests;

public partial class CollectionVariableRootTests : TestsBase
{
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

        result.Should().Be( expected );
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

        using ( new AssertionScope() )
        {
            onChange.IsDisposed.Should().BeTrue();
            onValidate.IsDisposed.Should().BeTrue();
            sut.State.Should().Be( VariableState.Changed | VariableState.ReadOnly | VariableState.Disposed );
            allElements.Should().OnlyContain( e => e.State == (VariableState.ReadOnly | VariableState.Disposed) );
        }
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenRootIsAlreadyDisposed()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );
        sut.Dispose();

        sut.Dispose();

        sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
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

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEquivalentTo( allElements[2].Key );
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( allElements[1].Key );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            allElements[0].State.Should().Be( VariableState.Dirty );
            allElements[1].State.Should().Be( VariableState.Default );
            allElements[2].State.Should().Be( VariableState.Dirty );

            onChangeEvents.Should().HaveCount( 3 );
            onChangeEvents.Should().ContainSingle( e => e.SourceEvent != null && e.SourceEvent.Variable == allElements[0] );
            onChangeEvents.Should().ContainSingle( e => e.SourceEvent != null && e.SourceEvent.Variable == allElements[2] );
            onChangeEvents[2].SourceEvent.Should().BeNull();
            onChangeEvents[2].Source.Should().Be( VariableChangeSource.Refresh );

            onValidateEvents.Should().HaveCount( 3 );
            onValidateEvents.Should().ContainSingle( e => e.SourceEvent != null && e.SourceEvent.Variable == allElements[0] );
            onValidateEvents.Should().ContainSingle( e => e.SourceEvent != null && e.SourceEvent.Variable == allElements[2] );
            onValidateEvents[2].SourceEvent.Should().BeNull();
            onValidateEvents[2].AssociatedChange.Should().BeSameAs( onChangeEvents[2] );
        }
    }

    [Fact]
    public void Refresh_ShouldDoNothing_WhenRootIsAlreadyDisposed()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.Dispose();

        sut.Refresh();

        sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
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

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed | VariableState.Invalid | VariableState.Warning );
            sut.Errors.Should().HaveCount( 1 );
            sut.Warnings.Should().HaveCount( 1 );
            sut.Elements.AddedElementKeys.Should().BeEquivalentTo( allElements[2].Key );
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( allElements[1].Key );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEquivalentTo( allElements[0].Key, allElements[2].Key );
            sut.Elements.WarningElementKeys.Should().BeEquivalentTo( allElements[0].Key, allElements[2].Key );
            allElements[0].State.Should().Be( VariableState.Invalid | VariableState.Warning );
            allElements[1].State.Should().Be( VariableState.Default );
            allElements[2].State.Should().Be( VariableState.Invalid | VariableState.Warning );

            onValidateEvents.Should().HaveCount( 3 );
            onValidateEvents.Should().ContainSingle( e => e.SourceEvent != null && e.SourceEvent.Variable == allElements[0] );
            onValidateEvents.Should().ContainSingle( e => e.SourceEvent != null && e.SourceEvent.Variable == allElements[2] );
            onValidateEvents[2].SourceEvent.Should().BeNull();
            onValidateEvents[2].AssociatedChange.Should().BeNull();
        }
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

        sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
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

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEquivalentTo( allElements[2].Key );
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( allElements[1].Key );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            allElements[0].State.Should().Be( VariableState.Default );
            allElements[1].State.Should().Be( VariableState.Default );
            allElements[2].State.Should().Be( VariableState.Default );

            onValidateEvents.Should().HaveCount( 3 );
            onValidateEvents.Should().ContainSingle( e => e.SourceEvent != null && e.SourceEvent.Variable == allElements[0] );
            onValidateEvents.Should().ContainSingle( e => e.SourceEvent != null && e.SourceEvent.Variable == allElements[2] );
            onValidateEvents[2].SourceEvent.Should().BeNull();
            onValidateEvents[2].AssociatedChange.Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEquivalentTo( allElements[2].Key );
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( allElements[1].Key );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            allElements[0].State.Should().Be( VariableState.Default );
            allElements[1].State.Should().Be( VariableState.Default );
            allElements[2].State.Should().Be( VariableState.Default );

            onValidateEvents.Should().HaveCount( 2 );
            onValidateEvents.Should().ContainSingle( e => e.SourceEvent != null && e.SourceEvent.Variable == allElements[0] );
            onValidateEvents.Should().ContainSingle( e => e.SourceEvent != null && e.SourceEvent.Variable == allElements[2] );
        }
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

        sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed | VariableState.Invalid | VariableState.Warning );
    }

    [Fact]
    public void SetReadOnly_ShouldEnableReadOnlyFlag_WhenNewValueIsTrueAndCurrentValueIsFalse()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChangeEvents = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChangeEvents.Add ) );

        sut.SetReadOnly( true );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly );
            onChangeEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.Source.Should().Be( VariableChangeSource.SetReadOnly );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeEmpty();
            changeEvent.RestoredElements.Should().BeEmpty();
            changeEvent.SourceEvent.Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            onChangeEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.ReadOnly );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.Source.Should().Be( VariableChangeSource.SetReadOnly );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeEmpty();
            changeEvent.RestoredElements.Should().BeEmpty();
            changeEvent.SourceEvent.Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( enabled ? VariableState.ReadOnly : VariableState.Default );
            onChangeEvents.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            onChangeEvents.Should().BeEmpty();
        }
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
