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
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Extensions;

/// <summary>
/// Contains <see cref="IValidator{T,TResult}"/> extension methods for value types.
/// </summary>
public static class StructValidatorExtensions
{
    /// <summary>
    /// Creates a new <see cref="ForNullableStructValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="ForNullableStructValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T?, TResult> ForNullable<T, TResult>(this IValidator<T, TResult> validator)
        where T : struct
    {
        return new ForNullableStructValidator<T, TResult>( validator );
    }

    /// <summary>
    /// Creates a new <see cref="ForDefaultIfNullStructValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <param name="defaultValue">Default value to use instead of a null object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="ForDefaultIfNullStructValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T?, TResult> ForDefaultIfNull<T, TResult>(this IValidator<T, TResult> validator, T defaultValue)
        where T : struct
    {
        return new ForDefaultIfNullStructValidator<T, TResult>( validator, defaultValue );
    }
}
