using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionTimeOnly : PostgreSqlColumnTypeDefinition<TimeOnly>
{
    internal PostgreSqlColumnTypeDefinitionTimeOnly()
        : base(
            PostgreSqlDataType.Time,
            TimeOnly.MinValue,
            static (reader, ordinal) => TimeOnly.FromTimeSpan( reader.GetTimeSpan( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(TimeOnly value)
    {
        return value.ToString( "TI\\ME\\'HH:mm:ss.ffffff\\'", CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(TimeOnly value)
    {
        return value;
    }
}
