﻿using System;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Core.Collections;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Collections.One
{
    public abstract class GenericOneTests<T> : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateCorrectOne()
        {
            var value = Fixture.Create<T>();

            var sut = Core.Collections.One.Create( value );

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
        public void GetIndexer_ShouldThrowWhenIndexIsNotEqualToZero(int index)
        {
            var value = Fixture.Create<T>();
            var sut = new One<T>( value );

            Action action = () =>
            {
                var _ = sut[index];
            };

            action.Should().Throw<ArgumentException>();
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