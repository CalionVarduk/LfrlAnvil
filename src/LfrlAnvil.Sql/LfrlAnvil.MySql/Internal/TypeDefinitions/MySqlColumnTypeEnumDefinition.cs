using System;
using LfrlAnvil.Sql;
using MySqlConnector;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

public sealed class MySqlColumnTypeEnumDefinition<TEnum, TUnderlying>
    : SqlColumnTypeEnumDefinition<TEnum, TUnderlying, MySqlDataReader, MySqlParameter>
    where TEnum : struct, Enum
    where TUnderlying : unmanaged
{
    public MySqlColumnTypeEnumDefinition(MySqlColumnTypeDefinition<TUnderlying> @base)
        : base( @base ) { }

    public new MySqlDataType DataType => ReinterpretCast.To<MySqlDataType>( base.DataType );

    public override void SetParameterInfo(MySqlParameter parameter, bool isNullable)
    {
        base.SetParameterInfo( parameter, isNullable );
        parameter.MySqlDbType = DataType.Value;
        parameter.IsNullable = isNullable;
    }
}
