using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteViewBuilder : SqliteObjectBuilder, ISqlViewBuilder
{
    private Dictionary<ulong, SqliteViewBuilder>? _referencingViews;
    private readonly Dictionary<ulong, SqliteObjectBuilder> _referencedObjects;
    private string _fullName;

    internal SqliteViewBuilder(SqliteSchemaBuilder schema, string name, SqlQueryExpressionNode source, SqliteViewSourceValidator visitor)
        : base( schema.Database.GetNextId(), name, SqlObjectType.View )
    {
        Schema = schema;
        Source = source;
        _referencingViews = null;
        _referencedObjects = visitor.ReferencedObjects;
        _fullName = string.Empty;
        UpdateFullName();
        AddSelfToReferencedObjects();
    }

    public SqliteSchemaBuilder Schema { get; }
    public SqlQueryExpressionNode Source { get; }
    public IReadOnlyCollection<SqliteObjectBuilder> ReferencedObjects => _referencedObjects.Values;
    public IReadOnlyCollection<SqliteViewBuilder> ReferencingViews => (_referencingViews?.Values).EmptyIfNull();
    public override string FullName => _fullName;
    public override SqliteDatabaseBuilder Database => Schema.Database;

    internal override bool CanRemove => _referencingViews is null || _referencingViews.Count == 0;

    ISqlSchemaBuilder ISqlViewBuilder.Schema => Schema;
    IReadOnlyCollection<ISqlObjectBuilder> ISqlViewBuilder.ReferencedObjects => ReferencedObjects;
    IReadOnlyCollection<ISqlViewBuilder> ISqlViewBuilder.ReferencingViews => ReferencingViews;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public SqliteViewBuilder SetName(string name)
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
            throw new SqliteObjectBuilderException( errors );
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true, nameof( CanRemove ) );
        ForceRemove();
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        SqliteHelpers.AssertName( name );
        if ( Schema.Objects.TryGet( name, out var obj ) )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        using var buffer = RemoveReferencingViewsIntoBuffer( Database, _referencingViews );

        Schema.Objects.ChangeName( this, name );
        var oldName = FullName;
        Name = name;
        UpdateFullName();
        Database.ChangeTracker.FullNameUpdated( this, oldName );

        foreach ( var view in buffer )
            ReinterpretCast.To<SqliteViewBuilder>( view ).Reactivate();
    }

    internal void AddReferencingView(SqliteViewBuilder view)
    {
        _referencingViews ??= new Dictionary<ulong, SqliteViewBuilder>();
        _referencingViews.Add( view.Id, view );
    }

    internal void RemoveReferencingView(SqliteViewBuilder view)
    {
        _referencingViews?.Remove( view.Id );
    }

    internal void UpdateFullName()
    {
        _fullName = SqliteHelpers.GetFullName( Schema.Name, Name );
    }

    internal void ForceRemove()
    {
        IsRemoved = true;

        RemoveSelfFromReferencedObjects();
        _referencedObjects.Clear();
        _referencingViews = null;

        Schema.Objects.ForceRemove( this );
        Schema.Database.ChangeTracker.ObjectRemoved( this );
    }

    internal void Reactivate()
    {
        Assume.Equals( IsRemoved, true, nameof( IsRemoved ) );
        IsRemoved = false;
        AddSelfToReferencedObjects();
        Schema.Objects.Reactivate( this );
        Schema.Database.ChangeTracker.ObjectCreated( this );
    }

    internal void OnSchemaNameChange()
    {
        using var buffer = RemoveReferencingViewsIntoBuffer( Database, _referencingViews );

        var oldName = FullName;
        UpdateFullName();
        Database.ChangeTracker.FullNameUpdated( this, oldName );

        foreach ( var view in buffer )
            ReinterpretCast.To<SqliteViewBuilder>( view ).Reactivate();
    }

    internal static RentedMemorySequence<SqliteObjectBuilder> RemoveReferencingViewsIntoBuffer(
        SqliteDatabaseBuilder database,
        Dictionary<ulong, SqliteViewBuilder>? views)
    {
        if ( views is null || views.Count == 0 )
            return RentedMemorySequence<SqliteObjectBuilder>.Empty;

        var reachedViews = new HashSet<ulong>();
        var buffer = database.ObjectPool.Rent( views.Count );

        var index = views.Count - 1;
        foreach ( var view in views.Values )
            index = AddToBufferInReconstructOrder( views, view, buffer, reachedViews, index );

        Assume.Equals( index, -1, nameof( index ) );

        for ( var i = buffer.Length - 1; i >= 0; --i )
            ReinterpretCast.To<SqliteViewBuilder>( buffer[i] ).RemovePartially();

        return buffer;

        static int AddToBufferInReconstructOrder(
            Dictionary<ulong, SqliteViewBuilder> views,
            SqliteViewBuilder view,
            RentedMemorySequence<SqliteObjectBuilder> buffer,
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
    internal static SqliteViewSourceValidator AssertSourceNode(SqliteDatabaseBuilder database, SqlQueryExpressionNode source)
    {
        var visitor = new SqliteViewSourceValidator( database );
        visitor.Visit( source );

        var errors = visitor.GetErrors();
        if ( errors.Count > 0 )
            throw new SqliteObjectBuilderException( errors );

        return visitor;
    }

    private void RemovePartially()
    {
        Assume.Equals( IsRemoved, false, nameof( IsRemoved ) );
        IsRemoved = true;
        RemoveSelfFromReferencedObjects();
        Schema.Objects.ForceRemove( this );
        Schema.Database.ChangeTracker.ObjectRemoved( this );
    }

    private void AddSelfToReferencedObjects()
    {
        foreach ( var obj in _referencedObjects.Values )
        {
            switch ( obj.Type )
            {
                case SqlObjectType.Column:
                    ReinterpretCast.To<SqliteColumnBuilder>( obj ).AddReferencingView( this );
                    break;

                case SqlObjectType.Table:
                    ReinterpretCast.To<SqliteTableBuilder>( obj ).AddReferencingView( this );
                    break;

                case SqlObjectType.View:
                    ReinterpretCast.To<SqliteViewBuilder>( obj ).AddReferencingView( this );
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
                    ReinterpretCast.To<SqliteColumnBuilder>( obj ).RemoveReferencingView( this );
                    break;

                case SqlObjectType.Table:
                    ReinterpretCast.To<SqliteTableBuilder>( obj ).RemoveReferencingView( this );
                    break;

                case SqlObjectType.View:
                    ReinterpretCast.To<SqliteViewBuilder>( obj ).RemoveReferencingView( this );
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
