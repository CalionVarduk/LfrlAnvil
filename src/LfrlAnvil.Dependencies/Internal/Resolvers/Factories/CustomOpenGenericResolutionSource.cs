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

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal readonly record struct CustomOpenGenericResolutionSource(Type Type, ConstructorInfo? Ctor)
{
    internal bool IsPartiallyOpen => ! Type.IsGenericTypeDefinition;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Value<ConstructorInfo?>? TryGetCtor(CustomOpenGenericResolutionSource? source)
    {
        return source is null || ! source.Value.IsPartiallyOpen
            ? ( Value<ConstructorInfo?>? )null
            : new Value<ConstructorInfo?>( source.Value.Ctor );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ConstructorInfo GetCtor(Value<ConstructorInfo?>? source, ConstructorInfo defaultCtor)
    {
        if ( source is null )
            return defaultCtor;

        Assume.IsNotNull( source.Value.Item );
        return source.Value.Item;
    }
}
