using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionInt32 : SqliteColumnTypeDefinition<int>
{
    internal SqliteColumnTypeDefinitionInt32()
        : base( SqliteDataType.Integer, 0, static (reader, ordinal) => (int)reader.GetInt64( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(int value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(int value)
    {
        return (long)value;
    }
}
