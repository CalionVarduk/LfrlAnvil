using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Tests.Helpers;

public sealed class EventLogger
{
    private readonly List<string> _streamEvents = new List<string>();
    private readonly List<string> _queueEvents = new List<string>();

    public void Add(MessageBrokerStreamEvent e)
    {
        lock ( _streamEvents )
            _streamEvents.Add( e.ToString() );
    }

    public void Add(MessageBrokerQueueEvent e)
    {
        lock ( _queueEvents )
            _queueEvents.Add( e.ToString() );
    }

    [Pure]
    public string[] GetAllStream()
    {
        lock ( _streamEvents )
            return _streamEvents.ToArray();
    }

    [Pure]
    public string[] GetAllQueue()
    {
        lock ( _queueEvents )
            return _queueEvents.ToArray();
    }
}
