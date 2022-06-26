using System;
using FluentAssertions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Functional.Tests.TypeCastTests;

public class TypeCastStaticTests : TestsBase
{
    [Fact]
    public void GetUnderlyingSourceType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = TypeCast.GetUnderlyingSourceType( null );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingSourceType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = TypeCast.GetUnderlyingSourceType( type );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( typeof( TypeCast<int, string> ), typeof( int ) )]
    [InlineData( typeof( TypeCast<decimal, bool> ), typeof( decimal ) )]
    [InlineData( typeof( TypeCast<double, byte> ), typeof( double ) )]
    public void GetUnderlyingSourceType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = TypeCast.GetUnderlyingSourceType( type );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetUnderlyingSourceType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( TypeCast<,> ).GetGenericArguments()[0];

        var result = TypeCast.GetUnderlyingSourceType( typeof( TypeCast<,> ) );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetUnderlyingDestinationType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = TypeCast.GetUnderlyingDestinationType( null );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingDestinationType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = TypeCast.GetUnderlyingDestinationType( type );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( typeof( TypeCast<int, string> ), typeof( string ) )]
    [InlineData( typeof( TypeCast<decimal, bool> ), typeof( bool ) )]
    [InlineData( typeof( TypeCast<double, byte> ), typeof( byte ) )]
    public void GetUnderlyingDestinationType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = TypeCast.GetUnderlyingDestinationType( type );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetUnderlyingDestinationType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( TypeCast<,> ).GetGenericArguments()[1];

        var result = TypeCast.GetUnderlyingDestinationType( typeof( TypeCast<,> ) );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetUnderlyingTypes_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = TypeCast.GetUnderlyingTypes( null );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingTypes_ShouldReturnNull_WhenTypeIsNotCorrect(Type type)
    {
        var result = TypeCast.GetUnderlyingTypes( type );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( typeof( TypeCast<int, string> ), typeof( int ), typeof( string ) )]
    [InlineData( typeof( TypeCast<decimal, bool> ), typeof( decimal ), typeof( bool ) )]
    [InlineData( typeof( TypeCast<double, byte> ), typeof( double ), typeof( byte ) )]
    public void GetUnderlyingTypes_ShouldReturnCorrectTypes_WhenTypeIsCorrect(Type type, Type expectedFirst, Type expectedSecond)
    {
        var result = TypeCast.GetUnderlyingTypes( type );

        result.Should().BeEquivalentTo( new Pair<Type, Type>( expectedFirst, expectedSecond ) );
    }

    [Fact]
    public void GetUnderlyingTypes_ShouldReturnCorrectTypes_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( TypeCast<,> ).GetGenericArguments();

        var result = TypeCast.GetUnderlyingTypes( typeof( TypeCast<,> ) );

        result.Should().BeEquivalentTo( new Pair<Type, Type>( expected[0], expected[1] ) );
    }
}