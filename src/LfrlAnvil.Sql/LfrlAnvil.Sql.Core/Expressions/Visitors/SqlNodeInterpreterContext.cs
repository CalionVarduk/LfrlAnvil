using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents a context for an <see cref="SqlNodeInterpreter"/> instance that contains the ongoing result of SQL nodes' interpretation.
/// </summary>
public sealed class SqlNodeInterpreterContext
{
    private Dictionary<string, SqlNodeInterpreterContextParameter>? _parameters;

    private SqlNodeInterpreterContext(StringBuilder sql)
    {
        Sql = sql;
        Indent = 0;
        _parameters = null;
        ChildDepth = 0;
    }

    /// <summary>
    /// Underlying SQL builder.
    /// </summary>
    public StringBuilder Sql { get; }

    /// <summary>
    /// Gets the current SQL statement indentation level.
    /// </summary>
    public int Indent { get; private set; }

    /// <summary>
    /// Gets the current SQL child node depth.
    /// </summary>
    public int ChildDepth { get; private set; }

    /// <summary>
    /// Gets the collection of registered SQL parameters.
    /// </summary>
    public IReadOnlyCollection<SqlNodeInterpreterContextParameter> Parameters =>
        ( IReadOnlyCollection<SqlNodeInterpreterContextParameter>? )_parameters?.Values
        ?? Array.Empty<SqlNodeInterpreterContextParameter>();

