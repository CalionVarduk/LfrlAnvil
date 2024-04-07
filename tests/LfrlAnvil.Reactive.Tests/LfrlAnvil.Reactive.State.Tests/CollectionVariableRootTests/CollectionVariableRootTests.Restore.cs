using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.TestExtensions.FluentAssertions;
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.Elements.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( element.Key );
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.Elements.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( element.Key );
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEquivalentTo( element.Key );
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Dirty );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.GetChildren().Should().BeEquivalentTo( element );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.PreviousState.Should().Be( VariableState.Changed );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeEmpty();
            changeEvent.RestoredElements.Should().BeSequentiallyEqualTo( element );
            changeEvent.SourceEvent.Should().BeNull();

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.SourceEvent.Should().BeNull();

            errorsValidator.VerifyCalls()
                .Received(
                    v => { _ = v.Validate( sut.Elements ); },
                    1 );

            warningsValidator.VerifyCalls()
                .Received(
                    v => { _ = v.Validate( sut.Elements ); },
                    1 );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.Elements.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( element.Key );
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.Elements.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.Elements.Values.Should().BeEquivalentTo( elements );
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Dirty );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            sut.GetChildren().Should().BeEquivalentTo( element );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.PreviousState.Should().Be( VariableState.Changed );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeEmpty();
            changeEvent.RestoredElements.Should().BeSequentiallyEqualTo( element );
            changeEvent.SourceEvent.Should().BeNull();

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.SourceEvent.Should().BeNull();

            errorsValidator.VerifyCalls()
                .Received(
                    v => { _ = v.Validate( sut.Elements ); },
                    1 );

            warningsValidator.VerifyCalls()
                .Received(
                    v => { _ = v.Validate( sut.Elements ); },
                    1 );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEquivalentTo( allElements[0], allElements[1], allElements[2], allElements[4] );
            sut.Elements.AddedElementKeys.Should().BeEquivalentTo( allElements[4].Key );
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( allElements[3].Key );
            sut.GetChildren().Should().BeEquivalentTo( allElements[0], allElements[1], allElements[2], allElements[3], allElements[4] );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.PreviousState.Should().Be( VariableState.Changed );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeEmpty();
            changeEvent.RestoredElements.Should().BeSequentiallyEqualTo( allElements[1], allElements[2] );
            changeEvent.SourceEvent.Should().BeNull();

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.SourceEvent.Should().BeNull();

            errorsValidator.VerifyCalls()
                .Received(
                    v => { _ = v.Validate( sut.Elements ); },
                    1 );

            warningsValidator.VerifyCalls()
                .Received(
                    v => { _ = v.Validate( sut.Elements ); },
                    1 );
        }
    }
}
