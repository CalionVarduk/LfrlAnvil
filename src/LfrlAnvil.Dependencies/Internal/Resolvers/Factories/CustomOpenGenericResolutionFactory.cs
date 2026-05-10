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

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal sealed record CustomOpenGenericResolutionFactory(DependencyResolverFactory Base, Type SourceType)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static object TryCreate(DependencyResolverFactory factory, Type sourceType, bool isCustom)
    {
        return isCustom && sourceType.ContainsGenericParameters
            ? new CustomOpenGenericResolutionFactory( factory, sourceType )
            : factory;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static (DependencyResolverFactory Factory, Type ImplementorType) Extract(object resolution)
    {
        DependencyResolverFactory baseResolver;
        Type implementorType;
        if ( resolution is CustomOpenGenericResolutionFactory partiallyOpen )
        {
            baseResolver = partiallyOpen.Base;
            implementorType = partiallyOpen.SourceType;
        }
        else
        {
            baseResolver = ReinterpretCast.To<DependencyResolverFactory>( resolution );
            implementorType = baseResolver.InternalImplementorKey.Type;
        }

        return (baseResolver, implementorType);
    }
}
