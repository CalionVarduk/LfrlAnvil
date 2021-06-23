using System;
using LfrlSoft.NET.TestExtensions;
using Xunit;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Extensions;

namespace LfrlSoft.NET.Common.Tests.Extensions.Func
{
    public abstract class ParameterlessFuncExtensionsTests<TReturnValue> : TestsBase
    {
        [Fact]
        public void ToLazy_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<TReturnValue>();
            Func<TReturnValue> sut = () => value;

            var result = sut.ToLazy();

            result.Value.Should().Be( value );
        }
    }
}
