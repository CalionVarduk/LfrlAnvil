using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionUInt16 : MySqlColumnTypeDefinition<ushort>
{
    internal MySqlColumnTypeDefinitionUInt16()
        : base( MySqlDataType.UnsignedSmallInt, 0, static (reader, ordinal) => reader.GetUInt16( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(ushort value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(ushort value)
    {
        return value;
    }
}
