using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableTests;

public partial class CollectionVariableTests
{
    [Fact]
    public void Reset_WithInitialElementsOnly_ShouldUpdateCollectionCorrectlyAndResetValidationAndDirtyState()
    {
        // 0: initial => initial
        // 1: initial => not found
        // 2: initial changed => initial
        // 3: initial changed => not found
        // 4: initial removed => initial
        // 5: added => initial
        // 6: added => not found
        // 7: not found => initial

        var allElements = Fixture.CreateDistinctCollection<TestElement>( count: 8 );
        var initialElements = new[] { allElements[0], allElements[1], allElements[2], allElements[3], allElements[4] };
        var elements = new[]
        {
            allElements[0],
            allElements[1],
            new TestElement( allElements[2].Key, Fixture.Create<string>() ),
            new TestElement( allElements[3].Key, Fixture.Create<string>() ),
            allElements[5],
            allElements[6]
        };

        var newElements = new[] { allElements[0], allElements[2], allElements[4], allElements[5], allElements[7] };

        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( initialElements, elements, keySelector );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Reset( newElements );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly );
            sut.InitialElements.Values.Should().BeEquivalentTo( newElements.AsEnumerable() );
            sut.Elements.Values.Should().BeEquivalentTo( newElements.AsEnumerable() );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();

            onChangeEvents.Should().HaveCount( 1 );
            var changeEvent = onChangeEvents[0];
            changeEvent.Source.Should().Be( VariableChangeSource.Reset );
            changeEvent.RefreshedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( elements[1], elements[3], elements[5] );
            changeEvent.AddedElements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( allElements[4], allElements[7] );
            changeEvent.ReplacedElements.Select( e => (e.Element, e.PreviousElement) )
                .Should()
                .BeSequentiallyEqualTo( (newElements[0], elements[0]), (newElements[1], elements[2]), (newElements[3], elements[4]) );

            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Select( e => e.Element )
                .Should()
                .BeSequentiallyEqualTo(
                    newElements[0],
                    newElements[1],
                    newElements[2],
                    newElements[3],
                    newElements[4],
                    elements[1],
                    elements[3],
                    elements[5] );

            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
        }
    }

    [Fact]
    public void Reset_WithInitialElementsOnly_ShouldDoNothing_WhenVariableIsDisposed()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        sut.Dispose();

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Reset( new[] { element } );

        using ( new AssertionScope() )
        {
            sut.InitialElements.Should().BeEmpty();
            sut.Elements.Should().BeEmpty();
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void Reset_ShouldUpdateCollectionCorrectlyAndResetValidationAndDirtyState()
    {
        var allElements = Fixture.CreateDistinctCollection<TestElement>( count: 10 );
        var initialElements = new[] { allElements[0], allElements[1], allElements[2], allElements[3], allElements[4], allElements[5] };
        var elements = new[]
        {
            allElements[0],
            allElements[1],
            new TestElement( allElements[2].Key, Fixture.Create<string>() ),
            new TestElement( allElements[3].Key, Fixture.Create<string>() ),
            allElements[6],
            allElements[7]
        };

        var newInitialElements = new[] { allElements[0], allElements[1], allElements[2], allElements[6], allElements[7], allElements[8] };
        var newElements = new[]
        {
            allElements[0],
            new TestElement( allElements[1].Key, Fixture.Create<string>() ),
            allElements[4],
            new TestElement( allElements[6].Key, Fixture.Create<string>() ),
            allElements[8],
            allElements[9]
        };

        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( initialElements, elements, keySelector );
        sut.Refresh();

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Reset( newInitialElements, newElements );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed );
            sut.InitialElements.Values.Should().BeEquivalentTo( newInitialElements.AsEnumerable() );
            sut.Elements.Values.Should().BeEquivalentTo( newElements.AsEnumerable() );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();

            onChangeEvents.Should().HaveCount( 1 );
            var changeEvent = onChangeEvents[0];
            changeEvent.Source.Should().Be( VariableChangeSource.Reset );
            changeEvent.RefreshedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Select( e => e.Element ).Should().BeSequentiallyEqualTo( elements[2], elements[3], elements[5] );
            changeEvent.AddedElements.Select( e => e.Element )
                .Should()
                .BeSequentiallyEqualTo( allElements[4], allElements[8], allElements[9] );

            changeEvent.ReplacedElements.Select( e => (e.Element, e.PreviousElement) )
                .Should()
                .BeSequentiallyEqualTo( (newElements[0], elements[0]), (newElements[1], elements[1]), (newElements[3], elements[4]) );

            onValidateEvents.Should().HaveCount( 1 );
            var validateEvent = onValidateEvents[0];
            validateEvent.Elements.Select( e => e.Element )
                .Should()
                .BeSequentiallyEqualTo(
                    newElements[0],
                    newElements[1],
                    newElements[2],
                    newElements[3],
                    newElements[4],
                    newElements[5],
                    elements[2],
                    elements[3],
                    elements[5] );

            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );
        }
    }

    [Fact]
    public void Reset_ShouldDoNothing_WhenVariableIsDisposed()
    {
        var (element, addedElement) = Fixture.CreateDistinctCollection<TestElement>( count: 2 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        sut.Dispose();

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        var onValidateEvents = new List<CollectionVariableValidationEvent<int, TestElement, string>>();
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableValidationEvent<int, TestElement, string>>( onValidateEvents.Add ) );

        sut.Reset( new[] { element }, new[] { element, addedElement } );

        using ( new AssertionScope() )
        {
            sut.InitialElements.Should().BeEmpty();
            sut.Elements.Should().BeEmpty();
            onChangeEvents.Should().BeEmpty();
            onValidateEvents.Should().BeEmpty();
        }
    }
}
