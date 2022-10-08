using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.TestExtensions.FluentAssertions;
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
    public void Clear_ShouldDoNothing_WhenOldElementsAreEmpty()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( Array.Empty<VariableMock>(), keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        var result = sut.Clear();

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
}
