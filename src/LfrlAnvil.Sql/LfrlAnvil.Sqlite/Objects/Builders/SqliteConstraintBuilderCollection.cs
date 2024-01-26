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
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteConstraintBuilderCollection : ISqlConstraintBuilderCollection
{
    private readonly Dictionary<string, SqliteConstraintBuilder> _map;
    private SqlitePrimaryKeyBuilder? _primaryKey;

    internal SqliteConstraintBuilderCollection(SqliteTableBuilder table)
    {
        Table = table;
        _map = new Dictionary<string, SqliteConstraintBuilder>( StringComparer.OrdinalIgnoreCase );
        _primaryKey = null;
    }

    public SqliteTableBuilder Table { get; }
    public int Count => _map.Count;

    ISqlTableBuilder ISqlConstraintBuilderCollection.Table => Table;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqliteConstraintBuilder GetConstraint(string name)
    {
        return _map[name];
    }

    [Pure]
    public SqliteConstraintBuilder? TryGetConstraint(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public SqlitePrimaryKeyBuilder GetPrimaryKey()
    {
        return TryGetPrimaryKey() ?? throw new SqliteObjectBuilderException( ExceptionResources.PrimaryKeyIsMissing( Table ) );
    }

    [Pure]
    public SqlitePrimaryKeyBuilder? TryGetPrimaryKey()
    {
        return _primaryKey;
    }

    [Pure]
    public SqliteIndexBuilder GetIndex(string name)
    {
        return GetTypedObject<SqliteIndexBuilder>( name, SqlObjectType.Index );
    }

    [Pure]
    public SqliteIndexBuilder? TryGetIndex(string name)
    {
        return TryGetTypedObject<SqliteIndexBuilder>( name, SqlObjectType.Index );
    }

    [Pure]
    public SqliteForeignKeyBuilder GetForeignKey(string name)
    {
        return GetTypedObject<SqliteForeignKeyBuilder>( name, SqlObjectType.ForeignKey );
    }

    [Pure]
    public SqliteForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return TryGetTypedObject<SqliteForeignKeyBuilder>( name, SqlObjectType.ForeignKey );
    }

    [Pure]
    public SqliteCheckBuilder GetCheck(string name)
    {
        return GetTypedObject<SqliteCheckBuilder>( name, SqlObjectType.Check );
    }

    [Pure]
    public SqliteCheckBuilder? TryGetCheck(string name)
    {
        return TryGetTypedObject<SqliteCheckBuilder>( name, SqlObjectType.Check );
    }

    public SqlitePrimaryKeyBuilder SetPrimaryKey(SqliteIndexBuilder index)
    {
        return SetPrimaryKey( SqliteHelpers.GetDefaultPrimaryKeyName( Table ), index );
    }

    public SqlitePrimaryKeyBuilder SetPrimaryKey(string name, SqliteIndexBuilder index)
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

    public SqliteIndexBuilder CreateIndex(ReadOnlyMemory<ISqlIndexColumnBuilder> columns, bool isUnique = false)
    {
        return CreateIndex( SqliteHelpers.GetDefaultIndexName( Table, columns, isUnique ), columns, isUnique );
    }

    public SqliteIndexBuilder CreateIndex(string name, ReadOnlyMemory<ISqlIndexColumnBuilder> columns, bool isUnique = false)
    {
        Table.EnsureNotRemoved();
        var result = Table.Schema.Objects.CreateIndex( Table, name, columns, isUnique );
        _map.Add( name, result );
        return result;
    }

    public SqliteForeignKeyBuilder CreateForeignKey(SqliteIndexBuilder originIndex, SqliteIndexBuilder referencedIndex)
    {
        return CreateForeignKey( SqliteHelpers.GetDefaultForeignKeyName( originIndex, referencedIndex ), originIndex, referencedIndex );
    }

    public SqliteForeignKeyBuilder CreateForeignKey(string name, SqliteIndexBuilder originIndex, SqliteIndexBuilder referencedIndex)
    {
        Table.EnsureNotRemoved();
        var foreignKey = Table.Schema.Objects.CreateForeignKey( Table, name, originIndex, referencedIndex );
        _map.Add( name, foreignKey );
        return foreignKey;
    }

    public SqliteCheckBuilder CreateCheck(SqlConditionNode condition)
    {
        return CreateCheck( SqliteHelpers.GetDefaultCheckName( Table ), condition );
    }

    public SqliteCheckBuilder CreateCheck(string name, SqlConditionNode condition)
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

    public struct Enumerator : IEnumerator<SqliteConstraintBuilder>
    {
        private Dictionary<string, SqliteConstraintBuilder>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, SqliteConstraintBuilder> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public SqliteConstraintBuilder Current => _enumerator.Current;
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

    internal RentedMemorySequence<SqliteObjectBuilder> Clear()
    {
        var buffer = Table.Database.ObjectPool.GreedyRent();
        var foreignKeyCount = 0;

        foreach ( var constraint in _map.Values )
        {
            switch ( constraint.Type )
            {
                case SqlObjectType.Index:
                    buffer.Push( constraint );
                    ReinterpretCast.To<SqliteIndexBuilder>( constraint ).ClearOriginatingForeignKeys();
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

    internal void Reactivate(SqliteForeignKeyBuilder foreignKey)
    {
        _map.Add( foreignKey.Name, foreignKey );
    }

    internal void ChangeName(SqliteConstraintBuilder constraint, string name)
    {
        _map.Add( name, constraint );
        _map.Remove( constraint.Name );
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : SqliteConstraintBuilder
    {
        var obj = _map[name];
        return obj.Type == type
            ? ReinterpretCast.To<T>( obj )
            : throw new SqlObjectCastException( SqliteDialect.Instance, typeof( T ), obj.GetType() );
    }

    [Pure]
    private T? TryGetTypedObject<T>(string name, SqlObjectType type)
        where T : SqliteConstraintBuilder
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
        return SetPrimaryKey( SqliteHelpers.CastOrThrow<SqliteIndexBuilder>( index ) );
    }

    ISqlPrimaryKeyBuilder ISqlConstraintBuilderCollection.SetPrimaryKey(string name, ISqlIndexBuilder index)
    {
        return SetPrimaryKey( name, SqliteHelpers.CastOrThrow<SqliteIndexBuilder>( index ) );
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
            SqliteHelpers.CastOrThrow<SqliteIndexBuilder>( originIndex ),
            SqliteHelpers.CastOrThrow<SqliteIndexBuilder>( referencedIndex ) );
    }

    ISqlForeignKeyBuilder ISqlConstraintBuilderCollection.CreateForeignKey(
        string name,
        ISqlIndexBuilder originIndex,
        ISqlIndexBuilder referencedIndex)
    {
        return CreateForeignKey(
            name,
            SqliteHelpers.CastOrThrow<SqliteIndexBuilder>( originIndex ),
            SqliteHelpers.CastOrThrow<SqliteIndexBuilder>( referencedIndex ) );
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
