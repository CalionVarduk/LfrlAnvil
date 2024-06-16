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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal;

internal readonly struct NamedDependencyScopeStore : IDisposable
{
    private readonly Dictionary<string, ChildDependencyScope> _scopes;
    private readonly ReaderWriterLockSlim _lock;

    private NamedDependencyScopeStore(ReaderWriterLockSlim @lock)
    {
        _scopes = new Dictionary<string, ChildDependencyScope>();
        _lock = @lock;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        _lock.DisposeGracefully();
        Assume.IsEmpty( _scopes );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static NamedDependencyScopeStore Create()
    {
        return new NamedDependencyScopeStore( new ReaderWriterLockSlim() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ChildDependencyScope? TryGetScope(string name)
    {
        using ( ReadLockSlim.TryEnter( _lock, out var entered ) )
            return entered && _scopes.TryGetValue( name, out var scope ) ? scope : null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ChildDependencyScope CreateScope(DependencyScope parent, string name)
    {
        using ( WriteLockSlim.Enter( _lock ) )
        {
            ref var scope = ref CollectionsMarshal.GetValueRefOrAddDefault( _scopes, name, out var exists );
            if ( exists )
                ExceptionThrower.Throw( new NamedDependencyScopeCreationException( parent, name ) );

            scope = new ChildDependencyScope( parent.InternalContainer, parent, name );
            return scope;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void RemoveScope(string name)
    {
        using ( WriteLockSlim.Enter( _lock ) )
            _scopes.Remove( name );
    }
}
