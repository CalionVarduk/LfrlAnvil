using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased parameter binder.
/// </summary>
/// <param name="Dialect">SQL dialect with which this query reader is associated.</param>
/// <param name="Delegate">Underlying delegate.</param>
public readonly record struct SqlParameterBinder(SqlDialect Dialect, Action<IDbCommand, IEnumerable<SqlParameter>> Delegate)
{
    /// <summary>
    /// Binds the provided parameter collection to the <paramref name="command"/> or clears all of its <see cref="IDbCommand.Parameters"/>
    /// if no parameters have been specified.
    /// </summary>
    /// <param name="command">Command to bind parameters to.</param>
    /// <param name="source">Optional collection of parameters to bind.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Bind(IDbCommand command, IEnumerable<SqlParameter>? source = null)
    {
        if ( source is null )
            command.Parameters.Clear();
        else
            Delegate( command, source );
    }
}

/// <summary>
/// Represents a generic parameter binder.
/// </summary>
/// <param name="Dialect">SQL dialect with which this query reader is associated.</param>
/// <param name="Delegate">Underlying delegate.</param>
/// <typeparam name="TSource">Parameter source type.</typeparam>
public readonly record struct SqlParameterBinder<TSource>(SqlDialect Dialect, Action<IDbCommand, TSource> Delegate)
    where TSource : notnull
{
    /// <summary>
    /// Binds the provided parameter collection to the <paramref name="command"/> or clears all of its <see cref="IDbCommand.Parameters"/>
    /// if no parameters have been specified.
    /// </summary>
    /// <param name="command">Command to bind parameters to.</param>
    /// <param name="source">Optional source parameters to bind.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Bind(IDbCommand command, TSource? source = default)
    {
        if ( Generic<TSource>.IsNull( source ) )
            command.Parameters.Clear();
        else
            Delegate( command, source );
    }
}
