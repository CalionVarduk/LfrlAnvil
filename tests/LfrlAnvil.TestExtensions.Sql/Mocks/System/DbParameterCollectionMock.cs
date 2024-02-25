using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.TestExtensions.Sql.Mocks.System;

public sealed class DbParameterCollectionMock : DbParameterCollection
{
    private readonly List<DbParameterMock> _parameters = new List<DbParameterMock>();

    public override int Count => _parameters.Count;

    public override object SyncRoot { get; } = new object();

    public new DbParameterMock this[int index]
    {
        get => _parameters[index];
        set => _parameters[index] = value;
    }

    [Pure]
    public bool Contains(DbParameterMock value)
    {
        return _parameters.Contains( value );
    }

    [Pure]
    public override bool Contains(object? value)
    {
        return Contains( AsParameter( value ) );
    }

    [Pure]
    public override bool Contains(string value)
    {
        return _parameters.Exists( p => p.ParameterName == value );
    }

    [Pure]
    public int IndexOf(DbParameterMock value)
    {
        return _parameters.IndexOf( value );
    }

    [Pure]
    public override int IndexOf(object? value)
    {
        return IndexOf( AsParameter( value ) );
    }

    [Pure]
    public override int IndexOf(string parameterName)
    {
        return _parameters.FindIndex( p => p.ParameterName == parameterName );
    }

    public void Add(DbParameterMock value)
    {
        _parameters.Add( value );
    }

    public override int Add(object? value)
    {
        Add( AsParameter( value ) );
        return _parameters.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach ( var value in values )
            Add( value );
    }

    public void Insert(int index, DbParameterMock value)
    {
        _parameters.Insert( index, value );
    }

    public override void Insert(int index, object? value)
    {
        Insert( index, AsParameter( value ) );
    }

    public void Remove(DbParameterMock value)
    {
        _parameters.Remove( value );
    }

    public override void Remove(object? value)
    {
        Remove( AsParameter( value ) );
    }

    public override void RemoveAt(int index)
    {
        _parameters.RemoveAt( index );
    }

    public override void RemoveAt(string parameterName)
    {
        var i = IndexOf( parameterName );
        if ( i >= 0 )
            RemoveAt( i );
    }

    public override void Clear()
    {
        _parameters.Clear();
    }

    public override void CopyTo(Array array, int index)
    {
        ((ICollection)_parameters).CopyTo( array, index );
    }

    [Pure]
    public override IEnumerator<DbParameterMock> GetEnumerator()
    {
        return _parameters.GetEnumerator();
    }

    [Pure]
    public IEnumerable<DbParameterMock> GetAll()
    {
        foreach ( var p in this )
            yield return p;
    }

    [Pure]
    protected override DbParameter GetParameter(int index)
    {
        return _parameters[index];
    }

    [Pure]
    protected override DbParameter GetParameter(string parameterName)
    {
        return _parameters.First( p => p.ParameterName == parameterName );
    }

    protected override void SetParameter(int index, DbParameter value)
    {
        _parameters[index] = AsParameter( value );
    }

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        var index = IndexOf( parameterName );
        _parameters[index] = AsParameter( value );
    }

    [Pure]
    private static DbParameterMock AsParameter(object? obj)
    {
        Ensure.IsNotNull( obj );
        return (DbParameterMock)obj;
    }
}
