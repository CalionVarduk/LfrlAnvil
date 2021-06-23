using System;
using FluentAssertions;
using LfrlSoft.NET.TestExtensions;

namespace LfrlSoft.NET.Common.Tests.Assert
{
    public abstract class AssertTestsBase : TestsBase
    {
        protected static void ShouldPass(Action action)
        {
            action.Should().NotThrow();
        }

        protected static void ShouldThrow(Action action)
        {
            ShouldThrow<ArgumentException>( action );
        }

        protected static void ShouldThrow<TException>(Action action)
            where TException : Exception
        {
            action.Should().Throw<TException>();
        }
    }
}
