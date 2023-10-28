using System;
using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionBool : SqliteColumnTypeDefinition<bool>
{
    private static readonly object Zero = 0L;
    private static readonly object One = 1L;

    internal SqliteColumnTypeDefinitionBool()
        : base( SqliteDataType.Integer, false ) { }

    [Pure]
    public override string ToDbLiteral(bool value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    public override void SetParameter(IDbDataParameter parameter, bool value)
    {
        parameter.DbType = System.Data.DbType.Int64;
        parameter.Value = value ? One : Zero;
    }

    public override void SetNullParameter(IDbDataParameter parameter)
    {
        parameter.DbType = System.Data.DbType.Int64;
        parameter.Value = DBNull.Value;
    }
}
