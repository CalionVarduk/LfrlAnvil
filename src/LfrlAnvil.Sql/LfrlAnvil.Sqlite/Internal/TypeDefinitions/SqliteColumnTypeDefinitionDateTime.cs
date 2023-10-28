using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDateTime : SqliteColumnTypeDefinition<DateTime>
{
    internal SqliteColumnTypeDefinitionDateTime()
        : base( SqliteDataType.Text, DateTime.UnixEpoch ) { }

    [Pure]
    public override string ToDbLiteral(DateTime value)
    {
        return value.ToString( "\\'yyyy-MM-dd HH:mm:ss.fffffff\\'", CultureInfo.InvariantCulture );
    }

    public override void SetParameter(IDbDataParameter parameter, DateTime value)
    {
        parameter.DbType = System.Data.DbType.String;
        parameter.Value = value.ToString( "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture );
    }

    public override void SetNullParameter(IDbDataParameter parameter)
    {
        parameter.DbType = System.Data.DbType.String;
        parameter.Value = DBNull.Value;
    }
}
