using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionChar : PostgreSqlColumnTypeDefinition<char>
{
    internal PostgreSqlColumnTypeDefinitionChar()
        : base( PostgreSqlDataType.CreateVarChar( 1 ), '0', static (reader, ordinal) => reader.GetChar( ordinal ) ) { }

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
