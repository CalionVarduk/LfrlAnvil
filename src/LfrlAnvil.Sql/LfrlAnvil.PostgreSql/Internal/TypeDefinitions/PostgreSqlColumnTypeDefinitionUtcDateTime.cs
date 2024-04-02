using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionUtcDateTime : PostgreSqlColumnTypeDefinition<DateTime>
{
    internal PostgreSqlColumnTypeDefinitionUtcDateTime()
        : base(
            PostgreSqlDataType.TimestampTz,
            DateTime.UnixEpoch,
            static (reader, ordinal) => reader.GetDateTime( ordinal ).ToUniversalTime() ) { }

    [Pure]
    public override string ToDbLiteral(DateTime value)
    {
        return value.ToUniversalTime().ToString( PostgreSqlHelpers.TimestampTzFormatQuoted, CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateTime value)
    {
        return value.ToUniversalTime();
    }
}
