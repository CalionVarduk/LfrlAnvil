﻿using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Collections;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Collections.Many
{
    public abstract class ManyTests<T> : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateCorrectMany()
        {
            var values = Fixture.CreateMany<T>().ToArray();

            var sut = Common.Collections.Many.Create( values );

            sut.Should().BeEquivalentTo( values );
        }

        [Fact]
        public void Ctor_ShouldCreateWithCorrectValues()
        {
            var values = Fixture.CreateMany<T>().ToArray();

            var sut = new Many<T>( values );

            sut.Should().BeEquivalentTo( values );
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
        public void GetIndexer_ShouldThrowWhenIndexIsOutOfRange(int index)
        {
            var values = Fixture.CreateMany<T>( 3 ).ToArray();
            var sut = new Many<T>( values );

            Action action = () =>
            {
                var _ = sut[index];
            };

            action.Should().Throw<IndexOutOfRangeException>();
        }
    }
}
