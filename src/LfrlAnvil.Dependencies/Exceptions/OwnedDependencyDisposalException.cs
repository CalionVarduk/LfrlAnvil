using System;

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents an error that occurred during disposal of a resolved dependency instance owned by the disposed scope.
/// </summary>
public class OwnedDependencyDisposalException : Exception
{
    /// <summary>
    /// Creates a new <see cref="OwnedDependencyDisposalException"/> instance.
    /// </summary>
    /// <param name="scope">Disposed scope.</param>
    /// <param name="innerException">Disposal exception.</param>
    public OwnedDependencyDisposalException(IDependencyScope scope, Exception innerException)
        : base( Resources.OwnedDependencyHasThrownExceptionDuringDisposal( scope ), innerException )
    {
        Scope = scope;
    }

    /// <summary>
    /// Disposed scope.
    /// </summary>
    public IDependencyScope Scope { get; }
}
