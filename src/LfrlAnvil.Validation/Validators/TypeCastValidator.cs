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

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator that casts validated objects to <typeparamref name="TDestination"/> type
/// and performs conditional validation based on that type cast.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TDestination">Object type required for <see cref="IfIsOfType"/> validator invocation.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class TypeCastValidator<T, TDestination, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="TypeCastValidator{T,TDestination,TResult}"/> instance.
    /// </summary>
    /// <param name="ifIsOfType">Underlying validator invoked when validated object is of <typeparamref name="TDestination"/> type.</param>
    /// <param name="ifIsNotOfType">
    /// Underlying validator invoked when validated object is not of <typeparamref name="TDestination"/> type.
    /// </param>
    public TypeCastValidator(IValidator<TDestination, TResult> ifIsOfType, IValidator<T, TResult> ifIsNotOfType)
    {
        IfIsOfType = ifIsOfType;
        IfIsNotOfType = ifIsNotOfType;
    }

    /// <summary>
    /// Underlying validator invoked when validated object is of <typeparamref name="TDestination"/> type.
    /// </summary>
    public IValidator<TDestination, TResult> IfIsOfType { get; }

    /// <summary>
    /// Underlying validator invoked when validated object is not of <typeparamref name="TDestination"/> type.
    /// </summary>
    public IValidator<T, TResult> IfIsNotOfType { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return obj is TDestination dest ? IfIsOfType.Validate( dest ) : IfIsNotOfType.Validate( obj );
    }
}
