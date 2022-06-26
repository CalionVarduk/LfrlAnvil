using System;
using FluentAssertions;
using LfrlAnvil.TestExtensions;

namespace LfrlAnvil.Tests.EnsureTests;

public abstract class EnsureTestsBase : TestsBase
{
    protected static void ShouldPass(Action action)
    {
        action.Should().NotThrow();
    }

    protected static void ShouldThrowArgumentException(Action action)
    {
        ShouldThrowExactly<ArgumentException>( action );
    }

    protected static void ShouldThrowExactly<TException>(Action action)
        where TException : Exception
    {
        action.Should().ThrowExactly<TException>();
    }
}
