using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionInt16 : SqliteColumnTypeDefinition<short>
{
    internal SqliteColumnTypeDefinitionInt16()
        : base( SqliteDataType.Integer, 0, static (reader, ordinal) => ( short )reader.GetInt64( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(short value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(short value)
    {
        return ( long )value;
    }
}
