using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionString : PostgreSqlColumnTypeDefinition<string>
{
    internal PostgreSqlColumnTypeDefinitionString()
        : base( PostgreSqlDataType.VarChar, string.Empty, static (reader, ordinal) => reader.GetString( ordinal ) ) { }

    internal PostgreSqlColumnTypeDefinitionString(PostgreSqlColumnTypeDefinitionString @base, PostgreSqlDataType dataType)
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
