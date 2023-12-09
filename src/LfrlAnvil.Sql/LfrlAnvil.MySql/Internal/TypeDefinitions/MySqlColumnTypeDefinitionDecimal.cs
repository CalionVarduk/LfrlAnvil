using System.Diagnostics.Contracts;
using System.Globalization;

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
        return value.ToString( "0.0###########################", CultureInfo.InvariantCulture );
    }

    [Pure]
    public override object ToParameterValue(decimal value)
    {
        return value;
    }
}
