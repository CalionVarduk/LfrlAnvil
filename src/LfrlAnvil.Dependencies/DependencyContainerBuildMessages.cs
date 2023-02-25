using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies;

public readonly struct DependencyContainerBuildMessages
{
    public DependencyContainerBuildMessages(ImplementorKey implementorKey, Chain<string> errors, Chain<string> warnings)
    {
        ImplementorKey = implementorKey;
        Errors = errors;
        Warnings = warnings;
    }

    public ImplementorKey ImplementorKey { get; }
    public Chain<string> Errors { get; }
    public Chain<string> Warnings { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyContainerBuildMessages CreateErrors(ImplementorKey implementorKey, Chain<string> messages)
    {
        return new DependencyContainerBuildMessages( implementorKey, messages, Chain<string>.Empty );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyContainerBuildMessages CreateWarnings(ImplementorKey implementorKey, Chain<string> messages)
    {
        return new DependencyContainerBuildMessages( implementorKey, Chain<string>.Empty, messages );
    }
}
