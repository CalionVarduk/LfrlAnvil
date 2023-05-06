using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionInt8 : SqliteColumnTypeDefinition<sbyte, long>
{
    internal SqliteColumnTypeDefinitionInt8(SqliteColumnTypeDefinitionInt64 @base)
        : base( @base, (sbyte)@base.DefaultValue ) { }

    [Pure]
    protected override long MapToBaseType(sbyte value)
    {
        return value;
    }
}
