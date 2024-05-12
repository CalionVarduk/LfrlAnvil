using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc />
public abstract class SqlObjectCollection : ISqlObjectCollection
{
    private readonly Dictionary<string, SqlObject> _map;
    private SqlSchema? _schema;

    /// <summary>
    /// Creates a new <see cref="SqlObjectCollection"/> instance.
    /// </summary>
    /// <param name="source">Source collection.</param>
    protected SqlObjectCollection(SqlObjectBuilderCollection source)
    {
        _map = new Dictionary<string, SqlObject>( capacity: source.Count, comparer: SqlHelpers.NameComparer );
        _schema = null;
    }

    /// <inheritdoc />
    public int Count => _map.Count;

    /// <inheritdoc cref="ISqlObjectCollection.Schema" />
    public SqlSchema Schema
    {
        get
        {
            Assume.IsNotNull( _schema );
            return _schema;
        }
    }

    ISqlSchema ISqlObjectCollection.Schema => Schema;

    /// <inheritdoc />
    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    /// <inheritdoc cref="ISqlObjectCollection.Get(string)" />
    [Pure]
    public SqlObject Get(string name)
    {
        return _map[name];
    }

    /// <inheritdoc cref="ISqlObjectCollection.TryGet(string)" />
    [Pure]
    public SqlObject? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
    }

    /// <inheritdoc cref="ISqlObjectCollection.GetTable(string)" />
    [Pure]
    public SqlTable GetTable(string name)
    {
        return GetTypedObject<SqlTable>( name, SqlObjectType.Table );
    }

    /// <inheritdoc cref="ISqlObjectCollection.TryGetTable(string)" />
    [Pure]
    public SqlTable? TryGetTable(string name)
    {
        return TryGetTypedObject<SqlTable>( name, SqlObjectType.Table );
    }

    /// <inheritdoc cref="ISqlObjectCollection.GetIndex(string)" />
    [Pure]
    public SqlIndex GetIndex(string name)
    {
        return GetTypedObject<SqlIndex>( name, SqlObjectType.Index );
    }

    /// <inheritdoc cref="ISqlObjectCollection.TryGetIndex(string)" />
    [Pure]
    public SqlIndex? TryGetIndex(string name)
    {
        return TryGetTypedObject<SqlIndex>( name, SqlObjectType.Index );
    }

    /// <inheritdoc cref="ISqlObjectCollection.GetPrimaryKey(string)" />
    [Pure]
    public SqlPrimaryKey GetPrimaryKey(string name)
    {
        return GetTypedObject<SqlPrimaryKey>( name, SqlObjectType.PrimaryKey );
    }

    /// <inheritdoc cref="ISqlObjectCollection.TryGetPrimaryKey(string)" />
    [Pure]
    public SqlPrimaryKey? TryGetPrimaryKey(string name)
    {
        return TryGetTypedObject<SqlPrimaryKey>( name, SqlObjectType.PrimaryKey );
    }

    /// <inheritdoc cref="ISqlObjectCollection.GetForeignKey(string)" />
    [Pure]
    public SqlForeignKey GetForeignKey(string name)
    {
        return GetTypedObject<SqlForeignKey>( name, SqlObjectType.ForeignKey );
    }

    /// <inheritdoc cref="ISqlObjectCollection.TryGetForeignKey(string)" />
    [Pure]
    public SqlForeignKey? TryGetForeignKey(string name)
    {
        return TryGetTypedObject<SqlForeignKey>( name, SqlObjectType.ForeignKey );
    }

    /// <inheritdoc cref="ISqlObjectCollection.GetCheck(string)" />
    [Pure]
    public SqlCheck GetCheck(string name)
    {
        return GetTypedObject<SqlCheck>( name, SqlObjectType.Check );
    }

    /// <inheritdoc cref="ISqlObjectCollection.TryGetCheck(string)" />
    [Pure]
    public SqlCheck? TryGetCheck(string name)
    {
        return TryGetTypedObject<SqlCheck>( name, SqlObjectType.Check );
    }

    /// <inheritdoc cref="ISqlObjectCollection.GetView(string)" />
    [Pure]
    public SqlView GetView(string name)
    {
        return GetTypedObject<SqlView>( name, SqlObjectType.View );
    }

    /// <inheritdoc cref="ISqlObjectCollection.TryGetView(string)" />
    [Pure]
    public SqlView? TryGetView(string name)
    {
        return TryGetTypedObject<SqlView>( name, SqlObjectType.View );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectEnumerator{T}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectEnumerator{T}"/> instance.</returns>
    [Pure]
    public SqlObjectEnumerator<SqlObject> GetEnumerator()
    {
        return new SqlObjectEnumerator<SqlObject>( _map );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTable"/> instance.
    /// </summary>
    /// <param name="builder">Source table builder.</param>
    /// <returns>New <see cref="SqlTable"/> instance.</returns>
    [Pure]
    protected abstract SqlTable CreateTable(SqlTableBuilder builder);

    /// <summary>
    /// Creates a new <see cref="SqlView"/> instance.
    /// </summary>
    /// <param name="builder">Source view builder.</param>
    /// <returns>New <see cref="SqlView"/> instance.</returns>
    [Pure]
    protected abstract SqlView CreateView(SqlViewBuilder builder);

    /// <summary>
    /// Creates a new <see cref="SqlIndex"/> instance.
    /// </summary>
    /// <param name="table">Table that this index belongs to.</param>
    /// <param name="builder">Source index builder.</param>
    /// <returns>New <see cref="SqlIndex"/> instance.</returns>
    [Pure]
    protected abstract SqlIndex CreateIndex(SqlTable table, SqlIndexBuilder builder);

    /// <summary>
    /// Creates a new <see cref="SqlPrimaryKey"/> instance.
    /// </summary>
    /// <param name="index">Underlying index that defines this primary key.</param>
    /// <param name="builder">Source primary key builder.</param>
    /// <returns>New <see cref="SqlPrimaryKey"/> instance.</returns>
    [Pure]
    protected abstract SqlPrimaryKey CreatePrimaryKey(SqlIndex index, SqlPrimaryKeyBuilder builder);

    /// <summary>
    /// Creates a new <see cref="SqlCheck"/> instance.
    /// </summary>
    /// <param name="table">Table that this check constraint is attached to.</param>
    /// <param name="builder">Source check builder.</param>
    /// <returns>New <see cref="SqlCheck"/> instance.</returns>
    [Pure]
    protected abstract SqlCheck CreateCheck(SqlTable table, SqlCheckBuilder builder);

    /// <summary>
    /// Creates a new <see cref="SqlForeignKey"/> instance.
    /// </summary>
    /// <param name="originIndex">SQL index that this foreign key originates from.</param>
    /// <param name="referencedIndex">SQL index referenced by this foreign key.</param>
    /// <param name="builder">Source foreign key builder.</param>
    /// <returns>New <see cref="SqlForeignKey"/> instance.</returns>
    [Pure]
    protected abstract SqlForeignKey CreateForeignKey(SqlIndex originIndex, SqlIndex referencedIndex, SqlForeignKeyBuilder builder);

    /// <summary>
    /// Creates a new <see cref="SqlObjectType.Unknown"/> <see cref="SqlObject"/> instance.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns>New <see cref="SqlObject"/> instance.</returns>
    /// <exception cref="NotSupportedException">This method is not supported by default.</exception>
    [Pure]
    protected virtual SqlObject CreateUnknown(SqlObjectBuilder builder)
    {
        throw new NotSupportedException( ExceptionResources.UnknownObjectsAreUnsupported );
    }

    /// <summary>
    /// Specifies whether or not an <see cref="SqlObjectType.Unknown"/> object's creation should be deferred
    /// until all tables and views are created.
    /// </summary>
    /// <param name="builder">SQL object builder.</param>
    /// <returns>
    /// <b>true</b> when the <paramref name="builder"/> is an instance of <see cref="SqlConstraintBuilder"/> type, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    protected virtual bool DeferCreation(SqlObjectBuilder builder)
    {
        return builder is SqlConstraintBuilder;
    }

    internal void SetSchema(SqlSchema schema)
    {
        Assume.IsNull( _schema );
        Assume.Equals( schema.Objects, this );
        _schema = schema;
    }

    internal void AddImmediateObjects(SqlObjectBuilderCollection source, RentedMemorySequence<SqlObjectBuilder> deferredObjects)
    {
        foreach ( var b in source )
        {
            switch ( b.Type )
            {
                case SqlObjectType.Table:
                {
                    var t = ReinterpretCast.To<SqlTableBuilder>( b );
                    var table = CreateTable( t );
                    _map.Add( table.Name, table );

                    foreach ( var c in t.Constraints )
                    {
                        switch ( c.Type )
                        {
                            case SqlObjectType.Index:
                            {
                                var index = CreateIndex( table, ReinterpretCast.To<SqlIndexBuilder>( c ) );
                                table.Constraints.AddConstraint( index );
                                _map.Add( index.Name, index );
                                break;
                            }
                            case SqlObjectType.Check:
                            {
                                var check = CreateCheck( table, ReinterpretCast.To<SqlCheckBuilder>( c ) );
                                table.Constraints.AddConstraint( check );
                                _map.Add( check.Name, check );
                                break;
                            }
                            case SqlObjectType.ForeignKey:
                            {
                                deferredObjects.Push( c );
                                break;
                            }
                        }
                    }

                    var pk = t.Constraints.GetPrimaryKey();
                    var pkIndex = ReinterpretCast.To<SqlIndex>( _map[pk.Index.Name] );
                    var primaryKey = CreatePrimaryKey( pkIndex, pk );
                    table.Constraints.SetPrimaryKey( primaryKey );
                    _map.Add( primaryKey.Name, primaryKey );
                    break;
                }
                case SqlObjectType.View:
                {
                    var view = CreateView( ReinterpretCast.To<SqlViewBuilder>( b ) );
                    _map.Add( view.Name, view );
                    break;
                }
                case SqlObjectType.Unknown:
                {
                    if ( DeferCreation( b ) )
                        deferredObjects.Push( b );
                    else
                        AddUnknownObject( b );

                    break;
                }
            }
        }
    }

    internal void AddForeignKey(SqlObjectCollection referencedSchemaObjects, SqlForeignKeyBuilder builder)
    {
        var originIndex = ReinterpretCast.To<SqlIndex>( _map[builder.OriginIndex.Name] );
        var referencedIndex = ReinterpretCast.To<SqlIndex>( referencedSchemaObjects._map[builder.ReferencedIndex.Name] );
        var foreignKey = CreateForeignKey( originIndex, referencedIndex, builder );
        foreignKey.OriginIndex.Table.Constraints.AddConstraint( foreignKey );
        _map.Add( foreignKey.Name, foreignKey );
    }

    internal void AddUnknownObject(SqlObjectBuilder builder)
    {
        var obj = CreateUnknown( builder );
        if ( obj is SqlConstraint c )
            c.Table.Constraints.AddConstraint( c );

        _map.Add( obj.Name, obj );
    }

    [Pure]
    private T GetTypedObject<T>(string name, SqlObjectType type)
        where T : SqlObject
    {
        var obj = _map[name];
        return obj.Type == type
            ? ReinterpretCast.To<T>( obj )
            : throw SqlHelpers.CreateObjectCastException( Schema.Database, typeof( T ), obj.GetType() );
    }

    [Pure]
    private T? TryGetTypedObject<T>(string name, SqlObjectType type)
        where T : SqlObject
    {
        return _map.TryGetValue( name, out var obj ) && obj.Type == type ? ReinterpretCast.To<T>( obj ) : null;
    }

    [Pure]
    ISqlObject ISqlObjectCollection.Get(string name)
    {
        return Get( name );
    }

    [Pure]
    ISqlObject? ISqlObjectCollection.TryGet(string name)
    {
        return TryGet( name );
    }

    [Pure]
    ISqlTable ISqlObjectCollection.GetTable(string name)
    {
        return GetTable( name );
    }

    [Pure]
    ISqlTable? ISqlObjectCollection.TryGetTable(string name)
    {
        return TryGetTable( name );
    }

    [Pure]
    ISqlIndex ISqlObjectCollection.GetIndex(string name)
    {
        return GetIndex( name );
    }

    [Pure]
    ISqlIndex? ISqlObjectCollection.TryGetIndex(string name)
    {
        return TryGetIndex( name );
    }

    [Pure]
    ISqlPrimaryKey ISqlObjectCollection.GetPrimaryKey(string name)
    {
        return GetPrimaryKey( name );
    }

    [Pure]
    ISqlPrimaryKey? ISqlObjectCollection.TryGetPrimaryKey(string name)
    {
        return TryGetPrimaryKey( name );
    }

    [Pure]
    ISqlForeignKey ISqlObjectCollection.GetForeignKey(string name)
    {
        return GetForeignKey( name );
    }

    [Pure]
    ISqlForeignKey? ISqlObjectCollection.TryGetForeignKey(string name)
    {
        return TryGetForeignKey( name );
    }

    [Pure]
    ISqlCheck ISqlObjectCollection.GetCheck(string name)
    {
        return GetCheck( name );
    }

    [Pure]
    ISqlCheck? ISqlObjectCollection.TryGetCheck(string name)
    {
        return TryGetCheck( name );
    }

    [Pure]
    ISqlView ISqlObjectCollection.GetView(string name)
    {
        return GetView( name );
    }

    [Pure]
    ISqlView? ISqlObjectCollection.TryGetView(string name)
    {
        return TryGetView( name );
    }

    [Pure]
    IEnumerator<ISqlObject> IEnumerable<ISqlObject>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
