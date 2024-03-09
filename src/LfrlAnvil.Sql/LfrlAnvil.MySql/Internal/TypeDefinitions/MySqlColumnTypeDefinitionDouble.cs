using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionDouble : MySqlColumnTypeDefinition<double>
{
    internal MySqlColumnTypeDefinitionDouble()
        : base( MySqlDataType.Double, 0.0, static (reader, ordinal) => reader.GetDouble( ordinal ) ) { }

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
