using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionInt64 : MySqlColumnTypeDefinition<long>
{
    internal MySqlColumnTypeDefinitionInt64()
        : base( MySqlDataType.BigInt, 0, static (reader, ordinal) => reader.GetInt64( ordinal ) ) { }

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
