using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class OwnedDependenciesDisposalAggregateException : AggregateException
{
    public OwnedDependenciesDisposalAggregateException(IDependencyScope scope, Chain<OwnedDependencyDisposalException> innerExceptions)
        : base( Resources.SomeOwnedDependenciesHaveThrownExceptionsDuringDisposal( scope ), innerExceptions ) { }
}
