using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionFloat : SqliteColumnTypeDefinition<float>
{
    internal SqliteColumnTypeDefinitionFloat()
        : base( SqliteDataType.Real, 0.0F ) { }

    [Pure]
    public override string ToDbLiteral(float value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    public override void SetParameter(IDbDataParameter parameter, float value)
    {
        parameter.DbType = System.Data.DbType.Double;
        parameter.Value = (double)value;
    }
}
