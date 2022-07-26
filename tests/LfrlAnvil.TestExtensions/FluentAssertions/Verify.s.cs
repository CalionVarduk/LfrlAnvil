using System;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Exceptions;

namespace LfrlAnvil.TestExtensions.FluentAssertions
{
    public static class Verify
    {
        public static void CallOrder(Action verification)
        {
            Action action = () => Received.InOrder( verification );
            action.Should().NotThrow<ReceivedCallsException>();
        }
    }
}
