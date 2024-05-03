using System;

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents an error that occurred due to failed <see cref="IDependencyContainer"/> build attempt.
/// </summary>
public class DependencyContainerBuildException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="DependencyContainerBuildException"/> instance.
    /// </summary>
    /// <param name="messages">Messages that describe what went wrong.</param>
    public DependencyContainerBuildException(Chain<DependencyContainerBuildMessages> messages)
        : base( Resources.ContainerIsNotConfiguredCorrectly( messages ) )
    {
        Messages = messages;
    }

    /// <summary>
    /// Messages that describe what went wrong.
    /// </summary>
    public Chain<DependencyContainerBuildMessages> Messages { get; }
}
