using System;
using FluentAssertions;
using LfrlAnvil.Functional.Extensions;
using Xunit;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.ObjectTests;

public abstract class GenericObjectExtensionsOfRefTypeTests<T> : GenericObjectExtensionsTests<T>
    where T : class, IComparable<T>
{
    [Fact]
    public void ToMaybe_ShouldReturnCorrectResult_WhenNull()
    {
        var value = default( T );
        var sut = value.ToMaybe();
        sut.HasValue.Should().BeFalse();
    }
}
