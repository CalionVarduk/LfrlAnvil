using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionTimeSpan : SqliteColumnTypeDefinition<TimeSpan, long>
{
    internal SqliteColumnTypeDefinitionTimeSpan(SqliteColumnTypeDefinitionInt64 @base)
        : base( @base, TimeSpan.FromTicks( @base.DefaultValue ) ) { }

    [Pure]
    protected override long MapToBaseType(TimeSpan value)
    {
        return value.Ticks;
    }
}
