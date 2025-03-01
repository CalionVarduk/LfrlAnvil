namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents a callback to <see cref="MessageBrokerChannelEvent"/> instances emitted by <see cref="MessageBrokerChannel"/>.
/// <param name="e">Emitted event.</param>
/// </summary>
public delegate void MessageBrokerChannelEventHandler(MessageBrokerChannelEvent e);
