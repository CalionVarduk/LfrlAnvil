using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sqlite;

public readonly struct SqliteNodeInterpreterOptions
{
    public static readonly SqliteNodeInterpreterOptions Default = new SqliteNodeInterpreterOptions();

    private readonly bool _isUpdateFromDisabled;
    private readonly bool _isUpdateOrDeleteLimitDisabled;

    private SqliteNodeInterpreterOptions(
        SqliteColumnTypeDefinitionProvider? typeDefinitions,
        bool isStrictModeEnabled,
        bool isUpdateFromDisabled,
        bool isUpdateOrDeleteLimitDisabled)
    {
        TypeDefinitions = typeDefinitions;
        IsStrictModeEnabled = isStrictModeEnabled;
        _isUpdateFromDisabled = isUpdateFromDisabled;
        _isUpdateOrDeleteLimitDisabled = isUpdateOrDeleteLimitDisabled;
    }

    public SqliteColumnTypeDefinitionProvider? TypeDefinitions { get; }
    public bool IsStrictModeEnabled { get; }
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
            _isUpdateOrDeleteLimitDisabled );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteNodeInterpreterOptions EnableStrictMode(bool enabled = true)
    {
        return new SqliteNodeInterpreterOptions( TypeDefinitions, enabled, _isUpdateFromDisabled, _isUpdateOrDeleteLimitDisabled );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteNodeInterpreterOptions EnableUpdateFrom(bool enabled = true)
    {
        return new SqliteNodeInterpreterOptions( TypeDefinitions, IsStrictModeEnabled, ! enabled, _isUpdateOrDeleteLimitDisabled );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteNodeInterpreterOptions EnableUpdateOrDeleteLimit(bool enabled = true)
    {
        return new SqliteNodeInterpreterOptions( TypeDefinitions, IsStrictModeEnabled, _isUpdateFromDisabled, ! enabled );
    }
}
