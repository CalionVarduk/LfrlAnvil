using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.OptionalDisposableTests;

public class OptionalDisposableTests : TestsBase
{
    [Fact]
    public void Empty_ShouldReturnObjectWithoutValue()
    {
        var sut = OptionalDisposable<IDisposable>.Empty;
        sut.Value.TestNull().Go();
    }

    [Fact]
    public void Create_ShouldReturnObjectWithValue()
    {
        var value = Substitute.For<IDisposable>();
        var sut = OptionalDisposable.Create( value );
        sut.Value.TestRefEquals( value ).Go();
    }

    [Fact]
    public void TryCreate_ShouldReturnObjectWithValue_WhenRefValueIsNotNull()
    {
        var value = Substitute.For<IDisposable>();
        var sut = OptionalDisposable.TryCreate( value );
        sut.Value.TestRefEquals( value ).Go();
    }

    [Fact]
    public void TryCreate_ShouldReturnObjectWithoutValue_WhenRefValueIsNull()
    {
        IDisposable? value = null;
        var sut = OptionalDisposable.TryCreate( value );
        sut.Value.TestNull().Go();
    }

    [Fact]
    public void TryCreate_ShouldReturnObjectWithValue_WhenNullableValueIsNotNull()
    {
        OptionalDisposable<IDisposable>? value = OptionalDisposable.Create( Substitute.For<IDisposable>() );
        var sut = OptionalDisposable.TryCreate( value );
        sut.Value.ToNullable().TestEquals( value ).Go();
    }

    [Fact]
    public void TryCreate_ShouldReturnObjectWithDefaultValue_WhenNullableValueIsNull()
    {
        OptionalDisposable<IDisposable>? value = null;
        var sut = OptionalDisposable.TryCreate( value );
        sut.Value.TestEquals( default ).Go();
    }

    [Fact]
    public void Dispose_ShouldCallValueDispose_WhenObjectHasValue()
    {
        var value = Substitute.For<IDisposable>();
        var sut = OptionalDisposable.Create( value );

        sut.Dispose();

        value.ReceivedCalls( o => o.Dispose(), count: 1 ).Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenObjectDoesNotHaveValue()
    {
        var sut = OptionalDisposable<IDisposable>.Empty;
        var action = Lambda.Of( () => sut.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }
}
