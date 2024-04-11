using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal;

internal sealed class ChildDependencyScope : DependencyScope, IChildDependencyScope
{
    internal ChildDependencyScope(DependencyContainer container, DependencyScope parentScope, string? name)
        : base( container, parentScope, name )
    {
        PrevSibling = null;
        NextSibling = null;
        parentScope.AddChild( this );
    }

    internal ChildDependencyScope? PrevSibling { get; set; }
    internal ChildDependencyScope? NextSibling { get; set; }

    [Pure]
    public override string ToString()
    {
        return Name is null
            ? $"{nameof( ChildDependencyScope )} [{nameof( Level )}: {Level}, {nameof( OriginalThreadId )}: {OriginalThreadId}]"
            : $"{nameof( ChildDependencyScope )} [{nameof( Name )}: '{Name}', {nameof( Level )}: {Level}, {nameof( OriginalThreadId )}: {OriginalThreadId}]";
    }
}
