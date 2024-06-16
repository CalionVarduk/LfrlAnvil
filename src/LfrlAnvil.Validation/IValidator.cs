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

namespace LfrlAnvil.Validation;

/// <summary>
/// Represents a generic object validator.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IValidator<in T, TResult>
{
    /// <summary>
    /// Validates the provided <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">Object to validate.</param>
    /// <returns>Result of <paramref name="obj"/> validation.</returns>
    [Pure]
    Chain<TResult> Validate(T obj);
}
