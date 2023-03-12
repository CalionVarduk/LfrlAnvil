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
    public void Change_ShouldDoNothing_WhenRootIsReadOnly()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );
        sut.SetReadOnly( true );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Change( CollectionVariableRootChanges<int, VariableMock>.Empty );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
    }

    [Fact]
    public void Change_ShouldDoNothing_WhenOldAndNewElementsAreEmpty()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Change( CollectionVariableRootChanges<int, VariableMock>.Empty );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.Elements.Values.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
    }

    [Fact]
    public void Change_ShouldDoNothing_WhenOldAndNewElementsAreNotEmptyAndEqual()
    {
        var allElements = Fixture.CreateMany<VariableMock>( count: 3 ).ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create(
            new[] { allElements[0], allElements[1] },
            new CollectionVariableRootChanges<int, VariableMock>( new[] { allElements[2] }, new[] { allElements[0].Key } ),
            keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Change(
            new CollectionVariableRootChanges<int, VariableMock>(
                Array.Empty<VariableMock>(),
                new[] { allElements[0].Key, allElements[2].Key } ) );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.Elements.Values.Should().BeEquivalentTo( allElements[0], allElements[2] );
            sut.Elements.AddedElementKeys.Should().BeEquivalentTo( allElements[2].Key );
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( allElements[1].Key );
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
    }

    [Fact]
    public void Change_ShouldRemoveAllElementsAndPublishEvents_WhenOldElementsAreNotEmptyAndNewElementsAreEmpty()
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

        var result = sut.Change( CollectionVariableRootChanges<int, VariableMock>.Empty );

        using ( new AssertionScope() )
        {
            elements[0].State.Should().Be( VariableState.Default );
            elements[1].State.Should().Be( VariableState.Default );
            elements[2].State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( elements[0].Key, elements[1].Key );
            sut.GetChildren().Should().BeEquivalentTo( elements[0], elements[1] );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.PreviousState.Should().Be( VariableState.Changed );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeSequentiallyEqualTo( elements[0], elements[1], elements[2] );
            changeEvent.RestoredElements.Should().BeEmpty();
            changeEvent.SourceEvent.Should().BeNull();

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.SourceEvent.Should().BeNull();

            errorsValidator.VerifyCalls()
                .Received(
                    v =>
                    {
                        var _ = v.Validate( sut.Elements );
                    },
                    1 );

            warningsValidator.VerifyCalls()
                .Received(
                    v =>
                    {
                        var _ = v.Validate( sut.Elements );
                    },
                    1 );
        }
    }

    [Fact]
    public void Change_ShouldChangeElementsAndPublishEvents_WhenParameterContainsCollectionChanges()
    {
        var allElements = Fixture.CreateMany<VariableMock>( count: 13 ).ToList();
        allElements.Add( new VariableMock( allElements[5].Key, Fixture.Create<string>() ) );
        allElements.Add( new VariableMock( allElements[8].Key, Fixture.Create<string>() ) );
        allElements.Add( new VariableMock( allElements[9].Key, Fixture.Create<string>() ) );
        allElements[12].Dispose();
        allElements[15].Dispose();

        var initialElements = new[] { allElements[0], allElements[1], allElements[2] };
        var addedElements = new[]
        {
            allElements[3],
            allElements[4],
            allElements[5],
            allElements[6],
            allElements[7],
            allElements[8],
            allElements[9]
        };

        var elementsToAdd = new[]
        {
            allElements[10],
            allElements[10],
            allElements[11],
            allElements[1],
            allElements[11],
            allElements[12],
            allElements[6],
            allElements[7],
            allElements[7],
            allElements[13],
            allElements[14],
            allElements[15]
        };

        var keysToRestore = new[]
        {
            allElements[0].Key,
            allElements[1].Key,
            allElements[1].Key,
            allElements[3].Key,
            allElements[7].Key,
            allElements[8].Key,
            allElements[0].Key,
            allElements[9].Key,
            Fixture.Create<int>()
        };

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            initialElements,
            new CollectionVariableRootChanges<int, VariableMock>( addedElements, new[] { initialElements[0].Key, initialElements[2].Key } ),
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Change( new CollectionVariableRootChanges<int, VariableMock>( elementsToAdd, keysToRestore ) );

        using ( new AssertionScope() )
        {
            allElements[3].State.Should().Be( VariableState.Default );
            allElements[4].State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            allElements[5].State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            allElements[6].State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            allElements[7].State.Should().Be( VariableState.Default );
            allElements[8].State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            allElements[9].State.Should().Be( VariableState.Default );

            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should()
                .BeEquivalentTo(
                    allElements[0],
                    allElements[1],
                    allElements[3],
                    allElements[13],
                    allElements[7],
                    allElements[14],
                    allElements[9],
                    allElements[10],
                    allElements[11] );

            sut.Elements.AddedElementKeys.Should()
                .BeEquivalentTo(
                    allElements[3].Key,
                    allElements[13].Key,
                    allElements[7].Key,
                    allElements[14].Key,
                    allElements[9].Key,
                    allElements[10].Key,
                    allElements[11].Key );

            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( allElements[2].Key );

            sut.GetChildren()
                .Should()
                .BeEquivalentTo(
                    allElements[0],
                    allElements[1],
                    allElements[2],
                    allElements[3],
                    allElements[13],
                    allElements[7],
                    allElements[14],
                    allElements[9],
                    allElements[10],
                    allElements[11] );

            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Change );
            changeEvent.PreviousState.Should().Be( VariableState.Changed );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeSequentiallyEqualTo( allElements[10], allElements[11], allElements[13], allElements[14] );
            changeEvent.RemovedElements.Should()
                .BeEquivalentTo( allElements[2], allElements[4], allElements[5], allElements[6], allElements[8] );

            changeEvent.RestoredElements.Should().BeSequentiallyEqualTo( allElements[1] );
            changeEvent.SourceEvent.Should().BeNull();

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.SourceEvent.Should().BeNull();

            errorsValidator.VerifyCalls()
                .Received(
                    v =>
                    {
                        var _ = v.Validate( sut.Elements );
                    },
                    1 );

            warningsValidator.VerifyCalls()
                .Received(
                    v =>
                    {
                        var _ = v.Validate( sut.Elements );
                    },
                    1 );
        }
    }
}
