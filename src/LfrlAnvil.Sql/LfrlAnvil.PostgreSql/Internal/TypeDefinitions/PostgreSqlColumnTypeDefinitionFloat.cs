using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionFloat : PostgreSqlColumnTypeDefinition<float>
{
    internal PostgreSqlColumnTypeDefinitionFloat()
        : base( PostgreSqlDataType.Float4, 0.0F, static (reader, ordinal) => reader.GetFloat( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(float value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(float value)
    {
        return value;
    }
}
