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
        const string format = "TI\\MESTA\\MPTZ\\'yyyy-MM-dd HH:mm:ss.ffffff\\'";
        return value.ToUniversalTime().ToString( format, CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateTime value)
    {
        return value.ToUniversalTime();
    }
}
