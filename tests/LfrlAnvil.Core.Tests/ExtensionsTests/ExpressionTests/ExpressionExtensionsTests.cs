using System.Collections.Generic;
using System.Linq.Expressions;
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

    [Fact]
    public void ReplaceParameters_ShouldInjectNewExpressionsInPlaceOfSpecifiedNamedParameterExpressions()
    {
        var p1 = Expression.Parameter( typeof( int ), "p1" );
        var p2 = Expression.Parameter( typeof( int ), "p2" );
        var p3 = Expression.Parameter( typeof( int ), "p3" );
        var pNoName = Expression.Parameter( typeof( int ) );
        var c1 = Expression.Constant( 0 );

        var p1Replacement = Expression.Constant( 10 );
        var p3Replacement = Expression.Constant( 20 );

        var parametersToReplace = new Dictionary<string, Expression>
        {
            { "p1", p1Replacement },
            { "p3", p3Replacement }
        };

        var p1P2Add = Expression.Add( p1, p2 );
        var p3PNoNameAdd = Expression.Add( p3, pNoName );
        var p1P2P3PNoNameAdd = Expression.Add( p1P2Add, p3PNoNameAdd );

        // 0 + ((p1 + p2) + (p3 + pNoName))
        var sut = Expression.Add( c1, p1P2P3PNoNameAdd );

        // 0 + ((10 + p2) + (20 + pNoName))
        var result = sut.ReplaceParameters( parametersToReplace );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Add );
            result.Should().BeAssignableTo<BinaryExpression>();
            if ( result is not BinaryExpression newSut )
                return;

            newSut.Left.Should().BeSameAs( c1 );
            newSut.Right.NodeType.Should().Be( ExpressionType.Add );
            newSut.Right.Should().BeAssignableTo<BinaryExpression>();
            if ( newSut.Right is not BinaryExpression newP1P2P3PNoNameAdd )
                return;

            newP1P2P3PNoNameAdd.Left.NodeType.Should().Be( ExpressionType.Add );
            newP1P2P3PNoNameAdd.Right.NodeType.Should().Be( ExpressionType.Add );

            if ( newP1P2P3PNoNameAdd.Left is not BinaryExpression newP1P2Add ||
                newP1P2P3PNoNameAdd.Right is not BinaryExpression newP3PNoNameAdd )
                return;

            newP1P2Add.Left.Should().BeSameAs( p1Replacement );
            newP1P2Add.Right.Should().BeSameAs( p2 );
            newP3PNoNameAdd.Left.Should().BeSameAs( p3Replacement );
            newP3PNoNameAdd.Right.Should().BeSameAs( pNoName );
        }
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
