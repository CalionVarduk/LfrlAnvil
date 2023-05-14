using System;
using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionGuid : SqliteColumnTypeDefinition<Guid>
{
    internal SqliteColumnTypeDefinitionGuid()
        : base( SqliteDataType.Blob, Guid.Empty ) { }

    [Pure]
    public override string ToDbLiteral(Guid value)
    {
        return SqliteHelpers.GetDbLiteral( value.ToByteArray() );
    }

    public override void SetParameter(IDbDataParameter parameter, Guid value)
    {
        parameter.DbType = System.Data.DbType.Binary;
        parameter.Value = value.ToByteArray();
    }
}
