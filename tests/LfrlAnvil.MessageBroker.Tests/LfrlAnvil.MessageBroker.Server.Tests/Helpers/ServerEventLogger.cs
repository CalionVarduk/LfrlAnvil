using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Tests.Helpers;

public sealed class ServerEventLogger
{
    private readonly Dictionary<ulong, List<string>> _traces = new Dictionary<ulong, List<string>>();
    private readonly List<string> _awaitClient = new List<string>();

    public MessageBrokerServerLogger GetLogger(MessageBrokerServerLogger logger = default)
    {
        return MessageBrokerServerLogger.Create(
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
            listenerStarting: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ListenerStarting?.Invoke( e );
            },
            listenerStarted: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ListenerStarted?.Invoke( e );
            },
            awaitClient: e =>
            {
                AddAwaitClient( e.ToString() );
                logger.AwaitClient?.Invoke( e );
            },
            clientAccepted: e =>
            {
                Add( e.Source.TraceId, e.ToString() );
                logger.ClientAccepted?.Invoke( e );
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
    public string[][] GetAll()
    {
        lock ( _traces )
            return _traces.OrderBy( kv => kv.Key ).Select( kv => kv.Value.ToArray() ).ToArray();
    }

    [Pure]
    public string[] GetAllAwaitClient()
    {
        lock ( _awaitClient )
            return _awaitClient.ToArray();
    }

    private void AddAwaitClient(string e)
    {
        lock ( _awaitClient )
            _awaitClient.Add( e );
    }
}
