﻿using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ExtensionsTests.ObjectTests
{
    public abstract class GenericObjectExtensionsOfRefTypeTests<T> : GenericObjectExtensionsOfComparableTypeTests<T>
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
}