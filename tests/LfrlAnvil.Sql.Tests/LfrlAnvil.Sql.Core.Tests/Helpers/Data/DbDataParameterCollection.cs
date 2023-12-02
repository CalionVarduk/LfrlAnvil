using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Tests.Helpers.Data;

public sealed class DbDataParameterCollection : IDataParameterCollection
{
    private readonly List<string> _audit = new List<string>();
    private readonly List<DbDataParameter> _parameters = new List<DbDataParameter>();

    public int Count => _parameters.Count;
    public IReadOnlyList<string> Audit => _audit;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => this;
    bool IList.IsFixedSize => false;
    bool IList.IsReadOnly => false;

    public DbDataParameter this[int index]
    {
        get
        {
            _audit.Add( $"get: [indexer]({index})" );
            return _parameters[index];
        }
        set
        {
            _audit.Add( $"set: [indexer]({index})" );
            _parameters[index] = value;
        }
    }

    object? IList.this[int index]
    {
        get
        {
            _audit.Add( "[explicit]" );
            return this[index];
        }
        set
        {
            _audit.Add( "[explicit]" );
            this[index] = AsParameter( value );
        }
    }

    object IDataParameterCollection.this[string parameterName]
    {
        get
        {
            _audit.Add( "[explicit]" );
            _audit.Add( $"get: [indexer]('{parameterName}')" );
            return _parameters[IndexOf( parameterName )];
        }
        set
        {
            _audit.Add( "[explicit]" );
            _audit.Add( $"set: [indexer]('{parameterName}')" );
            var i = IndexOf( parameterName );
            if ( i >= 0 )
                _parameters[i] = AsParameter( value );
            else
                _parameters.Add( AsParameter( value ) );
        }
    }

    [Pure]
    public bool Contains(DbDataParameter value)
    {
        _audit.Add( nameof( Contains ) );
        return _parameters.Contains( value );
    }

    [Pure]
    public bool Contains(object? value)
    {
        _audit.Add( "[type-erased]" );
        return Contains( AsParameter( value ) );
    }

    [Pure]
    public bool Contains(string parameterName)
    {
        _audit.Add( $"{nameof( Contains )}('{parameterName}')" );
        return _parameters.Exists( p => p.ParameterName == parameterName );
    }

    [Pure]
    public int IndexOf(DbDataParameter value)
    {
        _audit.Add( nameof( IndexOf ) );
        return _parameters.IndexOf( value );
    }

    [Pure]
    public int IndexOf(object? value)
    {
        _audit.Add( "[type-erased]" );
        return IndexOf( AsParameter( value ) );
    }

    [Pure]
    public int IndexOf(string parameterName)
    {
        _audit.Add( $"{nameof( IndexOf )}('{parameterName}')" );
        return _parameters.FindIndex( p => p.ParameterName == parameterName );
    }

    public void Add(DbDataParameter value)
    {
        _audit.Add( nameof( Add ) );
        _parameters.Add( value );
    }

    public int Add(object? value)
    {
        _audit.Add( "[type-erased]" );
        Add( AsParameter( value ) );
        return _parameters.Count - 1;
    }

    public void Insert(int index, DbDataParameter value)
    {
        _audit.Add( $"{nameof( Insert )}({index})" );
        _parameters.Insert( index, value );
    }

    public void Insert(int index, object? value)
    {
        _audit.Add( "[type-erased]" );
        Insert( index, AsParameter( value ) );
    }

    public void Remove(DbDataParameter value)
    {
        _audit.Add( nameof( Remove ) );
        _parameters.Remove( value );
    }

    public void Remove(object? value)
    {
        _audit.Add( "[type-erased]" );
        Remove( AsParameter( value ) );
    }

    public void RemoveAt(int index)
    {
        _audit.Add( $"{nameof( RemoveAt )}({index})" );
        _parameters.RemoveAt( index );
    }

    public void RemoveAt(string parameterName)
    {
        _audit.Add( $"{nameof( RemoveAt )}('{parameterName}')" );
        var i = IndexOf( parameterName );
        if ( i >= 0 )
            RemoveAt( i );
    }

    public void Clear()
    {
        _audit.Add( nameof( Clear ) );
        _parameters.Clear();
    }

    public void ClearAudit()
    {
        _audit.Clear();
    }

    [Pure]
    public IEnumerator<DbDataParameter> GetEnumerator()
    {
        return _parameters.GetEnumerator();
    }

    [Pure]
    private static DbDataParameter AsParameter(object? obj)
    {
        Ensure.IsNotNull( obj );
        return (DbDataParameter)obj;
    }

    void ICollection.CopyTo(Array array, int index)
    {
        ((ICollection)_parameters).CopyTo( array, index );
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
