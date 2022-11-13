using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class DependencyScopeDisposalException : InvalidOperationException
{
    public DependencyScopeDisposalException(IDependencyScope scope, int actualThreadId)
        : base( Resources.CannotDisposeScopeFromThisThread( scope, actualThreadId ) )
    {
        Scope = scope;
        ActualThreadId = actualThreadId;
    }

    public IDependencyScope Scope { get; }
    public int ActualThreadId { get; }
}
