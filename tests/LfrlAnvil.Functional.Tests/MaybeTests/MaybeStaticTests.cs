using System;
using FluentAssertions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Functional.Tests.MaybeTests
{
    public class MaybeStaticTests : TestsBase
    {
        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Maybe.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = Maybe.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Maybe<int> ), typeof( int ) )]
        [InlineData( typeof( Maybe<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( Maybe<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Maybe.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( Maybe<> ).GetGenericArguments()[0];

            var result = Maybe.GetUnderlyingType( typeof( Maybe<> ) );

            result.Should().Be( expected );
        }
    }
}
