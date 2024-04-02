using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionDateOnly : PostgreSqlColumnTypeDefinition<DateOnly>
{
    internal PostgreSqlColumnTypeDefinitionDateOnly()
        : base(
            PostgreSqlDataType.Date,
            DateOnly.FromDateTime( DateTime.UnixEpoch ),
            static (reader, ordinal) => DateOnly.FromDateTime( reader.GetDateTime( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(DateOnly value)
    {
        return value.ToString( PostgreSqlHelpers.DateFormatQuoted, CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateOnly value)
    {
        return value;
    }
}
