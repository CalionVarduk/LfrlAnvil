using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionTimeSpan : MySqlColumnTypeDefinition<TimeSpan>
{
    internal MySqlColumnTypeDefinitionTimeSpan()
        : base( MySqlDataType.BigInt, TimeSpan.Zero, static (reader, ordinal) => TimeSpan.FromTicks( reader.GetInt64( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(TimeSpan value)
    {
        return MySqlHelpers.GetDbLiteral( value.Ticks );
    }

    [Pure]
    public override object ToParameterValue(TimeSpan value)
    {
        return value.Ticks;
    }
}
