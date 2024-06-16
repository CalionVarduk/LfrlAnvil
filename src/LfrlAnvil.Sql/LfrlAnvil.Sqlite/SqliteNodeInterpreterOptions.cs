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
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

/// <summary>
/// Represents available options for <see cref="SqliteNodeInterpreter"/>.
/// </summary>
public readonly struct SqliteNodeInterpreterOptions
{
    /// <summary>
    /// Represents default options.
    /// </summary>
    public static readonly SqliteNodeInterpreterOptions Default = new SqliteNodeInterpreterOptions()
        .SetUpsertOptions( SqliteUpsertOptions.Supported );

    private readonly bool _isUpdateFromDisabled;
    private readonly bool _isUpdateOrDeleteLimitDisabled;

    private SqliteNodeInterpreterOptions(
        SqliteColumnTypeDefinitionProvider? typeDefinitions,
        bool isStrictModeEnabled,
        bool isUpdateFromDisabled,
        bool isUpdateOrDeleteLimitDisabled,
        bool isAggregateFunctionOrderingEnabled,
        bool arePositionalParametersEnabled,
        SqliteUpsertOptions upsertOptions)
    {
        TypeDefinitions = typeDefinitions;
        IsStrictModeEnabled = isStrictModeEnabled;
        _isUpdateFromDisabled = isUpdateFromDisabled;
        _isUpdateOrDeleteLimitDisabled = isUpdateOrDeleteLimitDisabled;
        IsAggregateFunctionOrderingEnabled = isAggregateFunctionOrderingEnabled;
        ArePositionalParametersEnabled = arePositionalParametersEnabled;
        UpsertOptions = upsertOptions;
    }

    /// <summary>
    /// Specifies custom <see cref="SqliteColumnTypeDefinitionProvider"/>.
    /// </summary>
    /// <remarks>
    /// Default <see cref="SqliteColumnTypeDefinitionProvider"/> instance built by <see cref="SqliteColumnTypeDefinitionProviderBuilder"/>
    /// will be used when this is null.
    /// </remarks>
    public SqliteColumnTypeDefinitionProvider? TypeDefinitions { get; }

    /// <summary>
    /// Specifies whether or not the <b>STRICT</b> mode is enabled for table creation.
    /// </summary>
    public bool IsStrictModeEnabled { get; }

    /// <summary>
    /// Specifies <see cref="SqliteUpsertOptions"/> that defines interpreter's behavior during <see cref="SqlUpsertNode"/> interpretation.
    /// </summary>
    public SqliteUpsertOptions UpsertOptions { get; }

    /// <summary>
    /// Specifies whether or not <see cref="SqlSortTraitNode"/> instances that decorate aggregate function nodes
    /// should be included or ignored.
    /// </summary>
    public bool IsAggregateFunctionOrderingEnabled { get; }

    /// <summary>
    /// Specifies whether or not positional <see cref="SqliteParameter"/> instances are enabled.
    /// </summary>
    public bool ArePositionalParametersEnabled { get; }

    /// <summary>
    /// Specifies whether or not the <b>UPDATE FROM</b> syntax is supported.
    /// </summary>
    public bool IsUpdateFromEnabled => ! _isUpdateFromDisabled;

    /// <summary>
    /// Specifies whether or not the <b>ORDER BY</b>, <b>LIMIT</b> and <b>OFFSET</b> clauses are enabled
    /// for <b>UPDATE</b> and <b>DELETE</b> statements.
    /// </summary>
    public bool IsUpdateOrDeleteLimitEnabled => ! _isUpdateOrDeleteLimitDisabled;

    /// <summary>
    /// Creates a new <see cref="SqliteNodeInterpreterOptions"/> instance with changed <see cref="TypeDefinitions"/>.
    /// </summary>
    /// <param name="typeDefinitions">Value to set.</param>
    /// <returns>New <see cref="SqliteNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteNodeInterpreterOptions SetTypeDefinitions(SqliteColumnTypeDefinitionProvider? typeDefinitions)
    {
        return new SqliteNodeInterpreterOptions(
            typeDefinitions,
            IsStrictModeEnabled,
            _isUpdateFromDisabled,
            _isUpdateOrDeleteLimitDisabled,
            IsAggregateFunctionOrderingEnabled,
            ArePositionalParametersEnabled,
            UpsertOptions );
    }

    /// <summary>
    /// Creates a new <see cref="SqliteNodeInterpreterOptions"/> instance with changed <see cref="IsStrictModeEnabled"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="SqliteNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteNodeInterpreterOptions EnableStrictMode(bool enabled = true)
    {
        return new SqliteNodeInterpreterOptions(
            TypeDefinitions,
            enabled,
            _isUpdateFromDisabled,
            _isUpdateOrDeleteLimitDisabled,
            IsAggregateFunctionOrderingEnabled,
            ArePositionalParametersEnabled,
            UpsertOptions );
    }

    /// <summary>
    /// Creates a new <see cref="SqliteNodeInterpreterOptions"/> instance with changed <see cref="IsUpdateFromEnabled"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="SqliteNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteNodeInterpreterOptions EnableUpdateFrom(bool enabled = true)
    {
        return new SqliteNodeInterpreterOptions(
            TypeDefinitions,
            IsStrictModeEnabled,
            ! enabled,
            _isUpdateOrDeleteLimitDisabled,
            IsAggregateFunctionOrderingEnabled,
            ArePositionalParametersEnabled,
            UpsertOptions );
    }

    /// <summary>
    /// Creates a new <see cref="SqliteNodeInterpreterOptions"/> instance with changed <see cref="IsUpdateOrDeleteLimitEnabled"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="SqliteNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteNodeInterpreterOptions EnableUpdateOrDeleteLimit(bool enabled = true)
    {
        return new SqliteNodeInterpreterOptions(
            TypeDefinitions,
            IsStrictModeEnabled,
            _isUpdateFromDisabled,
            ! enabled,
            IsAggregateFunctionOrderingEnabled,
            ArePositionalParametersEnabled,
            UpsertOptions );
    }

    /// <summary>
    /// Creates a new <see cref="SqliteNodeInterpreterOptions"/> instance with changed <see cref="IsAggregateFunctionOrderingEnabled"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="SqliteNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteNodeInterpreterOptions EnableAggregateFunctionOrdering(bool enabled = true)
    {
        return new SqliteNodeInterpreterOptions(
            TypeDefinitions,
            IsStrictModeEnabled,
            _isUpdateFromDisabled,
            _isUpdateOrDeleteLimitDisabled,
            enabled,
            ArePositionalParametersEnabled,
            UpsertOptions );
    }

    /// <summary>
    /// Creates a new <see cref="SqliteNodeInterpreterOptions"/> instance with changed <see cref="ArePositionalParametersEnabled"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="SqliteNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteNodeInterpreterOptions EnablePositionalParameters(bool enabled = true)
    {
        return new SqliteNodeInterpreterOptions(
            TypeDefinitions,
            IsStrictModeEnabled,
            _isUpdateFromDisabled,
            _isUpdateOrDeleteLimitDisabled,
            IsAggregateFunctionOrderingEnabled,
            enabled,
            UpsertOptions );
    }

    /// <summary>
    /// Creates a new <see cref="SqliteNodeInterpreterOptions"/> instance with changed <see cref="UpsertOptions"/>.
    /// </summary>
    /// <param name="options">Value to set.</param>
    /// <returns>New <see cref="SqliteNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteNodeInterpreterOptions SetUpsertOptions(SqliteUpsertOptions options)
    {
        options &= SqliteUpsertOptions.Supported | SqliteUpsertOptions.AllowEmptyConflictTarget;
        if ( (options & SqliteUpsertOptions.AllowEmptyConflictTarget) != SqliteUpsertOptions.Disabled )
            options |= SqliteUpsertOptions.Supported;

        return new SqliteNodeInterpreterOptions(
            TypeDefinitions,
            IsStrictModeEnabled,
            _isUpdateFromDisabled,
            _isUpdateOrDeleteLimitDisabled,
            IsAggregateFunctionOrderingEnabled,
            ArePositionalParametersEnabled,
            options );
    }
}
