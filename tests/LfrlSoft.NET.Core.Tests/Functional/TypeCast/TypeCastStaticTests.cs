﻿using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.TypeCast
{
    public class TypeCastStaticTests : TestsBase
    {
        [Fact]
        public void GetUnderlyingSourceType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Core.Functional.TypeCast.GetUnderlyingSourceType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingSourceType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = Core.Functional.TypeCast.GetUnderlyingSourceType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( TypeCast<int, string> ), typeof( int ) )]
        [InlineData( typeof( TypeCast<decimal, bool> ), typeof( decimal ) )]
        [InlineData( typeof( TypeCast<double, byte> ), typeof( double ) )]
        public void GetUnderlyingSourceType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Core.Functional.TypeCast.GetUnderlyingSourceType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingSourceType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( TypeCast<,> ).GetGenericArguments()[0];

            var result = Core.Functional.TypeCast.GetUnderlyingSourceType( typeof( TypeCast<,> ) );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingDestinationType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Core.Functional.TypeCast.GetUnderlyingDestinationType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingDestinationType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = Core.Functional.TypeCast.GetUnderlyingDestinationType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( TypeCast<int, string> ), typeof( string ) )]
        [InlineData( typeof( TypeCast<decimal, bool> ), typeof( bool ) )]
        [InlineData( typeof( TypeCast<double, byte> ), typeof( byte ) )]
        public void GetUnderlyingDestinationType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Core.Functional.TypeCast.GetUnderlyingDestinationType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingDestinationType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( TypeCast<,> ).GetGenericArguments()[1];

            var result = Core.Functional.TypeCast.GetUnderlyingDestinationType( typeof( TypeCast<,> ) );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingTypes_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Core.Functional.TypeCast.GetUnderlyingTypes( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingTypes_ShouldReturnNull_WhenTypeIsNotCorrect(Type type)
        {
            var result = Core.Functional.TypeCast.GetUnderlyingTypes( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( TypeCast<int, string> ), typeof( int ), typeof( string ) )]
        [InlineData( typeof( TypeCast<decimal, bool> ), typeof( decimal ), typeof( bool ) )]
        [InlineData( typeof( TypeCast<double, byte> ), typeof( double ), typeof( byte ) )]
        public void GetUnderlyingTypes_ShouldReturnCorrectTypes_WhenTypeIsCorrect(Type type, Type expectedFirst, Type expectedSecond)
        {
            var result = Core.Functional.TypeCast.GetUnderlyingTypes( type );

            result.Should().BeEquivalentTo( new Pair<Type, Type>( expectedFirst, expectedSecond ) );
        }

        [Fact]
        public void GetUnderlyingTypes_ShouldReturnCorrectTypes_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( TypeCast<,> ).GetGenericArguments();

            var result = Core.Functional.TypeCast.GetUnderlyingTypes( typeof( TypeCast<,> ) );

            result.Should().BeEquivalentTo( new Pair<Type, Type>( expected[0], expected[1] ) );
        }
    }
}
