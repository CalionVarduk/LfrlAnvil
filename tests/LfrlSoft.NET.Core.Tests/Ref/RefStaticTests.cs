using System;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Ref
{
    public class RefStaticTests
    {
        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Core.Ref.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = Core.Ref.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Ref<int> ), typeof( int ) )]
        [InlineData( typeof( Ref<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( Ref<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Core.Ref.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( Ref<> ).GetGenericArguments()[0];

            var result = Core.Ref.GetUnderlyingType( typeof( Ref<> ) );

            result.Should().Be( expected );
        }
    }
}
