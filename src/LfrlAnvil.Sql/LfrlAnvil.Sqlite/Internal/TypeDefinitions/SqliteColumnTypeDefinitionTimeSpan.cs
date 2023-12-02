using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionTimeSpan : SqliteColumnTypeDefinition<TimeSpan>
{
    internal SqliteColumnTypeDefinitionTimeSpan()
        : base( SqliteDataType.Integer, TimeSpan.Zero, static (reader, ordinal) => TimeSpan.FromTicks( reader.GetInt64( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(TimeSpan value)
    {
        return SqliteHelpers.GetDbLiteral( value.Ticks );
    }

    [Pure]
    public override object ToParameterValue(TimeSpan value)
    {
        return value.Ticks;
    }
}
