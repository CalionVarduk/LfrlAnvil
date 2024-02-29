﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlTableBuilder : MySqlObjectBuilder, ISqlTableBuilder
{
    private Dictionary<ulong, MySqlViewBuilder>? _referencingViews;
    private SqlRecordSetInfo? _info;
    private SqlTableBuilderNode? _recordSet;

    internal MySqlTableBuilder(MySqlSchemaBuilder schema, string name)
        : base( schema.Database.GetNextId(), name, SqlObjectType.Table )
    {
        _referencingViews = null;
        Schema = schema;
        Columns = new MySqlColumnBuilderCollection( this );
        Constraints = new MySqlConstraintBuilderCollection( this );
        _info = null;
        _recordSet = null;
    }

    public MySqlSchemaBuilder Schema { get; }
    public MySqlColumnBuilderCollection Columns { get; }
    public MySqlConstraintBuilderCollection Constraints { get; }
    public IReadOnlyCollection<MySqlViewBuilder> ReferencingViews => (_referencingViews?.Values).EmptyIfNull();
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlTableBuilderNode Node => _recordSet ??= SqlNode.Table( this );
    public override MySqlDatabaseBuilder Database => Schema.Database;

    public override bool CanRemove
    {
        get
        {
            if ( _referencingViews is not null && _referencingViews.Count > 0 )
                return false;

            foreach ( var constraint in Constraints )
            {
                if ( constraint.Type != SqlObjectType.Index )
                    continue;

                var ix = ReinterpretCast.To<MySqlIndexBuilder>( constraint );
                if ( ! ix.CanRemove )
                    return false;
            }

            return true;
        }
    }

    ISqlSchemaBuilder ISqlTableBuilder.Schema => Schema;
    ISqlColumnBuilderCollection ISqlTableBuilder.Columns => Columns;
    ISqlConstraintBuilderCollection ISqlTableBuilder.Constraints => Constraints;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {MySqlHelpers.GetFullName( Schema.Name, Name )}";
    }

    public MySqlTableBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    internal void MarkAsRemoved()
    {
        Assume.False( IsRemoved );
        IsRemoved = true;
        _referencingViews = null;
        Columns.MarkAllAsRemoved();
        Constraints.MarkAllAsRemoved();
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

            var ix = ReinterpretCast.To<MySqlIndexBuilder>( constraint );
            foreach ( var fk in ix.ReferencingForeignKeys )
            {
                if ( ! fk.IsSelfReference() )
                    errors = errors.Extend( ExceptionResources.DetectedExternalForeignKey( fk ) );
            }
        }

        if ( errors.Count > 0 )
            throw new MySqlObjectBuilderException( errors );
    }

    protected override void RemoveCore()
    {
        Assume.True( CanRemove );

        _referencingViews = null;

        using var columns = Database.ObjectPool.Rent( Columns.Count );
        Columns.ClearInto( columns );

        using var constraints = Constraints.Clear();
        foreach ( var constraint in constraints )
            constraint.Remove();

        foreach ( var column in columns )
            column.Remove();

        Schema.Objects.Remove( Name );
        Database.Changes.ObjectRemoved( this, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        MySqlHelpers.AssertName( name );
        var obj = Schema.Objects.TryGet( name );
        if ( obj is not null )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        using var viewBuffer = MySqlViewBuilder.RemoveReferencingViewsIntoBuffer( Database, _referencingViews );

        Schema.Objects.ChangeName( this, name );
        var oldName = Name;
        Name = name;
        ResetInfoCache();
        Database.Changes.NameUpdated( this, this, oldName );

        foreach ( var view in viewBuffer )
            ReinterpretCast.To<MySqlViewBuilder>( view ).Reactivate();
    }

    internal void OnSchemaNameChange(string oldName)
    {
        ResetInfoCache();
        Database.Changes.SchemaNameUpdated( this, this, oldName );
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

    ISqlTableBuilder ISqlTableBuilder.SetName(string name)
    {
        return SetName( name );
    }
}