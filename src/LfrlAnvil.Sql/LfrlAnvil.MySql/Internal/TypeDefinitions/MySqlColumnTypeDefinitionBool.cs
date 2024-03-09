using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionBool : MySqlColumnTypeDefinition<bool>
{
    internal MySqlColumnTypeDefinitionBool()
        : base( MySqlDataType.Bool, false, static (reader, ordinal) => reader.GetBoolean( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(bool value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(bool value)
    {
        return value;
    }
}
