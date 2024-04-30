using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation;

/// <summary>
/// A lightweight generic, potentially parameterized, validation message.
/// </summary>
/// <typeparam name="TResource">Resource type.</typeparam>
public readonly struct ValidationMessage<TResource>
{
    /// <summary>
    /// Creates a new <see cref="ValidationMessage{TResource}"/> instance.
    /// </summary>
    /// <param name="resource">Resource.</param>
    /// <param name="parameters">Optional range of parameters.</param>
    public ValidationMessage(TResource resource, params object?[] parameters)
    {
        Resource = resource;
        Parameters = parameters;
    }

    /// <summary>
    /// Resource or key of this message.
    /// </summary>
    public TResource Resource { get; }

    /// <summary>
    /// Optional range of parameters.
    /// </summary>
    public object?[] Parameters { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ValidationMessage{TResource}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Resource )}: '{Resource}', {nameof( Parameters )}: {Parameters.Length}";
    }
}
