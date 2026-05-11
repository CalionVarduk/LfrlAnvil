// Copyright 2026 Łukasz Furlepa
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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Dependencies.Internal;

internal readonly struct ResolvedInstanceDisposalStrategy
{
    private readonly Delegate? _callback;
    private readonly Type _strategyType;

    private ResolvedInstanceDisposalStrategy(Type strategyType)
    {
        Assume.True( strategyType == Type.RenounceOwnership );
        _callback = null;
        _strategyType = strategyType;
    }

    internal ResolvedInstanceDisposalStrategy(DependencyImplementorDisposalStrategy @base, ConstructorInfo? ctor)
    {
        var implementorType = ctor?.DeclaringType;
        switch ( @base.Type )
        {
            case DependencyImplementorDisposalStrategyType.UseCallback:
                _callback = @base.Callback;
                Assume.IsNotNull( _callback );
                _strategyType = _callback is Func<object, ValueTask> ? Type.UseAsyncCallback : Type.UseCallback;
                break;

            case DependencyImplementorDisposalStrategyType.RenounceOwnership:
                _callback = null;
                _strategyType = Type.RenounceOwnership;
                break;

            default:
                _callback = null;
                if ( implementorType is null )
                    _strategyType = Type.UseDisposableInterfaceIfPossible;
                else if ( implementorType == typeof( IAsyncDisposable ) || implementorType.Implements<IAsyncDisposable>() )
                    _strategyType = Type.UseAsyncDisposableInterface;
                else if ( implementorType == typeof( IDisposable ) || implementorType.Implements<IDisposable>() )
                    _strategyType = Type.UseDisposableInterface;
                else
                    _strategyType = Type.RenounceOwnership;

                break;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ResolvedInstanceDisposalStrategy RenounceOwnership()
    {
        return new ResolvedInstanceDisposalStrategy( Type.RenounceOwnership );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyDisposer? TryCreateDisposer(object instance)
    {
        switch ( _strategyType )
        {
            case Type.UseDisposableInterface:
                return new DependencyDisposer( instance, callback: null, isAsync: false );

            case Type.UseAsyncDisposableInterface:
                return new DependencyDisposer( instance, callback: null, isAsync: true );

            case Type.UseDisposableInterfaceIfPossible:
            {
                if ( instance is IAsyncDisposable )
                    return new DependencyDisposer( instance, callback: null, isAsync: true );

                if ( instance is IDisposable )
                    return new DependencyDisposer( instance, callback: null, isAsync: false );

                break;
            }

            case Type.UseCallback:
            {
                Assume.IsNotNull( _callback );
                return new DependencyDisposer( instance, _callback, isAsync: false );
            }

            case Type.UseAsyncCallback:
            {
                Assume.IsNotNull( _callback );
                return new DependencyDisposer( instance, _callback, isAsync: true );
            }
        }

        return null;
    }

    private enum Type : byte
    {
        UseDisposableInterface = 0,
        UseAsyncDisposableInterface = 1,
        UseDisposableInterfaceIfPossible = 2,
        UseCallback = 3,
        UseAsyncCallback = 4,
        RenounceOwnership = 5
    }
}
