using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionInt32 : PostgreSqlColumnTypeDefinition<int>
{
    internal PostgreSqlColumnTypeDefinitionInt32()
        : base( PostgreSqlDataType.Int4, 0, static (reader, ordinal) => reader.GetInt32( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(int value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(int value)
    {
        return value;
    }
}
