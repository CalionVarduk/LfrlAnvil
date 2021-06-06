using System;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Pair
{
    public class Static
    {
        [Fact]
        public void GetUnderlyingFirstType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Common.Pair.GetUnderlyingFirstType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingFirstType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = Common.Pair.GetUnderlyingFirstType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Pair<int, string> ), typeof( int ) )]
        [InlineData( typeof( Pair<decimal, bool> ), typeof( decimal ) )]
        [InlineData( typeof( Pair<double, byte> ), typeof( double ) )]
        public void GetUnderlyingFirstType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Common.Pair.GetUnderlyingFirstType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingFirstType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( Pair<,> ).GetGenericArguments()[0];

            var result = Common.Pair.GetUnderlyingFirstType( typeof( Pair<,> ) );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingSecondType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Common.Pair.GetUnderlyingSecondType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingSecondType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = Common.Pair.GetUnderlyingSecondType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Pair<int, string> ), typeof( string ) )]
        [InlineData( typeof( Pair<decimal, bool> ), typeof( bool ) )]
        [InlineData( typeof( Pair<double, byte> ), typeof( byte ) )]
        public void GetUnderlyingSecondType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Common.Pair.GetUnderlyingSecondType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingSecondType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( Pair<,> ).GetGenericArguments()[1];

            var result = Common.Pair.GetUnderlyingSecondType( typeof( Pair<,> ) );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingTypes_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Common.Pair.GetUnderlyingTypes( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingTypes_ShouldReturnNull_WhenTypeIsNotCorrect(Type type)
        {
            var result = Common.Pair.GetUnderlyingTypes( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Pair<int, string> ), typeof( int ), typeof( string ) )]
        [InlineData( typeof( Pair<decimal, bool> ), typeof( decimal ), typeof( bool ) )]
        [InlineData( typeof( Pair<double, byte> ), typeof( double ), typeof( byte ) )]
        public void GetUnderlyingTypes_ShouldReturnCorrectTypes_WhenTypeIsCorrect(Type type, Type expectedFirst, Type expectedSecond)
        {
            var result = Common.Pair.GetUnderlyingTypes( type );

            result.Should().BeEquivalentTo( new Pair<Type, Type>( expectedFirst, expectedSecond ) );
        }

        [Fact]
        public void GetUnderlyingTypes_ShouldReturnCorrectTypes_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( Pair<,> ).GetGenericArguments();

            var result = Common.Pair.GetUnderlyingTypes( typeof( Pair<,> ) );

            result.Should().BeEquivalentTo( new Pair<Type, Type>( expected[0], expected[1] ) );
        }
    }
}
