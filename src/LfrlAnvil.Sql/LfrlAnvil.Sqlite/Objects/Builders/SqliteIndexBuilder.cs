using System;
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

public sealed class SqliteIndexBuilder : SqliteObjectBuilder, ISqlIndexBuilder
{
    private Dictionary<ulong, SqliteForeignKeyBuilder>? _foreignKeys;
    private Dictionary<ulong, SqliteForeignKeyBuilder>? _referencingForeignKeys;
    private Dictionary<ulong, SqliteColumnBuilder>? _filterColumns;
    private SqliteIndexColumnBuilder[] _columns;
    private string _fullName;

    internal SqliteIndexBuilder(SqliteTableBuilder table, SqliteIndexColumnBuilder[] columns, string name, bool isUnique)
        : base( table.Database.GetNextId(), name, SqlObjectType.Index )
    {
        Table = table;
        IsUnique = isUnique;
        _columns = columns;
        PrimaryKey = null;
        Filter = null;
        _foreignKeys = null;
        _referencingForeignKeys = null;
        _filterColumns = null;
        _fullName = string.Empty;
        UpdateFullName();

        foreach ( var c in _columns )
            c.Column.AddIndex( this );
    }

    public SqliteTableBuilder Table { get; }
    public SqlitePrimaryKeyBuilder? PrimaryKey { get; private set; }
    public bool IsUnique { get; private set; }
    public SqlConditionNode? Filter { get; private set; }
    public ReadOnlyMemory<SqliteIndexColumnBuilder> Columns => _columns;
    public IReadOnlyCollection<SqliteForeignKeyBuilder> ForeignKeys => (_foreignKeys?.Values).EmptyIfNull();
    public IReadOnlyCollection<SqliteForeignKeyBuilder> ReferencingForeignKeys => (_referencingForeignKeys?.Values).EmptyIfNull();
    public IReadOnlyCollection<SqliteColumnBuilder> FilterColumns => (_filterColumns?.Values).EmptyIfNull();
    public override SqliteDatabaseBuilder Database => Table.Database;
    public override string FullName => _fullName;

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
    IReadOnlyCollection<ISqlForeignKeyBuilder> ISqlIndexBuilder.ForeignKeys => ForeignKeys;
    IReadOnlyCollection<ISqlForeignKeyBuilder> ISqlIndexBuilder.ReferencingForeignKeys => ReferencingForeignKeys;
    IReadOnlyCollection<ISqlColumnBuilder> ISqlIndexBuilder.FilterColumns => FilterColumns;

    public SqliteIndexBuilder SetDefaultName()
    {
        return SetName( SqliteHelpers.GetDefaultIndexName( Table, _columns, IsUnique ) );
    }

    public SqliteIndexBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
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
            Database.ChangeTracker.IsUniqueUpdated( this );
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
                if ( PrimaryKey is not null )
                    throw new SqliteObjectBuilderException( ExceptionResources.PrimaryKeyIndexCannotBePartial );

                var validator = new SqliteIndexFilterSourceValidator( Table );
                validator.Visit( filter );

                var errors = validator.GetErrors();
                if ( errors.Count > 0 )
                    throw new SqliteObjectBuilderException( errors );

                ClearFilterColumns();
                RegisterFilterColumns( validator.ReferencedColumns );
            }
            else
                ClearFilterColumns();

            var oldValue = Filter;
            Filter = filter;
            Database.ChangeTracker.IsFilterUpdated( this, oldValue );
        }

        return this;
    }

    internal void AssignPrimaryKey(SqlitePrimaryKeyBuilder primaryKey)
    {
        Assume.IsNull( PrimaryKey, nameof( PrimaryKey ) );
        Assume.Equals( IsUnique, true, nameof( IsUnique ) );
        Assume.IsNull( Filter, nameof( Filter ) );

        PrimaryKey = primaryKey;
        Database.ChangeTracker.PrimaryKeyUpdated( this, null );
    }

    internal void AddForeignKey(SqliteForeignKeyBuilder foreignKey)
    {
        _foreignKeys ??= new Dictionary<ulong, SqliteForeignKeyBuilder>();
        _foreignKeys.Add( foreignKey.Id, foreignKey );
    }

    internal void AddReferencingForeignKey(SqliteForeignKeyBuilder foreignKey)
    {
        Assume.Equals( IsUnique, true, nameof( IsUnique ) );
        _referencingForeignKeys ??= new Dictionary<ulong, SqliteForeignKeyBuilder>();
        _referencingForeignKeys.Add( foreignKey.Id, foreignKey );
    }

    internal void RemoveForeignKey(SqliteForeignKeyBuilder foreignKey)
    {
        _foreignKeys?.Remove( foreignKey.Id );
    }

    internal void RemoveReferencingForeignKey(SqliteForeignKeyBuilder foreignKey)
    {
        _referencingForeignKeys?.Remove( foreignKey.Id );
    }

    [Pure]
    internal bool AreColumnsEqual(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        return Table.Indexes.Comparer.Equals( _columns, columns );
    }

    internal void UpdateFullName()
    {
        _fullName = SqliteHelpers.GetFullName( Table.Schema.Name, Name );
    }

    internal void ClearForeignKeysInto(RentedMemorySequenceSpan<SqliteObjectBuilder> buffer)
    {
        _foreignKeys?.Values.CopyTo( buffer );
        _foreignKeys = null;
    }

    internal void ClearReferencingForeignKeysInto(RentedMemorySequenceSpan<SqliteObjectBuilder> buffer)
    {
        _referencingForeignKeys?.Values.CopyTo( buffer );
        _referencingForeignKeys?.Clear();
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
        Assume.Equals( CanRemove, true, nameof( CanRemove ) );

        var fkCount = ForeignKeys.Count;
        using var buffer = Database.ObjectPool.Rent( fkCount + ReferencingForeignKeys.Count );
        ClearForeignKeysInto( buffer );
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
            c.Column.RemoveIndex( this );

        _columns = Array.Empty<SqliteIndexColumnBuilder>();

        if ( Filter is not null )
        {
            var filter = Filter;
            ClearFilterColumns();
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

        SqliteHelpers.AssertName( name );
        Table.Schema.Objects.ChangeName( this, name );

        var oldName = FullName;
        Name = name;
        UpdateFullName();
        Database.ChangeTracker.FullNameUpdated( Table, this, oldName );
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

    private void ClearFilterColumns()
    {
        if ( _filterColumns is not null && _filterColumns.Count > 0 )
        {
            foreach ( var column in _filterColumns.Values )
                column.RemoveIndexFilter( this );
        }

        _filterColumns = null;
    }

    private void RegisterFilterColumns(Dictionary<ulong, SqliteColumnBuilder> columns)
    {
        Assume.IsNull( _filterColumns, nameof( _filterColumns ) );

        if ( columns.Count == 0 )
            return;

        _filterColumns = columns;
        foreach ( var column in _filterColumns.Values )
            column.AddIndexFilter( this );
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
