using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionGuid : SqliteColumnTypeDefinition<Guid, byte[]>
{
    internal SqliteColumnTypeDefinitionGuid(SqliteColumnTypeDefinitionByteArray @base)
        : base( @base, Guid.Empty ) { }

    [Pure]
    protected override byte[] MapToBaseType(Guid value)
    {
        return value.ToByteArray();
    }
}
