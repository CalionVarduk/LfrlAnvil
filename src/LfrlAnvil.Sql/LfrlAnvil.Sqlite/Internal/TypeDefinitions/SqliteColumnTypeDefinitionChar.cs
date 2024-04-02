using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionChar : SqliteColumnTypeDefinition<char>
{
    internal SqliteColumnTypeDefinitionChar()
        : base( SqliteDataType.Text, '0', static (reader, ordinal) => reader.GetChar( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(char value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(char value)
    {
        return value.ToString();
    }
}
