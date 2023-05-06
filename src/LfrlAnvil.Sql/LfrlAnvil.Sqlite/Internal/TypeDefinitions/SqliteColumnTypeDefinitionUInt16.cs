using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionUInt16 : SqliteColumnTypeDefinition<ushort, long>
{
    internal SqliteColumnTypeDefinitionUInt16(SqliteColumnTypeDefinitionInt64 @base)
        : base( @base, (ushort)@base.DefaultValue ) { }

    [Pure]
    protected override long MapToBaseType(ushort value)
    {
        return value;
    }
}
