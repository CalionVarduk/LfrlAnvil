using LfrlAnvil.Sql;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlColumnTypeEnumDefinitionMock<TEnum, TUnderlying>
    : SqlColumnTypeEnumDefinition<TEnum, TUnderlying, DbDataReaderMock, DbParameterMock>
    where TEnum : struct, Enum
    where TUnderlying : unmanaged
{
    public SqlColumnTypeEnumDefinitionMock(SqlColumnTypeDefinitionMock<TUnderlying> @base)
        : base( @base ) { }
}
