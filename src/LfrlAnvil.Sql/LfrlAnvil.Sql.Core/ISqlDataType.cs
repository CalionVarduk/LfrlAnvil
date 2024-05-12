using System;
using System.Data;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a DB data type.
/// </summary>
public interface ISqlDataType
{
    /// <summary>
    /// Specifies the SQL dialect of this data type.
    /// </summary>
    SqlDialect Dialect { get; }

    /// <summary>
    /// DB name of this data type.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// <see cref="System.Data.DbType"/> of this data type.
    /// </summary>
    DbType DbType { get; }

    /// <summary>
    /// Collection of applied parameters to this data type.
    /// </summary>
    ReadOnlySpan<int> Parameters { get; }

    /// <summary>
    /// Collection of parameter definitions for this data type.
    /// </summary>
    ReadOnlySpan<SqlDataTypeParameter> ParameterDefinitions { get; }
}
