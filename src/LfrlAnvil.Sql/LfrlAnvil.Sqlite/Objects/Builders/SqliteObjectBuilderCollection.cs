using System;
using System.Collections;
using System.Collections.Generic;
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
    public SqliteObjectBuilder GetObject(string name)
    {
        return _map[name];
    }

    [Pure]
    public SqliteObjectBuilder? TryGetObject(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public SqliteTableBuilder GetTable(string name)
    {
        return GetTypedObject<SqliteTableBuilder>( name, SqlObjectType.Table );
    }

    [Pure]
    public SqliteTableBuilder? TryGetTable(string name)
    {
        return TryGetTypedObject<SqliteTableBuilder>( name, SqlObjectType.Table );
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
    public SqlitePrimaryKeyBuilder GetPrimaryKey(string name)
    {
        return GetTypedObject<SqlitePrimaryKeyBuilder>( name, SqlObjectType.PrimaryKey );
    }

    [Pure]
    public SqlitePrimaryKeyBuilder? TryGetPrimaryKey(string name)
    {
        return TryGetTypedObject<SqlitePrimaryKeyBuilder>( name, SqlObjectType.PrimaryKey );
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

    [Pure]
    public SqliteViewBuilder GetView(string name)
    {
        return GetTypedObject<SqliteViewBuilder>( name, SqlObjectType.View );
    }

    [Pure]
    public SqliteViewBuilder? TryGetView(string name)
    {
        return TryGetTypedObject<SqliteViewBuilder>( name, SqlObjectType.View );
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
                throw new SqlObjectCastException( SqliteDialect.Instance, typeof( SqliteTableBuilder ), obj.GetType() );

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
        Schema.Database.Changes.ObjectCreated( view );
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

    internal void ChangeName(SqliteTableBuilder table, string name)
    {
        ChangeNameCore( table, name );
    }

    internal void ChangeName(SqliteViewBuilder view, string name)
    {
        ChangeNameCore( view, name );
    }

    internal void ChangeName(SqliteConstraintBuilder constraint, string name)
    {
        ChangeNameCore( constraint, name );
        constraint.Table.Constraints.ChangeName( constraint, name );
    }

    internal SqliteIndexBuilder CreateIndex(
        SqliteTableBuilder table,
        string name,
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns,
        bool isUnique)
    {
        SqliteHelpers.AssertName( name );
        var indexColumns = SqliteHelpers.CreateIndexColumns( table, columns );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = new SqliteIndexBuilder( table, indexColumns, name, isUnique );
        obj = result;
        Schema.Database.Changes.ObjectCreated( table, result );
        return result;
    }

    internal SqlitePrimaryKeyBuilder CreatePrimaryKey(
        SqliteTableBuilder table,
        string name,
        SqliteIndexBuilder index,
        SqlitePrimaryKeyBuilder? oldPrimaryKey)
    {
        SqliteHelpers.AssertName( name );
        SqliteHelpers.AssertPrimaryKey( table, index );

        if ( _map.TryGetValue( name, out var obj ) && ! CanReplaceWithPrimaryKey( obj, oldPrimaryKey ) )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        oldPrimaryKey?.Remove();

        var primaryKey = new SqlitePrimaryKeyBuilder( index, name );
        _map[name] = primaryKey;
        Schema.Database.Changes.ObjectCreated( table, primaryKey );
        index.AssignPrimaryKey( primaryKey );
        return primaryKey;
    }

    internal SqliteForeignKeyBuilder CreateForeignKey(
        SqliteTableBuilder table,
        string name,
        SqliteIndexBuilder originIndex,
        SqliteIndexBuilder referencedIndex)
    {
        SqliteHelpers.AssertName( name );
        SqliteHelpers.AssertForeignKey( table, originIndex, referencedIndex );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = new SqliteForeignKeyBuilder( originIndex, referencedIndex, name );
        obj = result;
        Schema.Database.Changes.ObjectCreated( originIndex.Table, result );
        return result;
    }

    internal SqliteCheckBuilder CreateCheck(SqliteTableBuilder table, string name, SqlConditionNode condition)
    {
        SqliteHelpers.AssertName( name );
        var visitor = SqliteCheckBuilder.AssertConditionNode( table, condition );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = new SqliteCheckBuilder( name, condition, visitor );
        obj = result;
        Schema.Database.Changes.ObjectCreated( table, result );
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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool CanReplaceWithPrimaryKey(SqliteObjectBuilder obj, SqlitePrimaryKeyBuilder? oldPrimaryKey)
    {
        if ( oldPrimaryKey is null )
            return false;

        if ( ReferenceEquals( obj, oldPrimaryKey ) )
            return true;

        return obj.Type == SqlObjectType.Index &&
            ReferenceEquals( ReinterpretCast.To<SqliteIndexBuilder>( obj ).PrimaryKey, oldPrimaryKey );
    }

    private void ChangeNameCore(SqliteObjectBuilder obj, string name)
    {
        ref var objRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( objRef, name ) );

        objRef = obj;
        _map.Remove( obj.Name );
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
        Schema.Database.Changes.ObjectCreated( result, result );
        return result;
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : SqliteObjectBuilder
    {
        var obj = _map[name];
        return obj.Type == type
            ? ReinterpretCast.To<T>( obj )
            : throw new SqlObjectCastException( SqliteDialect.Instance, typeof( T ), obj.GetType() );
    }

    [Pure]
    private T? TryGetTypedObject<T>(string name, SqlObjectType type)
        where T : SqliteObjectBuilder
    {
        return _map.TryGetValue( name, out var obj ) && obj.Type == type ? ReinterpretCast.To<T>( obj ) : null;
    }

    [Pure]
    ISqlObjectBuilder ISqlObjectBuilderCollection.Get(string name)
    {
        return GetObject( name );
    }

    [Pure]
    ISqlObjectBuilder? ISqlObjectBuilderCollection.TryGet(string name)
    {
        return TryGetObject( name );
    }

    [Pure]
    ISqlTableBuilder ISqlObjectBuilderCollection.GetTable(string name)
    {
        return GetTable( name );
    }

    [Pure]
    ISqlTableBuilder? ISqlObjectBuilderCollection.TryGetTable(string name)
    {
        return TryGetTable( name );
    }

    [Pure]
    ISqlIndexBuilder ISqlObjectBuilderCollection.GetIndex(string name)
    {
        return GetIndex( name );
    }

    [Pure]
    ISqlIndexBuilder? ISqlObjectBuilderCollection.TryGetIndex(string name)
    {
        return TryGetIndex( name );
    }

    [Pure]
    ISqlPrimaryKeyBuilder ISqlObjectBuilderCollection.GetPrimaryKey(string name)
    {
        return GetPrimaryKey( name );
    }

    [Pure]
    ISqlPrimaryKeyBuilder? ISqlObjectBuilderCollection.TryGetPrimaryKey(string name)
    {
        return TryGetPrimaryKey( name );
    }

    [Pure]
    ISqlForeignKeyBuilder ISqlObjectBuilderCollection.GetForeignKey(string name)
    {
        return GetForeignKey( name );
    }

    [Pure]
    ISqlForeignKeyBuilder? ISqlObjectBuilderCollection.TryGetForeignKey(string name)
    {
        return TryGetForeignKey( name );
    }

    [Pure]
    ISqlCheckBuilder ISqlObjectBuilderCollection.GetCheck(string name)
    {
        return GetCheck( name );
    }

    [Pure]
    ISqlCheckBuilder? ISqlObjectBuilderCollection.TryGetCheck(string name)
    {
        return TryGetCheck( name );
    }

    [Pure]
    ISqlViewBuilder ISqlObjectBuilderCollection.GetView(string name)
    {
        return GetView( name );
    }

    [Pure]
    ISqlViewBuilder? ISqlObjectBuilderCollection.TryGetView(string name)
    {
        return TryGetView( name );
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
