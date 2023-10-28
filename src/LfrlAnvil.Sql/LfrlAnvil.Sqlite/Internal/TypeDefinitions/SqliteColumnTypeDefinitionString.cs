using System;
using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionString : SqliteColumnTypeDefinition<string>
{
    internal SqliteColumnTypeDefinitionString()
        : base( SqliteDataType.Text, string.Empty ) { }

    [Pure]
    public override string ToDbLiteral(string value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    public override void SetParameter(IDbDataParameter parameter, string value)
    {
        parameter.DbType = System.Data.DbType.String;
        parameter.Value = value;
    }

    public override void SetNullParameter(IDbDataParameter parameter)
    {
        parameter.DbType = System.Data.DbType.String;
        parameter.Value = DBNull.Value;
    }
}
