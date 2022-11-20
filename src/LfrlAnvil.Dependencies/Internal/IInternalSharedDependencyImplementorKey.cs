using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Internal.Builders;

namespace LfrlAnvil.Dependencies.Internal;

internal interface IInternalSharedDependencyImplementorKey : ISharedDependencyImplementorKey
{
    [Pure]
    DependencyImplementorBuilder? GetSharedImplementor(DependencyLocatorBuilderStore builderStore);
}
