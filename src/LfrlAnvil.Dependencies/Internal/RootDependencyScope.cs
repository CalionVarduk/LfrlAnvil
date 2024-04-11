using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal;

internal sealed class RootDependencyScope : DependencyScope
{
    internal RootDependencyScope(DependencyContainer container)
        : base( container: container, parentScope: null, name: null ) { }

    [Pure]
    public override string ToString()
    {
        return $"{nameof( RootDependencyScope )} [{nameof( Level )}: {Level}, {nameof( OriginalThreadId )}: {OriginalThreadId}]";
    }
}
