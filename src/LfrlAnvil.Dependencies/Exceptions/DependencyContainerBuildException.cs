using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class DependencyContainerBuildException : InvalidOperationException
{
    public DependencyContainerBuildException(Chain<DependencyContainerBuildMessages> messages)
        : base( Resources.ContainerIsNotConfiguredCorrectly( messages ) )
    {
        Messages = messages;
    }

    public Chain<DependencyContainerBuildMessages> Messages { get; }
}
