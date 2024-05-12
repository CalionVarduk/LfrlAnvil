using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a type-erased definition of a column type.
/// </summary>
public interface ISqlColumnTypeDefinition
{
    /// <summary>
    /// Underlying DB data type.
    /// </summary>
    ISqlDataType DataType { get; }

    /// <summary>
    /// Underlying .NET type.
    /// </summary>
    Type RuntimeType { get; }

    /// <summary>
    /// Specifies the default value for this type.
    /// </summary>
    SqlLiteralNode DefaultValue { get; }

    /// <summary>
    /// Specifies the mapping of values read by <see cref="IDataReader"/> to objects of the specified <see cref="RuntimeType"/>.
    /// This <see cref="LambdaExpression"/> should have two parameters, where the first represents an <see cref="IDataReader"/> instance
    /// and the second represents an ordinal of the field from which to read the value.
    /// </summary>
    LambdaExpression OutputMapping { get; }

    /// <summary>
    /// Attempts to create an inline DB literal representation of the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns>DB literal from <paramref name="value"/> or null when it is not of the specified <see cref="RuntimeType"/>.</returns>
    [Pure]
    string? TryToDbLiteral(object value);

    /// <summary>
    /// Attempts to create an object from the provided <paramref name="value"/>
    /// that can be used to set DB parameter's <see cref="IDataParameter.Value"/> with.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns>Converted <paramref name="value"/> or null when it is not of the specified <see cref="RuntimeType"/>.</returns>
    [Pure]
    object? TryToParameterValue(object value);

    /// <summary>
    /// Updates information of the provided <paramref name="parameter"/> with this type's definition.
    /// </summary>
    /// <param name="parameter">Parameter to update.</param>
    /// <param name="isNullable">Specifies whether or not the <paramref name="parameter"/> should be marked as nullable.</param>
    void SetParameterInfo(IDbDataParameter parameter, bool isNullable);
}

/// <summary>
/// Represents a generic definition of a column type.
/// </summary>
/// <typeparam name="T">Underlying .NET type.</typeparam>
public interface ISqlColumnTypeDefinition<T> : ISqlColumnTypeDefinition
    where T : notnull
{
    /// <summary>
    /// Specifies the default value for this type.
    /// </summary>
    new SqlLiteralNode<T> DefaultValue { get; }

    /// <summary>
    /// Creates an inline DB literal representation of the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns>DB literal from <paramref name="value"/>.</returns>
    [Pure]
    string ToDbLiteral(T value);

    /// <summary>
    /// Creates an object from the provided <paramref name="value"/>
    /// that can be used to set DB parameter's <see cref="IDataParameter.Value"/> with.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns>Converted <paramref name="value"/>.</returns>
    [Pure]
    object ToParameterValue(T value);
}
