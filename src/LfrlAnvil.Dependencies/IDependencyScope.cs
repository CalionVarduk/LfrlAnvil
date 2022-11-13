using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyScope
{
    [MemberNotNullWhen( false, nameof( ParentScope ) )]
    [MemberNotNullWhen( false, nameof( ThreadId ) )]
    bool IsRoot { get; }

    int? ThreadId { get; }
    bool IsActive { get; }
    int Level { get; }
    bool IsDisposed { get; }
    IDependencyContainer Container { get; }
    IDependencyScope? ParentScope { get; }
    IDependencyLocator Locator { get; }

    [Pure]
    IChildDependencyScope BeginScope();
}
