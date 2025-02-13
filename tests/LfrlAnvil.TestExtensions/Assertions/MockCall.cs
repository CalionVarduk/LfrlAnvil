using NSubstitute.Core;

namespace LfrlAnvil.TestExtensions.Assertions;

public sealed class MockCall
{
    private readonly ICall? _call;

    internal MockCall(ICall? call)
    {
        _call = call;
    }

    public bool Exists => _call is not null;
    public object?[] Arguments => _call?.GetArguments() ?? Array.Empty<object?>();
}
