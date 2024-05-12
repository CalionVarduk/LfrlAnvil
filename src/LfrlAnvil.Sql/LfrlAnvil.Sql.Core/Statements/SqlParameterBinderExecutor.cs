using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents an <see cref="SqlParameterBinder"/> bound to a specific source of parameters.
/// </summary>
/// <param name="Binder">Underlying parameter binder.</param>
/// <param name="Source">Bound source of parameters.</param>
public readonly record struct SqlParameterBinderExecutor(SqlParameterBinder Binder, IEnumerable<SqlParameter>? Source)
{
    /// <summary>
    /// Binds <see cref="Source"/> parameters to the provided <paramref name="command"/>.
    /// </summary>
    /// <param name="command">Command to bind parameters to.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Execute(IDbCommand command)
    {
        Binder.Bind( command, Source );
    }
}

/// <summary>
/// Represents an <see cref="SqlParameterBinder{TSource}"/> bound to a specific source of parameters.
/// </summary>
/// <param name="Binder">Underlying parameter binder.</param>
/// <param name="Source">Bound source of parameters.</param>
/// <typeparam name="TSource">Parameter source type.</typeparam>
public readonly record struct SqlParameterBinderExecutor<TSource>(SqlParameterBinder<TSource> Binder, TSource? Source)
    where TSource : notnull
{
    /// <summary>
    /// Binds <see cref="Source"/> parameters to the provided <paramref name="command"/>.
    /// </summary>
    /// <param name="command">Command to bind parameters to.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Execute(IDbCommand command)
    {
        Binder.Bind( command, Source );
    }
}
