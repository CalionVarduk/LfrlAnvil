using System;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Specifies available automatic dependency implementor disposal strategies.
/// </summary>
public enum DependencyImplementorDisposalStrategyType : byte
{
    /// <summary>
    /// Invokes the <see cref="IDisposable.Dispose()"/> method if possible. This is the default strategy.
    /// </summary>
    UseDisposableInterface = 0,

    /// <summary>
    /// Invokes a custom callback.
    /// </summary>
    UseCallback = 1,

    /// <summary>
    /// Disables automatic disposal.
    /// </summary>
    RenounceOwnership = 2
}
