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

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents an <see cref="ISqlDataType"/> parameter definition.
/// </summary>
public readonly struct SqlDataTypeParameter
{
    private readonly string? _name;

    /// <summary>
    /// Creates a new <see cref="SqlDataTypeParameter"/> instance.
    /// </summary>
    /// <param name="name">Parameter's name.</param>
    /// <param name="bounds">Range of valid values for this parameter.</param>
    public SqlDataTypeParameter(string name, Bounds<int> bounds)
    {
        _name = name;
        Bounds = bounds;
    }

    /// <summary>
    /// Range of valid values for this parameter.
    /// </summary>
    public Bounds<int> Bounds { get; }

    /// <summary>
    /// Parameter's name.
    /// </summary>
    public string Name => _name ?? string.Empty;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlDataTypeParameter"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"'{Name}' [{Bounds.Min}, {Bounds.Max}]";
    }
}
