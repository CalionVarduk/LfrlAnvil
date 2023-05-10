﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Builders;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Builders;

public sealed class SqliteObjectBuilderCollection : ISqlObjectBuilderCollection
{
    private readonly Dictionary<string, SqliteObjectBuilder> _map;

    internal SqliteObjectBuilderCollection(SqliteSchemaBuilder schema)
    {
        Schema = schema;
        _map = new Dictionary<string, SqliteObjectBuilder>();
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

    public bool Remove(string name)
    {
        if ( ! _map.TryGetValue( name, out var obj ) || ! obj.CanRemove )
            return false;

        _map.Remove( name );
        obj.Remove();
        return true;
    }

    [Pure]
    public IReadOnlyCollection<SqliteObjectBuilder> AsCollection()
    {
        return _map.Values;
    }

    [Pure]
    public IEnumerator<SqliteObjectBuilder> GetEnumerator()
    {
        return AsCollection().GetEnumerator();
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

    internal RentedMemorySequence<SqliteObjectBuilder> ClearIntoBuffer()
    {
        var buffer = CopyTablesIntoBuffer();
        _map.Clear();
        return buffer;
    }

    internal RentedMemorySequence<SqliteObjectBuilder> CopyTablesIntoBuffer()
    {
        var buffer = Schema.Database.ObjectPool.GreedyRent();
        foreach ( var obj in _map.Values )
        {
            if ( obj.Type == SqlObjectType.Table )
                buffer.Push( obj );
        }

        return buffer;
    }

    internal void Reactivate(SqliteObjectBuilder obj)
    {
        _map.Add( obj.Name, obj );
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
        return GetTypedObject<ISqlTableBuilder>( name, SqlObjectType.Table );
    }

    bool ISqlObjectBuilderCollection.TryGetTable(string name, [MaybeNullWhen( false )] out ISqlTableBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.Table, out result );
    }

    [Pure]
    ISqlIndexBuilder ISqlObjectBuilderCollection.GetIndex(string name)
    {
        return GetTypedObject<ISqlIndexBuilder>( name, SqlObjectType.Index );
    }

    bool ISqlObjectBuilderCollection.TryGetIndex(string name, [MaybeNullWhen( false )] out ISqlIndexBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.Index, out result );
    }

    [Pure]
    ISqlPrimaryKeyBuilder ISqlObjectBuilderCollection.GetPrimaryKey(string name)
    {
        return GetTypedObject<ISqlPrimaryKeyBuilder>( name, SqlObjectType.PrimaryKey );
    }

    bool ISqlObjectBuilderCollection.TryGetPrimaryKey(string name, [MaybeNullWhen( false )] out ISqlPrimaryKeyBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.PrimaryKey, out result );
    }

    [Pure]
    ISqlForeignKeyBuilder ISqlObjectBuilderCollection.GetForeignKey(string name)
    {
        return GetTypedObject<ISqlForeignKeyBuilder>( name, SqlObjectType.ForeignKey );
    }

    bool ISqlObjectBuilderCollection.TryGetForeignKey(string name, [MaybeNullWhen( false )] out ISqlForeignKeyBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.ForeignKey, out result );
    }

    ISqlTableBuilder ISqlObjectBuilderCollection.CreateTable(string name)
    {
        return CreateTable( name );
    }

    ISqlTableBuilder ISqlObjectBuilderCollection.GetOrCreateTable(string name)
    {
        return GetOrCreateTable( name );
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