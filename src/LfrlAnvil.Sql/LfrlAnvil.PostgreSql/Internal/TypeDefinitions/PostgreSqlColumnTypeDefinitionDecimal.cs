using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionDecimal : PostgreSqlColumnTypeDefinition<decimal>
{
    internal PostgreSqlColumnTypeDefinitionDecimal()
        : base( PostgreSqlDataType.Decimal, 0m, static (reader, ordinal) => reader.GetDecimal( ordinal ) ) { }

    internal PostgreSqlColumnTypeDefinitionDecimal(PostgreSqlColumnTypeDefinitionDecimal @base, PostgreSqlDataType dataType)
        : base( dataType, @base.DefaultValue.Value, @base.OutputMapping ) { }

    [Pure]
    public override string ToDbLiteral(decimal value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(decimal value)
    {
        return value;
    }
}
