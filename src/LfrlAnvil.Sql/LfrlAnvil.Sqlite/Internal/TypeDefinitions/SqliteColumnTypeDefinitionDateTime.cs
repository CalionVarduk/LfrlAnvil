using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using LfrlAnvil.Sql.Internal;

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
        const string format = $@"\'{SqlHelpers.DateTimeFormat}\'";
        return value.ToString( format, CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateTime value)
    {
        return value.ToString( SqlHelpers.DateTimeFormat, CultureInfo.InvariantCulture );
    }
}
