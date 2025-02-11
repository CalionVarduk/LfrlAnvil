using NSubstitute.Core;

namespace LfrlAnvil.TestExtensions.Assertions;

public sealed class DelegateCall
{
    private readonly ICall? _call;

    internal DelegateCall(ICall? call)
    {
        _call = call;
    }

    public bool Exists => _call is not null;
    public object?[] Arguments => _call?.GetArguments() ?? Array.Empty<object?>();
}
