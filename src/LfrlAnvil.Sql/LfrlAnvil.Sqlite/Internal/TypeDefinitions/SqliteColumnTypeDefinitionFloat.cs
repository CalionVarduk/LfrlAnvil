using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionFloat : SqliteColumnTypeDefinition<float, double>
{
    internal SqliteColumnTypeDefinitionFloat(SqliteColumnTypeDefinitionDouble @base)
        : base( @base, (float)@base.DefaultValue ) { }

    [Pure]
    protected override double MapToBaseType(float value)
    {
        return value;
    }
}
