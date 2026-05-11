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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace LfrlAnvil.Dependencies.MicrosoftExtensions;

internal sealed class DependencyContainerServiceProvider
    : ISupportRequiredService,
        IKeyedServiceProvider,
        IServiceProviderIsKeyedService,
        IServiceScope,
        IAsyncDisposable
{
    private readonly IDependencyScope _scope;

    internal DependencyContainerServiceProvider(IDependencyScope scope)
    {
        _scope = scope;
    }

    public IServiceProvider ServiceProvider => this;

    public void Dispose()
    {
        (( IDisposable )_scope).Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return (( IAsyncDisposable )_scope).DisposeAsync();
    }

    [Pure]
    public override string ToString()
    {
        return _scope.ToString() ?? string.Empty;
    }

    [Pure]
    public bool IsKeyedService(Type serviceType, object? serviceKey)
    {
        return serviceKey is null
            ? IsService( serviceType )
            : _scope.GetTypeErasedKeyedLocator( serviceKey ).TryGetLifetime( serviceType ) is not null;
    }

    [Pure]
    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
    {
        if ( serviceKey is null )
            return GetRequiredService( serviceType );

        serviceKey = SanitizeServiceKey( serviceKey );
        return _scope.GetTypeErasedKeyedLocator( serviceKey ).Resolve( serviceType );
    }

    [Pure]
    public object? GetKeyedService(Type serviceType, object? serviceKey)
    {
        if ( serviceKey is null )
            return GetService( serviceType );

        serviceKey = SanitizeServiceKey( serviceKey );
        return _scope.GetTypeErasedKeyedLocator( serviceKey ).TryResolve( serviceType );
    }

    [Pure]
    public bool IsService(Type serviceType)
    {
        return _scope.Locator.TryGetLifetime( serviceType ) is not null;
    }

    [Pure]
    public object GetRequiredService(Type serviceType)
    {
        return _scope.Locator.Resolve( serviceType );
    }

    [Pure]
    public object? GetService(Type serviceType)
    {
        return _scope.Locator.TryResolve( serviceType );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static object SanitizeServiceKey(object key)
    {
        if ( ReferenceEquals( key, KeyedService.AnyKey ) )
            ExceptionThrower.Throw( new InvalidOperationException( Resources.AnyKeyIsNotSupported ) );

        return key;
    }
}
