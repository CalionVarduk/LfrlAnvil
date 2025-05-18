using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using LfrlAnvil.MessageBroker.Client.Events;

namespace LfrlAnvil.MessageBroker.Client.Tests.Helpers;

public sealed class EventLogger
{
    private readonly Dictionary<ulong, List<string>> _traces = new Dictionary<ulong, List<string>>();
    private readonly List<string> _awaitPacket = new List<string>();

    public MessageBrokerClientLogger GetLogger(MessageBrokerClientLogger logger = default)
    {
        return MessageBrokerClientLogger.Create(
            traceStart: e =>
            {
                Add( e.Source.TraceId, $"{e} (start)" );
                logger.TraceStart?.Invoke( e );
            },
            traceEnd: e =>
            {
                Add( e.Source.TraceId, $"{e} (end)" );
                logger.TraceEnd?.Invoke( e );
            },
            connecting: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.Connecting?.Invoke( e );
            },
            connected: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.Connected?.Invoke( e );
            },
            handshaking: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.Handshaking?.Invoke( e );
            },
            handshakeEstablished: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.HandshakeEstablished?.Invoke( e );
            },
            awaitPacket: e =>
            {
                AddAwaitPacket( e.ToString() );
                logger.AwaitPacket?.Invoke( e );
            },
            sendPacket: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.SendPacket?.Invoke( e );
            },
            readPacket: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ReadPacket?.Invoke( e );
            },
            bindingPublisher: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.BindingPublisher?.Invoke( e );
            },
            publisherChange: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.PublisherChange?.Invoke( e );
            },
            bindingListener: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.BindingListener?.Invoke( e );
            },
            listenerChange: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ListenerChange?.Invoke( e );
            },
            messagePushing: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.MessagePushing?.Invoke( e );
            },
            messagePushed: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.MessagePushed?.Invoke( e );
            },
            messageProcessing: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.MessageProcessing?.Invoke( e );
            },
            messageProcessed: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.MessageProcessed?.Invoke( e );
            },
            disposing: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.Disposing?.Invoke( e );
            },
            disposed: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.Disposed?.Invoke( e );
            },
            error: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.Error?.Invoke( e );
            } );
    }

    public void Add(ulong traceId, string e)
    {
        lock ( _traces )
        {
            ref var events = ref CollectionsMarshal.GetValueRefOrAddDefault( _traces, traceId, out var exists )!;
            if ( ! exists )
                events = new List<string>();

            events.Add( e );
        }
    }

    private void AddAwaitPacket(string e)
    {
        lock ( _awaitPacket )
            _awaitPacket.Add( e );
    }

    [Pure]
    public string[][] GetAll()
    {
        lock ( _traces )
            return _traces.OrderBy( kv => kv.Key ).Select( kv => kv.Value.ToArray() ).ToArray();
    }

    [Pure]
    public string[] GetAllAwaitPacket()
    {
        lock ( _awaitPacket )
            return _awaitPacket.ToArray();
    }
}
