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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.Elements.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            element.State.Should().Be( VariableState.Default );
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( element.Key );
            sut.GetChildren().Should().BeEquivalentTo( element );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeSequentiallyEqualTo( element );
            changeEvent.RestoredElements.Should().BeEmpty();
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

        using ( new AssertionScope() )
        {
            element.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Dirty );
            sut.Elements.Values.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.GetChildren().Should().BeEmpty();
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.PreviousState.Should().Be( VariableState.Changed );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeSequentiallyEqualTo( element );
            changeEvent.RestoredElements.Should().BeEmpty();
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.Elements.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.Elements.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( element.Key );
            sut.GetChildren().Should().BeEquivalentTo( element );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeSequentiallyEqualTo( element );
            changeEvent.RestoredElements.Should().BeEmpty();
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

        using ( new AssertionScope() )
        {
            allElements[0].State.Should().Be( VariableState.Default );
            allElements[2].State.Should().Be( VariableState.Default );
            allElements[3].State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            result.Should().Be( VariableChangeResult.Changed );
            sut.Elements.Values.Should().BeEquivalentTo( allElements[1], allElements[4] );
            sut.Elements.AddedElementKeys.Should().BeEquivalentTo( allElements[4].Key );
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( allElements[0].Key, allElements[2].Key );
            sut.GetChildren().Should().BeEquivalentTo( allElements[0], allElements[1], allElements[2], allElements[4] );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.PreviousState.Should().Be( VariableState.Changed );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeSequentiallyEqualTo( allElements[0], allElements[3] );
            changeEvent.RestoredElements.Should().BeEmpty();
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
