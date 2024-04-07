using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionByteArray : PostgreSqlColumnTypeDefinition<byte[]>
{
    internal PostgreSqlColumnTypeDefinitionByteArray()
        : base( PostgreSqlDataType.Bytea, Array.Empty<byte>(), static (reader, ordinal) => ( byte[] )reader.GetValue( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(byte[] value)
    {
        return PostgreSqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(byte[] value)
    {
        return value;
    }
}
