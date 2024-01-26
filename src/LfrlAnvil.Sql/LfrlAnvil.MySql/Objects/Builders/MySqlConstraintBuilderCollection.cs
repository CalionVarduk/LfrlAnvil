using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlConstraintBuilderCollection : ISqlConstraintBuilderCollection
{
    private readonly Dictionary<string, MySqlConstraintBuilder> _map;
    private MySqlPrimaryKeyBuilder? _primaryKey;

    internal MySqlConstraintBuilderCollection(MySqlTableBuilder table)
    {
        Table = table;
        _map = new Dictionary<string, MySqlConstraintBuilder>( StringComparer.OrdinalIgnoreCase );
        _primaryKey = null;
    }

    public MySqlTableBuilder Table { get; }
    public int Count => _map.Count;

    ISqlTableBuilder ISqlConstraintBuilderCollection.Table => Table;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public MySqlConstraintBuilder GetConstraint(string name)
    {
        return _map[name];
    }

    [Pure]
    public MySqlConstraintBuilder? TryGetConstraint(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public MySqlPrimaryKeyBuilder GetPrimaryKey()
    {
        return TryGetPrimaryKey() ?? throw new MySqlObjectBuilderException( ExceptionResources.PrimaryKeyIsMissing( Table ) );
    }

    [Pure]
    public MySqlPrimaryKeyBuilder? TryGetPrimaryKey()
    {
        return _primaryKey;
    }

    [Pure]
    public MySqlIndexBuilder GetIndex(string name)
    {
        return GetTypedObject<MySqlIndexBuilder>( name, SqlObjectType.Index );
    }

    [Pure]
    public MySqlIndexBuilder? TryGetIndex(string name)
    {
        return TryGetTypedObject<MySqlIndexBuilder>( name, SqlObjectType.Index );
    }

    [Pure]
    public MySqlForeignKeyBuilder GetForeignKey(string name)
    {
        return GetTypedObject<MySqlForeignKeyBuilder>( name, SqlObjectType.ForeignKey );
    }

    [Pure]
    public MySqlForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return TryGetTypedObject<MySqlForeignKeyBuilder>( name, SqlObjectType.ForeignKey );
    }

    [Pure]
    public MySqlCheckBuilder GetCheck(string name)
    {
        return GetTypedObject<MySqlCheckBuilder>( name, SqlObjectType.Check );
    }

    [Pure]
    public MySqlCheckBuilder? TryGetCheck(string name)
    {
        return TryGetTypedObject<MySqlCheckBuilder>( name, SqlObjectType.Check );
    }

    public MySqlPrimaryKeyBuilder SetPrimaryKey(MySqlIndexBuilder index)
    {
        return SetPrimaryKey( MySqlHelpers.GetDefaultPrimaryKeyName( Table ), index );
    }

    public MySqlPrimaryKeyBuilder SetPrimaryKey(string name, MySqlIndexBuilder index)
    {
        Table.EnsureNotRemoved();

        if ( _primaryKey is null || ! ReferenceEquals( _primaryKey.Index, index ) )
        {
            var oldPrimaryKey = _primaryKey;
            _primaryKey = Table.Schema.Objects.CreatePrimaryKey( Table, name, index, oldPrimaryKey );
            _map.Add( name, _primaryKey );
            Table.Database.ChangeTracker.PrimaryKeyUpdated( Table, oldPrimaryKey );
            return _primaryKey;
        }

        return _primaryKey.SetName( name );
    }

    public MySqlIndexBuilder CreateIndex(ReadOnlyMemory<ISqlIndexColumnBuilder> columns, bool isUnique = false)
    {
        return CreateIndex( MySqlHelpers.GetDefaultIndexName( Table, columns, isUnique ), columns, isUnique );
    }

    public MySqlIndexBuilder CreateIndex(string name, ReadOnlyMemory<ISqlIndexColumnBuilder> columns, bool isUnique = false)
    {
        Table.EnsureNotRemoved();
        var result = Table.Schema.Objects.CreateIndex( Table, name, columns, isUnique );
        _map.Add( name, result );
        return result;
    }

    public MySqlForeignKeyBuilder CreateForeignKey(MySqlIndexBuilder originIndex, MySqlIndexBuilder referencedIndex)
    {
        return CreateForeignKey( MySqlHelpers.GetDefaultForeignKeyName( originIndex, referencedIndex ), originIndex, referencedIndex );
    }

    public MySqlForeignKeyBuilder CreateForeignKey(string name, MySqlIndexBuilder originIndex, MySqlIndexBuilder referencedIndex)
    {
        Table.EnsureNotRemoved();
        var foreignKey = Table.Schema.Objects.CreateForeignKey( Table, name, originIndex, referencedIndex );
        _map.Add( name, foreignKey );
        return foreignKey;
    }

    public MySqlCheckBuilder CreateCheck(SqlConditionNode condition)
    {
        return CreateCheck( MySqlHelpers.GetDefaultCheckName( Table ), condition );
    }

    public MySqlCheckBuilder CreateCheck(string name, SqlConditionNode condition)
    {
        Table.EnsureNotRemoved();
        var check = Table.Schema.Objects.CreateCheck( Table, name, condition );
        _map.Add( name, check );
        return check;
    }

    public bool Remove(string name)
    {
        if ( ! _map.TryGetValue( name, out var obj ) || ! obj.CanRemove )
            return false;

        _map.Remove( name );
        if ( ReferenceEquals( obj, _primaryKey ) )
        {
            var oldPrimaryKey = _primaryKey;
            _primaryKey = null;
            Table.Database.ChangeTracker.PrimaryKeyUpdated( Table, oldPrimaryKey );
        }

        obj.Remove();
        return true;
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<MySqlConstraintBuilder>
    {
        private Dictionary<string, MySqlConstraintBuilder>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, MySqlConstraintBuilder> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlConstraintBuilder Current => _enumerator.Current;
        object IEnumerator.Current => Current;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            _enumerator.Dispose();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        void IEnumerator.Reset()
        {
            ((IEnumerator)_enumerator).Reset();
        }
    }

    internal RentedMemorySequence<MySqlObjectBuilder> Clear()
    {
        var buffer = Table.Database.ObjectPool.GreedyRent();
        var foreignKeyCount = 0;

        foreach ( var constraint in _map.Values )
        {
            switch ( constraint.Type )
            {
                case SqlObjectType.Index:
                    buffer.Push( constraint );
                    ReinterpretCast.To<MySqlIndexBuilder>( constraint ).ClearOriginatingForeignKeys();
                    break;

                case SqlObjectType.Check:
                    buffer.Push( constraint );
                    break;

                case SqlObjectType.ForeignKey:
                    if ( buffer.Length == foreignKeyCount )
                        buffer.Push( constraint );
                    else
                    {
                        buffer.Push( buffer[foreignKeyCount] );
                        buffer[foreignKeyCount] = constraint;
                    }

                    ++foreignKeyCount;
                    break;
            }
        }

        _map.Clear();
        if ( _primaryKey is not null )
        {
            var oldPrimaryKey = _primaryKey;
            _primaryKey = null;
            Table.Database.ChangeTracker.PrimaryKeyUpdated( Table, oldPrimaryKey );
        }

        return buffer;
    }

    internal void MarkAllAsRemoved()
    {
        foreach ( var constraint in _map.Values )
            constraint.MarkAsRemoved();

        _primaryKey = null;
        _map.Clear();
    }

    internal void ChangeName(MySqlConstraintBuilder constraint, string name)
    {
        _map.Add( name, constraint );
        _map.Remove( constraint.Name );
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : MySqlConstraintBuilder
    {
        var obj = _map[name];
        return obj.Type == type
            ? ReinterpretCast.To<T>( obj )
            : throw new SqlObjectCastException( MySqlDialect.Instance, typeof( T ), obj.GetType() );
    }

    [Pure]
    private T? TryGetTypedObject<T>(string name, SqlObjectType type)
        where T : MySqlConstraintBuilder
    {
        return _map.TryGetValue( name, out var obj ) && obj.Type == type ? ReinterpretCast.To<T>( obj ) : null;
    }

    [Pure]
    ISqlConstraintBuilder ISqlConstraintBuilderCollection.GetConstraint(string name)
    {
        return GetConstraint( name );
    }

    [Pure]
    ISqlConstraintBuilder? ISqlConstraintBuilderCollection.TryGetConstraint(string name)
    {
        return TryGetConstraint( name );
    }

    [Pure]
    ISqlPrimaryKeyBuilder ISqlConstraintBuilderCollection.GetPrimaryKey()
    {
        return GetPrimaryKey();
    }

    [Pure]
    ISqlPrimaryKeyBuilder? ISqlConstraintBuilderCollection.TryGetPrimaryKey()
    {
        return TryGetPrimaryKey();
    }

    [Pure]
    ISqlIndexBuilder ISqlConstraintBuilderCollection.GetIndex(string name)
    {
        return GetIndex( name );
    }

    [Pure]
    ISqlIndexBuilder? ISqlConstraintBuilderCollection.TryGetIndex(string name)
    {
        return TryGetIndex( name );
    }

    [Pure]
    ISqlForeignKeyBuilder ISqlConstraintBuilderCollection.GetForeignKey(string name)
    {
        return GetForeignKey( name );
    }

    [Pure]
    ISqlForeignKeyBuilder? ISqlConstraintBuilderCollection.TryGetForeignKey(string name)
    {
        return TryGetForeignKey( name );
    }

    [Pure]
    ISqlCheckBuilder ISqlConstraintBuilderCollection.GetCheck(string name)
    {
        return GetCheck( name );
    }

    [Pure]
    ISqlCheckBuilder? ISqlConstraintBuilderCollection.TryGetCheck(string name)
    {
        return TryGetCheck( name );
    }

    ISqlPrimaryKeyBuilder ISqlConstraintBuilderCollection.SetPrimaryKey(ISqlIndexBuilder index)
    {
        return SetPrimaryKey( MySqlHelpers.CastOrThrow<MySqlIndexBuilder>( index ) );
    }

    ISqlPrimaryKeyBuilder ISqlConstraintBuilderCollection.SetPrimaryKey(string name, ISqlIndexBuilder index)
    {
        return SetPrimaryKey( name, MySqlHelpers.CastOrThrow<MySqlIndexBuilder>( index ) );
    }

    ISqlIndexBuilder ISqlConstraintBuilderCollection.CreateIndex(ReadOnlyMemory<ISqlIndexColumnBuilder> columns, bool isUnique)
    {
        return CreateIndex( columns, isUnique );
    }

    ISqlIndexBuilder ISqlConstraintBuilderCollection.CreateIndex(
        string name,
        ReadOnlyMemory<ISqlIndexColumnBuilder> columns,
        bool isUnique)
    {
        return CreateIndex( name, columns, isUnique );
    }

    ISqlForeignKeyBuilder ISqlConstraintBuilderCollection.CreateForeignKey(
        ISqlIndexBuilder originIndex,
        ISqlIndexBuilder referencedIndex)
    {
        return CreateForeignKey(
            MySqlHelpers.CastOrThrow<MySqlIndexBuilder>( originIndex ),
            MySqlHelpers.CastOrThrow<MySqlIndexBuilder>( referencedIndex ) );
    }

    ISqlForeignKeyBuilder ISqlConstraintBuilderCollection.CreateForeignKey(
        string name,
        ISqlIndexBuilder originIndex,
        ISqlIndexBuilder referencedIndex)
    {
        return CreateForeignKey(
            name,
            MySqlHelpers.CastOrThrow<MySqlIndexBuilder>( originIndex ),
            MySqlHelpers.CastOrThrow<MySqlIndexBuilder>( referencedIndex ) );
    }

    ISqlCheckBuilder ISqlConstraintBuilderCollection.CreateCheck(SqlConditionNode condition)
    {
        return CreateCheck( condition );
    }

    ISqlCheckBuilder ISqlConstraintBuilderCollection.CreateCheck(string name, SqlConditionNode condition)
    {
        return CreateCheck( name, condition );
    }

    [Pure]
    IEnumerator<ISqlConstraintBuilder> IEnumerable<ISqlConstraintBuilder>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
