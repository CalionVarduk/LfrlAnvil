using System;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Tests.OneTests
{
    public abstract class GenericOneTests<T> : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateCorrectOne()
        {
            var value = Fixture.Create<T>();
            var sut = One.Create( value );
            sut.Value.Should().Be( value );
        }

        [Fact]
        public void Ctor_ShouldCreateWithCorrectValue()
        {
            var value = Fixture.Create<T>();
            var sut = new One<T>( value );
            sut.Value.Should().Be( value );
        }

        [Fact]
        public void Count_ShouldReturnOne()
        {
            var value = Fixture.Create<T>();
            var sut = new One<T>( value );
            sut.Count.Should().Be( 1 );
        }

        [Fact]
        public void GetIndexer_ShouldReturnValueWhenIndexIsEqualToZero()
        {
            var value = Fixture.Create<T>();
            var sut = new One<T>( value );

            var result = sut[0];

            result.Should().Be( value );
        }

        [Theory]
        [InlineData( -1 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        public void GetIndexer_ShouldThrowIndexOutOfRangeException_WhenIndexIsNotEqualToZero(int index)
        {
            var value = Fixture.Create<T>();
            var sut = new One<T>( value );

            var action = Lambda.Of( () => sut[index] );

            action.Should().ThrowExactly<IndexOutOfRangeException>();
        }

        [Fact]
        public void GetEnumerator_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<T>();
            var sut = new One<T>( value );
            sut.Should().BeEquivalentTo( new[] { value } );
        }
    }
}
