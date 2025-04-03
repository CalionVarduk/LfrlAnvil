using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client;
using LfrlAnvil.MessageBroker.Server;

namespace LfrlAnvil.MessageBroker.Core.Tests;

public class QueueTests : TestsBase
{
    [Fact]
    public async Task Server_ShouldAcceptSentMessages()
    {
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) ) );

        await server.StartAsync();

        await using var client = new MessageBrokerClient(
            server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client.StartAsync();

        var messageIds = new List<ulong?>();
        await client.Publishers.BindAsync( "foo" );
        var publisher = client.Publishers.TryGetByChannelId( 1 );
        if ( publisher is not null )
        {
            var result = await publisher.SendAsync( new byte[] { 1 } );
            messageIds.Add( result.Value.Id );
            result = await publisher.SendAsync( new byte[] { 2, 3 } );
            messageIds.Add( result.Value.Id );
            result = await publisher.SendAsync( new byte[] { 4, 5, 6 } );
            messageIds.Add( result.Value.Id );
        }

        messageIds.TestSequence( [ 0UL, 1UL, 2UL ] ).Go();
    }
}
