using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Dependencies.Internal.Resolvers;

namespace LfrlAnvil.Dependencies.Internal;

internal interface IInternalDependencyKey : IDependencyKey
{
    [Pure]
    DependencyImplementorBuilder? GetSharedImplementor(DependencyLocatorBuilderStore builderStore);

    [Pure]
    Dictionary<Type, DependencyResolver> GetTargetResolvers(
        Dictionary<Type, DependencyResolver> globalResolvers,
        KeyedDependencyResolversStore keyedResolversStore);

    [Pure]
    IInternalDependencyKey WithType(Type type);
}
