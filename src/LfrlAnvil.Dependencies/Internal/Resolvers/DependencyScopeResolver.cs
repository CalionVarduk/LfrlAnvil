using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class DependencyScopeResolver : DependencyResolver
{
    internal DependencyScopeResolver(ulong id)
        : base( id, typeof( IDependencyScope ) ) { }

    [Pure]
    protected override object CreateInternal(DependencyScope scope)
    {
        return scope;
    }
}
