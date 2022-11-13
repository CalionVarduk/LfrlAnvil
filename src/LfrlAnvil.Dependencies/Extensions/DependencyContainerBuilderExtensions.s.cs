using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies.Extensions;

public static class DependencyContainerBuilderExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyContainer Build(this DependencyContainerBuilder builder)
    {
        return builder.TryBuild().GetContainerOrThrow();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDisposableDependencyContainer Build(this IDependencyContainerBuilder builder)
    {
        return builder.TryBuild().GetContainerOrThrow();
    }
}
