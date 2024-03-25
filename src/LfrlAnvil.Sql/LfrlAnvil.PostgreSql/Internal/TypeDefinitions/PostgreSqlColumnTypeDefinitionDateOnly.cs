using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using LfrlAnvil.Sql.Internal;

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
        const string format = $@"DATE \'{SqlHelpers.DateFormat}\'";
        return value.ToString( format, CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateOnly value)
    {
        return value;
    }
}
