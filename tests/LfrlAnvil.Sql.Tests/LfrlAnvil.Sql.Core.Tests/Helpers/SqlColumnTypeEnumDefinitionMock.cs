using LfrlAnvil.Sql.Tests.Helpers.Data;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlColumnTypeEnumDefinitionMock<TEnum, TUnderlying>
    : SqlColumnTypeEnumDefinition<TEnum, TUnderlying, DbDataReaderMock, DbDataParameterMock>
    where TEnum : struct, Enum
    where TUnderlying : unmanaged
{
    public SqlColumnTypeEnumDefinitionMock(SqlColumnTypeDefinitionMock<TUnderlying> @base)
        : base( @base ) { }
}
