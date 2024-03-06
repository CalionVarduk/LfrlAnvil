using System;
using System.Linq.Expressions;
using LfrlAnvil.Sql;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

public abstract class SqliteColumnTypeDefinition<T> : SqlColumnTypeDefinition<T, SqliteDataReader, SqliteParameter>
    where T : notnull
{
    protected SqliteColumnTypeDefinition(SqliteDataType dataType, T defaultValue, Expression<Func<SqliteDataReader, int, T>> outputMapping)
        : base( dataType, defaultValue, outputMapping ) { }

    public new SqliteDataType DataType => ReinterpretCast.To<SqliteDataType>( base.DataType );

    public override void SetParameterInfo(SqliteParameter parameter, bool isNullable)
    {
        base.SetParameterInfo( parameter, isNullable );
        parameter.SqliteType = DataType.Value;
        parameter.IsNullable = isNullable;
    }
}
