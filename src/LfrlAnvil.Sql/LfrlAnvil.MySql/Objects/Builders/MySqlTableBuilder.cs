using System;
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
        PrimaryKey = null;
        Columns = new MySqlColumnBuilderCollection( this );
        Indexes = new MySqlIndexBuilderCollection( this );
        ForeignKeys = new MySqlForeignKeyBuilderCollection( this );
        Checks = new MySqlCheckBuilderCollection( this );
        _fullName = null;
        _info = null;
        _recordSet = null;
    }

    public MySqlSchemaBuilder Schema { get; }
    public MySqlColumnBuilderCollection Columns { get; }
    public MySqlIndexBuilderCollection Indexes { get; }
    public MySqlForeignKeyBuilderCollection ForeignKeys { get; }
    public MySqlCheckBuilderCollection Checks { get; }
    public MySqlPrimaryKeyBuilder? PrimaryKey { get; private set; }
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

            foreach ( var ix in Indexes )
            {
                if ( ! ix.CanRemove )
                    return false;
            }

            return true;
        }
    }

    ISqlSchemaBuilder ISqlTableBuilder.Schema => Schema;
    ISqlPrimaryKeyBuilder? ISqlTableBuilder.PrimaryKey => PrimaryKey;
    ISqlColumnBuilderCollection ISqlTableBuilder.Columns => Columns;
    ISqlIndexBuilderCollection ISqlTableBuilder.Indexes => Indexes;
    ISqlForeignKeyBuilderCollection ISqlTableBuilder.ForeignKeys => ForeignKeys;
    ISqlCheckBuilderCollection ISqlTableBuilder.Checks => Checks;
    IReadOnlyCollection<ISqlViewBuilder> ISqlTableBuilder.ReferencingViews => ReferencingViews;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public MySqlTableBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    public MySqlPrimaryKeyBuilder SetPrimaryKey(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        EnsureNotRemoved();

        if ( PrimaryKey is null )
        {
            var indexColumns = MySqlHelpers.CreateIndexColumns( this, columns, allowNullableColumns: false );
            PrimaryKey = Schema.Objects.CreatePrimaryKey( this, indexColumns );
            Database.ChangeTracker.PrimaryKeyUpdated( this, null );
        }
        else if ( ! PrimaryKey.Index.AreColumnsEqual( columns ) )
        {
            var oldPrimaryKey = PrimaryKey;
            var indexColumns = MySqlHelpers.CreateIndexColumns( this, columns, allowNullableColumns: false );
            PrimaryKey = Schema.Objects.ReplacePrimaryKey( this, indexColumns, PrimaryKey );
            Database.ChangeTracker.PrimaryKeyUpdated( this, oldPrimaryKey );
        }

        return PrimaryKey;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal SqlRecordSetInfo? GetCachedInfo()
    {
        return _info;
    }

    internal void UnassignPrimaryKey()
    {
        Assume.IsNotNull( PrimaryKey );

        var oldPrimaryKey = PrimaryKey;
        PrimaryKey = null;
        Database.ChangeTracker.PrimaryKeyUpdated( this, oldPrimaryKey );
    }

    internal void MarkAsRemoved()
    {
        Assume.Equals( IsRemoved, false );
        IsRemoved = true;
        _referencingViews = null;
        PrimaryKey = null;

        foreach ( var column in Columns )
            column.MarkAsRemoved();

        foreach ( var index in Indexes )
            index.MarkAsRemoved();

        foreach ( var fk in ForeignKeys )
            fk.MarkAsRemoved();

        foreach ( var check in Checks )
            check.MarkAsRemoved();

        Checks.Clear();
        ForeignKeys.Clear();
        Indexes.Clear();
        Columns.Clear();
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

        foreach ( var ix in Indexes )
        {
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
        ForeignKeys.Clear();

        using var buffer = Database.ObjectPool.GreedyRent( Columns.Count + Indexes.Count + Checks.Count );
        var columns = buffer.Slice( 0, Columns.Count );
        var indexes = buffer.Slice( columns.Length, Indexes.Count );
        var checks = buffer.Slice( columns.Length + indexes.Length, Checks.Count );

        Columns.ClearInto( columns );
        Indexes.ClearInto( indexes );
        Checks.ClearInto( checks );

        foreach ( var obj in indexes )
        {
            var index = ReinterpretCast.To<MySqlIndexBuilder>( obj );
            var count = index.OriginatingForeignKeys.Count;
            buffer.Expand( count );
            index.ClearOriginatingForeignKeysInto( buffer.Slice( buffer.Length - count ) );
        }

        var foreignKeys = buffer.Slice( checks.StartIndex + checks.Length );
        foreach ( var fk in foreignKeys )
            fk.Remove();

        foreach ( var index in indexes )
            index.Remove();

        foreach ( var check in checks )
            check.Remove();

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
        if ( Schema.Objects.TryGet( name, out var obj ) )
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
        PrimaryKey?.ResetFullName();

        foreach ( var fk in ForeignKeys )
            fk.ResetFullName();

        foreach ( var chk in Checks )
            chk.ResetFullName();

        foreach ( var ix in Indexes )
            ix.ResetFullName();

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

    ISqlPrimaryKeyBuilder ISqlTableBuilder.SetPrimaryKey(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        return SetPrimaryKey( columns );
    }
}
