using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlObjectBuilderCollection : ISqlObjectBuilderCollection
{
    private readonly Dictionary<string, MySqlObjectBuilder> _map;

    internal MySqlObjectBuilderCollection(MySqlSchemaBuilder schema)
    {
        Schema = schema;
        _map = new Dictionary<string, MySqlObjectBuilder>( StringComparer.OrdinalIgnoreCase );
    }

    public MySqlSchemaBuilder Schema { get; }
    public int Count => _map.Count;

    ISqlSchemaBuilder ISqlObjectBuilderCollection.Schema => Schema;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public MySqlObjectBuilder Get(string name)
    {
        return _map[name];
    }

    public bool TryGet(string name, [MaybeNullWhen( false )] out MySqlObjectBuilder result)
    {
        return _map.TryGetValue( name, out result );
    }

    [Pure]
    public MySqlTableBuilder GetTable(string name)
    {
        return GetTypedObject<MySqlTableBuilder>( name, SqlObjectType.Table );
    }

    public bool TryGetTable(string name, [MaybeNullWhen( false )] out MySqlTableBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.Table, out result );
    }

    [Pure]
    public MySqlIndexBuilder GetIndex(string name)
    {
        return GetTypedObject<MySqlIndexBuilder>( name, SqlObjectType.Index );
    }

    public bool TryGetIndex(string name, [MaybeNullWhen( false )] out MySqlIndexBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.Index, out result );
    }

    [Pure]
    public MySqlPrimaryKeyBuilder GetPrimaryKey(string name)
    {
        return GetTypedObject<MySqlPrimaryKeyBuilder>( name, SqlObjectType.PrimaryKey );
    }

    public bool TryGetPrimaryKey(string name, [MaybeNullWhen( false )] out MySqlPrimaryKeyBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.PrimaryKey, out result );
    }

    [Pure]
    public MySqlForeignKeyBuilder GetForeignKey(string name)
    {
        return GetTypedObject<MySqlForeignKeyBuilder>( name, SqlObjectType.ForeignKey );
    }

    public bool TryGetForeignKey(string name, [MaybeNullWhen( false )] out MySqlForeignKeyBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.ForeignKey, out result );
    }

    [Pure]
    public MySqlCheckBuilder GetCheck(string name)
    {
        return GetTypedObject<MySqlCheckBuilder>( name, SqlObjectType.Check );
    }

    public bool TryGetCheck(string name, [MaybeNullWhen( false )] out MySqlCheckBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.Check, out result );
    }

    [Pure]
    public MySqlViewBuilder GetView(string name)
    {
        return GetTypedObject<MySqlViewBuilder>( name, SqlObjectType.View );
    }

    public bool TryGetView(string name, [MaybeNullWhen( false )] out MySqlViewBuilder result)
    {
        return TryGetTypedObject( name, SqlObjectType.View, out result );
    }

    public MySqlTableBuilder CreateTable(string name)
    {
        Schema.EnsureNotRemoved();
        MySqlHelpers.AssertName( name );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var table = CreateNewTable( name );
        obj = table;
        return table;
    }

    public MySqlTableBuilder GetOrCreateTable(string name)
    {
        Schema.EnsureNotRemoved();
        MySqlHelpers.AssertName( name );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
        {
            if ( obj.Type != SqlObjectType.Table )
                throw new SqlObjectCastException( MySqlDialect.Instance, typeof( MySqlTableBuilder ), obj.GetType() );

            return ReinterpretCast.To<MySqlTableBuilder>( obj );
        }

        var table = CreateNewTable( name );
        obj = table;
        return table;
    }

    public MySqlViewBuilder CreateView(string name, SqlQueryExpressionNode source)
    {
        Schema.EnsureNotRemoved();
        MySqlHelpers.AssertName( name );
        var visitor = MySqlViewBuilder.AssertSourceNode( Schema.Database, source );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var view = new MySqlViewBuilder( Schema, name, source, visitor );
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

    public struct Enumerator : IEnumerator<MySqlObjectBuilder>
    {
        private Dictionary<string, MySqlObjectBuilder>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, MySqlObjectBuilder> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlObjectBuilder Current => _enumerator.Current;
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

    internal void ChangeName(MySqlObjectBuilder obj, string name)
    {
        ref var objRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( objRef, name ) );

        objRef = obj;
        _map.Remove( obj.Name );
    }

    internal MySqlIndexBuilder CreateIndex(MySqlTableBuilder table, MySqlIndexColumnBuilder[] columns, bool isUnique)
    {
        var name = MySqlHelpers.GetDefaultIndexName( table, columns, isUnique );
        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = new MySqlIndexBuilder( table, columns, name, isUnique );
        obj = result;
        Schema.Database.ChangeTracker.ObjectCreated( table, result );
        return result;
    }

    internal MySqlPrimaryKeyBuilder CreatePrimaryKey(MySqlTableBuilder table, MySqlIndexColumnBuilder[] columns)
    {
        var name = MySqlHelpers.GetDefaultPrimaryKeyName( table );
        if ( _map.TryGetValue( name, out var obj ) )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var index = table.Indexes.GetOrCreateForPrimaryKey( columns );
        var primaryKey = new MySqlPrimaryKeyBuilder( index, name );
        _map.Add( name, primaryKey );
        Schema.Database.ChangeTracker.ObjectCreated( table, primaryKey );
        index.AssignPrimaryKey( primaryKey );
        return primaryKey;
    }

    internal MySqlPrimaryKeyBuilder ReplacePrimaryKey(
        MySqlTableBuilder table,
        MySqlIndexColumnBuilder[] columns,
        MySqlPrimaryKeyBuilder oldKey)
    {
        oldKey.Remove();
        var index = table.Indexes.GetOrCreateForPrimaryKey( columns );

        var primaryKey = new MySqlPrimaryKeyBuilder( index, oldKey.Name );
        _map[primaryKey.Name] = primaryKey;
        index.AssignPrimaryKey( primaryKey );
        return primaryKey;
    }

    internal MySqlForeignKeyBuilder CreateForeignKey(
        MySqlTableBuilder table,
        MySqlIndexBuilder originIndex,
        MySqlIndexBuilder referencedIndex)
    {
        MySqlHelpers.AssertForeignKey( table, originIndex, referencedIndex );

        var name = MySqlHelpers.GetDefaultForeignKeyName( originIndex, referencedIndex );
        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = new MySqlForeignKeyBuilder( originIndex, referencedIndex, name );
        Schema.Database.ChangeTracker.ObjectCreated( originIndex.Table, result );
        obj = result;
        return result;
    }

    internal MySqlCheckBuilder CreateCheck(MySqlTableBuilder table, string name, SqlConditionNode condition)
    {
        Schema.EnsureNotRemoved();
        MySqlHelpers.AssertName( name );
        var visitor = MySqlCheckBuilder.AssertConditionNode( table, condition );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = new MySqlCheckBuilder( name, condition, visitor );
        Schema.Database.ChangeTracker.ObjectCreated( table, result );
        obj = result;
        return result;
    }

    internal void Clear()
    {
        _map.Clear();
    }

    internal void ForceRemove(MySqlObjectBuilder obj)
    {
        _map.Remove( obj.Name );
    }

    internal void Reactivate(MySqlObjectBuilder obj)
    {
        _map.Add( obj.Name, obj );
    }

    [Pure]
    private MySqlTableBuilder CreateNewTable(string name)
    {
        var result = new MySqlTableBuilder( Schema, name );
        Schema.Database.ChangeTracker.ObjectCreated( result, result );
        return result;
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : class, ISqlObjectBuilder
    {
        var obj = _map[name];
        if ( obj.Type != type )
            throw new SqlObjectCastException( MySqlDialect.Instance, typeof( T ), obj.GetType() );

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
