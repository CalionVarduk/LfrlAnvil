using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionTimeOnly : SqliteColumnTypeDefinition<TimeOnly>
{
    internal SqliteColumnTypeDefinitionTimeOnly()
        : base( SqliteDataType.Text, TimeOnly.MinValue ) { }

    [Pure]
    public override string ToDbLiteral(TimeOnly value)
    {
        return value.ToString( "\\'HH:mm:ss.fffffff\\'", CultureInfo.InvariantCulture );
    }

    public override void SetParameter(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = System.Data.DbType.String;
        parameter.Value = value.ToString( "HH:mm:ss.fffffff", CultureInfo.InvariantCulture );
    }
}
