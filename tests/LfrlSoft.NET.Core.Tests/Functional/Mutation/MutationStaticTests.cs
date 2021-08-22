using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.Mutation
{
    public class MutationStaticTests : TestsBase
    {
        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Core.Functional.Mutation.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = Core.Functional.Mutation.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Mutation<int> ), typeof( int ) )]
        [InlineData( typeof( Mutation<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( Mutation<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Core.Functional.Mutation.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( Mutation<> ).GetGenericArguments()[0];

            var result = Core.Functional.Mutation.GetUnderlyingType( typeof( Mutation<> ) );

            result.Should().Be( expected );
        }
    }
}
