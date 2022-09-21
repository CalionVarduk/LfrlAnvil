using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation;

public readonly struct ValidationMessage<TResource>
{
    public ValidationMessage(TResource resource, params object?[] parameters)
    {
        Resource = resource;
        Parameters = parameters;
    }

    public TResource Resource { get; }
    public object?[] Parameters { get; }

    [Pure]
    public override string ToString()
    {
        return $"{nameof( Resource )}: '{Resource}', {nameof( Parameters )}: {Parameters.Length}";
    }
}
