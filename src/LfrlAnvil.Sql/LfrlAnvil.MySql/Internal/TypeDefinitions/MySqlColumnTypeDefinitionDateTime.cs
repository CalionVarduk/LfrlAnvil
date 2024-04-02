using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionDateTime : MySqlColumnTypeDefinition<DateTime>
{
    internal MySqlColumnTypeDefinitionDateTime()
        : base(
            MySqlDataType.DateTime,
            DateTime.SpecifyKind( DateTime.UnixEpoch, DateTimeKind.Unspecified ),
            static (reader, ordinal) => reader.GetDateTime( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(DateTime value)
    {
        return value.ToString( SqlHelpers.DateTimeFormatMicrosecondQuoted, CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateTime value)
    {
        return DateTime.SpecifyKind( value, DateTimeKind.Unspecified );
    }
}
