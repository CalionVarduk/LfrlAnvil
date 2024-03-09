using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionByteArray : MySqlColumnTypeDefinition<byte[]>
{
    internal MySqlColumnTypeDefinitionByteArray()
        : base( MySqlDataType.Blob, Array.Empty<byte>(), static (reader, ordinal) => (byte[])reader.GetValue( ordinal ) ) { }

    internal MySqlColumnTypeDefinitionByteArray(MySqlColumnTypeDefinitionByteArray @base, MySqlDataType dataType)
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
