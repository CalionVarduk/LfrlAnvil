using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlColumnCollection" />
public abstract class SqlColumnCollection : ISqlColumnCollection
{
    private readonly Dictionary<string, SqlColumn> _map;
    private SqlTable? _table;

    /// <summary>
    /// Creates a new <see cref="SqlColumnCollection"/> instance.
    /// </summary>
    /// <param name="source">Source builder collection.</param>
    protected SqlColumnCollection(SqlColumnBuilderCollection source)
    {
        _map = new Dictionary<string, SqlColumn>( capacity: source.Count, comparer: SqlHelpers.NameComparer );
        _table = null;
    }

    /// <inheritdoc />
    public int Count => _map.Count;

    /// <inheritdoc cref="ISqlColumnCollection.Table" />
    public SqlTable Table
    {
        get
        {
            Assume.IsNotNull( _table );
            return _table;
        }
    }

    ISqlTable ISqlColumnCollection.Table => Table;

    /// <inheritdoc />
    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    /// <inheritdoc cref="ISqlColumnCollection.Get(string)" />
    [Pure]
    public SqlColumn Get(string name)
    {
        return _map[name];
    }

    /// <inheritdoc cref="ISqlColumnCollection.TryGet(string)" />
    [Pure]
    public SqlColumn? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectEnumerator{T}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectEnumerator{T}"/> instance.</returns>
    [Pure]
    public SqlObjectEnumerator<SqlColumn> GetEnumerator()
    {
        return new SqlObjectEnumerator<SqlColumn>( _map );
    }

    internal void SetTable(SqlTable table, SqlColumnBuilderCollection source)
    {
        Assume.IsNull( _table );
        Assume.Equals( table.Columns, this );
        _table = table;

        foreach ( var builder in source )
        {
            var column = CreateColumn( builder );
            _map.Add( column.Name, column );
        }
    }

    /// <summary>
    /// Creates a new <see cref="SqlColumn"/> instance.
    /// </summary>
    /// <param name="builder">Source column builder.</param>
    /// <returns>New <see cref="SqlColumn"/> instance.</returns>
    [Pure]
    protected abstract SqlColumn CreateColumn(SqlColumnBuilder builder);

    [Pure]
    ISqlColumn ISqlColumnCollection.Get(string name)
    {
        return Get( name );
    }

    [Pure]
    ISqlColumn? ISqlColumnCollection.TryGet(string name)
    {
        return TryGet( name );
    }

    [Pure]
    IEnumerator<ISqlColumn> IEnumerable<ISqlColumn>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
