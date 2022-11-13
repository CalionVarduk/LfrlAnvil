using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal;

internal sealed class ChildDependencyScope : DependencyScope, IChildDependencyScope
{
    internal ChildDependencyScope(DependencyContainer container, DependencyScope parentScope, int threadId)
        : base( container, parentScope, threadId )
    {
        Child = null;
    }

    internal ChildDependencyScope? Child { get; set; }

    [Pure]
    public override string ToString()
    {
        return $"{nameof( ChildDependencyScope )} [{nameof( Level )}: {Level}, {nameof( ThreadId )}: {ThreadId}]";
    }
}
