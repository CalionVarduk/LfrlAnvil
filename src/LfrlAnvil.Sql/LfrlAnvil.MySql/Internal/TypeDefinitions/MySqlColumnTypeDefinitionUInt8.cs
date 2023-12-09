using System.Diagnostics.Contracts;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionUInt8 : MySqlColumnTypeDefinition<byte>
{
    internal MySqlColumnTypeDefinitionUInt8()
        : base( MySqlDataType.UnsignedTinyInt, 0, static (reader, ordinal) => reader.GetByte( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(byte value)
    {
        return MySqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(byte value)
    {
        return value;
    }
}
