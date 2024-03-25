using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionBool : PostgreSqlColumnTypeDefinition<bool>
{
    internal PostgreSqlColumnTypeDefinitionBool()
        : base( PostgreSqlDataType.Boolean, false, static (reader, ordinal) => reader.GetBoolean( ordinal ) ) { }

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
