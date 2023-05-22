using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.OptionalDisposableTests;

public class OptionalDisposableTests : TestsBase
{
    [Fact]
    public void Empty_ShouldReturnObjectWithoutValue()
    {
        var sut = OptionalDisposable<IDisposable>.Empty;
        sut.Value.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldReturnObjectWithValue()
    {
        var value = Substitute.For<IDisposable>();
        var sut = OptionalDisposable.Create( value );
        sut.Value.Should().BeSameAs( value );
    }

    [Fact]
    public void TryCreate_ShouldReturnObjectWithValue_WhenRefValueIsNotNull()
    {
        var value = Substitute.For<IDisposable>();
        var sut = OptionalDisposable.TryCreate( value );
        sut.Value.Should().BeSameAs( value );
    }

    [Fact]
    public void TryCreate_ShouldReturnObjectWithoutValue_WhenRefValueIsNull()
    {
        IDisposable? value = null;
        var sut = OptionalDisposable.TryCreate( value );
        sut.Value.Should().BeNull();
    }

    [Fact]
    public void TryCreate_ShouldReturnObjectWithValue_WhenNullableValueIsNotNull()
    {
        OptionalDisposable<IDisposable>? value = OptionalDisposable.Create( Substitute.For<IDisposable>() );
        var sut = OptionalDisposable.TryCreate( value );
        sut.Value.Should().BeEquivalentTo( value );
    }

    [Fact]
    public void TryCreate_ShouldReturnObjectWithDefaultValue_WhenNullableValueIsNull()
    {
        OptionalDisposable<IDisposable>? value = null;
        var sut = OptionalDisposable.TryCreate( value );
        sut.Value.Should().BeEquivalentTo( default( OptionalDisposable<IDisposable> ) );
    }

    [Fact]
    public void Dispose_ShouldCallValueDispose_WhenObjectHasValue()
    {
        var value = Substitute.For<IDisposable>();
        var sut = OptionalDisposable.Create( value );

        sut.Dispose();

        value.VerifyCalls().Received( o => o.Dispose(), 1 );
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenObjectDoesNotHaveValue()
    {
        var sut = OptionalDisposable<IDisposable>.Empty;
        var action = Lambda.Of( () => sut.Dispose() );
        action.Should().NotThrow();
    }
}
