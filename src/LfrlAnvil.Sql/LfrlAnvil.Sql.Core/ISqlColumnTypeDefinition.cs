using System;
using System.Data;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql;

public interface ISqlColumnTypeDefinition
{
    ISqlDataType DbType { get; }
    Type RuntimeType { get; }
    SqlLiteralNode DefaultValue { get; }

    [Pure]
    string? TryToDbLiteral(object value);

    bool TrySetParameter(IDbDataParameter parameter, object value);
}

public interface ISqlColumnTypeDefinition<T> : ISqlColumnTypeDefinition
    where T : notnull
{
    new SqlLiteralNode<T> DefaultValue { get; }

    [Pure]
    string ToDbLiteral(T value);

    void SetParameter(IDbDataParameter parameter, T value);

    [Pure]
    ISqlColumnTypeDefinition<TTarget> Extend<TTarget>(Func<TTarget, T> mapper, TTarget defaultValue)
        where TTarget : notnull;
}
