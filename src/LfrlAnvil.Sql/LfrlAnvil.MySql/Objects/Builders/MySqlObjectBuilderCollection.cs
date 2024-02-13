using System;
using System.Collections;
using System.Collections.Generic;
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
    public MySqlObjectBuilder GetObject(string name)
    {
        return _map[name];
    }

    [Pure]
    public MySqlObjectBuilder? TryGetObject(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    public MySqlTableBuilder GetTable(string name)
    {
        return GetTypedObject<MySqlTableBuilder>( name, SqlObjectType.Table );
    }

    [Pure]
    public MySqlTableBuilder? TryGetTable(string name)
    {
        return TryGetTypedObject<MySqlTableBuilder>( name, SqlObjectType.Table );
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
    public MySqlPrimaryKeyBuilder GetPrimaryKey(string name)
    {
        return GetTypedObject<MySqlPrimaryKeyBuilder>( name, SqlObjectType.PrimaryKey );
    }

    [Pure]
    public MySqlPrimaryKeyBuilder? TryGetPrimaryKey(string name)
    {
        return TryGetTypedObject<MySqlPrimaryKeyBuilder>( name, SqlObjectType.PrimaryKey );
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

    [Pure]
    public MySqlViewBuilder GetView(string name)
    {
        return GetTypedObject<MySqlViewBuilder>( name, SqlObjectType.View );
    }

    [Pure]
    public MySqlViewBuilder? TryGetView(string name)
    {
        return TryGetTypedObject<MySqlViewBuilder>( name, SqlObjectType.View );
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

    internal void ChangeName(MySqlTableBuilder table, string name)
    {
        ChangeNameCore( table, name );
    }

    internal void ChangeName(MySqlViewBuilder view, string name)
    {
        ChangeNameCore( view, name );
    }

    internal void ChangeName(MySqlConstraintBuilder constraint, string name)
    {
        ChangeNameCore( constraint, name );
        constraint.Table.Constraints.ChangeName( constraint, name );
    }

    internal MySqlIndexBuilder CreateIndex(
        MySqlTableBuilder table,
        string name,
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns,
        bool isUnique)
    {
        MySqlHelpers.AssertName( name );
        var indexColumns = MySqlHelpers.CreateIndexColumns( table, columns );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = new MySqlIndexBuilder( table, indexColumns, name, isUnique );
        obj = result;
        Schema.Database.Changes.ObjectCreated( table, result );
        return result;
    }

    internal MySqlPrimaryKeyBuilder CreatePrimaryKey(
        MySqlTableBuilder table,
        string name,
        MySqlIndexBuilder index,
        MySqlPrimaryKeyBuilder? oldPrimaryKey)
    {
        MySqlHelpers.AssertName( name );
        MySqlHelpers.AssertPrimaryKey( table, index );

        if ( _map.TryGetValue( name, out var obj ) && ! CanReplaceWithPrimaryKey( obj, oldPrimaryKey ) )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        oldPrimaryKey?.Remove();

        var primaryKey = new MySqlPrimaryKeyBuilder( index, name );
        _map[name] = primaryKey;
        Schema.Database.Changes.ObjectCreated( table, primaryKey );
        index.AssignPrimaryKey( primaryKey );
        return primaryKey;
    }

    internal MySqlForeignKeyBuilder CreateForeignKey(
        MySqlTableBuilder table,
        string name,
        MySqlIndexBuilder originIndex,
        MySqlIndexBuilder referencedIndex)
    {
        MySqlHelpers.AssertName( name );
        MySqlHelpers.AssertForeignKey( table, originIndex, referencedIndex );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = new MySqlForeignKeyBuilder( originIndex, referencedIndex, name );
        obj = result;
        Schema.Database.Changes.ObjectCreated( originIndex.Table, result );
        return result;
    }

    internal MySqlCheckBuilder CreateCheck(MySqlTableBuilder table, string name, SqlConditionNode condition)
    {
        MySqlHelpers.AssertName( name );
        var visitor = MySqlCheckBuilder.AssertConditionNode( table, condition );

        ref var obj = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        var result = new MySqlCheckBuilder( name, condition, visitor );
        obj = result;
        Schema.Database.Changes.ObjectCreated( table, result );
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
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool CanReplaceWithPrimaryKey(MySqlObjectBuilder obj, MySqlPrimaryKeyBuilder? oldPrimaryKey)
    {
        if ( oldPrimaryKey is null )
            return false;

        if ( ReferenceEquals( obj, oldPrimaryKey ) )
            return true;

        return obj.Type == SqlObjectType.Index &&
            ReferenceEquals( ReinterpretCast.To<MySqlIndexBuilder>( obj ).PrimaryKey, oldPrimaryKey );
    }

    private void ChangeNameCore(MySqlObjectBuilder obj, string name)
    {
        ref var objRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists )!;
        if ( exists )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( objRef, name ) );

        objRef = obj;
        _map.Remove( obj.Name );
    }

    [Pure]
    private MySqlTableBuilder CreateNewTable(string name)
    {
        var result = new MySqlTableBuilder( Schema, name );
        Schema.Database.Changes.ObjectCreated( result, result );
        return result;
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : MySqlObjectBuilder
    {
        var obj = _map[name];
        return obj.Type == type
            ? ReinterpretCast.To<T>( obj )
            : throw new SqlObjectCastException( MySqlDialect.Instance, typeof( T ), obj.GetType() );
    }

    [Pure]
    private T? TryGetTypedObject<T>(string name, SqlObjectType type)
        where T : MySqlObjectBuilder
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
