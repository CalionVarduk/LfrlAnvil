using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionInt64 : SqliteColumnTypeDefinition<long>
{
    internal SqliteColumnTypeDefinitionInt64()
        : base( SqliteDataType.Integer, 0, static (reader, ordinal) => reader.GetInt64( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(long value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(long value)
    {
        return value;
    }
}
