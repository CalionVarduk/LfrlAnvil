using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteObjectBuilderCollection : ISqlObjectBuilderCollection
{
    private readonly Dictionary<string, SqliteObjectBuilder> _map;

    internal SqliteObjectBuilderCollection(SqliteSchemaBuilder schema)
    {
        Schema = schema;
        _map = new Dictionary<string, SqliteObjectBuilder>( StringComparer.OrdinalIgnoreCase );
    }

    public SqliteSchemaBuilder Schema { get; }
    public int Count => _map.Count;

    ISqlSchemaBuilder ISqlObjectBuilderCollection.Schema => Schema;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqliteObjectBuilder Get(string name)
    {
        return _map[name];
    }

    public bool TryGet(string name, [MaybeNullWhen( false )] out SqliteObjectBuilder result)
    {
        return _map.TryGetValue( name, out result );
    }

    [Pure]
    public SqliteTableBuilder GetTable(string name)
    {
        return GetTypedObject<SqliteTableBuilder>( name, SqlObjectType.Table );
    }

    public bool TryGetTable(string name, [MaybeNullWhen( false )] out SqliteTableBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.Table, out result );
    }

    [Pure]
    public SqliteIndexBuilder GetIndex(string name)
    {
        return GetTypedObject<SqliteIndexBuilder>( name, SqlObjectType.Index );
    }

    public bool TryGetIndex(string name, [MaybeNullWhen( false )] out SqliteIndexBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.Index, out result );
    }

    [Pure]
    public SqlitePrimaryKeyBuilder GetPrimaryKey(string name)
    {
        return GetTypedObject<SqlitePrimaryKeyBuilder>( name, SqlObjectType.PrimaryKey );
    }

    public bool TryGetPrimaryKey(string name, [MaybeNullWhen( false )] out SqlitePrimaryKeyBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.PrimaryKey, out result );
    }

    [Pure]
    public SqliteForeignKeyBuilder GetForeignKey(string name)
    {
        return GetTypedObject<SqliteForeignKeyBuilder>( name, SqlObjectType.ForeignKey );
    }

    public bool TryGetForeignKey(string name, [MaybeNullWhen( false )] out SqliteForeignKeyBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.ForeignKey, out result );
    }

    [Pure]
    public SqliteCheckBuilder GetCheck(string name)
    {
        return GetTypedObject<SqliteCheckBuilder>( name, SqlObjectType.Check );
    }

    public bool TryGetCheck(string name, [MaybeNullWhen( false )] out SqliteCheckBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.Check, out result );
    }

    [Pure]
    public SqliteViewBuilder GetView(string name)
    {
        return GetTypedObject<SqliteViewBuilder>( name, SqlObjectType.View );
    }

    public bool TryGetView(string name, [MaybeNullWhen( false )] out SqliteViewBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.View, out result );
    }

    public SqliteTableBuilder CreateTable(string name)
    {
        Schema.EnsureNotRemoved();
        SqliteHelpers.AssertName( name );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var table = CreateNewTable( name );
        obj = table;
        return table;
    }

    public SqliteTableBuilder GetOrCreateTable(string name)
    {
        Schema.EnsureNotRemoved();
        SqliteHelpers.AssertName( name );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
        {
            if ( obj.Type != SqlObjectType.Table )
                throw new SqliteObjectCastException( typeof( SqliteTableBuilder ), obj.GetType() );

            return ReinterpretCast.To<SqliteTableBuilder>( obj );
        }

        var table = CreateNewTable( name );
        obj = table;
        return table;
    }

    public SqliteViewBuilder CreateView(string name, SqlQueryExpressionNode source)
    {
        Schema.EnsureNotRemoved();
        SqliteHelpers.AssertName( name );
        var visitor = SqliteViewBuilder.AssertSourceNode( Schema.Database, source );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var view = new SqliteViewBuilder( Schema, name, source, visitor );
        Schema.Database.ChangeTracker.ObjectCreated( view );
        obj = view;
        return view;
    }

    public bool Remove(string name)
    {
        if ( ! _map.TryGetValue( name, out var obj ) || ! obj.CanRemove )
            return false;

        _map.Remove( name );
        obj.Remove();
        return true;
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<SqliteObjectBuilder>
    {
        private Dictionary<string, SqliteObjectBuilder>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, SqliteObjectBuilder> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public SqliteObjectBuilder Current => _enumerator.Current;
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

    internal void ChangeName(SqliteObjectBuilder obj, string name)
    {
        ref var objRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( objRef, name ) );

        objRef = obj;
        _map.Remove( obj.Name );
    }

    internal SqliteIndexBuilder CreateIndex(SqliteTableBuilder table, SqliteIndexColumnBuilder[] columns, bool isUnique)
    {
        var name = SqliteHelpers.GetDefaultIndexName( table, columns, isUnique );
        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = new SqliteIndexBuilder( table, columns, name, isUnique );
        obj = result;
        Schema.Database.ChangeTracker.ObjectCreated( table, result );
        return result;
    }

    internal SqlitePrimaryKeyBuilder CreatePrimaryKey(SqliteTableBuilder table, SqliteIndexColumnBuilder[] columns)
    {
        var name = SqliteHelpers.GetDefaultPrimaryKeyName( table );
        if ( _map.TryGetValue( name, out var obj ) )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var index = table.Indexes.GetOrCreateForPrimaryKey( columns );
        var primaryKey = new SqlitePrimaryKeyBuilder( index, name );
        _map.Add( name, primaryKey );
        Schema.Database.ChangeTracker.ObjectCreated( table, primaryKey );
        index.AssignPrimaryKey( primaryKey );
        return primaryKey;
    }

    internal SqlitePrimaryKeyBuilder ReplacePrimaryKey(
        SqliteTableBuilder table,
        SqliteIndexColumnBuilder[] columns,
        SqlitePrimaryKeyBuilder oldKey)
    {
        oldKey.Remove();
        var index = table.Indexes.GetOrCreateForPrimaryKey( columns );

        var primaryKey = new SqlitePrimaryKeyBuilder( index, oldKey.Name );
        _map[primaryKey.Name] = primaryKey;
        index.AssignPrimaryKey( primaryKey );
        return primaryKey;
    }

    internal SqliteForeignKeyBuilder CreateForeignKey(
        SqliteTableBuilder table,
        SqliteIndexBuilder index,
        SqliteIndexBuilder referencedIndex)
    {
        SqliteHelpers.AssertForeignKey( table, index, referencedIndex );

        var name = SqliteHelpers.GetDefaultForeignKeyName( index, referencedIndex );
        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = new SqliteForeignKeyBuilder( index, referencedIndex, name );
        Schema.Database.ChangeTracker.ObjectCreated( index.Table, result );
        obj = result;
        return result;
    }

    internal SqliteCheckBuilder CreateCheck(SqliteTableBuilder table, string name, SqlConditionNode condition)
    {
        Schema.EnsureNotRemoved();
        SqliteHelpers.AssertName( name );
        var visitor = SqliteCheckBuilder.AssertConditionNode( table, condition );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = new SqliteCheckBuilder( name, condition, visitor );
        Schema.Database.ChangeTracker.ObjectCreated( table, result );
        obj = result;
        return result;
    }

    internal void Clear()
    {
        _map.Clear();
    }

    internal RentedMemorySequence<SqliteObjectBuilder> CopyTablesIntoBuffer()
    {
        return CopyObjectsIntoBuffer( SqlObjectType.Table );
    }

    internal RentedMemorySequence<SqliteObjectBuilder> CopyViewsIntoBuffer()
    {
        return CopyObjectsIntoBuffer( SqlObjectType.View );
    }

    internal void ForceRemove(SqliteObjectBuilder obj)
    {
        _map.Remove( obj.Name );
    }

    internal void Reactivate(SqliteObjectBuilder obj)
    {
        _map.Add( obj.Name, obj );
    }

    private RentedMemorySequence<SqliteObjectBuilder> CopyObjectsIntoBuffer(SqlObjectType type)
    {
        var buffer = Schema.Database.ObjectPool.GreedyRent();
        foreach ( var obj in _map.Values )
        {
            if ( obj.Type == type )
                buffer.Push( obj );
        }

        return buffer;
    }

    [Pure]
    private SqliteTableBuilder CreateNewTable(string name)
    {
        var result = new SqliteTableBuilder( Schema, name );
        Schema.Database.ChangeTracker.ObjectCreated( result, result );
        return result;
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : class, ISqlObjectBuilder
    {
        var obj = _map[name];
        if ( obj.Type != type )
            throw new SqliteObjectCastException( typeof( T ), obj.GetType() );

        return ReinterpretCast.To<T>( obj );
    }

    private bool TryGetTypedObject<T>(string name, SqlObjectType type, [MaybeNullWhen( false )] out T result)
        where T : class, ISqlObjectBuilder
    {
        if ( _map.TryGetValue( name, out var obj ) && obj.Type == type )
        {
            result = ReinterpretCast.To<T>( obj );
            return true;
        }

        result = null;
        return false;
    }

    [Pure]
    ISqlObjectBuilder ISqlObjectBuilderCollection.Get(string name)
    {
        return Get( name );
    }

    bool ISqlObjectBuilderCollection.TryGet(string name, [MaybeNullWhen( false )] out ISqlObjectBuilder result)
    {
        if ( TryGet( name, out var obj ) )
        {
            result = obj;
            return true;
        }

        result = null;
        return false;
    }

    [Pure]
    ISqlTableBuilder ISqlObjectBuilderCollection.GetTable(string name)
    {
        return GetTable( name );
    }

    bool ISqlObjectBuilderCollection.TryGetTable(string name, [MaybeNullWhen( false )] out ISqlTableBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.Table, out result );
    }

    [Pure]
    ISqlIndexBuilder ISqlObjectBuilderCollection.GetIndex(string name)
    {
        return GetIndex( name );
    }

    bool ISqlObjectBuilderCollection.TryGetIndex(string name, [MaybeNullWhen( false )] out ISqlIndexBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.Index, out result );
    }

    [Pure]
    ISqlPrimaryKeyBuilder ISqlObjectBuilderCollection.GetPrimaryKey(string name)
    {
        return GetPrimaryKey( name );
    }

    bool ISqlObjectBuilderCollection.TryGetPrimaryKey(string name, [MaybeNullWhen( false )] out ISqlPrimaryKeyBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.PrimaryKey, out result );
    }

    [Pure]
    ISqlForeignKeyBuilder ISqlObjectBuilderCollection.GetForeignKey(string name)
    {
        return GetForeignKey( name );
    }

    bool ISqlObjectBuilderCollection.TryGetForeignKey(string name, [MaybeNullWhen( false )] out ISqlForeignKeyBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.ForeignKey, out result );
    }

    [Pure]
    ISqlCheckBuilder ISqlObjectBuilderCollection.GetCheck(string name)
    {
        return GetCheck( name );
    }

    bool ISqlObjectBuilderCollection.TryGetCheck(string name, [MaybeNullWhen( false )] out ISqlCheckBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.Check, out result );
    }

    [Pure]
    ISqlViewBuilder ISqlObjectBuilderCollection.GetView(string name)
    {
        return GetView( name );
    }

    bool ISqlObjectBuilderCollection.TryGetView(string name, [MaybeNullWhen( false )] out ISqlViewBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.View, out result );
    }

    ISqlTableBuilder ISqlObjectBuilderCollection.CreateTable(string name)
    {
        return CreateTable( name );
    }

    ISqlTableBuilder ISqlObjectBuilderCollection.GetOrCreateTable(string name)
    {
        return GetOrCreateTable( name );
    }

    ISqlViewBuilder ISqlObjectBuilderCollection.CreateView(string name, SqlQueryExpressionNode source)
    {
        return CreateView( name, source );
    }

    [Pure]
    IEnumerator<ISqlObjectBuilder> IEnumerable<ISqlObjectBuilder>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
