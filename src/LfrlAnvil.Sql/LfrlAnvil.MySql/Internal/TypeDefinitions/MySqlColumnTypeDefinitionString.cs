using System.Diagnostics.Contracts;

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
        return MySqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(string value)
    {
        return value;
    }
}
