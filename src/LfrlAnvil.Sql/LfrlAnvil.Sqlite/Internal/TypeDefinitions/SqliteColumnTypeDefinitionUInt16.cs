using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionUInt16 : SqliteColumnTypeDefinition<ushort>
{
    internal SqliteColumnTypeDefinitionUInt16()
        : base( SqliteDataType.Integer, 0, static (reader, ordinal) => unchecked( (ushort)reader.GetInt64( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(ushort value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(ushort value)
    {
        return (long)value;
    }
}
