using LfrlAnvil.Sql;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

public sealed class SqliteDataType : Enumeration<SqliteDataType, SqliteType>, ISqlDataType
{
    public static readonly SqliteDataType Integer = new SqliteDataType( "INTEGER", SqliteType.Integer );
    public static readonly SqliteDataType Real = new SqliteDataType( "REAL", SqliteType.Real );
    public static readonly SqliteDataType Text = new SqliteDataType( "TEXT", SqliteType.Text );
    public static readonly SqliteDataType Blob = new SqliteDataType( "BLOB", SqliteType.Blob );
    public static readonly SqliteDataType Any = new SqliteDataType( "ANY", 0 );

    private SqliteDataType(string name, SqliteType value)
        : base( name, value ) { }

    public SqlDialect Dialect => SqliteDialect.Instance;
    ISqlDataType? ISqlDataType.ParentType => null;
}
