using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal;

internal sealed class ChildDependencyScope : DependencyScope, IChildDependencyScope
{
    internal ChildDependencyScope(DependencyContainer container, DependencyScope parentScope, int threadId, string? name)
        : base( container, parentScope, threadId, name )
    {
        Child = null;
    }

    internal ChildDependencyScope? Child { get; set; }

    [Pure]
    public override string ToString()
    {
        return Name is null
            ? $"{nameof( ChildDependencyScope )} [{nameof( Level )}: {Level}, {nameof( ThreadId )}: {ThreadId}]"
            : $"{nameof( ChildDependencyScope )} [{nameof( Name )}: '{Name}', {nameof( Level )}: {Level}, {nameof( ThreadId )}: {ThreadId}]";
    }
}
