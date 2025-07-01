using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Tests.Helpers;

public sealed class StreamEventLogger
{
    private readonly Dictionary<ulong, List<string>> _traces = new Dictionary<ulong, List<string>>();

    public MessageBrokerStreamLogger GetLogger(MessageBrokerStreamLogger logger = default)
    {
        return MessageBrokerStreamLogger.Create(
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
            clientTrace: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ClientTrace?.Invoke( e );
            },
            created: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.Created?.Invoke( e );
            },
            publisherBound: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.PublisherBound?.Invoke( e );
            },
            publisherUnbound: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.PublisherUnbound?.Invoke( e );
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
}
