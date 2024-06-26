﻿// Copyright 2024 Łukasz Furlepa
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

namespace LfrlAnvil.PostgreSql;

/// <summary>
/// Represents available options for <see cref="PostgreSqlNodeInterpreter"/>.
/// </summary>
public readonly struct PostgreSqlNodeInterpreterOptions
{
    /// <summary>
    /// Represents default options.
    /// </summary>
    public static readonly PostgreSqlNodeInterpreterOptions Default = new PostgreSqlNodeInterpreterOptions();

    private PostgreSqlNodeInterpreterOptions(
        PostgreSqlColumnTypeDefinitionProvider? typeDefinitions,
        bool isVirtualGeneratedColumnStorageParsingEnabled)
    {
        TypeDefinitions = typeDefinitions;
        IsVirtualGeneratedColumnStorageParsingEnabled = isVirtualGeneratedColumnStorageParsingEnabled;
    }

    /// <summary>
    /// Specifies custom <see cref="PostgreSqlColumnTypeDefinitionProvider"/>.
    /// </summary>
    /// <remarks>
    /// Default <see cref="PostgreSqlColumnTypeDefinitionProvider"/> instance built by
    /// <see cref="PostgreSqlColumnTypeDefinitionProviderBuilder"/> will be used when this is null.
    /// </remarks>
    public PostgreSqlColumnTypeDefinitionProvider? TypeDefinitions { get; }

    /// <summary>
    /// Specifies whether or not the <b>VIRTUAL</b> keyword for computed columns should be included in SQL statements.
    /// </summary>
    public bool IsVirtualGeneratedColumnStorageParsingEnabled { get; }

    /// <summary>
    /// Creates a new <see cref="PostgreSqlNodeInterpreterOptions"/> instance with changed <see cref="TypeDefinitions"/>.
    /// </summary>
    /// <param name="typeDefinitions">Value to set.</param>
    /// <returns>New <see cref="PostgreSqlNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlNodeInterpreterOptions SetTypeDefinitions(PostgreSqlColumnTypeDefinitionProvider? typeDefinitions)
    {
        return new PostgreSqlNodeInterpreterOptions( typeDefinitions, IsVirtualGeneratedColumnStorageParsingEnabled );
    }

    /// <summary>
    /// Creates a new <see cref="PostgreSqlNodeInterpreterOptions"/> instance
    /// with changed <see cref="IsVirtualGeneratedColumnStorageParsingEnabled"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="PostgreSqlNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlNodeInterpreterOptions EnableVirtualGeneratedColumnStorageParsing(bool enabled = true)
    {
        return new PostgreSqlNodeInterpreterOptions( TypeDefinitions, enabled );
    }
}
