using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Internal.Builders;

namespace LfrlAnvil.Dependencies.Internal;

internal interface IInternalDependencyImplementorKey : IDependencyImplementorKey
{
    [Pure]
    DependencyImplementorBuilder? GetSharedImplementor(DependencyLocatorBuilderStore builderStore);
}
