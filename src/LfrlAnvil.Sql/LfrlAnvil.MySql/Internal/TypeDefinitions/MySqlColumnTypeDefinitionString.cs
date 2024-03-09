using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionString : MySqlColumnTypeDefinition<string>
{
    internal MySqlColumnTypeDefinitionString()
        : base( MySqlDataType.Text, string.Empty, static (reader, ordinal) => reader.GetString( ordinal ) ) { }

    internal MySqlColumnTypeDefinitionString(MySqlColumnTypeDefinitionString @base, MySqlDataType dataType)
        : base( dataType, @base.DefaultValue.Value, @base.OutputMapping ) { }

    [Pure]
    public override string ToDbLiteral(string value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(string value)
    {
        return value;
    }
}
