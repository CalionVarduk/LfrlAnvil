using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlViewBuilder : MySqlObjectBuilder, ISqlViewBuilder
{
    private Dictionary<ulong, MySqlViewBuilder>? _referencingViews;
    private readonly Dictionary<ulong, MySqlObjectBuilder> _referencedObjects;
    private SqlRecordSetInfo? _info;
    private SqlViewBuilderNode? _recordSet;

    internal MySqlViewBuilder(
        MySqlSchemaBuilder schema,
        string name,
        SqlQueryExpressionNode source,
        MySqlDatabaseScopeExpressionValidator visitor)
        : base( schema.Database.GetNextId(), name, SqlObjectType.View )
    {
        Schema = schema;
        Source = source;
        _referencingViews = null;
        _referencedObjects = visitor.ReferencedObjects;
        _info = null;
        AddSelfToReferencedObjects();
        _recordSet = null;
    }

    public MySqlSchemaBuilder Schema { get; }
    public SqlQueryExpressionNode Source { get; }
    public IReadOnlyCollection<MySqlObjectBuilder> ReferencedObjects => _referencedObjects.Values;
    public IReadOnlyCollection<MySqlViewBuilder> ReferencingViews => (_referencingViews?.Values).EmptyIfNull();
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlViewBuilderNode Node => _recordSet ??= SqlNode.View( this );
    public override MySqlDatabaseBuilder Database => Schema.Database;

    public override bool CanRemove => _referencingViews is null || _referencingViews.Count == 0;

    ISqlSchemaBuilder ISqlViewBuilder.Schema => Schema;
    IReadOnlyCollection<ISqlObjectBuilder> ISqlViewBuilder.ReferencedObjects => ReferencedObjects;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {MySqlHelpers.GetFullName( Schema.Name, Name )}";
    }

    public MySqlViewBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    protected override void AssertRemoval()
    {
        var errors = Chain<string>.Empty;

        if ( _referencingViews is not null && _referencingViews.Count > 0 )
        {
            foreach ( var view in _referencingViews.Values )
                errors = errors.Extend( ExceptionResources.ViewIsReferencedByObject( view ) );
        }

        if ( errors.Count > 0 )
            throw new MySqlObjectBuilderException( errors );
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true );

        RemoveSelfFromReferencedObjects();
        _referencedObjects.Clear();
        _referencingViews = null;

        Schema.Objects.ForceRemove( this );
        Schema.Database.Changes.ObjectRemoved( this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        MySqlHelpers.AssertName( name );
        var obj = Schema.Objects.TryGet( name );
        if ( obj is not null )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        using var buffer = RemoveReferencingViewsIntoBuffer( Database, _referencingViews );

        Schema.Objects.ChangeName( this, name );
        var oldName = Name;
        Name = name;
        ResetInfoCache();
        Database.Changes.NameUpdated( this, oldName );

        foreach ( var view in buffer )
            ReinterpretCast.To<MySqlViewBuilder>( view ).Reactivate();
    }

    internal void AddReferencingView(MySqlViewBuilder view)
    {
        _referencingViews ??= new Dictionary<ulong, MySqlViewBuilder>();
        _referencingViews.Add( view.Id, view );
    }

    internal void RemoveReferencingView(MySqlViewBuilder view)
    {
        _referencingViews?.Remove( view.Id );
    }

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

    internal void MarkAsRemoved()
    {
        Assume.Equals( IsRemoved, false );
        IsRemoved = true;
        RemoveSelfFromReferencedObjects();
        _referencedObjects.Clear();
        _referencingViews = null;
    }

    internal void Reactivate()
    {
        Assume.Equals( IsRemoved, true );
        IsRemoved = false;
        AddSelfToReferencedObjects();
        Schema.Objects.Reactivate( this );
        Schema.Database.Changes.ObjectCreated( this );
    }

    internal static RentedMemorySequence<MySqlObjectBuilder> RemoveReferencingViewsIntoBuffer(
        MySqlDatabaseBuilder database,
        Dictionary<ulong, MySqlViewBuilder>? views)
    {
        if ( views is null || views.Count == 0 )
            return RentedMemorySequence<MySqlObjectBuilder>.Empty;

        var reachedViews = new HashSet<ulong>();
        var buffer = database.ObjectPool.Rent( views.Count );

        var index = views.Count - 1;
        foreach ( var view in views.Values )
            index = AddToBufferInReconstructOrder( views, view, buffer, reachedViews, index );

        Assume.Equals( index, -1 );

        for ( var i = buffer.Length - 1; i >= 0; --i )
            ReinterpretCast.To<MySqlViewBuilder>( buffer[i] ).RemovePartially();

        return buffer;

        static int AddToBufferInReconstructOrder(
            Dictionary<ulong, MySqlViewBuilder> views,
            MySqlViewBuilder view,
            RentedMemorySequence<MySqlObjectBuilder> buffer,
            HashSet<ulong> reachedViews,
            int index)
        {
            if ( ! reachedViews.Add( view.Id ) )
                return index;

            if ( view._referencingViews is not null && view._referencingViews.Count > 0 )
            {
                foreach ( var v in view._referencingViews.Values )
                {
                    if ( views.ContainsKey( v.Id ) )
                        index = AddToBufferInReconstructOrder( views, v, buffer, reachedViews, index );
                }
            }

            buffer[index--] = view;
            return index;
        }
    }

    [Pure]
    internal static MySqlDatabaseScopeExpressionValidator AssertSourceNode(MySqlDatabaseBuilder database, SqlQueryExpressionNode source)
    {
        var visitor = new MySqlDatabaseScopeExpressionValidator( database );
        visitor.Visit( source );

        var errors = visitor.GetErrors();
        if ( errors.Count > 0 )
            throw new MySqlObjectBuilderException( errors );

        return visitor;
    }

    private void RemovePartially()
    {
        Assume.Equals( IsRemoved, false );
        IsRemoved = true;
        RemoveSelfFromReferencedObjects();
        Schema.Objects.ForceRemove( this );
        Schema.Database.Changes.ObjectRemoved( this );
    }

    private void AddSelfToReferencedObjects()
    {
        foreach ( var obj in _referencedObjects.Values )
        {
            switch ( obj.Type )
            {
                case SqlObjectType.Column:
                    ReinterpretCast.To<MySqlColumnBuilder>( obj ).AddReferencingView( this );
                    break;

                case SqlObjectType.Table:
                    ReinterpretCast.To<MySqlTableBuilder>( obj ).AddReferencingView( this );
                    break;

                case SqlObjectType.View:
                    ReinterpretCast.To<MySqlViewBuilder>( obj ).AddReferencingView( this );
                    break;

                default:
                    Assume.Unreachable();
                    break;
            }
        }
    }

    private void RemoveSelfFromReferencedObjects()
    {
        foreach ( var obj in _referencedObjects.Values )
        {
            switch ( obj.Type )
            {
                case SqlObjectType.Column:
                    ReinterpretCast.To<MySqlColumnBuilder>( obj ).RemoveReferencingView( this );
                    break;

                case SqlObjectType.Table:
                    ReinterpretCast.To<MySqlTableBuilder>( obj ).RemoveReferencingView( this );
                    break;

                case SqlObjectType.View:
                    ReinterpretCast.To<MySqlViewBuilder>( obj ).RemoveReferencingView( this );
                    break;

                default:
                    Assume.Unreachable();
                    break;
            }
        }
    }

    ISqlViewBuilder ISqlViewBuilder.SetName(string name)
    {
        return SetName( name );
    }
}
