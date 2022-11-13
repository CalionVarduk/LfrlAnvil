using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal;

internal sealed class RootDependencyScope : DependencyScope
{
    internal RootDependencyScope(DependencyContainer container)
        : base( container: container, parentScope: null, threadId: null )
    {
        ChildrenByThreadId = new Dictionary<int, ChildDependencyScope>();
    }

    internal Dictionary<int, ChildDependencyScope> ChildrenByThreadId { get; }

    [Pure]
    public override string ToString()
    {
        return $"{nameof( RootDependencyScope )} [{nameof( Level )}: {Level}]";
    }
}
