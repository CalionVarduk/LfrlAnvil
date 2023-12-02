using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionUInt64 : SqliteColumnTypeDefinition<ulong>
{
    internal SqliteColumnTypeDefinitionUInt64()
        : base( SqliteDataType.Integer, 0, static (reader, ordinal) => unchecked( (ulong)reader.GetInt64( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(ulong value)
    {
        return SqliteHelpers.GetDbLiteral( checked( (long)value ) );
    }

    [Pure]
    public override object ToParameterValue(ulong value)
    {
        return checked( (long)value );
    }
}
