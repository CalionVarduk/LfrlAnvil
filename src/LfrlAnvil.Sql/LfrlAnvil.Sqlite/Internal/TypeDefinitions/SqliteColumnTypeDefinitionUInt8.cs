using System;
using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionUInt8 : SqliteColumnTypeDefinition<byte>
{
    internal SqliteColumnTypeDefinitionUInt8()
        : base( SqliteDataType.Integer, 0 ) { }

    [Pure]
    public override string ToDbLiteral(byte value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    public override void SetParameter(IDbDataParameter parameter, byte value)
    {
        parameter.DbType = System.Data.DbType.Int64;
        parameter.Value = (long)value;
    }

    public override void SetNullParameter(IDbDataParameter parameter)
    {
        parameter.DbType = System.Data.DbType.Int64;
        parameter.Value = DBNull.Value;
    }
}
