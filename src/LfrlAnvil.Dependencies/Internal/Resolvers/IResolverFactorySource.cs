using System;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal interface IResolverFactorySource
{
    Func<DependencyScope, object> Factory { get; set; }
}
