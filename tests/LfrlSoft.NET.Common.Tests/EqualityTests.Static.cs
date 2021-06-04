﻿using System;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class EqualityTests
    {
        [Fact]
        public void Create_ShouldCreateWithCorrectProperties()
        {
            var value1 = _fixture.Create<int>();
            var value2 = _fixture.Create<int>();

            var sut = Equality.Create( value1, value2 );

            sut.Should().Match<Equality<int>>( e => e.First == value1 && e.Second == value2 );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Equality.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNotCorrect(Type type)
        {
            var result = Equality.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Equality<int> ), typeof( int ) )]
        [InlineData( typeof( Equality<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( Equality<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Equality.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( Equality<> ).GetGenericArguments()[0];

            var result = Equality.GetUnderlyingType( typeof( Equality<> ) );

            result.Should().Be( expected );
        }
    }
}
