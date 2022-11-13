using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class OwnedDependencyDisposalException : Exception
{
    public OwnedDependencyDisposalException(IDependencyScope scope, Exception innerException)
        : base( Resources.OwnedDependencyHasThrownExceptionDuringDisposal( scope ), innerException )
    {
        Scope = scope;
    }

    public IDependencyScope Scope { get; }
}
