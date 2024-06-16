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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents available options for creating query reader expressions through <see cref="ISqlQueryReaderFactory"/>.
/// </summary>
public readonly struct SqlQueryReaderCreationOptions
{
    /// <summary>
    /// Represents default options.
    /// </summary>
    public static readonly SqlQueryReaderCreationOptions Default = new SqlQueryReaderCreationOptions();

    private readonly List<SqlQueryMemberConfiguration>? _memberConfigurations;
    private readonly int _configurationCount;

    private SqlQueryReaderCreationOptions(
        SqlQueryReaderResultSetFieldsPersistenceMode resultSetFieldsPersistenceMode,
        bool alwaysTestForNull,
        Func<ConstructorInfo, bool>? rowTypeConstructorPredicate,
        Func<MemberInfo, bool>? rowTypeMemberPredicate,
        List<SqlQueryMemberConfiguration>? memberConfigurations)
    {
        ResultSetFieldsPersistenceMode = resultSetFieldsPersistenceMode;
        AlwaysTestForNull = alwaysTestForNull;
        RowTypeConstructorPredicate = rowTypeConstructorPredicate;
        RowTypeMemberPredicate = rowTypeMemberPredicate;
        _memberConfigurations = memberConfigurations;
        _configurationCount = memberConfigurations?.Count ?? 0;
    }

    /// <summary>
    /// <see cref="SqlQueryReaderResultSetFieldsPersistenceMode"/> that specifies how query result set fields should be extracted,
    /// if at all.
    /// </summary>
    public SqlQueryReaderResultSetFieldsPersistenceMode ResultSetFieldsPersistenceMode { get; }

    /// <summary>
    /// Specifies whether or not all source values should be tested for null.
    /// </summary>
    public bool AlwaysTestForNull { get; }

    /// <summary>
    /// Specifies an optional row type's constructor filter. Constructors that return <b>false</b> will be ignored.
    /// </summary>
    /// <remarks>
    /// <see cref="ISqlQueryReaderFactory"/> will use the first encountered constructor with the largest number of parameters,
    /// unless a constructor does not pass this predicate.
    /// </remarks>
    public Func<ConstructorInfo, bool>? RowTypeConstructorPredicate { get; }

    /// <summary>
    /// Specifies an optional row type's field or property filter. Members that return <b>false</b> will be ignored.
    /// </summary>
    public Func<MemberInfo, bool>? RowTypeMemberPredicate { get; }

    /// <summary>
    /// Collection of explicit <see cref="SqlQueryMemberConfiguration"/> instances.
    /// </summary>
    public ReadOnlySpan<SqlQueryMemberConfiguration> MemberConfigurations =>
        CollectionsMarshal.AsSpan( _memberConfigurations ).Slice( 0, _configurationCount );

    /// <summary>
    /// Creates a new <see cref="SqlQueryReaderCreationOptions"/> instance with changed <see cref="ResultSetFieldsPersistenceMode"/>.
    /// </summary>
    /// <param name="mode">Value to set.</param>
    /// <returns>New <see cref="SqlQueryReaderCreationOptions"/> instance.</returns>
    [Pure]
    public SqlQueryReaderCreationOptions SetResultSetFieldsPersistenceMode(SqlQueryReaderResultSetFieldsPersistenceMode mode)
    {
        Ensure.IsDefined( mode );
        return new SqlQueryReaderCreationOptions(
            mode,
            AlwaysTestForNull,
            RowTypeConstructorPredicate,
            RowTypeMemberPredicate,
            _memberConfigurations );
    }

    /// <summary>
    /// Creates a new <see cref="SqlQueryReaderCreationOptions"/> instance with changed <see cref="AlwaysTestForNull"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="SqlQueryReaderCreationOptions"/> instance.</returns>
    [Pure]
    public SqlQueryReaderCreationOptions EnableAlwaysTestingForNull(bool enabled = true)
    {
        return new SqlQueryReaderCreationOptions(
            ResultSetFieldsPersistenceMode,
            enabled,
            RowTypeConstructorPredicate,
            RowTypeMemberPredicate,
            _memberConfigurations );
    }

    /// <summary>
    /// Creates a new <see cref="SqlQueryReaderCreationOptions"/> instance with changed <see cref="RowTypeConstructorPredicate"/>.
    /// </summary>
    /// <param name="predicate">Value to set.</param>
    /// <returns>New <see cref="SqlQueryReaderCreationOptions"/> instance.</returns>
    [Pure]
    public SqlQueryReaderCreationOptions SetRowTypeConstructorPredicate(Func<ConstructorInfo, bool>? predicate)
    {
        return new SqlQueryReaderCreationOptions(
            ResultSetFieldsPersistenceMode,
            AlwaysTestForNull,
            predicate,
            RowTypeMemberPredicate,
            _memberConfigurations );
    }

    /// <summary>
    /// Creates a new <see cref="SqlQueryReaderCreationOptions"/> instance with changed <see cref="RowTypeMemberPredicate"/>.
    /// </summary>
    /// <param name="predicate">Value to set.</param>
    /// <returns>New <see cref="SqlQueryReaderCreationOptions"/> instance.</returns>
    [Pure]
    public SqlQueryReaderCreationOptions SetRowTypeMemberPredicate(Func<MemberInfo, bool>? predicate)
    {
        return new SqlQueryReaderCreationOptions(
            ResultSetFieldsPersistenceMode,
            AlwaysTestForNull,
            RowTypeConstructorPredicate,
            predicate,
            _memberConfigurations );
    }

    /// <summary>
    /// Creates a new <see cref="SqlQueryReaderCreationOptions"/> instance with added <see cref="SqlQueryMemberConfiguration"/> instance.
    /// </summary>
    /// <param name="configuration">Value to add.</param>
    /// <returns>New <see cref="SqlQueryReaderCreationOptions"/> instance.</returns>
    [Pure]
    public SqlQueryReaderCreationOptions With(SqlQueryMemberConfiguration configuration)
    {
        var configurations = _memberConfigurations ?? new List<SqlQueryMemberConfiguration>();
        configurations.Add( configuration );

        return new SqlQueryReaderCreationOptions(
            ResultSetFieldsPersistenceMode,
            AlwaysTestForNull,
            RowTypeConstructorPredicate,
            RowTypeMemberPredicate,
            configurations );
    }

    /// <summary>
    /// Creates a new lookup of current <see cref="MemberConfigurations"/> by member name.
    /// </summary>
    /// <param name="dataReaderType">Source DB data reader type.</param>
    /// <returns>
    /// New <see cref="Dictionary{TKey,TValue}"/> instance or null when no valid <see cref="SqlQueryMemberConfiguration"/> instances exist.
    /// </returns>
    [Pure]
    public Dictionary<string, SqlQueryMemberConfiguration>? CreateMemberConfigurationByNameLookup(Type dataReaderType)
    {
        var configurations = MemberConfigurations;
        if ( configurations.Length == 0 )
            return null;

        var result = new Dictionary<string, SqlQueryMemberConfiguration>(
            capacity: configurations.Length,
            comparer: SqlHelpers.NameComparer );

        foreach ( var cfg in configurations )
        {
            var customMappingDataReaderType = cfg.CustomMappingDataReaderType;
            if ( customMappingDataReaderType is null || dataReaderType.IsAssignableTo( customMappingDataReaderType ) )
                result[cfg.MemberName] = cfg;
        }

        return result.Count == 0 ? null : result;
    }
}
