using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteTableBuilder : SqliteObjectBuilder, ISqlTableBuilder
{
    private Dictionary<ulong, SqliteViewBuilder>? _referencingViews;
    private SqlRecordSetInfo? _info;
    private SqlTableBuilderNode? _recordSet;

    internal SqliteTableBuilder(SqliteSchemaBuilder schema, string name)
        : base( schema.Database.GetNextId(), name, SqlObjectType.Table )
    {
        _referencingViews = null;
        Schema = schema;
        Columns = new SqliteColumnBuilderCollection( this );
        Constraints = new SqliteConstraintBuilderCollection( this );
        _info = null;
        ResetInfoCache();
        _recordSet = null;
    }

    public SqliteSchemaBuilder Schema { get; }
    public SqliteColumnBuilderCollection Columns { get; }
    public SqliteConstraintBuilderCollection Constraints { get; }
    public IReadOnlyCollection<SqliteViewBuilder> ReferencingViews => (_referencingViews?.Values).EmptyIfNull();

    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlTableBuilderNode RecordSet => _recordSet ??= SqlNode.Table( this );
    public override SqliteDatabaseBuilder Database => Schema.Database;

    internal override bool CanRemove
    {
        get
        {
            if ( _referencingViews is not null && _referencingViews.Count > 0 )
                return false;

            foreach ( var constraint in Constraints )
            {
                if ( constraint.Type != SqlObjectType.Index )
                    continue;

                var ix = ReinterpretCast.To<SqliteIndexBuilder>( constraint );
                if ( ! ix.CanRemove )
                    return false;
            }

            return true;
        }
    }

    ISqlSchemaBuilder ISqlTableBuilder.Schema => Schema;
    ISqlColumnBuilderCollection ISqlTableBuilder.Columns => Columns;
    ISqlConstraintBuilderCollection ISqlTableBuilder.Constraints => Constraints;
    IReadOnlyCollection<ISqlViewBuilder> ISqlTableBuilder.ReferencingViews => ReferencingViews;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Schema.Name, Name )}";
    }

    public SqliteTableBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
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

    internal void ForceRemove()
    {
        Assume.Equals( IsRemoved, false );
        IsRemoved = true;

        using ( var buffer = Database.ObjectPool.GreedyRent() )
        {
            foreach ( var constraint in Constraints )
            {
                if ( constraint.Type != SqlObjectType.Index )
                    continue;

                var ix = ReinterpretCast.To<SqliteIndexBuilder>( constraint );
                var count = ix.ReferencingForeignKeys.Count;
                buffer.Expand( count );
                ix.ClearReferencingForeignKeysInto( buffer.Slice( buffer.Length - count ) );
            }

            SqliteDatabaseBuilder.RemoveReferencingForeignKeys( this, buffer );
        }

        RemoveCore();
    }

    protected override void AssertRemoval()
    {
        var errors = Chain<string>.Empty;

        if ( _referencingViews is not null && _referencingViews.Count > 0 )
        {
            foreach ( var view in _referencingViews.Values )
                errors = errors.Extend( ExceptionResources.TableIsReferencedByObject( view ) );
        }

        foreach ( var constraint in Constraints )
        {
            if ( constraint.Type != SqlObjectType.Index )
                continue;

            var ix = ReinterpretCast.To<SqliteIndexBuilder>( constraint );
            foreach ( var fk in ix.ReferencingForeignKeys )
            {
                if ( ! fk.IsSelfReference() )
                    errors = errors.Extend( ExceptionResources.DetectedExternalForeignKey( fk ) );
            }
        }

        if ( errors.Count > 0 )
            throw new SqliteObjectBuilderException( errors );
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true );

        _referencingViews = null;

        using var columns = Database.ObjectPool.Rent( Columns.Count );
        Columns.ClearInto( columns );

        using var constraints = Constraints.Clear();
        foreach ( var constraint in constraints )
            constraint.Remove();

        foreach ( var column in columns )
            column.Remove();

        Schema.Objects.Remove( Name );
        Database.ChangeTracker.ObjectRemoved( this, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        SqliteHelpers.AssertName( name );
        var obj = Schema.Objects.TryGetObject( name );
        if ( obj is not null )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        Rename(
            name,
            static (t, n) =>
            {
                t.Schema.Objects.ChangeName( t, n );
                var oldName = t.Name;
                t.Name = n;
                t.ResetInfoCache();
                t.Database.ChangeTracker.NameUpdated( t, t, oldName );
            } );
    }

    internal void OnSchemaNameChange(string oldName)
    {
        Rename(
            Name,
            (t, _) =>
            {
                t.ResetInfoCache();
                t.Database.ChangeTracker.SchemaNameUpdated( t, t, oldName );

                foreach ( var constraint in Constraints )
                    t.Database.ChangeTracker.SchemaNameUpdated( t, constraint, oldName );
            } );
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

    private void Rename(string newName, Action<SqliteTableBuilder, string> update)
    {
        using var viewBuffer = SqliteViewBuilder.RemoveReferencingViewsIntoBuffer( Database, _referencingViews );

        using ( var fkBuffer = Database.ObjectPool.GreedyRent() )
        {
            var hasSelfRefForeignKeys = false;

            foreach ( var constraint in Constraints )
            {
                if ( constraint.Type != SqlObjectType.Index )
                    continue;

                var index = ReinterpretCast.To<SqliteIndexBuilder>( constraint );
                foreach ( var fk in index.ReferencingForeignKeys )
                {
                    if ( fk.IsSelfReference() )
                    {
                        hasSelfRefForeignKeys = true;
                        continue;
                    }

                    fkBuffer.Push( fk );
                }
            }

            SqliteDatabaseBuilder.RemoveReferencingForeignKeys( this, fkBuffer );

            update( this, newName );

            if ( hasSelfRefForeignKeys )
                Database.ChangeTracker.ReconstructionRequested( this );

            foreach ( var fk in fkBuffer )
                ReinterpretCast.To<SqliteForeignKeyBuilder>( fk ).Reactivate();
        }

        foreach ( var view in viewBuffer )
            ReinterpretCast.To<SqliteViewBuilder>( view ).Reactivate();
    }

    ISqlTableBuilder ISqlTableBuilder.SetName(string name)
    {
        return SetName( name );
    }
}
