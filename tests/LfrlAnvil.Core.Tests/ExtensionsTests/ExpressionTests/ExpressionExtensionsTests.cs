using System.Linq.Expressions;
using FluentAssertions.Execution;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.ExtensionsTests.ExpressionTests;

public class ExpressionExtensionsTests : TestsBase
{
    [Fact]
    public void GetMemberName_ShouldReturnCorrectResult_WhenMemberIsField()
    {
        Expression<Func<TestClass, string?>> sut = t => t.Field;
        var result = sut.GetMemberName();
        result.Should().Be( nameof( TestClass.Field ) );
    }

    [Fact]
    public void GetMemberName_ShouldReturnCorrectResult_WhenMemberIsProperty()
    {
        Expression<Func<TestClass, string?>> sut = t => t.Property;
        var result = sut.GetMemberName();
        result.Should().Be( nameof( TestClass.Property ) );
    }

    [Fact]
    public void GetMemberName_ShouldThrowArgumentException_WhenBodyIsMethodCall()
    {
        Expression<Func<TestClass, string?>> sut = t => t.Method();
        var action = Lambda.Of( () => sut.GetMemberName() );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void GetMemberName_ShouldThrowArgumentException_WhenBodyIsStaticMember()
    {
        Expression<Func<TestClass, string?>> sut = _ => TestClass.StaticProperty;
        var action = Lambda.Of( () => sut.GetMemberName() );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void GetMemberName_ShouldThrowArgumentException_WhenBodyIsStaticMemberFromDifferentType()
    {
        Expression<Func<TestClass, string>> sut = _ => string.Empty;
        var action = Lambda.Of( () => sut.GetMemberName() );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void GetMemberName_ShouldThrowArgumentException_WhenBodyIsAccessingMemberOfMember()
    {
        Expression<Func<TestClass, string?>> sut = t => t.Other!.Property;
        var action = Lambda.Of( () => sut.GetMemberName() );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void TryGetValue_ShouldReturnCorrectResult_WhenExpressionHasValueOfTheSpecifiedType()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Expression.Constant( value );

        var result = sut.TryGetValue<int>( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( value );
        }
    }

    [Fact]
    public void TryGetValue_ShouldReturnCorrectResult_WhenExpressionHasValueAssignableToTheSpecifiedType()
    {
        var value = Fixture.Create<string>();
        var sut = Expression.Constant( value );

        var result = sut.TryGetValue<object>( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( value );
        }
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalse_WhenExpressionHasValueNotAssignableToTheSpecifiedType()
    {
        var value = Fixture.Create<string>();
        var sut = Expression.Constant( value );

        var result = sut.TryGetValue<int>( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalse_WhenExpressionHasNullValue()
    {
        var sut = Expression.Constant( null, typeof( string ) );

        var result = sut.TryGetValue<string>( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnCorrectResult_WhenExpressionHasValueOfTheSpecifiedType()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Expression.Constant( value );

        var result = sut.GetValueOrDefault<int>();

        result.Should().Be( value );
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnCorrectResult_WhenExpressionHasValueAssignableToTheSpecifiedType()
    {
        var value = Fixture.Create<string>();
        var sut = Expression.Constant( value );

        var result = sut.GetValueOrDefault<object>();

        result.Should().Be( value );
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnDefault_WhenExpressionHasValueNotAssignableToTheSpecifiedType()
    {
        var value = Fixture.Create<string>();
        var sut = Expression.Constant( value );

        var result = sut.GetValueOrDefault<int>();

        result.Should().Be( default );
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnNull_WhenExpressionHasNullValue()
    {
        var sut = Expression.Constant( null, typeof( string ) );
        var result = sut.GetValueOrDefault<string>();
        result.Should().BeNull();
    }
}

public class TestClass
{
    public static string? StaticProperty { get; set; }

    public string? Property { get; set; }
    public string? Field;
    public TestClass? Other { get; set; }

    public string? Method()
    {
        return Property;
    }
}
