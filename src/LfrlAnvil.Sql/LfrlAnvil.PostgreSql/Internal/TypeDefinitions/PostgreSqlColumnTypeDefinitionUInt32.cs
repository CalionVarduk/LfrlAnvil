using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionUInt32 : PostgreSqlColumnTypeDefinition<uint>
{
    internal PostgreSqlColumnTypeDefinitionUInt32()
        : base( PostgreSqlDataType.Int8, 0, static (reader, ordinal) => unchecked( ( uint )reader.GetInt64( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(uint value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(uint value)
    {
        return ( long )value;
    }
}
