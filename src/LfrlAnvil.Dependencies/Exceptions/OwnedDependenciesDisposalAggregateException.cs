using System;

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents a collection of errors that occurred during scope disposal.
/// </summary>
public class OwnedDependenciesDisposalAggregateException : AggregateException
{
    /// <summary>
    /// Creates a new <see cref="OwnedDependenciesDisposalAggregateException"/> instance.
    /// </summary>
    /// <param name="scope">Disposed scope.</param>
    /// <param name="innerExceptions">Collection of errors.</param>
    public OwnedDependenciesDisposalAggregateException(IDependencyScope scope, Chain<OwnedDependencyDisposalException> innerExceptions)
        : base( Resources.SomeOwnedDependenciesHaveThrownExceptionsDuringDisposal( scope ), innerExceptions ) { }
}
