using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionByteArray : PostgreSqlColumnTypeDefinition<byte[]>
{
    internal PostgreSqlColumnTypeDefinitionByteArray(PostgreSqlDataType dataType)
        : base( dataType, Array.Empty<byte>(), static (reader, ordinal) => (byte[])reader.GetValue( ordinal ) ) { }

    internal PostgreSqlColumnTypeDefinitionByteArray(PostgreSqlColumnTypeDefinitionByteArray @base, PostgreSqlDataType dataType)
        : base( dataType, @base.DefaultValue.Value, @base.OutputMapping ) { }

    [Pure]
    public override string ToDbLiteral(byte[] value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(byte[] value)
    {
        return value;
    }
}
