using System;
using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionByteArray : SqliteColumnTypeDefinition<byte[]>
{
    internal SqliteColumnTypeDefinitionByteArray()
        : base( SqliteDataType.Blob, Array.Empty<byte>() ) { }

    [Pure]
    public override string ToDbLiteral(byte[] value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    public override void SetParameter(IDbDataParameter parameter, byte[] value)
    {
        parameter.DbType = System.Data.DbType.Binary;
        parameter.Value = value;
    }
}
