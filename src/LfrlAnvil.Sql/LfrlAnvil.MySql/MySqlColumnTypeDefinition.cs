using System;
using System.Linq.Expressions;
using LfrlAnvil.Sql;
using MySqlConnector;

namespace LfrlAnvil.MySql;

public abstract class MySqlColumnTypeDefinition<T> : SqlColumnTypeDefinition<T, MySqlDataReader, MySqlParameter>
    where T : notnull
{
    protected MySqlColumnTypeDefinition(MySqlDataType dataType, T defaultValue, Expression<Func<MySqlDataReader, int, T>> outputMapping)
        : base( dataType, defaultValue, outputMapping ) { }

    public new MySqlDataType DataType => ReinterpretCast.To<MySqlDataType>( base.DataType );

    public override void SetParameterInfo(MySqlParameter parameter, bool isNullable)
    {
        base.SetParameterInfo( parameter, isNullable );
        parameter.MySqlDbType = DataType.Value;
        parameter.IsNullable = isNullable;
    }
}
