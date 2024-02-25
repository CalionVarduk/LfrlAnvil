using LfrlAnvil.Sql.Internal;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests;

public class TemporaryCommandTimeoutTests : TestsBase
{
    [Fact]
    public void Dispose_ShouldDoNothing_WhenNewTimeoutIsNull()
    {
        var command = new DbCommandMock { CommandTimeout = 123 };
        var sut = new TemporaryCommandTimeout( command, null );
        var timeout = command.CommandTimeout;

        sut.Dispose();

        using ( new AssertionScope() )
        {
            timeout.Should().Be( 123 );
            command.CommandTimeout.Should().Be( 123 );
        }
    }

    [Theory]
    [InlineData( 42.0, 42 )]
    [InlineData( 42.01, 43 )]
    [InlineData( 42.99, 43 )]
    public void Dispose_ShouldChangeTimeoutToPreviousValue_WhenNewTimeoutIsNotNull(double totalSeconds, int expectedTimeout)
    {
        var command = new DbCommandMock { CommandTimeout = 123 };
        var sut = new TemporaryCommandTimeout( command, TimeSpan.FromSeconds( totalSeconds ) );
        var timeout = command.CommandTimeout;

        sut.Dispose();

        using ( new AssertionScope() )
        {
            timeout.Should().Be( expectedTimeout );
            command.CommandTimeout.Should().Be( 123 );
        }
    }
}
