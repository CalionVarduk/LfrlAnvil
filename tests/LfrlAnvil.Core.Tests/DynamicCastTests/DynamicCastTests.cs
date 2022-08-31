using System.Collections.Generic;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.DynamicCastTests;

public class DynamicCastTests : TestsBase
{
    [Fact]
    public void TryTo_ShouldReturnParameter_WhenParameterIsNotNullAndOfType()
    {
        IEnumerable<int> value = new List<int>();
        var result = DynamicCast.TryTo<List<int>>( value );
        result.Should().BeSameAs( value );
    }

    [Fact]
    public void TryTo_ShouldReturnNull_WhenParameterIsNotNullAndNotOfType()
    {
        var value = new List<int>();
        var result = DynamicCast.TryTo<string>( value );
        result.Should().BeNull();
    }

    [Fact]
    public void TryTo_ShouldReturnNull_WhenParameterIsNull()
    {
        var result = DynamicCast.TryTo<List<int>>( null );
        result.Should().BeNull();
    }

    [Fact]
    public void To_ShouldReturnParameter_WhenParameterIsNotNullAndOfType()
    {
        IEnumerable<int> value = new List<int>();
        var result = DynamicCast.To<List<int>>( value );
        result.Should().BeSameAs( value );
    }

    [Fact]
    public void To_ShouldThrowInvalidCastException_WhenParameterIsNotNullAndNotOfType()
    {
        var value = new List<int>();
        var action = Lambda.Of( () => DynamicCast.To<string>( value ) );
        action.Should().ThrowExactly<InvalidCastException>();
    }

    [Fact]
    public void To_ShouldReturnNull_WhenParameterIsNull()
    {
        var result = DynamicCast.To<List<int>>( null );
        result.Should().BeNull();
    }

    [Fact]
    public void TryUnbox_ShouldReturnUnboxedParameter_WhenParameterIsNotNullAndOfType()
    {
        var value = 1;
        var result = DynamicCast.TryUnbox<int>( value );
        result.Should().Be( value );
    }

    [Fact]
    public void TryUnbox_ShouldReturnNull_WhenParameterIsNotNullAndNotOfType()
    {
        var value = "foo";
        var result = DynamicCast.TryUnbox<int>( value );
        result.Should().BeNull();
    }

    [Fact]
    public void TryUnbox_ShouldReturnNull_WhenParameterIsNull()
    {
        var result = DynamicCast.TryUnbox<int>( null );
        result.Should().BeNull();
    }

    [Fact]
    public void Unbox_ShouldReturnUnboxedParameter_WhenParameterIsNotNullAndOfType()
    {
        var value = 1;
        var result = DynamicCast.Unbox<int>( value );
        result.Should().Be( value );
    }

    [Fact]
    public void Unbox_ShouldThrowInvalidCastException_WhenParameterIsNotNullAndNotOfType()
    {
        var value = "foo";
        var action = Lambda.Of( () => DynamicCast.Unbox<int>( value ) );
        action.Should().ThrowExactly<InvalidCastException>();
    }

    [Fact]
    public void Unbox_ShouldThrowNullReferenceException_WhenParameterIsNull()
    {
        var action = Lambda.Of( () => DynamicCast.Unbox<int>( null ) );
        action.Should().ThrowExactly<NullReferenceException>();
    }
}
