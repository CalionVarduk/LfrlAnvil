using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionInt8 : PostgreSqlColumnTypeDefinition<sbyte>
{
    internal PostgreSqlColumnTypeDefinitionInt8()
        : base( PostgreSqlDataType.Int2, 0, static (reader, ordinal) => (sbyte)reader.GetInt16( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(sbyte value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(sbyte value)
    {
        return (short)value;
    }
}
