using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionTimeOnly : SqliteColumnTypeDefinition<TimeOnly>
{
    internal SqliteColumnTypeDefinitionTimeOnly()
        : base(
            SqliteDataType.Text,
            TimeOnly.MinValue,
            static (reader, ordinal) => TimeOnly.Parse( reader.GetString( ordinal ), CultureInfo.InvariantCulture ) ) { }

    [Pure]
    public override string ToDbLiteral(TimeOnly value)
    {
        const string format = $@"\'{SqlHelpers.TimeFormat}\'";
        return value.ToString( format, CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(TimeOnly value)
    {
        return value.ToString( SqlHelpers.TimeFormat, CultureInfo.InvariantCulture );
    }
}
