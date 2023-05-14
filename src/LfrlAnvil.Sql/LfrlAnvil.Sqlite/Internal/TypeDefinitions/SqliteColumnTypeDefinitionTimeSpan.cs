using System;
using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionTimeSpan : SqliteColumnTypeDefinition<TimeSpan>
{
    internal SqliteColumnTypeDefinitionTimeSpan()
        : base( SqliteDataType.Integer, TimeSpan.Zero ) { }

    [Pure]
    public override string ToDbLiteral(TimeSpan value)
    {
        return SqliteHelpers.GetDbLiteral( value.Ticks );
    }

    public override void SetParameter(IDbDataParameter parameter, TimeSpan value)
    {
        parameter.DbType = System.Data.DbType.Int64;
        parameter.Value = value.Ticks;
    }
}
