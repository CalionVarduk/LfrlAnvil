using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

public abstract class SqliteDatabase : ISqlDatabase
{
    private readonly Func<SqliteCommand, List<SqlDatabaseVersionRecord>> _versionRecordsReader;

    internal SqliteDatabase(
        SqliteDatabaseBuilder builder,
        Func<SqliteCommand, List<SqlDatabaseVersionRecord>> versionRecordsReader,
        Version version)
    {
        _versionRecordsReader = versionRecordsReader;
        Version = version;
        DataTypes = builder.DataTypes;
        TypeDefinitions = builder.TypeDefinitions;
        Schemas = new SqliteSchemaCollection( this, builder.Schemas );
    }

    public Version Version { get; }
    public SqliteSchemaCollection Schemas { get; }
    public SqliteDataTypeProvider DataTypes { get; }
    public SqliteColumnTypeDefinitionProvider TypeDefinitions { get; }

    ISqlSchemaCollection ISqlDatabase.Schemas => Schemas;
    ISqlDataTypeProvider ISqlDatabase.DataTypes => DataTypes;
    ISqlColumnTypeDefinitionProvider ISqlDatabase.TypeDefinitions => TypeDefinitions;

    [Pure]
    public abstract SqliteConnection Connect();

    [Pure]
    public SqlDatabaseVersionRecord[] GetRegisteredVersions()
    {
        using var connection = Connect();
        using var command = connection.CreateCommand();
        return _versionRecordsReader( command ).ToArray();
    }

    public virtual void Dispose() { }

    [Pure]
    IDbConnection ISqlDatabase.Connect()
    {
        return Connect();
    }
}
