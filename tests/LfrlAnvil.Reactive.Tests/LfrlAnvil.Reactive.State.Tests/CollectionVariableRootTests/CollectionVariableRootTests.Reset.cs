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

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            elements.Take( 3 )
                .Should()
                .OnlyContain( e => e.State == (VariableState.ReadOnly | VariableState.Disposed | VariableState.Dirty) );

            sut.State.Should().Be( VariableState.Default );
            sut.Elements.Values.Should().BeEquivalentTo( elements.Skip( 3 ) );
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Reset );
            changeEvent.PreviousState.Should()
                .Be( VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.Dirty );

            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeSequentiallyEqualTo( elements.Skip( 3 ) );
            changeEvent.RemovedElements.Should().BeEquivalentTo( elements.Take( 3 ) );
            changeEvent.RestoredElements.Should().BeEmpty();
            changeEvent.SourceEvent.Should().BeNull();

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.SourceEvent.Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            elements[0].State.Should().Be( VariableState.ReadOnly | VariableState.Disposed | VariableState.Dirty );
            elements[1].State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            elements[2].State.Should().Be( VariableState.ReadOnly | VariableState.Disposed | VariableState.Dirty );

            sut.State.Should().Be( VariableState.Changed | VariableState.ReadOnly );
            sut.Elements.Values.Should().BeEquivalentTo( elements[3], elements[5] );
            sut.Elements.AddedElementKeys.Should().BeEquivalentTo( elements[5].Key );
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( elements[4].Key );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.Source.Should().Be( VariableChangeSource.Reset );
            changeEvent.PreviousState.Should()
                .Be( VariableState.Changed | VariableState.Invalid | VariableState.Warning | VariableState.ReadOnly | VariableState.Dirty );

            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.AddedElements.Should().BeSequentiallyEqualTo( elements[3], elements[5] );
            changeEvent.RemovedElements.Should().BeEquivalentTo( elements[0], elements[2] );
            changeEvent.RestoredElements.Should().BeEmpty();
            changeEvent.SourceEvent.Should().BeNull();

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
            validateEvent.SourceEvent.Should().BeNull();
        }
    }
}
