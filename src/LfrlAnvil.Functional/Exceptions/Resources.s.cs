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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Functional.Exceptions;

internal static class Resources
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingFirstEitherValue<T1, T2>()
    {
        return $"{GetEitherName<T1, T2>()} instance doesn't have the {nameof( Either<T1, T2>.First )} value.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingSecondEitherValue<T1, T2>()
    {
        return $"{GetEitherName<T1, T2>()} instance doesn't have the {nameof( Either<T1, T2>.Second )} value.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingErraticValue<T>()
    {
        return $"{GetErraticName<T>()} instance doesn't contain a {nameof( Erratic<T>.Value )}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingErraticError<T>()
    {
        return $"{GetErraticName<T>()} instance doesn't contain an {nameof( Erratic<T>.Error )}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingMaybeValue<T>()
        where T : notnull
    {
        return $"{GetMaybeName<T>()} instance doesn't contain a {nameof( Maybe<T>.Value )}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingTypeCastResult<TSource, TDestination>()
    {
        return
            $"{GetTypeCastName<TSource, TDestination>()} instance doesn't contain a valid {nameof( TypeCast<TSource, TDestination>.Result )}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string GetEitherName<T1, T2>()
    {
        return $"{nameof( Either )}<{typeof( T1 ).GetDebugString()}, {typeof( T2 ).GetDebugString()}>";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string GetErraticName<T>()
    {
        return $"{nameof( Erratic )}<{typeof( T ).GetDebugString()}>";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string GetMaybeName<T>()
    {
        return $"{nameof( Maybe )}<{typeof( T ).GetDebugString()}>";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string GetTypeCastName<TSource, TDestination>()
    {
        return $"{nameof( TypeCast )}<{typeof( TSource ).GetDebugString()}, {typeof( TDestination ).GetDebugString()}>";
    }
}
