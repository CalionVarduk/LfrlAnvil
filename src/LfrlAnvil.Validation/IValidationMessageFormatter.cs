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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;

namespace LfrlAnvil.Validation;

/// <summary>
/// Represents a formatter of generic <see cref="ValidationMessage{TResource}"/> instances.
/// </summary>
/// <typeparam name="TResource">Resource type.</typeparam>
public interface IValidationMessageFormatter<TResource>
{
    /// <summary>
    /// Returns a <see cref="String"/> representation of the provided <paramref name="resource"/>.
    /// </summary>
    /// <param name="resource">Resource to get string template for.</param>
    /// <param name="formatProvider">Optional format provider.</param>
    /// <returns><see cref="String"/> representation of the provided <paramref name="resource"/>.</returns>
    [Pure]
    string GetResourceTemplate(TResource resource, IFormatProvider? formatProvider);

    /// <summary>
    /// Returns a <see cref="ValidationMessageFormatterArgs"/> instance associated with this message formatter.
    /// </summary>
    /// <param name="formatProvider">Optional format provider.</param>
    /// <returns><see cref="ValidationMessageFormatterArgs"/> instance associated with this message formatter.</returns>
    [Pure]
    ValidationMessageFormatterArgs GetArgs(IFormatProvider? formatProvider);

    /// <summary>
    /// Formats the provided sequence of <paramref name="messages"/>.
    /// </summary>
    /// <param name="builder">Optional <see cref="StringBuilder"/> instance to append formatted messages to.</param>
    /// <param name="messages">Sequence of messages to format.</param>
    /// <param name="formatProvider">Optional format provider.</param>
    /// <returns>
    /// Provided <paramref name="builder"/> or a new <see cref="StringBuilder"/> instance
    /// or null when <paramref name="messages"/> are empty.
    /// </returns>
    [return: NotNullIfNotNull( "builder" )]
    StringBuilder? Format(StringBuilder? builder, Chain<ValidationMessage<TResource>> messages, IFormatProvider? formatProvider = null);
}
