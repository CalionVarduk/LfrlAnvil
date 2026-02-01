using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Tests.Helpers;

public sealed class QueueEventLogger
{
    private readonly Dictionary<ulong, List<string>> _traces = new Dictionary<ulong, List<string>>();

    public MessageBrokerQueueLogger GetLogger(MessageBrokerQueueLogger logger = default)
    {
        return MessageBrokerQueueLogger.Create(
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
            clientTrace: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ClientTrace?.Invoke( e );
            },
            streamTrace: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.StreamTrace?.Invoke( e );
            },
            created: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.Created?.Invoke( e );
            },
            listenerBound: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ListenerBound?.Invoke( e );
            },
            listenerUnbound: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ListenerUnbound?.Invoke( e );
            },
            enqueueingMessage: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.EnqueueingMessage?.Invoke( e );
            },
            messageEnqueued: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.MessageEnqueued?.Invoke( e );
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
            messageDiscarded: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.MessageDiscarded?.Invoke( e );
            },
            ackProcessed: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.AckProcessed?.Invoke( e );
            },
            negativeAckProcessed: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.NegativeAckProcessed?.Invoke( e );
            },
            deactivating: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.Deactivating?.Invoke( e );
            },
            deactivated: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.Deactivated?.Invoke( e );
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
}
