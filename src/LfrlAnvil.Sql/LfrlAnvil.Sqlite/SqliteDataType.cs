using System;
using System.Data;
using LfrlAnvil.Sql;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

/// <inheritdoc cref="ISqlDataType" />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteDataType : Enumeration<SqliteDataType, SqliteType>, ISqlDataType
{
    /// <summary>
    /// Represents the <b>INTEGER</b> type.
    /// </summary>
    public static readonly SqliteDataType Integer = new SqliteDataType( "INTEGER", SqliteType.Integer, DbType.Int64 );

    /// <summary>
    /// Represents the <b>REAL</b> type.
    /// </summary>
    public static readonly SqliteDataType Real = new SqliteDataType( "REAL", SqliteType.Real, DbType.Double );

    /// <summary>
    /// Represents the <b>TEXT</b> type.
    /// </summary>
    public static readonly SqliteDataType Text = new SqliteDataType( "TEXT", SqliteType.Text, DbType.String );

    /// <summary>
    /// Represents the <b>BLOB</b> type.
    /// </summary>
    public static readonly SqliteDataType Blob = new SqliteDataType( "BLOB", SqliteType.Blob, DbType.Binary );

    /// <summary>
    /// Represents the <b>ANY</b> type.
    /// </summary>
    public static readonly SqliteDataType Any = new SqliteDataType( "ANY", 0, DbType.Object );

    private SqliteDataType(string name, SqliteType value, DbType dbType)
        : base( name, value )
    {
        DbType = dbType;
    }

    /// <inheritdoc />
    public DbType DbType { get; }

    /// <inheritdoc />
    public SqlDialect Dialect => SqliteDialect.Instance;

    ReadOnlySpan<int> ISqlDataType.Parameters => ReadOnlySpan<int>.Empty;
    ReadOnlySpan<SqlDataTypeParameter> ISqlDataType.ParameterDefinitions => ReadOnlySpan<SqlDataTypeParameter>.Empty;
}
