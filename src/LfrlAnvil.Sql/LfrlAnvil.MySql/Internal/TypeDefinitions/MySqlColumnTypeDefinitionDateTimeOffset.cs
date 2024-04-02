using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionDateTimeOffset : MySqlColumnTypeDefinition<DateTimeOffset>
{
    internal MySqlColumnTypeDefinitionDateTimeOffset()
        : base(
            MySqlDataType.CreateChar( 33 ),
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
