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
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal sealed record CustomOpenGenericResolution(DependencyResolver Base, CustomOpenGenericResolutionSource Source)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static (DependencyResolver Resolver, CustomOpenGenericResolutionSource? Source) Extract(object resolution)
    {
        return resolution is CustomOpenGenericResolution partiallyOpen
            ? (partiallyOpen.Base, partiallyOpen.Source)
            : (ReinterpretCast.To<DependencyResolver>( resolution ), null);
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static object TryCreate(
        UlongSequenceGenerator idGenerator,
        DependencyContainerConfigurationBuilder configuration,
        object resolution)
    {
        DependencyResolverFactory factory;
        Type? implementorType;
        if ( resolution is CustomOpenGenericResolutionFactory partiallyOpen )
        {
            factory = partiallyOpen.Base;
            implementorType = partiallyOpen.SourceType;
        }
        else
        {
            factory = ReinterpretCast.To<DependencyResolverFactory>( resolution );
            implementorType = null;
        }

        factory.Build( idGenerator, configuration );
        var resolver = factory.GetResolver();
        if ( implementorType is null )
            return resolver;

        ConstructorInfo? sourceCtor = null;
        if ( ! implementorType.IsGenericTypeDefinition )
        {
            sourceCtor = (factory as RegisteredDependencyResolverFactory)?.ConstructorInfo;
            if ( factory.ImplementorKey.IsShared )
                implementorType = factory.ImplementorKey.Value.Type.CloseImplementorType( implementorType );

            sourceCtor = sourceCtor?.TryCloseGenericCtor( factory.ImplementorKey.Value.Type, implementorType );
        }

        return new CustomOpenGenericResolution( resolver, new CustomOpenGenericResolutionSource( implementorType, sourceCtor ) );
    }
}
