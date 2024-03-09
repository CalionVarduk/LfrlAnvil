using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionUInt8 : MySqlColumnTypeDefinition<byte>
{
    internal MySqlColumnTypeDefinitionUInt8()
        : base( MySqlDataType.UnsignedTinyInt, 0, static (reader, ordinal) => reader.GetByte( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(byte value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(byte value)
    {
        return value;
    }
}
