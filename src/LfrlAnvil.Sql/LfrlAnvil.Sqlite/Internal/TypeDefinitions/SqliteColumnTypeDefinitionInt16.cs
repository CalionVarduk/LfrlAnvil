using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionInt16 : SqliteColumnTypeDefinition<short, long>
{
    internal SqliteColumnTypeDefinitionInt16(SqliteColumnTypeDefinitionInt64 @base)
        : base( @base, (short)@base.DefaultValue ) { }

    [Pure]
    protected override long MapToBaseType(short value)
    {
        return value;
    }
}
