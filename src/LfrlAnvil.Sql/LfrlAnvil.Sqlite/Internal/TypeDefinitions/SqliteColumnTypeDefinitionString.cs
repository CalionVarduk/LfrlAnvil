using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionString : SqliteColumnTypeDefinition<string>
{
    internal SqliteColumnTypeDefinitionString()
        : base( SqliteDataType.Text, string.Empty, static (reader, ordinal) => reader.GetString( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(string value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(string value)
    {
        return value;
    }
}
