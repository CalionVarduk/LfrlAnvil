using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionInt32 : SqliteColumnTypeDefinition<int, long>
{
    internal SqliteColumnTypeDefinitionInt32(SqliteColumnTypeDefinitionInt64 @base)
        : base( @base, (int)@base.DefaultValue ) { }

    [Pure]
    protected override long MapToBaseType(int value)
    {
        return value;
    }
}
