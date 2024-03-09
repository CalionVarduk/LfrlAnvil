using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionUInt64 : MySqlColumnTypeDefinition<ulong>
{
    internal MySqlColumnTypeDefinitionUInt64()
        : base( MySqlDataType.UnsignedBigInt, 0, static (reader, ordinal) => reader.GetUInt64( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(ulong value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(ulong value)
    {
        return value;
    }
}
