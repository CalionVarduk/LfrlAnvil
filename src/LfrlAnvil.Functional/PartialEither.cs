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

namespace LfrlAnvil.Functional;

/// <summary>
/// An intermediate object used for creating <see cref="Either{T1,T2}"/> instances.
/// </summary>
/// <typeparam name="T1">Value type.</typeparam>
public readonly struct PartialEither<T1>
{
    /// <summary>
    /// Underlying value.
    /// </summary>
    public readonly T1 Value;

    internal PartialEither(T1 value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="Either{T1,T2}"/> instance with the underlying <see cref="Value"/> being the first value.
    /// </summary>
    /// <typeparam name="T2">Second value type.</typeparam>
    /// <returns>New <see cref="Either{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Either<T1, T2> WithSecond<T2>()
    {
        return new Either<T1, T2>( Value );
    }

    /// <summary>
    /// Creates a new <see cref="Either{T1,T2}"/> instance with the underlying <see cref="Value"/> being the second value.
    /// </summary>
    /// <typeparam name="T2">First value type.</typeparam>
    /// <returns>New <see cref="Either{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Either<T2, T1> WithFirst<T2>()
    {
        return new Either<T2, T1>( Value );
    }
}
