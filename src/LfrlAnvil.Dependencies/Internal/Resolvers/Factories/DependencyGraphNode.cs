using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal readonly struct DependencyGraphNode
{
    internal DependencyGraphNode(object? reachedFrom, DependencyResolverFactory factory)
    {
        ReachedFrom = reachedFrom;
        Factory = factory;
    }

    internal object? ReachedFrom { get; }
    internal DependencyResolverFactory Factory { get; }

    [Pure]
    public override string ToString()
    {
        return ReachedFrom is not null ? $"{ReachedFrom} => {Factory}" : Factory.ToString();
    }
}
