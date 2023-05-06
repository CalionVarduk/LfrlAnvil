using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionInt64 : SqliteColumnTypeDefinition<long>
{
    internal SqliteColumnTypeDefinitionInt64()
        : base( SqliteDataType.Integer, 0L ) { }

    [Pure]
    public override string ToDbLiteral(long value)
    {
        return value.ToString( CultureInfo.InvariantCulture );
    }
}
