using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlViewBuilder" />
public abstract class SqlViewBuilder : SqlObjectBuilder, ISqlViewBuilder
{
    private ReadOnlyArray<SqlObjectBuilder> _referencedObjects;
    private SqlRecordSetInfo? _info;
    private SqlViewBuilderNode? _node;

    /// <summary>
    /// Creates a new <see cref="SqlViewBuilder"/> instance.
    /// </summary>
    /// <param name="schema">Schema that this view belongs to.</param>
    /// <param name="name">Object's name.</param>
    /// <param name="source">Underlying source query expression that defines this view.</param>
    /// <param name="referencedObjects">
    /// Collection of objects (tables, views and columns) referenced by this view's <see cref="Source"/>.
    /// </param>
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

    /// <inheritdoc cref="ISqlViewBuilder.Schema" />
    public SqlSchemaBuilder Schema { get; }

    /// <inheritdoc />
    public SqlQueryExpressionNode Source { get; }

    /// <inheritdoc cref="ISqlViewBuilder.ReferencedObjects" />
    public SqlObjectBuilderArray<SqlObjectBuilder> ReferencedObjects => SqlObjectBuilderArray<SqlObjectBuilder>.From( _referencedObjects );

    /// <inheritdoc />
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );

    /// <inheritdoc />
    public SqlViewBuilderNode Node => _node ??= SqlNode.View( this );

    ISqlSchemaBuilder ISqlViewBuilder.Schema => Schema;
    IReadOnlyCollection<ISqlObjectBuilder> ISqlViewBuilder.ReferencedObjects => _referencedObjects.GetUnderlyingArray();

    /// <summary>
    /// Returns a string representation of this <see cref="SqlViewBuilder"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Schema.Name, Name )}";
    }

    /// <inheritdoc cref="SqlObjectBuilder.SetName(string)" />
    public new SqlViewBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <summary>
    /// Adds a collection of <paramref name="objects"/> to <see cref="ReferencedObjects"/> and adds this view to their reference sources.
    /// </summary>
    /// <param name="objects">Collection of objects to add.</param>
    protected void SetReferencedObjects(ReadOnlyArray<SqlObjectBuilder> objects)
    {
        _referencedObjects = objects;
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var obj in _referencedObjects )
            AddReference( obj, refSource );
    }

    /// <summary>
    /// Removes all objects from <see cref="ReferencedObjects"/> and removes this view from their reference sources.
    /// </summary>
    protected void ClearReferencedObjects()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var obj in _referencedObjects )
            RemoveReference( obj, refSource );

        _referencedObjects = ReadOnlyArray<SqlObjectBuilder>.Empty;
    }

    /// <inheritdoc />
    protected override SqlPropertyChange<string> BeforeNameChange(string newValue)
    {
        var change = base.BeforeNameChange( newValue );
        if ( change.IsCancelled )
            return change;

        ChangeNameInCollection( Schema.Objects, this, newValue );
        return change;
    }

    /// <inheritdoc />
    protected override void AfterNameChange(string originalValue)
    {
        ResetInfoCache();
        AddNameChange( this, this, originalValue );
    }

    /// <inheritdoc />
    protected override void BeforeRemove()
    {
        base.BeforeRemove();
        RemoveFromCollection( Schema.Objects, this );
        ClearReferencedObjects();
    }

    /// <inheritdoc />
    protected override void AfterRemove()
    {
        AddRemoval( this, this );
    }

    /// <inheritdoc />
    protected override void QuickRemoveCore()
    {
        base.QuickRemoveCore();
        _referencedObjects = ReadOnlyArray<SqlObjectBuilder>.Empty;
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
