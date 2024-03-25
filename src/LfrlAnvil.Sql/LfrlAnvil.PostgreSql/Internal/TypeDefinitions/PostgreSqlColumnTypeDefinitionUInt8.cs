using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionUInt8 : PostgreSqlColumnTypeDefinition<byte>
{
    internal PostgreSqlColumnTypeDefinitionUInt8()
        : base( PostgreSqlDataType.Int2, 0, static (reader, ordinal) => reader.GetByte( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(byte value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(byte value)
    {
        return (short)value;
    }
}
