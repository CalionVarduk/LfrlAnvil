using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class DependencyScopeCreationException : InvalidOperationException
{
    public DependencyScopeCreationException(IDependencyScope scope, IDependencyScope expectedScope, int threadId)
        : base( Resources.ScopeCannotBeginNewScopeForCurrentThread( scope, expectedScope, threadId ) )
    {
        Scope = scope;
        ExpectedScope = expectedScope;
        ThreadId = threadId;
    }

    public IDependencyScope Scope { get; }
    public IDependencyScope ExpectedScope { get; }
    public int ThreadId { get; }
}
