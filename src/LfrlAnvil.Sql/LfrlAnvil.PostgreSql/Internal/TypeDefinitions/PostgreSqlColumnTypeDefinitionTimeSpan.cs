using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionTimeSpan : PostgreSqlColumnTypeDefinition<TimeSpan>
{
    internal PostgreSqlColumnTypeDefinitionTimeSpan()
        : base( PostgreSqlDataType.Int8, TimeSpan.Zero, static (reader, ordinal) => TimeSpan.FromTicks( reader.GetInt64( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(TimeSpan value)
    {
        return SqlHelpers.GetDbLiteral( value.Ticks );
    }

    [Pure]
    public override object ToParameterValue(TimeSpan value)
    {
        return value.Ticks;
    }
}
