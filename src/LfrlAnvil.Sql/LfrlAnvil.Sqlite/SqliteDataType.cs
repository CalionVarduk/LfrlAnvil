using System.Data;
using LfrlAnvil.Sql;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

public sealed class SqliteDataType : Enumeration<SqliteDataType, SqliteType>, ISqlDataType
{
    public static readonly SqliteDataType Integer = new SqliteDataType( "INTEGER", SqliteType.Integer, DbType.Int64 );
    public static readonly SqliteDataType Real = new SqliteDataType( "REAL", SqliteType.Real, DbType.Double );
    public static readonly SqliteDataType Text = new SqliteDataType( "TEXT", SqliteType.Text, DbType.String );
    public static readonly SqliteDataType Blob = new SqliteDataType( "BLOB", SqliteType.Blob, DbType.Binary );
    public static readonly SqliteDataType Any = new SqliteDataType( "ANY", 0, DbType.Object );

    private SqliteDataType(string name, SqliteType value, DbType dbType)
        : base( name, value )
    {
        DbType = dbType;
    }

    public DbType DbType { get; }
    public SqlDialect Dialect => SqliteDialect.Instance;
}
