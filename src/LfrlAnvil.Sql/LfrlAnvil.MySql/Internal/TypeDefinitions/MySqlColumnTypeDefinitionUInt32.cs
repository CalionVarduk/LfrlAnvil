using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionUInt32 : MySqlColumnTypeDefinition<uint>
{
    internal MySqlColumnTypeDefinitionUInt32()
        : base( MySqlDataType.UnsignedInt, 0, static (reader, ordinal) => reader.GetUInt32( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(uint value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(uint value)
    {
        return value;
    }
}
