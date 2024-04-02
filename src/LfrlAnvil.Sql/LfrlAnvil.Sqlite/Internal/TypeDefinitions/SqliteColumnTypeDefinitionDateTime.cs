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
            DateTime.SpecifyKind( DateTime.UnixEpoch, DateTimeKind.Unspecified ),
            static (reader, ordinal) => DateTime.Parse( reader.GetString( ordinal ), CultureInfo.InvariantCulture ) ) { }

    [Pure]
    public override string ToDbLiteral(DateTime value)
    {
        return value.ToString( SqlHelpers.DateTimeFormatTickQuoted, CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateTime value)
    {
        return value.ToString( SqlHelpers.DateTimeFormatTick, CultureInfo.InvariantCulture );
    }
}
