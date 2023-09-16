using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlDatabaseBuilder
{
    SqlDialect Dialect { get; }
    ISqlDataTypeProvider DataTypes { get; }
    ISqlColumnTypeDefinitionProvider TypeDefinitions { get; }
    ISqlNodeInterpreterFactory NodeInterpreterFactory { get; }
    ISqlSchemaBuilderCollection Schemas { get; }
    SqlDatabaseCreateMode Mode { get; }
    bool IsAttached { get; }

    [Pure]
    ReadOnlySpan<string> GetPendingStatements();

    void AddRawStatement(string statement);

    ISqlDatabaseBuilder SetNodeInterpreterFactory(ISqlNodeInterpreterFactory factory);
    ISqlDatabaseBuilder SetAttachedMode(bool enabled = true);
    ISqlDatabaseBuilder SetDetachedMode(bool enabled = true);
}
