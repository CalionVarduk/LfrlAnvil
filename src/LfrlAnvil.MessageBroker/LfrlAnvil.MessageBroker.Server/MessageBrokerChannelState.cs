namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Defines possible <see cref="MessageBrokerRemoteClient"/> states.
/// </summary>
public enum MessageBrokerChannelState : byte
{
    /// <summary>
    /// Specifies that the channel is currently running.
    /// </summary>
    Running = 0,

    /// <summary>
    /// Specifies the the channel is currently being disposed.
    /// </summary>
    Disposing = 1,

    /// <summary>
    /// Specifies that the channel has been disposed.
    /// </summary>
    Disposed = 2
}
