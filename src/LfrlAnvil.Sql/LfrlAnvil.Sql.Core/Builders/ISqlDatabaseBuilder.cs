using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Builders;

public interface ISqlDatabaseBuilder
{
    SqlDialect Dialect { get; }
    ISqlDataTypeProvider DataTypes { get; }
    ISqlColumnTypeDefinitionProvider TypeDefinitions { get; }
    ISqlSchemaBuilderCollection Schemas { get; }

    [Pure]
    ReadOnlySpan<string> GetPendingStatements();

    void AddRawStatement(string statement);
}
