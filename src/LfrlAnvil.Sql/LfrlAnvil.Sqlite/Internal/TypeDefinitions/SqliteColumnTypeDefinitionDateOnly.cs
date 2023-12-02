using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDateOnly : SqliteColumnTypeDefinition<DateOnly>
{
    internal SqliteColumnTypeDefinitionDateOnly()
        : base(
            SqliteDataType.Text,
            DateOnly.FromDateTime( DateTime.UnixEpoch ),
            static (reader, ordinal) => DateOnly.Parse( reader.GetString( ordinal ), CultureInfo.InvariantCulture ) ) { }

    [Pure]
    public override string ToDbLiteral(DateOnly value)
    {
        return value.ToString( "\\'yyyy-MM-dd\\'", CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(DateOnly value)
    {
        return value.ToString( "yyyy-MM-dd", CultureInfo.InvariantCulture );
    }
}
