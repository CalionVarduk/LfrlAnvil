using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDouble : SqliteColumnTypeDefinition<double>
{
    internal SqliteColumnTypeDefinitionDouble()
        : base( SqliteDataType.Real, 0.0 ) { }

    [Pure]
    public override string ToDbLiteral(double value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    public override void SetParameter(IDbDataParameter parameter, double value)
    {
        parameter.DbType = System.Data.DbType.Double;
        parameter.Value = value;
    }
}
