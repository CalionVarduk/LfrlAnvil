using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

public interface ISqlColumnTypeDefinition
{
    ISqlDataType DbType { get; }
    Type RuntimeType { get; }
    object DefaultValue { get; }

    [Pure]
    string? TryToDbLiteral(object value);
}

public interface ISqlColumnTypeDefinition<T> : ISqlColumnTypeDefinition
    where T : notnull
{
    new T DefaultValue { get; }

    [Pure]
    string ToDbLiteral(T value);

    [Pure]
    ISqlColumnTypeDefinition<TTarget> Extend<TTarget>(Func<TTarget, T> mapper, TTarget defaultValue)
        where TTarget : notnull;
}
