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
        result.TestRefEquals( value ).Go();
    }

    [Fact]
    public void TryTo_ShouldReturnNull_WhenParameterIsNotNullAndNotOfType()
    {
        var value = new List<int>();
        var result = DynamicCast.TryTo<string>( value );
        result.TestNull().Go();
    }

    [Fact]
    public void TryTo_ShouldReturnNull_WhenParameterIsNull()
    {
        var result = DynamicCast.TryTo<List<int>>( null );
        result.TestNull().Go();
    }

    [Fact]
    public void To_ShouldReturnParameter_WhenParameterIsNotNullAndOfType()
    {
        IEnumerable<int> value = new List<int>();
        var result = DynamicCast.To<List<int>>( value );
        result.TestRefEquals( value ).Go();
    }

    [Fact]
    public void To_ShouldThrowInvalidCastException_WhenParameterIsNotNullAndNotOfType()
    {
        var value = new List<int>();
        var action = Lambda.Of( () => DynamicCast.To<string>( value ) );
        action.Test( exc => exc.TestType().Exact<InvalidCastException>() ).Go();
    }

    [Fact]
    public void To_ShouldReturnNull_WhenParameterIsNull()
    {
        var result = DynamicCast.To<List<int>>( null );
        result.TestNull().Go();
    }

    [Fact]
    public void TryUnbox_ShouldReturnUnboxedParameter_WhenParameterIsNotNullAndOfType()
    {
        var value = 1;
        var result = DynamicCast.TryUnbox<int>( value );
        result.TestEquals( value ).Go();
    }

    [Fact]
    public void TryUnbox_ShouldReturnNull_WhenParameterIsNotNullAndNotOfType()
    {
        var value = "foo";
        var result = DynamicCast.TryUnbox<int>( value );
        result.TestNull().Go();
    }

    [Fact]
    public void TryUnbox_ShouldReturnNull_WhenParameterIsNull()
    {
        var result = DynamicCast.TryUnbox<int>( null );
        result.TestNull().Go();
    }

    [Fact]
    public void Unbox_ShouldReturnUnboxedParameter_WhenParameterIsNotNullAndOfType()
    {
        var value = 1;
        var result = DynamicCast.Unbox<int>( value );
        result.TestEquals( value ).Go();
    }

    [Fact]
    public void Unbox_ShouldThrowInvalidCastException_WhenParameterIsNotNullAndNotOfType()
    {
        var value = "foo";
        var action = Lambda.Of( () => DynamicCast.Unbox<int>( value ) );
        action.Test( exc => exc.TestType().Exact<InvalidCastException>() ).Go();
    }

    [Fact]
    public void Unbox_ShouldThrowNullReferenceException_WhenParameterIsNull()
    {
        var action = Lambda.Of( () => DynamicCast.Unbox<int>( null ) );
        action.Test( exc => exc.TestType().Exact<NullReferenceException>() ).Go();
    }
}
