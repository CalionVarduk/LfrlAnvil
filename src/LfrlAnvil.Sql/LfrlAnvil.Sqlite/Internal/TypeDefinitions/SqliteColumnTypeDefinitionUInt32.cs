using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionUInt32 : SqliteColumnTypeDefinition<uint>
{
    internal SqliteColumnTypeDefinitionUInt32()
        : base( SqliteDataType.Integer, 0, static (reader, ordinal) => unchecked( (uint)reader.GetInt64( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(uint value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(uint value)
    {
        return (long)value;
    }
}
