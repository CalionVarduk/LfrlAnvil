using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.EventInfoTests;

public class EventInfoExtensionsTests : TestsBase
{
    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_WithoutIncludedDeclaringType()
    {
        var @event = typeof( TestEventClass ).GetEvent( nameof( TestEventClass.Foo ) )!;
        var result = @event.GetDebugString( includeDeclaringType: false );
        result.TestEquals( "System.EventHandler`1[TEventArgs is System.EventArgs] Foo [event]" ).Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_WithIncludedDeclaringType()
    {
        var @event = typeof( TestEventClass ).GetEvent( nameof( TestEventClass.Foo ) )!;
        var result = @event.GetDebugString( includeDeclaringType: true );

        result.TestEquals(
                "System.EventHandler`1[TEventArgs is System.EventArgs] LfrlAnvil.Tests.ExtensionsTests.EventInfoTests.TestEventClass.Foo [event]" )
            .Go();
    }
}

public sealed class TestEventClass
{
    public event EventHandler<EventArgs>? Foo;
}
