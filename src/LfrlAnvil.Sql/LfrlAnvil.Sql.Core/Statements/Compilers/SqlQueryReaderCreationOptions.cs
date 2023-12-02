using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LfrlAnvil.Sql.Statements.Compilers;

public readonly struct SqlQueryReaderCreationOptions
{
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

    public SqlQueryReaderResultSetFieldsPersistenceMode ResultSetFieldsPersistenceMode { get; }
    public bool AlwaysTestForNull { get; }
    public Func<ConstructorInfo, bool>? RowTypeConstructorPredicate { get; }
    public Func<MemberInfo, bool>? RowTypeMemberPredicate { get; }

    public ReadOnlySpan<SqlQueryMemberConfiguration> MemberConfigurations =>
        CollectionsMarshal.AsSpan( _memberConfigurations ).Slice( 0, _configurationCount );

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

    [Pure]
    public Dictionary<string, SqlQueryMemberConfiguration>? CreateMemberConfigurationByNameLookup(Type dataReaderType)
    {
        var configurations = MemberConfigurations;
        if ( configurations.Length == 0 )
            return null;

        var result = new Dictionary<string, SqlQueryMemberConfiguration>(
            capacity: configurations.Length,
            comparer: StringComparer.OrdinalIgnoreCase );

        foreach ( var cfg in configurations )
        {
            var customMappingDataReaderType = cfg.CustomMappingDataReaderType;
            if ( customMappingDataReaderType is null || dataReaderType.IsAssignableTo( customMappingDataReaderType ) )
                result[cfg.MemberName] = cfg;
        }

        return result.Count == 0 ? null : result;
    }
}
