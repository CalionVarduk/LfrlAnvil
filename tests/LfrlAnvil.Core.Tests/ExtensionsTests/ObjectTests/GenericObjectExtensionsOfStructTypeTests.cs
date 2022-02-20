using System;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Extensions;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.ObjectTests
{
    public abstract class GenericObjectExtensionsOfStructTypeTests<T> : GenericObjectExtensionsOfComparableTypeTests<T>
        where T : struct, IComparable<T>
    {
        [Fact]
        public void ToNullable_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<T>();
            var result = value.ToNullable();
            result.Should().BeEquivalentTo( value );
        }
    }
}
