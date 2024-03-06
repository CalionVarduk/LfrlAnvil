using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionUInt8 : SqliteColumnTypeDefinition<byte>
{
    internal SqliteColumnTypeDefinitionUInt8()
        : base( SqliteDataType.Integer, 0, static (reader, ordinal) => unchecked( (byte)reader.GetInt64( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(byte value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(byte value)
    {
        return (long)value;
    }
}
