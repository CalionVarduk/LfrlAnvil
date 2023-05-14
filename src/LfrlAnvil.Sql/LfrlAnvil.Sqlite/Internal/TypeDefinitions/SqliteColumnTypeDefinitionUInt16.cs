using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionUInt16 : SqliteColumnTypeDefinition<ushort>
{
    internal SqliteColumnTypeDefinitionUInt16()
        : base( SqliteDataType.Integer, 0 ) { }

    [Pure]
    public override string ToDbLiteral(ushort value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    public override void SetParameter(IDbDataParameter parameter, ushort value)
    {
        parameter.DbType = System.Data.DbType.Int64;
        parameter.Value = (long)value;
    }
}
