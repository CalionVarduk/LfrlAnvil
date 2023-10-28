using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDateTimeOffset : SqliteColumnTypeDefinition<DateTimeOffset>
{
    internal SqliteColumnTypeDefinitionDateTimeOffset()
        : base( SqliteDataType.Text, DateTimeOffset.UnixEpoch ) { }

    [Pure]
    public override string ToDbLiteral(DateTimeOffset value)
    {
        return value.ToString( "\\'yyyy-MM-dd HH:mm:ss.fffffffzzz\\'", CultureInfo.InvariantCulture );
    }

    public override void SetParameter(IDbDataParameter parameter, DateTimeOffset value)
    {
        parameter.DbType = System.Data.DbType.String;
        parameter.Value = value.ToString( "yyyy-MM-dd HH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture );
    }

    public override void SetNullParameter(IDbDataParameter parameter)
    {
        parameter.DbType = System.Data.DbType.String;
        parameter.Value = DBNull.Value;
    }
}
