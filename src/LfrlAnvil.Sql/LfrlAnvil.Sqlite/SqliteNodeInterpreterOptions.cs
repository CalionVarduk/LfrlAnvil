using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sqlite;

public readonly struct SqliteNodeInterpreterOptions
{
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
        SqliteUpsertOptions upsertOptions)
    {
        TypeDefinitions = typeDefinitions;
        IsStrictModeEnabled = isStrictModeEnabled;
        _isUpdateFromDisabled = isUpdateFromDisabled;
        _isUpdateOrDeleteLimitDisabled = isUpdateOrDeleteLimitDisabled;
        IsAggregateFunctionOrderingEnabled = isAggregateFunctionOrderingEnabled;
        UpsertOptions = upsertOptions;
    }

    public SqliteColumnTypeDefinitionProvider? TypeDefinitions { get; }
    public bool IsStrictModeEnabled { get; }
    public SqliteUpsertOptions UpsertOptions { get; }
    public bool IsAggregateFunctionOrderingEnabled { get; }
    public bool IsUpdateFromEnabled => ! _isUpdateFromDisabled;
    public bool IsUpdateOrDeleteLimitEnabled => ! _isUpdateOrDeleteLimitDisabled;

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
            UpsertOptions );
    }

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
            UpsertOptions );
    }

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
            UpsertOptions );
    }

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
            UpsertOptions );
    }

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
            UpsertOptions );
    }

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
            options );
    }
}
