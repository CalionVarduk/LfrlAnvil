using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionInt16 : MySqlColumnTypeDefinition<short>
{
    internal MySqlColumnTypeDefinitionInt16()
        : base( MySqlDataType.SmallInt, 0, static (reader, ordinal) => reader.GetInt16( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(short value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(short value)
    {
        return value;
    }
}
