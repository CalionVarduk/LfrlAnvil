using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies;

public readonly struct DependencyImplementorDisposalStrategy
{
    private DependencyImplementorDisposalStrategy(DependencyImplementorDisposalStrategyType type, Action<object>? callback)
    {
        Type = type;
        Callback = callback;
    }

    public DependencyImplementorDisposalStrategyType Type { get; }
    public Action<object>? Callback { get; }

    [Pure]
    public override string ToString()
    {
        return Type.ToString();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyImplementorDisposalStrategy UseDisposableInterface()
    {
        return new DependencyImplementorDisposalStrategy(
            DependencyImplementorDisposalStrategyType.UseDisposableInterface,
            callback: null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyImplementorDisposalStrategy UseCallback(Action<object> callback)
    {
        return new DependencyImplementorDisposalStrategy( DependencyImplementorDisposalStrategyType.UseCallback, callback );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyImplementorDisposalStrategy RenounceOwnership()
    {
        return new DependencyImplementorDisposalStrategy( DependencyImplementorDisposalStrategyType.RenounceOwnership, callback: null );
    }
}
