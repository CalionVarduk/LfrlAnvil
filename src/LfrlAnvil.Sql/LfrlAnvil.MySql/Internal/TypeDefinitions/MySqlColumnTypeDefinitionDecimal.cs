using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionDecimal : MySqlColumnTypeDefinition<decimal>
{
    internal MySqlColumnTypeDefinitionDecimal()
        : base( MySqlDataType.Decimal, 0m, static (reader, ordinal) => reader.GetDecimal( ordinal ) ) { }

    internal MySqlColumnTypeDefinitionDecimal(MySqlColumnTypeDefinitionDecimal @base, MySqlDataType dataType)
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
