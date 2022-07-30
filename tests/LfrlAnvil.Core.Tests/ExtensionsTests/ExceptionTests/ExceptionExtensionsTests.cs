using System;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.ExceptionTests;

public class ExceptionExtensionsTests : TestsBase
{
    [Fact]
    public void Rethrow_ShouldThrowExceptionWithOriginalStackTrace()
    {
        var exception = Unsafe.Try( () => throw new Exception() ).GetError();
        var originalStackTrace = exception.StackTrace!.Split( Environment.NewLine );

        var result = Unsafe.Try( () => exception.Rethrow() );

        using ( new AssertionScope() )
        {
            result.HasError.Should().BeTrue();
            result.GetError().Should().BeSameAs( exception );
            result.GetError().StackTrace!.Split( Environment.NewLine ).Should().ContainInOrder( originalStackTrace );
        }
    }
}
