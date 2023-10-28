using System;
using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionInt64 : SqliteColumnTypeDefinition<long>
{
    internal SqliteColumnTypeDefinitionInt64()
        : base( SqliteDataType.Integer, 0 ) { }

    [Pure]
    public override string ToDbLiteral(long value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    public override void SetParameter(IDbDataParameter parameter, long value)
    {
        parameter.DbType = System.Data.DbType.Int64;
        parameter.Value = value;
    }

    public override void SetNullParameter(IDbDataParameter parameter)
    {
        parameter.DbType = System.Data.DbType.Int64;
        parameter.Value = DBNull.Value;
    }
}
