using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlViewBuilder : SqlObjectBuilder, ISqlViewBuilder
{
    private ReadOnlyArray<SqlObjectBuilder> _referencedObjects;
    private SqlRecordSetInfo? _info;
    private SqlViewBuilderNode? _node;

    protected SqlViewBuilder(
        SqlSchemaBuilder schema,
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects)
        : base( schema.Database, SqlObjectType.View, name )
    {
        _referencedObjects = ReadOnlyArray<SqlObjectBuilder>.Empty;
        Schema = schema;
        Source = source;
        _info = null;
        _node = null;
        SetReferencedObjects( referencedObjects );
    }

    public SqlSchemaBuilder Schema { get; }
    public SqlQueryExpressionNode Source { get; }
    public SqlObjectBuilderArray<SqlObjectBuilder> ReferencedObjects => SqlObjectBuilderArray<SqlObjectBuilder>.From( _referencedObjects );
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlViewBuilderNode Node => _node ??= SqlNode.View( this );

    ISqlSchemaBuilder ISqlViewBuilder.Schema => Schema;
    IReadOnlyCollection<ISqlObjectBuilder> ISqlViewBuilder.ReferencedObjects => _referencedObjects.GetUnderlyingArray();

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Schema.Name, Name )}";
    }

    public new SqlViewBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    protected void SetReferencedObjects(ReadOnlyArray<SqlObjectBuilder> objects)
    {
        _referencedObjects = objects;
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var obj in _referencedObjects )
            AddReference( obj, refSource );
    }

    protected void ClearReferencedObjects()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var obj in _referencedObjects )
            RemoveReference( obj, refSource );

        _referencedObjects = ReadOnlyArray<SqlObjectBuilder>.Empty;
    }

    protected override SqlPropertyChange<string> BeforeNameChange(string newValue)
    {
        var change = base.BeforeNameChange( newValue );
        if ( change.IsCancelled )
            return change;

        ChangeNameInCollection( Schema.Objects, this, newValue );
        return change;
    }

    protected override void AfterNameChange(string originalValue)
    {
        ResetInfoCache();
        AddNameChange( this, this, originalValue );
    }

    protected override void BeforeRemove()
    {
        base.BeforeRemove();
        RemoveFromCollection( Schema.Objects, this );
        ClearReferencedObjects();
    }

    protected override void AfterRemove()
    {
        AddRemoval( this, this );
    }

    protected override void QuickRemoveCore()
    {
        base.QuickRemoveCore();
        _referencedObjects = ReadOnlyArray<SqlObjectBuilder>.Empty;
    }

    [Pure]
    internal static SqlSchemaScopeExpressionValidator AssertSourceNode(SqlSchemaBuilder schema, SqlQueryExpressionNode source)
    {
        // TODO:
        // move to configurable db builder interface (low priority, later)
        var visitor = new SqlSchemaScopeExpressionValidator( schema );
        visitor.Visit( source );

        var errors = visitor.GetErrors();
        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( schema.Database, errors );

        return visitor;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ResetInfoCache()
    {
        _info = null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal SqlRecordSetInfo? GetCachedInfo()
    {
        return _info;
    }

    ISqlViewBuilder ISqlViewBuilder.SetName(string name)
    {
        return SetName( name );
    }
}
