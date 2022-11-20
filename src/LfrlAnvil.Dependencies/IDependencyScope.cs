using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyScope
{
    [MemberNotNullWhen( false, nameof( ParentScope ) )]
    [MemberNotNullWhen( false, nameof( ThreadId ) )]
    bool IsRoot { get; }

    string? Name { get; }
    int? ThreadId { get; }
    bool IsActive { get; }
    int Level { get; }
    bool IsDisposed { get; }
    IDependencyContainer Container { get; }
    IDependencyScope? ParentScope { get; }
    IDependencyLocator Locator { get; }

    [Pure]
    IDependencyLocator<TKey> GetKeyedLocator<TKey>(TKey key)
        where TKey : notnull;

    [Pure]
    IChildDependencyScope BeginScope(string? name = null);

    [Pure]
    IDependencyScope? UseScope(string name);

    bool EndScope(string name);
}
