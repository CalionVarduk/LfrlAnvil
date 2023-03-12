using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableTests;

public partial class CollectionVariableTests
{
    [Fact]
    public void TryChange_ShouldUpdateElementsWithNewCollection()
    {
        var allElements = Fixture.CreateDistinctCollection<TestElement>( count: 7 );
        var initialElements = new[] { allElements[0], allElements[1], allElements[2], allElements[3], allElements[4] };
        var elements = new[]
        {
            allElements[0],
            new TestElement( allElements[2].Key, allElements[2].Value ),
            new TestElement( allElements[3].Key, Fixture.Create<string>() ),
            allElements[4],
            allElements[0],
            allElements[4],
            allElements[4],
            allElements[6]
        };

        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( initialElements, keySelector );
        sut.Remove( allElements[4].Key );
        sut.Add( allElements[5] );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.TryChange( elements );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEquivalentTo( elements[0], elements[1], elements[2], elements[3], elements[7] );
            sut.Elements.ModifiedElementKeys.Should().BeEquivalentTo( allElements[1].Key, allElements[3].Key, allElements[6].Key );
            sut.Elements.GetState( allElements[0].Key ).Should().Be( CollectionVariableElementState.Default );
            sut.Elements.GetState( allElements[1].Key ).Should().Be( CollectionVariableElementState.Removed );
            sut.Elements.GetState( allElements[2].Key ).Should().Be( CollectionVariableElementState.Default );
            sut.Elements.GetState( allElements[3].Key ).Should().Be( CollectionVariableElementState.Changed );
            sut.Elements.GetState( allElements[4].Key ).Should().Be( CollectionVariableElementState.Default );
            sut.Elements.GetState( allElements[5].Key ).Should().Be( CollectionVariableElementState.NotFound );
            sut.Elements.GetState( allElements[6].Key ).Should().Be( CollectionVariableElementState.Added );
            onChangeEvents.Should().HaveCount( 1 );
            onValidateEvents.Should().HaveCount( 1 );

            var changeEvent = onChangeEvents[0];
            changeEvent.Source.Should().Be( VariableChangeSource.TryChange );
            changeEvent.RefreshedElements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( allElements[0], allElements[2] );
            changeEvent.AddedElements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( allElements[4], allElements[6] );
            changeEvent.RemovedElements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( allElements[1], allElements[5] );
            changeEvent.ReplacedElements.Select( e => (e.Element, e.PreviousElement) )
                .Should()
                .BeSequentiallyEqualTo( (elements[2], allElements[3]) );

            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Select( e => e.Element )
                .Should()
                .BeSequentiallyEqualTo( elements[0], elements[1], elements[2], elements[3], elements[7], allElements[1], allElements[5] );

            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
        }
    }

    [Fact]
    public void TryChange_ShouldRemoveAllElements_WhenCurrentElementsAreNotEmptyAndNewElementsAreEmpty()
    {
        var elements = Fixture.CreateDistinctCollection<TestElement>( count: 2 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( elements, keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.TryChange( Array.Empty<TestElement>() );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.Changed );
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.Values.Should().BeEmpty();
            onChangeEvents.Should().HaveCount( 1 );
            onChangeEvents[0].RemovedElements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( elements );
            onValidateEvents.Should().HaveCount( 1 );
            onValidateEvents[0].AssociatedChange.Should().BeSameAs( onChangeEvents[0] );
        }
    }

    [Fact]
    public void TryChange_ShouldDoNothing_WhenCurrentAndNewElementsAreEmpty()
    {
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.TryChange( Array.Empty<TestElement>() );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.NotChanged );
            sut.State.Should().Be( VariableState.Default );
            sut.Elements.Values.Should().BeEmpty();
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void TryChange_ShouldDoNothing_WhenStateContainsReadOnlyFlag()
    {
        var elements = new[] { Fixture.Create<TestElement>() };
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        var result = sut.TryChange( elements );

        using ( new AssertionScope() )
        {
            result.Should().Be( VariableChangeResult.ReadOnly );
            sut.State.Should().Be( VariableState.ReadOnly );
            sut.Elements.Values.Should().BeEmpty();
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }
}
