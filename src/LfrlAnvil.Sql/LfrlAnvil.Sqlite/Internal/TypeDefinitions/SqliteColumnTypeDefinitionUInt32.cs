using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionUInt32 : SqliteColumnTypeDefinition<uint>
{
    internal SqliteColumnTypeDefinitionUInt32()
        : base( SqliteDataType.Integer, 0 ) { }

    [Pure]
    public override string ToDbLiteral(uint value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    public override void SetParameter(IDbDataParameter parameter, uint value)
    {
        parameter.DbType = System.Data.DbType.Int64;
        parameter.Value = (long)value;
    }
}
