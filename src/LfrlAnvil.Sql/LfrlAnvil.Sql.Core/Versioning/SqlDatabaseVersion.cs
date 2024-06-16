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
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Versioning;

/// <summary>
/// Represents a single database version.
/// </summary>
/// <typeparam name="TDatabaseBuilder">SQL database builder type.</typeparam>
public abstract class SqlDatabaseVersion<TDatabaseBuilder> : ISqlDatabaseVersion
    where TDatabaseBuilder : class, ISqlDatabaseBuilder
{
    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="description">Optional description of this version.</param>
    protected SqlDatabaseVersion(Version value, string? description = null)
    {
        Value = value;
        Description = description ?? string.Empty;
    }

    /// <inheritdoc />
    public Version Value { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc cref="ISqlDatabaseVersion.Apply(ISqlDatabaseBuilder)" />
    public abstract void Apply(TDatabaseBuilder database);

    /// <summary>
    /// Returns a string representation of this <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return Description.Length > 0 ? $"{Value} ({Description})" : Value.ToString();
    }

    void ISqlDatabaseVersion.Apply(ISqlDatabaseBuilder database)
    {
        Apply( SqlHelpers.CastOrThrow<TDatabaseBuilder>( database, database ) );
    }
}

/// <summary>
/// Creates instances of <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> type.
/// </summary>
public static class SqlDatabaseVersion
{
    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="description">Optional description of this version.</param>
    /// <param name="apply">Delegate that defines this version's changes.</param>
    /// <typeparam name="TDatabaseBuilder">SQL database builder type.</typeparam>
    /// <returns>New <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.</returns>
    [Pure]
    public static SqlDatabaseVersion<TDatabaseBuilder> Create<TDatabaseBuilder>(
        Version value,
        string? description,
        Action<TDatabaseBuilder> apply)
        where TDatabaseBuilder : class, ISqlDatabaseBuilder
    {
        return new SqlLambdaDatabaseVersion<TDatabaseBuilder>( apply, value, description );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="apply">Delegate that defines this version's changes.</param>
    /// <typeparam name="TDatabaseBuilder">SQL database builder type.</typeparam>
    /// <returns>New <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.</returns>
    [Pure]
    public static SqlDatabaseVersion<TDatabaseBuilder> Create<TDatabaseBuilder>(Version value, Action<TDatabaseBuilder> apply)
        where TDatabaseBuilder : class, ISqlDatabaseBuilder
    {
        return Create( value, null, apply );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="description">Optional description of this version.</param>
    /// <param name="apply">Delegate that defines this version's changes.</param>
    /// <returns>New <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.</returns>
    [Pure]
    public static SqlDatabaseVersion<SqlDatabaseBuilder> Create(Version value, string? description, Action<SqlDatabaseBuilder> apply)
    {
        return Create<SqlDatabaseBuilder>( value, description, apply );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="apply">Delegate that defines this version's changes.</param>
    /// <returns>New <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.</returns>
    [Pure]
    public static SqlDatabaseVersion<SqlDatabaseBuilder> Create(Version value, Action<SqlDatabaseBuilder> apply)
    {
        return Create<SqlDatabaseBuilder>( value, apply );
    }
}
