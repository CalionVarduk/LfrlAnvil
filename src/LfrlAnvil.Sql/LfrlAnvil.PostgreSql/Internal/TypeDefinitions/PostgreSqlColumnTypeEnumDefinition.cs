using System;
using LfrlAnvil.Sql;
using Npgsql;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

public sealed class PostgreSqlColumnTypeEnumDefinition<TEnum, TUnderlying>
    : SqlColumnTypeEnumDefinition<TEnum, TUnderlying, NpgsqlDataReader, NpgsqlParameter>
    where TEnum : struct, Enum
    where TUnderlying : unmanaged
{
    public PostgreSqlColumnTypeEnumDefinition(PostgreSqlColumnTypeDefinition<TUnderlying> @base)
        : base( @base ) { }

    public new PostgreSqlDataType DataType => ReinterpretCast.To<PostgreSqlDataType>( base.DataType );

    public override void SetParameterInfo(NpgsqlParameter parameter, bool isNullable)
    {
        parameter.NpgsqlDbType = DataType.Value;
        parameter.IsNullable = isNullable;
    }
}
