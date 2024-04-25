using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Statements;

public sealed class SqlNamedParameterCollection : IReadOnlyCollection<SqlParameter>
{
    private readonly Dictionary<string, SqlParameter> _map;

    public SqlNamedParameterCollection(int capacity = 0, IEqualityComparer<string>? comparer = null)
    {
        _map = new Dictionary<string, SqlParameter>( capacity, comparer ?? SqlHelpers.NameComparer );
    }

    public int Count => _map.Count;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqlParameter? TryGet(string name)
    {
        return _map.TryGetValue( name, out var result ) ? result : null;
    }

    public bool TryAdd(string name, object? value)
    {
        return _map.TryAdd( name, SqlParameter.Named( name, value ) );
    }

    public void AddOrUpdate(string name, object? value)
    {
        _map[name] = SqlParameter.Named( name, value );
    }

    public void Clear()
    {
        _map.Clear();
    }

    [Pure]
    public IEnumerator<SqlParameter> GetEnumerator()
    {
        return _map.Values.GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
