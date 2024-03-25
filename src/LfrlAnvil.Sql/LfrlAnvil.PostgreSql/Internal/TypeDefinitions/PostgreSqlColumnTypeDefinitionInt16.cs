using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionInt16 : PostgreSqlColumnTypeDefinition<short>
{
    internal PostgreSqlColumnTypeDefinitionInt16()
        : base( PostgreSqlDataType.Int2, 0, static (reader, ordinal) => reader.GetInt16( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(short value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(short value)
    {
        return value;
    }
}
