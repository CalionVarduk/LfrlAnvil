using System;
using LfrlAnvil.Sql;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

public sealed class SqliteColumnTypeEnumDefinition<TEnum, TUnderlying>
    : SqlColumnTypeEnumDefinition<TEnum, TUnderlying, SqliteDataReader, SqliteParameter>
    where TEnum : struct, Enum
    where TUnderlying : unmanaged
{
    public SqliteColumnTypeEnumDefinition(SqliteColumnTypeDefinition<TUnderlying> @base)
        : base( @base ) { }

    public new SqliteDataType DataType => ReinterpretCast.To<SqliteDataType>( base.DataType );

    public override void SetParameterInfo(SqliteParameter parameter, bool isNullable)
    {
        base.SetParameterInfo( parameter, isNullable );
        parameter.SqliteType = DataType.Value;
        parameter.IsNullable = isNullable;
    }
}
