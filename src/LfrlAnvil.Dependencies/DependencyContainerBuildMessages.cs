using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies;

public readonly struct DependencyContainerBuildMessages
{
    private DependencyContainerBuildMessages(
        Type dependencyType,
        ISharedDependencyImplementorKey implementorKey,
        Chain<string> errors,
        Chain<string> warnings)
    {
        DependencyType = dependencyType;
        ImplementorKey = implementorKey;
        Errors = errors;
        Warnings = warnings;
    }

    public Type DependencyType { get; }
    public ISharedDependencyImplementorKey ImplementorKey { get; }
    public Chain<string> Errors { get; }
    public Chain<string> Warnings { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyContainerBuildMessages Create(
        Type dependencyType,
        ISharedDependencyImplementorKey implementorKey,
        Chain<string> errors,
        Chain<string> warnings)
    {
        return new DependencyContainerBuildMessages( dependencyType, implementorKey, errors, warnings );
    }
}
