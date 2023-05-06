using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionBool : SqliteColumnTypeDefinition<bool>
{
    internal SqliteColumnTypeDefinitionBool()
        : base( SqliteDataType.Integer, false ) { }

    [Pure]
    public override string ToDbLiteral(bool value)
    {
        return value ? "1" : "0";
    }
}
