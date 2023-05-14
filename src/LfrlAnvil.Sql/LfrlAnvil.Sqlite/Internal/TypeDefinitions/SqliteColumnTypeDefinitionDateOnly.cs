using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDateOnly : SqliteColumnTypeDefinition<DateOnly>
{
    internal SqliteColumnTypeDefinitionDateOnly()
        : base( SqliteDataType.Text, DateOnly.FromDateTime( DateTime.UnixEpoch ) ) { }

    [Pure]
    public override string ToDbLiteral(DateOnly value)
    {
        return value.ToString( "\\'yyyy-MM-dd\\'", CultureInfo.InvariantCulture );
    }

    public override void SetParameter(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = System.Data.DbType.String;
        parameter.Value = value.ToString( "yyyy-MM-dd", CultureInfo.InvariantCulture );
    }
}
