using System.Diagnostics.Contracts;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionInt8 : MySqlColumnTypeDefinition<sbyte>
{
    internal MySqlColumnTypeDefinitionInt8()
        : base( MySqlDataType.TinyInt, 0, static (reader, ordinal) => reader.GetSByte( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(sbyte value)
    {
        return MySqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(sbyte value)
    {
        return value;
    }
}
