using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionDouble : PostgreSqlColumnTypeDefinition<double>
{
    internal PostgreSqlColumnTypeDefinitionDouble()
        : base( PostgreSqlDataType.Float8, 0.0, static (reader, ordinal) => reader.GetDouble( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(double value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(double value)
    {
        return value;
    }
}
