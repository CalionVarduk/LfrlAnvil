using System.Collections.Generic;
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
    private string? _fullName;
    private SqlRecordSetInfo? _info;
    private SqlTableBuilderNode? _recordSet;

    internal MySqlTableBuilder(MySqlSchemaBuilder schema, string name)
        : base( schema.Database.GetNextId(), name, SqlObjectType.Table )
    {
        _referencingViews = null;
        Schema = schema;
        Columns = new MySqlColumnBuilderCollection( this );
        Constraints = new MySqlConstraintBuilderCollection( this );
        _fullName = null;
        _info = null;
        _recordSet = null;
    }

    public MySqlSchemaBuilder Schema { get; }
    public MySqlColumnBuilderCollection Columns { get; }
    public MySqlConstraintBuilderCollection Constraints { get; }
    public IReadOnlyCollection<MySqlViewBuilder> ReferencingViews => (_referencingViews?.Values).EmptyIfNull();

    public override string FullName => _fullName ??= MySqlHelpers.GetFullName( Schema.Name, Name );
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlTableBuilderNode RecordSet => _recordSet ??= SqlNode.Table( this );
    public override MySqlDatabaseBuilder Database => Schema.Database;

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
    IReadOnlyCollection<ISqlViewBuilder> ISqlTableBuilder.ReferencingViews => ReferencingViews;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public MySqlTableBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
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

        MySqlHelpers.AssertName( name );
        var obj = Schema.Objects.TryGetObject( name );
        if ( obj is not null )
            throw new MySqlObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        using var viewBuffer = MySqlViewBuilder.RemoveReferencingViewsIntoBuffer( Database, _referencingViews );

        Schema.Objects.ChangeName( this, name );
        var oldName = Name;
        Name = name;
        ResetFullName();
        Database.ChangeTracker.NameUpdated( this, this, oldName );

        foreach ( var view in viewBuffer )
            ReinterpretCast.To<MySqlViewBuilder>( view ).Reactivate();
    }

    internal void OnSchemaNameChange(string oldName)
    {
        ResetFullName();
        foreach ( var constraint in Constraints )
            constraint.ResetFullName();

        Database.ChangeTracker.SchemaNameUpdated( this, this, oldName );
    }

    internal void ResetFullName()
    {
        _info = null;
        _fullName = null;
        foreach ( var column in Columns )
            column.ResetFullName();
    }

    ISqlTableBuilder ISqlTableBuilder.SetName(string name)
    {
        return SetName( name );
    }
}
