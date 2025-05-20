using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Tests.Helpers;

public sealed class EventLogger
{
    private readonly List<string> _clientEvents = new List<string>();
    private readonly List<string> _channelEvents = new List<string>();
    private readonly List<string> _streamEvents = new List<string>();
    private readonly List<string> _queueEvents = new List<string>();
    private readonly List<string> _publisherEvents = new List<string>();
    private readonly List<string> _listenerEvents = new List<string>();

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

    public void Add(MessageBrokerChannelPublisherBindingEvent e)
    {
        lock ( _publisherEvents )
            _publisherEvents.Add( e.ToString() );
    }

    public void Add(MessageBrokerChannelListenerBindingEvent e)
    {
        lock ( _listenerEvents )
            _listenerEvents.Add( e.ToString() );
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
    public string[] GetAllPublisher()
    {
        lock ( _publisherEvents )
            return _publisherEvents.ToArray();
    }

    [Pure]
    public string[] GetAllListener()
    {
        lock ( _listenerEvents )
            return _listenerEvents.ToArray();
    }
}
