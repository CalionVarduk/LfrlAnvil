using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionUInt32 : SqliteColumnTypeDefinition<uint, long>
{
    internal SqliteColumnTypeDefinitionUInt32(SqliteColumnTypeDefinitionInt64 @base)
        : base( @base, (uint)@base.DefaultValue ) { }

    [Pure]
    protected override long MapToBaseType(uint value)
    {
        return value;
    }
}
