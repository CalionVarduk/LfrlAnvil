using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionInt8 : SqliteColumnTypeDefinition<sbyte>
{
    internal SqliteColumnTypeDefinitionInt8()
        : base( SqliteDataType.Integer, 0, static (reader, ordinal) => (sbyte)reader.GetInt64( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(sbyte value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(sbyte value)
    {
        return (long)value;
    }
}
