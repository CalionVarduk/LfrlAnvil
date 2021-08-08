using FluentAssertions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.Nil
{
    public class NilTests : TestsBase
    {
        [Fact]
        public void GetHashCode_ShouldReturnZero()
        {
            var sut = Core.Functional.Nil.Instance;
            var result = sut.GetHashCode();
            result.Should().Be( 0 );
        }

        [Fact]
        public void Equals_ShouldReturnTrue()
        {
            var a = Core.Functional.Nil.Instance;
            var b = Core.Functional.Nil.Instance;
            var result = a.Equals( b );
            result.Should().BeTrue();
        }

        [Fact]
        public void CompareTo_ShouldReturnZero()
        {
            var a = Core.Functional.Nil.Instance;
            var b = Core.Functional.Nil.Instance;
            var result = a.CompareTo( b );
            result.Should().Be( 0 );
        }

        [Fact]
        public void EqualityOperator_ShouldReturnTrue()
        {
            var a = Core.Functional.Nil.Instance;
            var b = Core.Functional.Nil.Instance;
            var result = a == b;
            result.Should().BeTrue();
        }

        [Fact]
        public void InequalityOperator_ShouldReturnFalse()
        {
            var a = Core.Functional.Nil.Instance;
            var b = Core.Functional.Nil.Instance;
            var result = a != b;
            result.Should().BeFalse();
        }

        [Fact]
        public void LessThanOrEqualToOperator_ShouldReturnTrue()
        {
            var a = Core.Functional.Nil.Instance;
            var b = Core.Functional.Nil.Instance;
            var result = a <= b;
            result.Should().BeTrue();
        }

        [Fact]
        public void GreaterThanOperator_ShouldReturnFalse()
        {
            var a = Core.Functional.Nil.Instance;
            var b = Core.Functional.Nil.Instance;
            var result = a > b;
            result.Should().BeFalse();
        }

        [Fact]
        public void GreaterThanOrEqualToOperator_ShouldReturnTrue()
        {
            var a = Core.Functional.Nil.Instance;
            var b = Core.Functional.Nil.Instance;
            var result = a >= b;
            result.Should().BeTrue();
        }

        [Fact]
        public void LessThanOperator_ShouldReturnFalse()
        {
            var a = Core.Functional.Nil.Instance;
            var b = Core.Functional.Nil.Instance;
            var result = a < b;
            result.Should().BeFalse();
        }
    }
}
