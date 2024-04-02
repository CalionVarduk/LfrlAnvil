using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDateTimeOffset : SqliteColumnTypeDefinition<DateTimeOffset>
{
    internal SqliteColumnTypeDefinitionDateTimeOffset()
        : base(
            SqliteDataType.Text,
            DateTimeOffset.UnixEpoch,
            static (reader, ordinal) => DateTimeOffset.Parse( reader.GetString( ordinal ), CultureInfo.InvariantCulture ) ) { }

    [Pure]
    public override string ToDbLiteral(DateTimeOffset value)
    {
        return value.ToString( SqlHelpers.DateTimeOffsetFormatQuoted, CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateTimeOffset value)
    {
        return value.ToString( SqlHelpers.DateTimeOffsetFormat, CultureInfo.InvariantCulture );
    }
}
