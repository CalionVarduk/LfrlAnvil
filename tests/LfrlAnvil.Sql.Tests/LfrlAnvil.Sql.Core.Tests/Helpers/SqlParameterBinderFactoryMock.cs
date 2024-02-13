using System.Data.Common;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlParameterBinderFactoryMock : SqlParameterBinderFactory<DbCommand>
{
    public SqlParameterBinderFactoryMock(SqlColumnTypeDefinitionProviderMock columnTypeDefinitions)
        : base( SqlDialectMock.Instance, columnTypeDefinitions ) { }
}
