using System.Data.Common;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlColumnTypeEnumDefinitionMock<TEnum, TUnderlying>
    : SqlColumnTypeEnumDefinition<TEnum, TUnderlying, DbDataRecord, DbParameter>
    where TEnum : struct, Enum
    where TUnderlying : unmanaged
{
    public SqlColumnTypeEnumDefinitionMock(SqlColumnTypeDefinitionMock<TUnderlying> @base)
        : base( @base ) { }
}
