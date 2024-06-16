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

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a generic variable.
/// </summary>
/// <typeparam name="TValue">Value type.</typeparam>
/// <typeparam name="TValidationResult">Validation result type.</typeparam>
public interface IVariable<TValue, TValidationResult> : IReadOnlyVariable<TValue, TValidationResult>
{
    /// <summary>
    /// Attempts to change the <see cref="IReadOnlyVariable{TValue}.Value"/> if the value to set is not equal to the current value.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult TryChange(TValue value);

    /// <summary>
    /// Changes the <see cref="IReadOnlyVariable{TValue}.Value"/>, even if the value to set is equal to the current value.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Change(TValue value);

    /// <summary>
    /// Refreshes this variable.
    /// </summary>
    void Refresh();

    /// <summary>
    /// Refreshes this variable's validation.
    /// </summary>
    void RefreshValidation();

    /// <summary>
    /// Removes all errors and warnings from this variable.
    /// </summary>
    void ClearValidation();
}
