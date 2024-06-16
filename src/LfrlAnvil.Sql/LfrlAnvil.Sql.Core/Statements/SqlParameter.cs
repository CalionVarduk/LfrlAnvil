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

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased bindable SQL parameter.
/// </summary>
/// <param name="Name">Optional parameter name.</param>
/// <param name="Value">Parameter value.</param>
public readonly record struct SqlParameter(string? Name, object? Value)
{
    /// <summary>
    /// Specifies whether or not this parameter is positional (does not have a <see cref="Name"/>).
    /// </summary>
    [MemberNotNullWhen( false, nameof( Name ) )]
    public bool IsPositional => Name is null;

    /// <summary>
    /// Creates a new named <see cref="SqlParameter"/> instance.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <returns>New named <see cref="SqlParameter"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameter Named(string name, object? value)
    {
        return new SqlParameter( name, value );
    }

    /// <summary>
    /// Creates a new positional <see cref="SqlParameter"/> instance.
    /// </summary>
    /// <param name="value">Parameter value.</param>
    /// <returns>New positional <see cref="SqlParameter"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameter Positional(object? value)
    {
        return new SqlParameter( null, value );
    }
}
