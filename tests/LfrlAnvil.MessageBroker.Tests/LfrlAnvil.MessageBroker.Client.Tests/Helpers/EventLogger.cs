using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.MessageBroker.Client.Events;

namespace LfrlAnvil.MessageBroker.Client.Tests.Helpers;

public sealed class EventLogger
{
    private readonly List<string> _events = new List<string>();

    public void Add(MessageBrokerClientEvent e)
    {
        lock ( _events )
            _events.Add( e.ToString() );
    }

    [Pure]
    public string[] GetAll()
    {
        lock ( _events )
            return _events.ToArray();
    }
}
