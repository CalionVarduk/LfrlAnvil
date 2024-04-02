using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionChar : MySqlColumnTypeDefinition<char>
{
    internal MySqlColumnTypeDefinitionChar()
        : base( MySqlDataType.CreateChar( 1 ), '0', static (reader, ordinal) => reader.GetChar( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(char value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(char value)
    {
        return value.ToString();
    }
}
