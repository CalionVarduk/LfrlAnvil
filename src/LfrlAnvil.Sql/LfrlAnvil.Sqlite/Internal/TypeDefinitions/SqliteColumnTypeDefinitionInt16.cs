using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionInt16 : SqliteColumnTypeDefinition<short>
{
    internal SqliteColumnTypeDefinitionInt16()
        : base( SqliteDataType.Integer, 0 ) { }

    [Pure]
    public override string ToDbLiteral(short value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    public override void SetParameter(IDbDataParameter parameter, short value)
    {
        parameter.DbType = System.Data.DbType.Int64;
        parameter.Value = (long)value;
    }
}
