using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionDateTime : PostgreSqlColumnTypeDefinition<DateTime>
{
    internal PostgreSqlColumnTypeDefinitionDateTime()
        : base(
            PostgreSqlDataType.Timestamp,
            DateTime.SpecifyKind( DateTime.UnixEpoch, DateTimeKind.Unspecified ),
            static (reader, ordinal) => reader.GetDateTime( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(DateTime value)
    {
        return value.ToString( PostgreSqlHelpers.TimestampFormatQuoted, CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateTime value)
    {
        return DateTime.SpecifyKind( value, DateTimeKind.Unspecified );
    }
}
