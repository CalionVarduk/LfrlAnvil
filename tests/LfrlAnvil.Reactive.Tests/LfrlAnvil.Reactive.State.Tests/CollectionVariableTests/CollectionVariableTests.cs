using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableTests;

public partial class CollectionVariableTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnInformationAboutElementCountAndState()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<TestElement>( count: 2 );
        var expected = "Elements: 1, State: Changed, ReadOnly";
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { initialValue }, new[] { value }, keySelector );
        sut.SetReadOnly( true );

        var result = sut.ToString();

        result.Should().Be( expected );
    }

    [Fact]
    public void Dispose_ShouldDisposeUnderlyingOnChangeAndOnValidateEventPublishersAndAddReadOnlyAndDisposedState()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<TestElement>( count: 2 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { initialValue }, new[] { value }, keySelector );

        var onChange = sut.OnChange.Listen( Substitute.For<IEventListener<CollectionVariableChangeEvent<int, TestElement, string>>>() );
        var onValidate = sut.OnValidate.Listen(
            Substitute.For<IEventListener<CollectionVariableValidationEvent<int, TestElement, string>>>() );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            onChange.IsDisposed.Should().BeTrue();
            onValidate.IsDisposed.Should().BeTrue();
            sut.State.Should().Be( VariableState.Changed | VariableState.ReadOnly | VariableState.Disposed );
        }
    }

    [Fact]
    public void SetReadOnly_ShouldEnableReadOnlyFlag_WhenNewValueIsTrueAndCurrentValueIsFalse()
    {
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

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
            changeEvent.RefreshedElements.Should().BeEmpty();
            changeEvent.ReplacedElements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetReadOnly_ShouldDisableReadOnlyFlag_WhenNewValueIsFalseAndCurrentValueIsTrue()
    {
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );
        sut.SetReadOnly( true );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

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
            changeEvent.RefreshedElements.Should().BeEmpty();
            changeEvent.ReplacedElements.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetReadOnly_ShouldDoNothing_WhenReadOnlyFlagIsEqualToNewValue(bool enabled)
    {
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );
        sut.SetReadOnly( enabled );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

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
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        sut.Dispose();
        sut.SetReadOnly( enabled );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            onChangeEvents.Should().BeEmpty();
        }
    }
}

public sealed record TestElement(int Key)
{
    public TestElement(int key, string? value)
        : this( key )
    {
        Value = value;
    }

    public string? Value { get; set; }
}

internal sealed class CollectionVariableMock : CollectionVariable<int, TestElement, string>
{
    private readonly HashSet<int> _keysWithBlockedModification;

    internal CollectionVariableMock(IEnumerable<TestElement> elements, IEnumerable<int> keysWithBlockedModification)
        : base( elements, e => e.Key )
    {
        _keysWithBlockedModification = keysWithBlockedModification.ToHashSet();
    }

    protected override bool ContinueElementRemoval(TestElement element)
    {
        return ! _keysWithBlockedModification.Contains( element.Key );
    }

    protected override bool ContinueElementAddition(TestElement element)
    {
        return ! _keysWithBlockedModification.Contains( element.Key );
    }

    protected override bool ContinueElementReplacement(TestElement element, TestElement replacement)
    {
        return ! _keysWithBlockedModification.Contains( element.Key );
    }
}