    /// <summary>
    /// Creates a new <see cref="SqlNodeInterpreterContext"/> instance.
    /// </summary>
    /// <param name="capacity">Initial capacity of the underlying <see cref="Sql"/> builder. Equal to <b>1024</b> by default.</param>
    /// <returns>New <see cref="SqlNodeInterpreterContext"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNodeInterpreterContext Create(int capacity = 1024)
    {
        return Create( new StringBuilder( capacity ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNodeInterpreterContext"/> instance with a prepared SQL builder.
    /// </summary>
    /// <param name="builder">SQL builder.</param>
    /// <returns>New <see cref="SqlNodeInterpreterContext"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNodeInterpreterContext Create(StringBuilder builder)
    {
        return new SqlNodeInterpreterContext( builder );
    }

    /// <summary>
    /// Increases <see cref="Indent"/> by <b>2</b>.
    /// </summary>
    public void IncreaseIndent()
    {
        Indent += 2;
    }

    /// <summary>
    /// Decreases <see cref="Indent"/> by <b>2</b>, if possible.
    /// </summary>
    public void DecreaseIndent()
    {
        if ( Indent <= 2 )
        {
            Indent = 0;
            return;
        }

        Indent -= 2;
    }

    /// <summary>
    /// Increases <see cref="Indent"/> by <b>2</b> and creates a new disposable <see cref="TemporaryIndentIncrease"/> instance.
    /// </summary>
    /// <returns>New <see cref="TemporaryIndentIncrease"/> instance.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TemporaryIndentIncrease TempIndentIncrease()
    {
        return new TemporaryIndentIncrease( this );
    }

    /// <summary>
    /// Increases <see cref="ChildDepth"/> by <b>1</b>.
    /// </summary>
    public void IncreaseChildDepth()
    {
        ++ChildDepth;
    }

    /// <summary>
    /// Decreases <see cref="ChildDepth"/> by <b>1</b>, if possible.
    /// </summary>
    public void DecreaseChildDepth()
    {
        if ( ChildDepth > 0 )
            --ChildDepth;
    }

    /// <summary>
    /// Increases <see cref="ChildDepth"/> by <b>1</b> and creates a new disposable <see cref="TemporaryChildDepthIncrease"/> instance.
    /// </summary>
    /// <returns>New <see cref="TemporaryChildDepthIncrease"/> instance.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TemporaryChildDepthIncrease TempChildDepthIncrease()
    {
        return new TemporaryChildDepthIncrease( this );
    }

    /// <summary>
    /// Appends a new line with the current <see cref="Indent"/> to <see cref="Sql"/>.
    /// </summary>
    /// <returns><see cref="Sql"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public StringBuilder AppendIndent()
    {
        return Sql.Indent( Indent );
    }

    /// <summary>
    /// Appends a new line with the current <see cref="Indent"/> reduced by <b>2</b> to <see cref="Sql"/>.
    /// </summary>
    /// <returns><see cref="Sql"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public StringBuilder AppendShortIndent()
    {
        return Sql.Indent( Math.Max( Indent - 2, 0 ) );
    }

    /// <summary>
    /// Registers a new SQL parameter in this context.
    /// </summary>
    /// <param name="name">Parameter's name.</param>
    /// <param name="type">Optional runtime type of this parameter.</param>
    /// <param name="index">Optional 0-based position of this parameter.</param>
    /// <remarks>
    /// When SQL parameter with the provided <paramref name="name"/> already exists, then its type and index are validated:
    /// when new type does not equal old type, then the existing entry's type is set to null,
    /// and when new index does not equal old index, then the existing entry's index is set to null.
    /// </remarks>
    public void AddParameter(string name, TypeNullability? type, int? index)
    {
        if ( _parameters is null )
        {
            _parameters = new Dictionary<string, SqlNodeInterpreterContextParameter>( comparer: SqlHelpers.NameComparer );
            _parameters.Add( name, new SqlNodeInterpreterContextParameter( name, type, index ) );
            return;
        }

        ref var parameter = ref CollectionsMarshal.GetValueRefOrAddDefault( _parameters, name, out var exists );
        if ( ! exists )
        {
            parameter = new SqlNodeInterpreterContextParameter( name, type, index );
            return;
        }

        if ( parameter.Type != type )
            parameter = new SqlNodeInterpreterContextParameter( parameter.Name, null, parameter.Index );

        if ( parameter.Index != index )
            parameter = new SqlNodeInterpreterContextParameter( parameter.Name, parameter.Type, null );
    }

    /// <summary>
    /// Attempts to retrieve an SQL parameter by its <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Parameter's name.</param>
    /// <param name="result"><b>out</b> parameter that returns found SQL parameter info.</param>
    /// <returns><b>true</b> when SQL parameter exists, otherwise <b>false</b>.</returns>
    public bool TryGetParameter(string name, out SqlNodeInterpreterContextParameter result)
    {
        if ( _parameters is not null )
            return _parameters.TryGetValue( name, out result );

        result = default;
        return false;
    }

    /// <summary>
    /// Resets this context to initial empty state.
    /// </summary>
    public void Clear()
    {
        Sql.Clear();
        _parameters?.Clear();
        Indent = 0;
        ChildDepth = 0;
    }

    /// <summary>
    /// Creates a new <see cref="SqlNodeInterpreterContextSnapshot"/> instance from the current state of this context.
    /// </summary>
    /// <returns>New <see cref="SqlNodeInterpreterContextSnapshot"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlNodeInterpreterContextSnapshot ToSnapshot()
    {
        return new SqlNodeInterpreterContextSnapshot( this );
    }

    /// <summary>
    /// Represents a temporary disposable increase of a context's <see cref="SqlNodeInterpreterContext.ChildDepth"/>.
    /// </summary>
    public readonly struct TemporaryChildDepthIncrease : IDisposable
    {
        private readonly SqlNodeInterpreterContext _context;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal TemporaryChildDepthIncrease(SqlNodeInterpreterContext context)
        {
            _context = context;
            context.IncreaseChildDepth();
        }

        /// <inheritdoc />
        /// <remarks>Decreases context's <see cref="SqlNodeInterpreterContext.ChildDepth"/> by <b>1</b>.</remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            _context.DecreaseChildDepth();
        }
    }

    /// <summary>
    /// Represents a temporary disposable increase of a context's <see cref="SqlNodeInterpreterContext.Indent"/>.
    /// </summary>
    public readonly struct TemporaryIndentIncrease : IDisposable
    {
        private readonly SqlNodeInterpreterContext _context;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal TemporaryIndentIncrease(SqlNodeInterpreterContext context)
        {
            _context = context;
            context.IncreaseIndent();
        }

        /// <inheritdoc />
        /// <remarks>Decreases context's <see cref="SqlNodeInterpreterContext.Indent"/> by <b>2</b>.</remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            _context.DecreaseIndent();
        }
    }
}
