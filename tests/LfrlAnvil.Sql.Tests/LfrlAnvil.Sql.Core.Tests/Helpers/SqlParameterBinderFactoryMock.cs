using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sql.Tests.Helpers.Data;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlParameterBinderFactoryMock : SqlParameterBinderFactory<DbCommandMock>
{
    public SqlParameterBinderFactoryMock(SqlColumnTypeDefinitionProviderMock columnTypeDefinitions)
        : base( SqlDialectMock.Instance, columnTypeDefinitions ) { }
}
