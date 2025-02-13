using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableTests;

public partial class CollectionVariableTests : TestsBase
{
    public CollectionVariableTests()
    {
        Fixture.Customize<TestElement>( (_, _) => f => new TestElement( f.Create<int>() ) );
    }

    [Fact]
    public void ToString_ShouldReturnInformationAboutElementCountAndState()
    {
        var (initialValue, value) = Fixture.CreateManyDistinct<TestElement>( count: 2 );
        var expected = "Elements: 1, State: Changed, ReadOnly";
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { initialValue }, new[] { value }, keySelector );
        sut.SetReadOnly( true );

        var result = sut.ToString();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Dispose_ShouldDisposeUnderlyingOnChangeAndOnValidateEventPublishersAndAddReadOnlyAndDisposedState()
    {
        var (initialValue, value) = Fixture.CreateManyDistinct<TestElement>( count: 2 );
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( new[] { initialValue }, new[] { value }, keySelector );

        var onChange = sut.OnChange.Listen( Substitute.For<IEventListener<CollectionVariableChangeEvent<int, TestElement, string>>>() );
        var onValidate = sut.OnValidate.Listen(
            Substitute.For<IEventListener<CollectionVariableValidationEvent<int, TestElement, string>>>() );

        sut.Dispose();

        Assertion.All(
                onChange.IsDisposed.TestTrue(),
                onValidate.IsDisposed.TestTrue(),
                sut.State.TestEquals( VariableState.Changed | VariableState.ReadOnly | VariableState.Disposed ) )
            .Go();
    }

    [Fact]
    public void SetReadOnly_ShouldEnableReadOnlyFlag_WhenNewValueIsTrueAndCurrentValueIsFalse()
    {
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = CollectionVariable.WithoutValidators<string>.Create( Array.Empty<TestElement>(), keySelector );

        var onChangeEvents = new List<CollectionVariableChangeEvent<int, TestElement, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableChangeEvent<int, TestElement, string>>( onChangeEvents.Add ) );

        sut.SetReadOnly( true );

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.Default ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].Source.TestEquals( VariableChangeSource.SetReadOnly ),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestEmpty(),
                            e[0].RefreshedElements.TestEmpty(),
                            e[0].ReplacedElements.TestEmpty() ) ) )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                onChangeEvents.TestCount( count => count.TestEquals( 1 ) )
                    .Then(
                        e => Assertion.All(
                            "changeEvent",
                            e[0].Variable.TestRefEquals( sut ),
                            e[0].PreviousState.TestEquals( VariableState.ReadOnly ),
                            e[0].NewState.TestEquals( sut.State ),
                            e[0].Source.TestEquals( VariableChangeSource.SetReadOnly ),
                            e[0].AddedElements.TestEmpty(),
                            e[0].RemovedElements.TestEmpty(),
                            e[0].RefreshedElements.TestEmpty(),
                            e[0].ReplacedElements.TestEmpty() ) ) )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( enabled ? VariableState.ReadOnly : VariableState.Default ),
                onChangeEvents.TestEmpty() )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                onChangeEvents.TestEmpty() )
            .Go();
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
