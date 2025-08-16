using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Tests.Helpers;

public sealed class ClientEventLogger
{
    private readonly Dictionary<ulong, List<string>> _traces = new Dictionary<ulong, List<string>>();
    private readonly List<string> _awaitPacket = new List<string>();

    public MessageBrokerRemoteClientLogger GetLogger(MessageBrokerRemoteClientLogger logger = default)
    {
        return MessageBrokerRemoteClientLogger.Create(
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
            serverTrace: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ServerTrace?.Invoke( e );
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
            publisherBound: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.PublisherBound?.Invoke( e );
            },
            unbindingPublisher: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.UnbindingPublisher?.Invoke( e );
            },
            publisherUnbound: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.PublisherUnbound?.Invoke( e );
            },
            bindingListener: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.BindingListener?.Invoke( e );
            },
            listenerBound: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ListenerBound?.Invoke( e );
            },
            unbindingListener: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.UnbindingListener?.Invoke( e );
            },
            listenerUnbound: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ListenerUnbound?.Invoke( e );
            },
            enqueueingRouting: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.EnqueueingRouting?.Invoke( e );
            },
            routingEnqueued: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.RoutingEnqueued?.Invoke( e );
            },
            pushingMessage: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.PushingMessage?.Invoke( e );
            },
            messagePushed: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.MessagePushed?.Invoke( e );
            },
            processingMessage: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ProcessingMessage?.Invoke( e );
            },
            messageProcessed: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.MessageProcessed?.Invoke( e );
            },
            processingAck: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ProcessingAck?.Invoke( e );
            },
            processingNegativeAck: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ProcessingNegativeAck?.Invoke( e );
            },
            ackProcessed: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.AckProcessed?.Invoke( e );
            },
            queryingDeadLetter: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.QueryingDeadLetter?.Invoke( e );
            },
            deadLetterQueried: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.DeadLetterQueried?.Invoke( e );
            },
            sendingSenderName: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.SendingSenderName?.Invoke( e );
            },
            sendingStreamName: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.SendingStreamName?.Invoke( e );
            },
            systemNotificationSent: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.SystemNotificationSent?.Invoke( e );
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

    [Pure]
    public TraceLog[] GetAll()
    {
        lock ( _traces )
            return _traces.OrderBy( kv => kv.Key ).Select( kv => new TraceLog( kv.Key, kv.Value.ToArray() ) ).ToArray();
    }

    [Pure]
    public string[] GetAllAwaitPacket()
    {
        lock ( _awaitPacket )
            return _awaitPacket.ToArray();
    }

    private void AddAwaitPacket(string e)
    {
        lock ( _awaitPacket )
            _awaitPacket.Add( e );
    }
}
