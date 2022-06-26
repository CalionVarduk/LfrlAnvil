using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Tests.ManyTests;

public abstract class GenericManyTests<T> : TestsBase
{
    [Fact]
    public void Create_ShouldCreateCorrectMany()
    {
        var values = Fixture.CreateMany<T>().ToArray();
        var sut = Many.Create( values );
        sut.Should().BeSequentiallyEqualTo( values );
    }

    [Fact]
    public void Ctor_ShouldCreateWithCorrectValues()
    {
        var values = Fixture.CreateMany<T>().ToArray();
        var sut = new Many<T>( values );
        sut.Should().BeSequentiallyEqualTo( values );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void Count_ShouldReturnCorrectResult(int count)
    {
        var values = Fixture.CreateMany<T>( count ).ToArray();
        var sut = new Many<T>( values );
        sut.Count.Should().Be( count );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    public void GetIndexer_ShouldReturnCorrectResultWhenIndexIsValid(int index)
    {
        var values = Fixture.CreateMany<T>( 3 ).ToArray();
        var sut = new Many<T>( values );

        var result = sut[index];

        result.Should().Be( values[index] );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void GetIndexer_ShouldThrowIndexOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var values = Fixture.CreateMany<T>( 3 ).ToArray();
        var sut = new Many<T>( values );

        var action = Lambda.Of( () => sut[index] );

        action.Should().ThrowExactly<IndexOutOfRangeException>();
    }
}
