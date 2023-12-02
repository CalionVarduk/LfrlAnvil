using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql;

public interface ISqlColumnTypeDefinition
{
    ISqlDataType DataType { get; }
    Type RuntimeType { get; }
    SqlLiteralNode DefaultValue { get; }
    LambdaExpression OutputMapping { get; }

    [Pure]
    string? TryToDbLiteral(object value);

    [Pure]
    object? TryToParameterValue(object value);

    void SetParameterInfo(IDbDataParameter parameter, bool isNullable);
}

public interface ISqlColumnTypeDefinition<T> : ISqlColumnTypeDefinition
    where T : notnull
{
    new SqlLiteralNode<T> DefaultValue { get; }

    [Pure]
    string ToDbLiteral(T value);

    [Pure]
    object ToParameterValue(T value);

    [Pure]
    ISqlColumnTypeDefinition<TTarget> Extend<TTarget>(
        Func<TTarget, T> mapper,
        Expression<Func<T, TTarget>> outputMapper,
        TTarget defaultValue)
        where TTarget : notnull;
}
