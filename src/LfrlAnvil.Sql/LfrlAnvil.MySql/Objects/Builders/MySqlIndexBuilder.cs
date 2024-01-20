﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlIndexBuilder : MySqlObjectBuilder, ISqlIndexBuilder
{
    private Dictionary<ulong, MySqlForeignKeyBuilder>? _originatingForeignKeys;
    private Dictionary<ulong, MySqlForeignKeyBuilder>? _referencingForeignKeys;
    private Dictionary<ulong, MySqlColumnBuilder>? _referencedFilterColumns;
    private MySqlIndexColumnBuilder[] _columns;
    private string? _fullName;

    internal MySqlIndexBuilder(MySqlTableBuilder table, MySqlIndexColumnBuilder[] columns, string name, bool isUnique)
        : base( table.Database.GetNextId(), name, SqlObjectType.Index )
    {
        Table = table;
        IsUnique = isUnique;
        _columns = columns;
        PrimaryKey = null;
        Filter = null;
        _originatingForeignKeys = null;
        _referencingForeignKeys = null;
        _referencedFilterColumns = null;
        _fullName = null;

        foreach ( var c in _columns )
            c.Column.AddReferencingIndex( this );
    }

    public MySqlTableBuilder Table { get; }
    public MySqlPrimaryKeyBuilder? PrimaryKey { get; private set; }
    public bool IsUnique { get; private set; }
    public SqlConditionNode? Filter { get; private set; }
    public ReadOnlyMemory<MySqlIndexColumnBuilder> Columns => _columns;
    public IReadOnlyCollection<MySqlForeignKeyBuilder> OriginatingForeignKeys => (_originatingForeignKeys?.Values).EmptyIfNull();
    public IReadOnlyCollection<MySqlForeignKeyBuilder> ReferencingForeignKeys => (_referencingForeignKeys?.Values).EmptyIfNull();
    public IReadOnlyCollection<MySqlColumnBuilder> ReferencedFilterColumns => (_referencedFilterColumns?.Values).EmptyIfNull();
    public override MySqlDatabaseBuilder Database => Table.Database;
    public override string FullName => _fullName ??= MySqlHelpers.GetFullName( Table.Schema.Name, Name );

    internal override bool CanRemove
    {
        get
        {
            if ( _referencingForeignKeys is null || _referencingForeignKeys.Count == 0 )
                return true;

            foreach ( var fk in _referencingForeignKeys.Values )
            {
                if ( ! fk.IsSelfReference() )
                    return false;
            }

            return true;
        }
    }

    ISqlTableBuilder ISqlIndexBuilder.Table => Table;
    ISqlPrimaryKeyBuilder? ISqlIndexBuilder.PrimaryKey => PrimaryKey;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;
    ReadOnlyMemory<ISqlIndexColumnBuilder> ISqlIndexBuilder.Columns => _columns;
    IReadOnlyCollection<ISqlForeignKeyBuilder> ISqlIndexBuilder.OriginatingForeignKeys => OriginatingForeignKeys;
    IReadOnlyCollection<ISqlForeignKeyBuilder> ISqlIndexBuilder.ReferencingForeignKeys => ReferencingForeignKeys;
    IReadOnlyCollection<ISqlColumnBuilder> ISqlIndexBuilder.ReferencedFilterColumns => ReferencedFilterColumns;

    public MySqlIndexBuilder SetDefaultName()
    {
        return SetName( MySqlHelpers.GetDefaultIndexName( Table, _columns, IsUnique ) );
    }

    public MySqlIndexBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    public MySqlIndexBuilder MarkAsUnique(bool enabled = true)
    {
        EnsureNotRemoved();

        if ( IsUnique != enabled )
        {
            if ( ! enabled )
                AssertDropUnique();

            IsUnique = enabled;
            Database.ChangeTracker.IsUniqueUpdated( this );
        }

        return this;
    }

    public MySqlIndexBuilder SetFilter(SqlConditionNode? filter)
    {
        EnsureNotRemoved();

        if ( ! ReferenceEquals( Filter, filter ) )
        {
            if ( filter is not null )
            {
                var errors = Chain<string>.Empty;

                if ( PrimaryKey is not null )
                    errors = errors.Extend( ExceptionResources.PrimaryKeyIndexCannotBePartial );

                if ( _referencingForeignKeys is not null )
                {
                    foreach ( var fk in _referencingForeignKeys.Values )
                        errors = errors.Extend( ExceptionResources.IndexMustRemainNonPartialBecauseItIsReferencedByForeignKey( fk ) );
                }

                var validator = new MySqlTableScopeExpressionValidator( Table );
                validator.Visit( filter );

                errors = errors.Extend( validator.GetErrors() );
                if ( errors.Count > 0 )
                    throw new MySqlObjectBuilderException( errors );

                ClearReferencedFilterColumns();
                RegisterReferencedFilterColumns( validator.ReferencedColumns );
            }
            else
                ClearReferencedFilterColumns();

            var oldValue = Filter;
            Filter = filter;
            Database.ChangeTracker.IsFilterUpdated( this, oldValue );
        }

        return this;
    }

    internal void AssignPrimaryKey(MySqlPrimaryKeyBuilder primaryKey)
    {
        Assume.IsNull( PrimaryKey );
        Assume.Equals( IsUnique, true );
        Assume.IsNull( Filter );

        PrimaryKey = primaryKey;
        Database.ChangeTracker.PrimaryKeyUpdated( this, null );
    }

    internal void AddOriginatingForeignKey(MySqlForeignKeyBuilder foreignKey)
    {
        _originatingForeignKeys ??= new Dictionary<ulong, MySqlForeignKeyBuilder>();
        _originatingForeignKeys.Add( foreignKey.Id, foreignKey );
    }

    internal void AddReferencingForeignKey(MySqlForeignKeyBuilder foreignKey)
    {
        Assume.Equals( IsUnique, true );
        _referencingForeignKeys ??= new Dictionary<ulong, MySqlForeignKeyBuilder>();
        _referencingForeignKeys.Add( foreignKey.Id, foreignKey );
    }

    internal void RemoveOriginatingForeignKey(MySqlForeignKeyBuilder foreignKey)
    {
        _originatingForeignKeys?.Remove( foreignKey.Id );
    }

    internal void RemoveReferencingForeignKey(MySqlForeignKeyBuilder foreignKey)
    {
        _referencingForeignKeys?.Remove( foreignKey.Id );
    }

    [Pure]
    internal bool AreColumnsEqual(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        return Table.Indexes.Comparer.Equals( _columns, columns );
    }

    internal void ResetFullName()
    {
        _fullName = null;
    }

    internal void ClearOriginatingForeignKeysInto(RentedMemorySequenceSpan<MySqlObjectBuilder> buffer)
    {
        _originatingForeignKeys?.Values.CopyTo( buffer );
        _originatingForeignKeys = null;
    }

    internal void ClearReferencingForeignKeysInto(RentedMemorySequenceSpan<MySqlObjectBuilder> buffer)
    {
        _referencingForeignKeys?.Values.CopyTo( buffer );
        _referencingForeignKeys = null;
    }

    internal void MarkAsRemoved()
    {
        Assume.Equals( IsRemoved, false );
        IsRemoved = true;

        _originatingForeignKeys = null;
        _referencingForeignKeys = null;

        if ( PrimaryKey is not null )
        {
            PrimaryKey.MarkAsRemoved();
            PrimaryKey = null;
        }

        foreach ( var c in _columns )
            c.Column.RemoveReferencingIndex( this );

        _columns = Array.Empty<MySqlIndexColumnBuilder>();

        if ( Filter is not null )
        {
            ClearReferencedFilterColumns();
            Filter = null;
        }
    }

    protected override void AssertRemoval()
    {
        if ( _referencingForeignKeys is null || _referencingForeignKeys.Count == 0 )
            return;

        var errors = Chain<string>.Empty;

        foreach ( var fk in _referencingForeignKeys.Values )
        {
            if ( ! fk.IsSelfReference() )
                errors = errors.Extend( ExceptionResources.DetectedExternalForeignKey( fk ) );
        }

        if ( errors.Count > 0 )
            throw new MySqlObjectBuilderException( errors );
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true );

        var fkCount = OriginatingForeignKeys.Count;
        using var buffer = Database.ObjectPool.Rent( fkCount + ReferencingForeignKeys.Count );
        ClearOriginatingForeignKeysInto( buffer );
        ClearReferencingForeignKeysInto( buffer.Slice( fkCount ) );

        foreach ( var fk in buffer )
            fk.Remove();

        if ( PrimaryKey is not null )
        {
            var pk = PrimaryKey;
            PrimaryKey.Remove();
            PrimaryKey = null;
            Database.ChangeTracker.PrimaryKeyUpdated( this, pk );
        }

        var columns = _columns;
        foreach ( var c in _columns )
            c.Column.RemoveReferencingIndex( this );

        _columns = Array.Empty<MySqlIndexColumnBuilder>();

        if ( Filter is not null )
        {
            var filter = Filter;
            ClearReferencedFilterColumns();
            Filter = null;
            Database.ChangeTracker.IsFilterUpdated( this, filter );
        }

        Table.Schema.Objects.Remove( Name );
        Table.Indexes.Remove( columns );
        Database.ChangeTracker.ObjectRemoved( Table, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        MySqlHelpers.AssertName( name );
        Table.Schema.Objects.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        ResetFullName();
        Database.ChangeTracker.NameUpdated( Table, this, oldName );
    }

    private void AssertDropUnique()
    {
        var errors = Chain<string>.Empty;

        if ( PrimaryKey is not null )
            errors = errors.Extend( ExceptionResources.PrimaryKeyIndexMustRemainUnique );

        if ( _referencingForeignKeys is not null )
        {
            foreach ( var foreignKey in _referencingForeignKeys.Values )
                errors = errors.Extend( ExceptionResources.IndexMustRemainUniqueBecauseItIsReferencedByForeignKey( foreignKey ) );
        }

        if ( errors.Count > 0 )
            throw new MySqlObjectBuilderException( errors );
    }

    private void ClearReferencedFilterColumns()
    {
        if ( _referencedFilterColumns is not null && _referencedFilterColumns.Count > 0 )
        {
            foreach ( var column in _referencedFilterColumns.Values )
                column.RemoveReferencingIndexFilter( this );
        }

        _referencedFilterColumns = null;
    }

    private void RegisterReferencedFilterColumns(Dictionary<ulong, MySqlColumnBuilder> columns)
    {
        Assume.IsNull( _referencedFilterColumns );

        if ( columns.Count == 0 )
            return;

        _referencedFilterColumns = columns;
        foreach ( var column in _referencedFilterColumns.Values )
            column.AddReferencingIndexFilter( this );
    }

    ISqlIndexBuilder ISqlIndexBuilder.SetDefaultName()
    {
        return SetDefaultName();
    }

    ISqlIndexBuilder ISqlIndexBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlIndexBuilder ISqlIndexBuilder.MarkAsUnique(bool enabled)
    {
        return MarkAsUnique( enabled );
    }

    ISqlIndexBuilder ISqlIndexBuilder.SetFilter(SqlConditionNode? filter)
    {
        return SetFilter( filter );
    }
}
