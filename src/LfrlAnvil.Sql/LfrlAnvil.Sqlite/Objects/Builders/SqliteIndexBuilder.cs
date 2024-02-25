﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteIndexBuilder : SqliteConstraintBuilder, ISqlIndexBuilder
{
    private Dictionary<ulong, SqliteForeignKeyBuilder>? _originatingForeignKeys;
    private Dictionary<ulong, SqliteForeignKeyBuilder>? _referencingForeignKeys;
    private Dictionary<ulong, SqliteColumnBuilder>? _referencedFilterColumns;
    private SqlIndexColumnBuilder<ISqlColumnBuilder>[] _columns;

    internal SqliteIndexBuilder(SqliteTableBuilder table, SqlIndexColumnBuilder<ISqlColumnBuilder>[] columns, string name, bool isUnique)
        : base( table, name, SqlObjectType.Index )
    {
        IsUnique = isUnique;
        _columns = columns;
        PrimaryKey = null;
        Filter = null;
        _originatingForeignKeys = null;
        _referencingForeignKeys = null;
        _referencedFilterColumns = null;

        foreach ( var c in _columns )
            ReinterpretCast.To<SqliteColumnBuilder>( c.Column ).AddReferencingIndex( this );
    }

    public SqlitePrimaryKeyBuilder? PrimaryKey { get; private set; }
    public bool IsUnique { get; private set; }
    public SqlConditionNode? Filter { get; private set; }
    public ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> Columns => _columns;
    public IReadOnlyCollection<SqliteForeignKeyBuilder> OriginatingForeignKeys => (_originatingForeignKeys?.Values).EmptyIfNull();
    public IReadOnlyCollection<SqliteForeignKeyBuilder> ReferencingForeignKeys => (_referencingForeignKeys?.Values).EmptyIfNull();
    public IReadOnlyCollection<SqliteColumnBuilder> ReferencedFilterColumns => (_referencedFilterColumns?.Values).EmptyIfNull();
    public override SqliteDatabaseBuilder Database => Table.Database;

    public override bool CanRemove
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

    ISqlPrimaryKeyBuilder? ISqlIndexBuilder.PrimaryKey => PrimaryKey;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;
    IReadOnlyList<SqlIndexColumnBuilder<ISqlColumnBuilder>> ISqlIndexBuilder.Columns => _columns;
    IReadOnlyCollection<ISqlColumnBuilder> ISqlIndexBuilder.ReferencedFilterColumns => ReferencedFilterColumns;

    public new SqliteIndexBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqliteIndexBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public SqliteIndexBuilder MarkAsUnique(bool enabled = true)
    {
        EnsureNotRemoved();

        if ( IsUnique != enabled )
        {
            if ( ! enabled )
                AssertDropUnique();

            IsUnique = enabled;
            Database.Changes.IsUniqueUpdated( this );
        }

        return this;
    }

    public SqliteIndexBuilder SetFilter(SqlConditionNode? filter)
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

                var validator = new SqliteTableScopeExpressionValidator( Table );
                validator.Visit( filter );

                errors = errors.Extend( validator.GetErrors() );
                if ( errors.Count > 0 )
                    throw new SqliteObjectBuilderException( errors );

                ClearReferencedFilterColumns();
                RegisterReferencedFilterColumns( validator.ReferencedColumns );
            }
            else
                ClearReferencedFilterColumns();

            var oldValue = Filter;
            Filter = filter;
            Database.Changes.IsFilterUpdated( this, oldValue );
        }

        return this;
    }

    internal void AssignPrimaryKey(SqlitePrimaryKeyBuilder primaryKey)
    {
        Assume.IsNull( PrimaryKey );
        Assume.True( IsUnique );
        Assume.IsNull( Filter );

        PrimaryKey = primaryKey;
        Database.Changes.PrimaryKeyUpdated( this, null );
    }

    internal void AddOriginatingForeignKey(SqliteForeignKeyBuilder foreignKey)
    {
        _originatingForeignKeys ??= new Dictionary<ulong, SqliteForeignKeyBuilder>();
        _originatingForeignKeys.Add( foreignKey.Id, foreignKey );
    }

    internal void AddReferencingForeignKey(SqliteForeignKeyBuilder foreignKey)
    {
        Assume.True( IsUnique );
        _referencingForeignKeys ??= new Dictionary<ulong, SqliteForeignKeyBuilder>();
        _referencingForeignKeys.Add( foreignKey.Id, foreignKey );
    }

    internal void RemoveOriginatingForeignKey(SqliteForeignKeyBuilder foreignKey)
    {
        _originatingForeignKeys?.Remove( foreignKey.Id );
    }

    internal void RemoveReferencingForeignKey(SqliteForeignKeyBuilder foreignKey)
    {
        _referencingForeignKeys?.Remove( foreignKey.Id );
    }

    internal void ClearOriginatingForeignKeys()
    {
        _originatingForeignKeys = null;
    }

    internal void ClearOriginatingForeignKeysInto(RentedMemorySequenceSpan<SqliteObjectBuilder> buffer)
    {
        _originatingForeignKeys?.Values.CopyTo( buffer );
        ClearOriginatingForeignKeys();
    }

    internal void ClearReferencingForeignKeysInto(RentedMemorySequenceSpan<SqliteObjectBuilder> buffer)
    {
        _referencingForeignKeys?.Values.CopyTo( buffer );
        _referencingForeignKeys?.Clear();
    }

    [Pure]
    protected override string GetDefaultName()
    {
        return SqliteHelpers.GetDefaultIndexName( Table, _columns, IsUnique );
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
            throw new SqliteObjectBuilderException( errors );
    }

    protected override void RemoveCore()
    {
        Assume.True( CanRemove );

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
            Database.Changes.PrimaryKeyUpdated( this, pk );
        }

        foreach ( var c in _columns )
            ReinterpretCast.To<SqliteColumnBuilder>( c.Column ).RemoveReferencingIndex( this );

        _columns = Array.Empty<SqlIndexColumnBuilder<ISqlColumnBuilder>>();

        if ( Filter is not null )
        {
            var filter = Filter;
            ClearReferencedFilterColumns();
            Filter = null;
            Database.Changes.IsFilterUpdated( this, filter );
        }

        Table.Schema.Objects.Remove( Name );
        Table.Constraints.Remove( Name );
        Database.Changes.ObjectRemoved( Table, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        SqliteHelpers.AssertName( name );
        Table.Schema.Objects.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        Database.Changes.NameUpdated( Table, this, oldName );
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
            throw new SqliteObjectBuilderException( errors );
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

    private void RegisterReferencedFilterColumns(Dictionary<ulong, SqliteColumnBuilder> columns)
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
