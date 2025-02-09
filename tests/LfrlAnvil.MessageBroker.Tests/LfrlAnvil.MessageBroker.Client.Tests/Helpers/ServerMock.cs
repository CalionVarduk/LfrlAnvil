using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client.Tests.Helpers;

public sealed class ServerMock : IDisposable
{
    private readonly TcpListener _listener = new TcpListener( IPAddress.Loopback, 0 );
    private readonly List<byte[]> _received = new List<byte[]>();
    private TcpClient? _client;

    public void Dispose()
    {
        lock ( _listener )
        {
            _client?.TryDispose();
            _client = null;
            _listener.Stop();
        }
    }

    public IPEndPoint Start()
    {
        lock ( _listener )
        {
            _listener.Start();
            return ( IPEndPoint )_listener.LocalEndpoint;
        }
    }

    public void WaitForClient()
    {
        lock ( _listener )
        {
            _client = _listener.AcceptTcpClient();
            _client.ReceiveTimeout = ChronoConstants.MillisecondsPerSecond;
            _client.SendTimeout = ChronoConstants.MillisecondsPerSecond;
            _client.NoDelay = true;
            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
                _client.Client.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );
        }
    }

    public byte[] Read(int length)
    {
        lock ( _listener )
        {
            Assume.IsNotNull( _client );
            var buffer = new byte[length];
            _client.GetStream().ReadExactly( buffer );
            _received.Add( buffer );
            return buffer;
        }
    }

    public void SendHandshakeAccepted(int id, Duration messageTimeout, Duration pingInterval, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.HandshakeAcceptedResponse );
        writer.MoveWrite( payload ?? Protocol.HandshakeAcceptedResponse.Length );
        writer.MoveWrite( ( byte )(BitConverter.IsLittleEndian ? 1 : 0) );
        writer.MoveWrite( ( uint )id );
        writer.MoveWrite( ( uint )messageTimeout.FullMilliseconds );
        writer.Write( ( uint )pingInterval.FullMilliseconds );
        Send( buffer );
    }

    public void SendHandshakeRejected(bool invalidNameLength, bool nameDecodingFailure, bool nameAlreadyExists, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.HandshakeRejectedResponse );
        writer.MoveWrite( payload ?? Protocol.HandshakeRejectedResponse.Length );
        writer.Write( ( byte )((invalidNameLength ? 1 : 0) | (nameDecodingFailure ? 2 : 0) | (nameAlreadyExists ? 4 : 0)) );
        Send( buffer );
    }

    public void SendPing(uint? endiannessPayload = null, uint? payload = null)
    {
        var buffer = new byte[Protocol.PacketHeader.Length];
        var writer = new BinaryContractWriter( buffer );
        writer.MoveWrite( ( byte )MessageBrokerClientEndpoint.PingResponse );
        writer.Write( payload ?? Protocol.PacketHeader.Length );
        writer.Write( endiannessPayload ?? Protocol.Endianness.VerificationPayload );
        Send( buffer );
    }

    public void Send(byte[] data)
    {
        lock ( _listener )
        {
            Assume.IsNotNull( _client );
            _client.GetStream().Write( data );
        }
    }

    [Pure]
    public byte[][] GetAllReceived()
    {
        lock ( _listener )
            return _received.ToArray();
    }
}
