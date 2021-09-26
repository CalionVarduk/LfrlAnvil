using FluentAssertions;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.Core.Functional.Extensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.Unsafe
{
    public abstract class GenericUnsafeExtensionsOfRefTypeTests<T> : GenericUnsafeExtensionsTests<T>
        where T : class
    {
        [Fact]
        public void ToMaybe_ShouldReturnWithoutValue_WhenHasNullValue()
        {
            var value = default( T );
            var sut = (Unsafe<T>)value!;

            var result = sut.ToMaybe();

            result.HasValue.Should().BeFalse();
        }
    }
}
