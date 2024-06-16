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
using System.Linq;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a formatted validation result created from a sequence of generic <see cref="ValidationMessage{TResource}"/> instances.
/// </summary>
/// <typeparam name="TResource">Resource type.</typeparam>
public readonly struct FormattedValidatorResult<TResource>
{
    private readonly string? _result;

    /// <summary>
    /// Creates a new <see cref="FormattedValidatorResult{TResource}"/> instance.
    /// </summary>
    /// <param name="messages">Validation result.</param>
    /// <param name="result">Formatted message.</param>
    public FormattedValidatorResult(Chain<ValidationMessage<TResource>> messages, string result)
    {
        Messages = messages;
        _result = result;
    }

    /// <summary>
    /// Validation result.
    /// </summary>
    public Chain<ValidationMessage<TResource>> Messages { get; }

    /// <summary>
    /// Formatted message.
    /// </summary>
    public string Result => _result ?? string.Empty;

    /// <summary>
    /// Returns a string representation of this <see cref="FormattedValidatorResult{TResource}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var messagesText = string.Join( Environment.NewLine, Messages.Select( static (m, i) => $"{i + 1}. '{m}'" ) );
        return $"{nameof( Result )}: '{Result}', {nameof( Messages )}:{Environment.NewLine}{messagesText}";
    }
}
