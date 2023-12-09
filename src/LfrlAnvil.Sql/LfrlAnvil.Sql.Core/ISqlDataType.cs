using System;
using System.Data;

namespace LfrlAnvil.Sql;

public interface ISqlDataType
{
    SqlDialect Dialect { get; }
    string Name { get; }
    DbType DbType { get; }
    ReadOnlySpan<int> Parameters { get; }
    ReadOnlySpan<SqlDataTypeParameter> ParameterDefinitions { get; }
}
