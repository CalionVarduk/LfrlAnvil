using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionDateTime : PostgreSqlColumnTypeDefinition<DateTime>
{
    internal PostgreSqlColumnTypeDefinitionDateTime(PostgreSqlDataType dataType)
        : base(
            dataType,
            DateTime.SpecifyKind( DateTime.UnixEpoch, DateTimeKind.Unspecified ),
            static (reader, ordinal) => reader.GetDateTime( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(DateTime value)
    {
        const string format = "TI\\MESTA\\MP\\'yyyy-MM-dd HH:mm:ss.ffffff\\'";
        return value.ToString( format, CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateTime value)
    {
        return DateTime.SpecifyKind( value, DateTimeKind.Unspecified );
    }
}
