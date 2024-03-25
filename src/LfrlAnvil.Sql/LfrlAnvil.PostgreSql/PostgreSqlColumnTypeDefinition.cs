using System;
using System.Linq.Expressions;
using LfrlAnvil.Sql;
using Npgsql;

namespace LfrlAnvil.PostgreSql;

public abstract class PostgreSqlColumnTypeDefinition<T> : SqlColumnTypeDefinition<T, NpgsqlDataReader, NpgsqlParameter>
    where T : notnull
{
    protected PostgreSqlColumnTypeDefinition(
        PostgreSqlDataType dataType,
        T defaultValue,
        Expression<Func<NpgsqlDataReader, int, T>> outputMapping)
        : base( dataType, defaultValue, outputMapping ) { }

    public new PostgreSqlDataType DataType => ReinterpretCast.To<PostgreSqlDataType>( base.DataType );

    public override void SetParameterInfo(NpgsqlParameter parameter, bool isNullable)
    {
        parameter.NpgsqlDbType = DataType.Value;
        parameter.IsNullable = isNullable;
    }
}
