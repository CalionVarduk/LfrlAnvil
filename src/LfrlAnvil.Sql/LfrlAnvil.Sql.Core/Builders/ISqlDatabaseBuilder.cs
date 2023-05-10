﻿using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Builders;

public interface ISqlDatabaseBuilder
{
    SqlDialect Dialect { get; }
    ISqlDataTypeProvider DataTypes { get; }
    ISqlColumnTypeDefinitionProvider TypeDefinitions { get; }
    ISqlSchemaBuilderCollection Schemas { get; }
    SqlDatabaseCreateMode Mode { get; }
    bool IsAttached { get; }

    [Pure]
    ReadOnlySpan<string> GetPendingStatements();

    void AddRawStatement(string statement);

    ISqlDatabaseBuilder SetAttachedMode(bool enabled = true);
    ISqlDatabaseBuilder SetDetachedMode(bool enabled = true);
}