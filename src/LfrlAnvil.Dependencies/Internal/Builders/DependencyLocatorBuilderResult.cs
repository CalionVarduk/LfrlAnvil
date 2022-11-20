using System;
using System.Collections.Generic;
using LfrlAnvil.Dependencies.Internal.Resolvers;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal readonly struct DependencyLocatorBuilderResult
{
    internal readonly Dictionary<Type, DependencyResolver> Resolvers;
    internal readonly Chain<DependencyContainerBuildMessages> Messages;

    internal DependencyLocatorBuilderResult(
        Dictionary<Type, DependencyResolver> resolvers,
        Chain<DependencyContainerBuildMessages> messages)
    {
        Resolvers = resolvers;
        Messages = messages;
    }
}
