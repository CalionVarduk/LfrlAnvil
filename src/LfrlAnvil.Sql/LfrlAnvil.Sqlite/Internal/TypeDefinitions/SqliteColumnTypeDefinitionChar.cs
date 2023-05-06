using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionChar : SqliteColumnTypeDefinition<char>
{
    internal SqliteColumnTypeDefinitionChar()
        : base( SqliteDataType.Text, '0' ) { }

    [Pure]
    public override string ToDbLiteral(char value)
    {
        return $"'{value}'";
    }
}
