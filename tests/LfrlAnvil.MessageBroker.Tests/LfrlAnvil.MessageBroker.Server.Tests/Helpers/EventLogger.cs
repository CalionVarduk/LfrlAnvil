using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Tests.Helpers;

public sealed class EventLogger
{
    private readonly List<string> _queueEvents = new List<string>();

    public void Add(MessageBrokerQueueEvent e)
    {
        lock ( _queueEvents )
            _queueEvents.Add( e.ToString() );
    }

    [Pure]
    public string[] GetAllQueue()
    {
        lock ( _queueEvents )
            return _queueEvents.ToArray();
    }
}
