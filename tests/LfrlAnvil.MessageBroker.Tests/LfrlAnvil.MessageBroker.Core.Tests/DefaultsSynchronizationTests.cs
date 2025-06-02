using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Core.Tests;

public class DefaultsSynchronizationTests : TestsBase
{
    [Fact]
    public void ClientEndpoint()
    {
        var client = Enum.GetValues<MessageBrokerClientEndpoint>()
            .Order()
            .Select( static v => KeyValuePair.Create( v.ToString(), ( int )v ) );

        var server = Enum.GetValues<Server.Events.MessageBrokerClientEndpoint>()
            .Order()
            .Select( static v => KeyValuePair.Create( v.ToString(), ( int )v ) );

        client.TestSequence( server ).Go();
    }

    [Fact]
    public void ServerEndpoint()
    {
        var client = Enum.GetValues<MessageBrokerServerEndpoint>()
            .Order()
            .Select( static v => KeyValuePair.Create( v.ToString(), ( int )v ) );

        var server = Enum.GetValues<Server.Events.MessageBrokerServerEndpoint>()
            .Order()
            .Select( static v => KeyValuePair.Create( v.ToString(), ( int )v ) );

        client.TestSequence( server ).Go();
    }

    [Fact]
    public void SystemNotificationType()
    {
        var client = Enum.GetValues<MessageBrokerSystemNotificationType>()
            .Order()
            .Select( static v => KeyValuePair.Create( v.ToString(), ( int )v ) );

        var server = Enum.GetValues<Server.Events.MessageBrokerSystemNotificationType>()
            .Order()
            .Select( static v => KeyValuePair.Create( v.ToString(), ( int )v ) );

        client.TestSequence( server ).Go();
    }

    [Fact]
    public void ClientNameLength()
    {
        var client = Defaults.NameLengthBounds;
        var server = Server.Internal.Defaults.NameLengthBounds;
        client.TestEquals( server ).Go();
    }

    [Fact]
    public void Delay()
    {
        var client = Defaults.Temporal.Delay;
        var server = Server.Internal.Defaults.Temporal.Delay;
        client.TestEquals( server ).Go();
    }

    [Fact]
    public void MaxTimeout()
    {
        var client = Defaults.Temporal.MaxTimeout;
        var server = Server.Internal.Defaults.Temporal.MaxTimeout;
        client.TestEquals( server ).Go();
    }

    [Fact]
    public void TimeoutBounds()
    {
        var client = Defaults.Temporal.TimeoutBounds;
        var server = Server.Internal.Defaults.Temporal.TimeoutBounds;
        client.TestEquals( server ).Go();
    }

    [Fact]
    public void PingIntervalBounds()
    {
        var client = Defaults.Temporal.PingIntervalBounds;
        var server = Server.Internal.Defaults.Temporal.PingIntervalBounds;
        client.TestEquals( server ).Go();
    }

    [Fact]
    public void EndiannessVerificationPayload()
    {
        var client = Protocol.Endianness.VerificationPayload;
        var server = Server.Internal.Protocol.Endianness.VerificationPayload;
        client.TestEquals( server ).Go();
    }

    [Fact]
    public void TextEncoding()
    {
        var client = Client.Internal.TextEncoding.Instance;
        var server = Server.Internal.TextEncoding.Instance;
        client.TestEquals( server ).Go();
    }
}
