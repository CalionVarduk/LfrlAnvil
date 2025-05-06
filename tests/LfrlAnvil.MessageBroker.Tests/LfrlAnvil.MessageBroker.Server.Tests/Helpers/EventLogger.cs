using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Tests.Helpers;

public sealed class EventLogger
{
    private readonly List<string> _serverEvents = new List<string>();
    private readonly List<string> _clientEvents = new List<string>();
    private readonly List<string> _channelEvents = new List<string>();
    private readonly List<string> _streamEvents = new List<string>();
    private readonly List<string> _queueEvents = new List<string>();
    private readonly List<string> _bindingEvents = new List<string>();
    private readonly List<string> _subscriptionEvents = new List<string>();

    public void Add(MessageBrokerServerEvent e)
    {
        lock ( _serverEvents )
            _serverEvents.Add( e.ToString() );
    }

    public void Add(MessageBrokerRemoteClientEvent e)
    {
        lock ( _clientEvents )
            _clientEvents.Add( e.ToString() );
    }

    public void Add(MessageBrokerChannelEvent e)
    {
        lock ( _channelEvents )
            _channelEvents.Add( e.ToString() );
    }

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

    public void Add(MessageBrokerChannelBindingEvent e)
    {
        lock ( _bindingEvents )
            _bindingEvents.Add( e.ToString() );
    }

    public void Add(MessageBrokerSubscriptionEvent e)
    {
        lock ( _subscriptionEvents )
            _subscriptionEvents.Add( e.ToString() );
    }

    [Pure]
    public string[] GetAllServer()
    {
        lock ( _serverEvents )
            return _serverEvents.ToArray();
    }

    [Pure]
    public string[] GetAllClient()
    {
        lock ( _clientEvents )
            return _clientEvents.ToArray();
    }

    [Pure]
    public string[] GetAllChannel()
    {
        lock ( _channelEvents )
            return _channelEvents.ToArray();
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

    [Pure]
    public string[] GetAllBinding()
    {
        lock ( _bindingEvents )
            return _bindingEvents.ToArray();
    }

    [Pure]
    public string[] GetAllSubscription()
    {
        lock ( _subscriptionEvents )
            return _subscriptionEvents.ToArray();
    }
}
