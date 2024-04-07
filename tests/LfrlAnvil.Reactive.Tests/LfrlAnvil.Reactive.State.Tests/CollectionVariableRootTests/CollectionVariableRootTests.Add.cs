using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Reactive.State.Internal;
using LfrlAnvil.Reactive.State.Tests.VariableRootTests;
using LfrlAnvil.TestExtensions.FluentAssertions;
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.Elements.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            element.Parent.Should().BeSameAs( sut );
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.AddedElementKeys.Should().BeEquivalentTo( element.Key );
            sut.GetChildren().Should().BeEquivalentTo( element );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeSequentiallyEqualTo( element );
            changeEvent.RemovedElements.Should().BeEmpty();
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

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.Elements.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            element.Parent.Should().BeSameAs( sut );
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.AddedElementKeys.Should().BeEquivalentTo( element.Key );
            sut.GetChildren().Should().BeEquivalentTo( element );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeSequentiallyEqualTo( element );
            changeEvent.RemovedElements.Should().BeEmpty();
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

        using ( new AssertionScope() )
        {
            allElements[4].Parent.Should().BeSameAs( sut );
            allElements[5].Parent.Should().BeSameAs( sut );
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEquivalentTo( allElements[0], allElements[2], allElements[4], allElements[5] );
            sut.Elements.AddedElementKeys.Should().BeEquivalentTo( allElements[2].Key, allElements[4].Key, allElements[5].Key );
            sut.GetChildren().Should().BeEquivalentTo( allElements[0], allElements[1], allElements[2], allElements[4], allElements[5] );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.PreviousState.Should().Be( VariableState.Changed );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeSequentiallyEqualTo( allElements[4], allElements[5] );
            changeEvent.RemovedElements.Should().BeEmpty();
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
