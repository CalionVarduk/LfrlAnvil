using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDecimal : SqliteColumnTypeDefinition<decimal>
{
    internal SqliteColumnTypeDefinitionDecimal()
        : base( SqliteDataType.Text, 0m ) { }

    [Pure]
    public override string ToDbLiteral(decimal value)
    {
        return value >= 0
            ? value.ToString( "\\'0.0###########################\\'", CultureInfo.InvariantCulture )
            : (-value).ToString( "\\'-0.0###########################\\'", CultureInfo.InvariantCulture );
    }

    public override void SetParameter(IDbDataParameter parameter, decimal value)
    {
        parameter.DbType = System.Data.DbType.String;
        parameter.Value = value.ToString( "0.0###########################", CultureInfo.InvariantCulture );
    }

    public override void SetNullParameter(IDbDataParameter parameter)
    {
        parameter.DbType = System.Data.DbType.String;
        parameter.Value = DBNull.Value;
    }
}
