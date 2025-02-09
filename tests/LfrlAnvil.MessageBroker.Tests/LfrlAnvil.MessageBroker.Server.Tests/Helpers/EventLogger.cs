using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Tests.Helpers;

public sealed class EventLogger
{
    private readonly List<string> _events = new List<string>();
    private readonly List<string> _clientEvents = new List<string>();

    public void Add(MessageBrokerServerEvent e)
    {
        lock ( _events )
            _events.Add( e.ToString() );
    }

    public void Add(MessageBrokerRemoteClientEvent e)
    {
        lock ( _clientEvents )
            _clientEvents.Add( e.ToString() );
    }

    [Pure]
    public string[] GetAll()
    {
        lock ( _events )
            return _events.ToArray();
    }

    [Pure]
    public string[] GetAllClient()
    {
        lock ( _clientEvents )
            return _clientEvents.ToArray();
    }
}
