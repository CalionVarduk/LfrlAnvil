﻿using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.Unsafe
{
    public class UnsafeStaticTests : TestsBase
    {
        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Core.Functional.Unsafe.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = Core.Functional.Unsafe.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Unsafe<int> ), typeof( int ) )]
        [InlineData( typeof( Unsafe<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( Unsafe<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Core.Functional.Unsafe.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( Unsafe<> ).GetGenericArguments()[0];

            var result = Core.Functional.Unsafe.GetUnderlyingType( typeof( Unsafe<> ) );

            result.Should().Be( expected );
        }
    }
}