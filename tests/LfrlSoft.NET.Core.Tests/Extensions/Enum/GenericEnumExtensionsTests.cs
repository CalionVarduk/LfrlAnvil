﻿using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.Enum
{
    public abstract class GenericEnumExtensionsTests<T> : TestsBase
        where T : struct, System.Enum
    {
        [Fact]
        public void ToBitmask_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<T>();
            var result = value.ToBitmask();
            result.Value.Should().Be( value );
        }
    }
}
