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
using System.Runtime.CompilerServices;
using System.Text;

namespace LfrlAnvil.Validation.Extensions;

/// <summary>
/// Contains <see cref="IValidationMessageFormatter{TResource}"/> extension methods.
/// </summary>
public static class ValidationMessageFormatterExtensions
{
    /// <summary>
    /// Formats the provided sequence of <paramref name="messages"/>.
    /// </summary>
    /// <param name="formatter">Source validation message formatter.</param>
    /// <param name="messages">Sequence of messages to format.</param>
    /// <param name="formatProvider">Optional format provider.</param>
    /// <returns>New <see cref="StringBuilder"/> instance or null when <paramref name="messages"/> are empty.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringBuilder? Format<TResource>(
        this IValidationMessageFormatter<TResource> formatter,
        Chain<ValidationMessage<TResource>> messages,
        IFormatProvider? formatProvider = null)
    {
        return formatter.Format( builder: null, messages, formatProvider );
    }
}
