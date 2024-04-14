using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Internal;

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyDisposer? TryCreateDisposer(object instance)
    {
        switch ( Type )
        {
            case DependencyImplementorDisposalStrategyType.UseDisposableInterface:
                if ( instance is IDisposable disposable )
                    return new DependencyDisposer( disposable, callback: null );

                break;

            case DependencyImplementorDisposalStrategyType.UseCallback:
                Assume.IsNotNull( Callback );
                return new DependencyDisposer( instance, Callback );
        }

        return null;
    }
}
