using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionUInt16 : PostgreSqlColumnTypeDefinition<ushort>
{
    internal PostgreSqlColumnTypeDefinitionUInt16()
        : base( PostgreSqlDataType.Int4, 0, static (reader, ordinal) => unchecked( (ushort)reader.GetInt32( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(ushort value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(ushort value)
    {
        return (int)value;
    }
}
