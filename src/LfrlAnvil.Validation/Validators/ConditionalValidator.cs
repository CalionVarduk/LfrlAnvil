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

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a conditional generic object validator.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class ConditionalValidator<T, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="ConditionalValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="condition">
    /// Validator's condition. When it returns <b>true</b>, then <see cref="IfTrue"/> gets invoked,
    /// otherwise <see cref="IfFalse"/> gets invoked.
    /// </param>
    /// <param name="ifTrue">Underlying validator invoked when <see cref="Condition"/> returns <b>true</b>.</param>
    /// <param name="ifFalse">Underlying validator invoked when <see cref="Condition"/> returns <b>false</b>.</param>
    public ConditionalValidator(Func<T, bool> condition, IValidator<T, TResult> ifTrue, IValidator<T, TResult> ifFalse)
    {
        Condition = condition;
        IfTrue = ifTrue;
        IfFalse = ifFalse;
    }

    /// <summary>
    /// Validator's condition. When it returns <b>true</b>, then <see cref="IfTrue"/> gets invoked,
    /// otherwise <see cref="IfFalse"/> gets invoked.
    /// </summary>
    public Func<T, bool> Condition { get; }

    /// <summary>
    /// Underlying validator invoked when <see cref="Condition"/> returns <b>true</b>.
    /// </summary>
    public IValidator<T, TResult> IfTrue { get; }

    /// <summary>
    /// Underlying validator invoked when <see cref="Condition"/> returns <b>false</b>.
    /// </summary>
    public IValidator<T, TResult> IfFalse { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        var validator = Condition( obj ) ? IfTrue : IfFalse;
        return validator.Validate( obj );
    }
}
