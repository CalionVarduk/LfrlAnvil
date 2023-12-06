using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Sql.Tests.Helpers.Data;

public sealed class DbDataParameterCollection : DbParameterCollection
{
    private readonly List<string> _audit = new List<string>();
    private readonly List<DbDataParameter> _parameters = new List<DbDataParameter>();

    public override int Count => _parameters.Count;
    public IReadOnlyList<string> Audit => _audit;

    public override object SyncRoot { get; } = new object();

    public new DbDataParameter this[int index]
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

    [Pure]
    public bool Contains(DbDataParameter value)
    {
        _audit.Add( nameof( Contains ) );
        return _parameters.Contains( value );
    }

    [Pure]
    public override bool Contains(object? value)
    {
        _audit.Add( "[type-erased]" );
        return Contains( AsParameter( value ) );
    }

    [Pure]
    public override bool Contains(string value)
    {
        _audit.Add( $"{nameof( Contains )}('{value}')" );
        return _parameters.Exists( p => p.ParameterName == value );
    }

    [Pure]
    public int IndexOf(DbDataParameter value)
    {
        _audit.Add( nameof( IndexOf ) );
        return _parameters.IndexOf( value );
    }

    [Pure]
    public override int IndexOf(object? value)
    {
        _audit.Add( "[type-erased]" );
        return IndexOf( AsParameter( value ) );
    }

    [Pure]
    public override int IndexOf(string parameterName)
    {
        _audit.Add( $"{nameof( IndexOf )}('{parameterName}')" );
        return _parameters.FindIndex( p => p.ParameterName == parameterName );
    }

    public void Add(DbDataParameter value)
    {
        _audit.Add( nameof( Add ) );
        _parameters.Add( value );
    }

    public override int Add(object? value)
    {
        _audit.Add( "[type-erased]" );
        Add( AsParameter( value ) );
        return _parameters.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach ( var value in values )
            Add( value );
    }

    public void Insert(int index, DbDataParameter value)
    {
        _audit.Add( $"{nameof( Insert )}({index})" );
        _parameters.Insert( index, value );
    }

    public override void Insert(int index, object? value)
    {
        _audit.Add( "[type-erased]" );
        Insert( index, AsParameter( value ) );
    }

    public void Remove(DbDataParameter value)
    {
        _audit.Add( nameof( Remove ) );
        _parameters.Remove( value );
    }

    public override void Remove(object? value)
    {
        _audit.Add( "[type-erased]" );
        Remove( AsParameter( value ) );
    }

    public override void RemoveAt(int index)
    {
        _audit.Add( $"{nameof( RemoveAt )}({index})" );
        _parameters.RemoveAt( index );
    }

    public override void RemoveAt(string parameterName)
    {
        _audit.Add( $"{nameof( RemoveAt )}('{parameterName}')" );
        var i = IndexOf( parameterName );
        if ( i >= 0 )
            RemoveAt( i );
    }

    public override void Clear()
    {
        _audit.Add( nameof( Clear ) );
        _parameters.Clear();
    }

    public override void CopyTo(Array array, int index)
    {
        ((ICollection)_parameters).CopyTo( array, index );
    }

    public void ClearAudit()
    {
        _audit.Clear();
    }

    [Pure]
    public override IEnumerator<DbDataParameter> GetEnumerator()
    {
        return _parameters.GetEnumerator();
    }

    [Pure]
    protected override DbParameter GetParameter(int index)
    {
        _audit.Add( $"{nameof( GetParameter )}('{index}')" );
        return _parameters[index];
    }

    [Pure]
    protected override DbParameter GetParameter(string parameterName)
    {
        _audit.Add( $"{nameof( GetParameter )}('{parameterName}')" );
        return _parameters.First( p => p.ParameterName == parameterName );
    }

    protected override void SetParameter(int index, DbParameter value)
    {
        _audit.Add( $"{nameof( SetParameter )}('{index}')" );
        _parameters[index] = AsParameter( value );
    }

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        _audit.Add( $"{nameof( SetParameter )}('{parameterName}')" );
        var index = IndexOf( parameterName );
        _parameters[index] = AsParameter( value );
    }

    [Pure]
    private static DbDataParameter AsParameter(object? obj)
    {
        Ensure.IsNotNull( obj );
        return (DbDataParameter)obj;
    }
}
