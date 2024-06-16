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

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyImplementorOptions : IDependencyImplementorOptions
{
    private IInternalDependencyKey _key;

    internal DependencyImplementorOptions(IInternalDependencyKey key)
    {
        _key = key;
    }

    public IDependencyKey Key => _key;

    public void Keyed<TKey>(TKey key)
        where TKey : notnull
    {
        _key = new DependencyKey<TKey>( _key.Type, key );
    }

    public void NotKeyed()
    {
        _key = new DependencyKey( _key.Type );
    }

    [Pure]
    internal static IInternalDependencyKey CreateImplementorKey(
        IInternalDependencyKey defaultKey,
        Action<IDependencyImplementorOptions>? configuration)
    {
        if ( configuration is null )
            return defaultKey;

        var options = new DependencyImplementorOptions( defaultKey );
        configuration( options );

        var sharedImplementorKey = options.Key as IInternalDependencyKey;
        Ensure.IsNotNull( sharedImplementorKey );
        return sharedImplementorKey;
    }
}
