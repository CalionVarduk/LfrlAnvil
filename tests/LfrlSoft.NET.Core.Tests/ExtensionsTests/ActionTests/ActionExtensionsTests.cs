using System;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ExtensionsTests.ActionTests
{
    public class ActionExtensionsTests : TestsBase
    {
        [Fact]
        public void TryInvoke_ShouldReturnResultWithoutError_WhenDelegateDoesntThrow()
        {
            Action action = () => { };
            var result = action.TryInvoke();
            result.IsOk.Should().BeTrue();
        }

        [Fact]
        public void Try_ShouldReturnResultWithError_WhenDelegateThrows()
        {
            var error = new Exception();
            Action action = () => throw error;

            var result = action.TryInvoke();

            using ( new AssertionScope() )
            {
                result.HasError.Should().BeTrue();
                result.Error.Should().Be( error );
            }
        }
    }
}
