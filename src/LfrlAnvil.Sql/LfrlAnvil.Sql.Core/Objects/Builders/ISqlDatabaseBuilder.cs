using System;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlDatabaseBuilder
{
    SqlDialect Dialect { get; }
    ISqlDataTypeProvider DataTypes { get; }
    ISqlColumnTypeDefinitionProvider TypeDefinitions { get; }
    ISqlNodeInterpreterFactory NodeInterpreters { get; }
    ISqlQueryReaderFactory QueryReaders { get; }
    ISqlParameterBinderFactory ParameterBinders { get; }
    ISqlSchemaBuilderCollection Schemas { get; }
    ISqlDatabaseChangeTracker Changes { get; }
    string ServerVersion { get; }

    ISqlDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback);
}
