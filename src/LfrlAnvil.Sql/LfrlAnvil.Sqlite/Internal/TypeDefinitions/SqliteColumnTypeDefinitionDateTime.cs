using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDateTime : SqliteColumnTypeDefinition<DateTime>
{
    internal SqliteColumnTypeDefinitionDateTime()
        : base(
            SqliteDataType.Text,
            DateTime.UnixEpoch,
            static (reader, ordinal) => DateTime.Parse( reader.GetString( ordinal ), CultureInfo.InvariantCulture ) ) { }

    [Pure]
    public override string ToDbLiteral(DateTime value)
    {
        return value.ToString( "\\'yyyy-MM-dd HH:mm:ss.fffffff\\'", CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateTime value)
    {
        return value.ToString( "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture );
    }
}
