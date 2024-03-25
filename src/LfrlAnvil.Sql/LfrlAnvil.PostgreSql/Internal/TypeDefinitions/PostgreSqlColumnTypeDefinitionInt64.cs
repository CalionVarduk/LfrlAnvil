using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionInt64 : PostgreSqlColumnTypeDefinition<long>
{
    internal PostgreSqlColumnTypeDefinitionInt64()
        : base( PostgreSqlDataType.Int8, 0, static (reader, ordinal) => reader.GetInt64( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(long value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(long value)
    {
        return value;
    }
}
