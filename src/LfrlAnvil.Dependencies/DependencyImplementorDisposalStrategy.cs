// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Internal;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a descriptor of dependency implementor's automatic disposal strategy.
/// </summary>
public readonly struct DependencyImplementorDisposalStrategy
{
    private DependencyImplementorDisposalStrategy(DependencyImplementorDisposalStrategyType type, Action<object>? callback)
    {
        Type = type;
        Callback = callback;
    }

    /// <summary>
    /// Specifies the type of this disposal strategy.
    /// </summary>
    public DependencyImplementorDisposalStrategyType Type { get; }

    /// <summary>
    /// Specifies the optional disposal callback for <see cref="DependencyImplementorDisposalStrategyType.UseCallback"/> <see cref="Type"/>.
    /// </summary>
    public Action<object>? Callback { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="DependencyImplementorDisposalStrategy"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return Type.ToString();
    }

    /// <summary>
    /// Creates a new <see cref="DependencyImplementorDisposalStrategy"/> instance
    /// with <see cref="DependencyImplementorDisposalStrategyType.UseDisposableInterface"/> type.
    /// This is the default automatic disposal strategy that invokes the <see cref="IDisposable.Dispose()"/> method if an object
    /// implements the <see cref="IDisposable"/> interface.
    /// </summary>
    /// <returns>New <see cref="DependencyImplementorDisposalStrategy"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyImplementorDisposalStrategy UseDisposableInterface()
    {
        return new DependencyImplementorDisposalStrategy(
            DependencyImplementorDisposalStrategyType.UseDisposableInterface,
            callback: null );
    }

    /// <summary>
    /// Creates a new <see cref="DependencyImplementorDisposalStrategy"/> instance
    /// with <see cref="DependencyImplementorDisposalStrategyType.UseCallback"/> and custom <paramref name="callback"/>.
    /// </summary>
    /// <param name="callback">Custom disposal callback.</param>
    /// <returns>New <see cref="DependencyImplementorDisposalStrategy"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyImplementorDisposalStrategy UseCallback(Action<object> callback)
    {
        return new DependencyImplementorDisposalStrategy( DependencyImplementorDisposalStrategyType.UseCallback, callback );
    }

    /// <summary>
    /// Creates a new <see cref="DependencyImplementorDisposalStrategy"/> instance
    /// with <see cref="DependencyImplementorDisposalStrategyType.RenounceOwnership"/>.
    /// This strategy disables the automatic disposal.
    /// </summary>
    /// <returns>New <see cref="DependencyImplementorDisposalStrategy"/> instance.</returns>
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
