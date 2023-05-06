using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionUInt8 : SqliteColumnTypeDefinition<byte, long>
{
    internal SqliteColumnTypeDefinitionUInt8(SqliteColumnTypeDefinitionInt64 @base)
        : base( @base, (byte)@base.DefaultValue ) { }

    [Pure]
    protected override long MapToBaseType(byte value)
    {
        return value;
    }
}
