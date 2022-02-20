using System;
using FluentAssertions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Functional.Tests.UnsafeTests
{
    public class UnsafeStaticTests : TestsBase
    {
        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Unsafe.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = Unsafe.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Unsafe<int> ), typeof( int ) )]
        [InlineData( typeof( Unsafe<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( Unsafe<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Unsafe.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( Unsafe<> ).GetGenericArguments()[0];

            var result = Unsafe.GetUnderlyingType( typeof( Unsafe<> ) );

            result.Should().Be( expected );
        }
    }
}
