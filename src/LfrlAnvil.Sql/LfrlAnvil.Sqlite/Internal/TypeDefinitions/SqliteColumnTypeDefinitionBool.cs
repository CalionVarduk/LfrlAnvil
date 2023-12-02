using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionBool : SqliteColumnTypeDefinition<bool>
{
    private static readonly object Zero = 0L;
    private static readonly object One = 1L;

    internal SqliteColumnTypeDefinitionBool()
        : base( SqliteDataType.Integer, false, static (reader, ordinal) => reader.GetInt64( ordinal ) != 0 ) { }

    [Pure]
    public override string ToDbLiteral(bool value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(bool value)
    {
        return value ? One : Zero;
    }
}
