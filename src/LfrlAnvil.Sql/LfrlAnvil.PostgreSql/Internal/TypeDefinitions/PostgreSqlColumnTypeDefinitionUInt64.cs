using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionUInt64 : PostgreSqlColumnTypeDefinition<ulong>
{
    internal PostgreSqlColumnTypeDefinitionUInt64()
        : base( PostgreSqlDataType.Int8, 0, static (reader, ordinal) => unchecked( ( ulong )reader.GetInt64( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(ulong value)
    {
        return SqlHelpers.GetDbLiteral( checked( ( long )value ) );
    }

    [Pure]
    public override object ToParameterValue(ulong value)
    {
        return checked( ( long )value );
    }
}
