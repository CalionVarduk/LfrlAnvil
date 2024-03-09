using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlIndexBuilder : SqlConstraintBuilder, ISqlIndexBuilder
{
    private ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> _columns;
    private ReadOnlyArray<SqlColumnBuilder> _referencedFilterColumns;

    protected SqlIndexBuilder(
        SqlTableBuilder table,
        string name,
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns,
        bool isUnique)
        : base( table, SqlObjectType.Index, name )
    {
        _referencedFilterColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
        _columns = ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>>.Empty;
        IsUnique = isUnique;
        IsVirtual = false;
        PrimaryKey = null;
        Filter = null;
        SetColumnReferences( columns );
    }

    public SqlPrimaryKeyBuilder? PrimaryKey { get; private set; }
    public bool IsUnique { get; private set; }
    public bool IsVirtual { get; private set; }
    public SqlConditionNode? Filter { get; private set; }
    public override bool CanRemove => base.CanRemove && (PrimaryKey?.ReferencedTargets is null || PrimaryKey.ReferencedTargets.Count == 0);
    public SqlIndexColumnBuilderArray<SqlColumnBuilder> Columns => SqlIndexColumnBuilderArray<SqlColumnBuilder>.From( _columns );

    public SqlObjectBuilderArray<SqlColumnBuilder> ReferencedFilterColumns =>
        SqlObjectBuilderArray<SqlColumnBuilder>.From( _referencedFilterColumns );

    ISqlPrimaryKeyBuilder? ISqlIndexBuilder.PrimaryKey => PrimaryKey;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;
    IReadOnlyList<SqlIndexColumnBuilder<ISqlColumnBuilder>> ISqlIndexBuilder.Columns => _columns.GetUnderlyingArray();
    IReadOnlyCollection<ISqlColumnBuilder> ISqlIndexBuilder.ReferencedFilterColumns => _referencedFilterColumns.GetUnderlyingArray();

    public new SqlIndexBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqlIndexBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public SqlIndexBuilder MarkAsUnique(bool enabled = true)
    {
        ThrowIfRemoved();
        var change = BeforeIsUniqueChange( enabled );
        if ( change.IsCancelled )
            return this;

        var originalValue = IsUnique;
        IsUnique = change.NewValue;
        AfterIsUniqueChange( originalValue );
        return this;
    }

    public SqlIndexBuilder MarkAsVirtual(bool enabled = true)
    {
        ThrowIfRemoved();
        var change = BeforeIsVirtualChange( enabled );
        if ( change.IsCancelled )
            return this;

        var originalValue = IsVirtual;
        IsVirtual = change.NewValue;
        AfterIsVirtualChange( originalValue );
        return this;
    }

    public SqlIndexBuilder SetFilter(SqlConditionNode? filter)
    {
        ThrowIfRemoved();
        var change = BeforeFilterChange( filter );
        if ( change.IsCancelled )
            return this;

        var originalValue = Filter;
        Filter = change.NewValue;
        AfterFilterChange( originalValue );
        return this;
    }

    [Pure]
    protected override string GetDefaultName()
    {
        return SqlHelpers.GetDefaultIndexName( Table, _columns, IsUnique );
    }

    protected virtual SqlPropertyChange<bool> BeforeIsUniqueChange(bool newValue)
    {
        if ( IsUnique == newValue )
            return SqlPropertyChange.Cancel<bool>();

        if ( IsUnique )
            ThrowIfMustRemainUnique();
        else
            ThrowIfCannotBeUnique();

        return newValue;
    }

    protected virtual void AfterIsUniqueChange(bool originalValue)
    {
        AddIsUniqueChange( this, originalValue );
    }

    protected virtual SqlPropertyChange<bool> BeforeIsVirtualChange(bool newValue)
    {
        if ( IsVirtual == newValue )
            return SqlPropertyChange.Cancel<bool>();

        if ( IsVirtual )
            ThrowIfMustRemainVirtual();
        else
            ThrowIfCannotBeVirtual();

        return newValue;
    }

    protected virtual void AfterIsVirtualChange(bool originalValue)
    {
        AddIsVirtualChange( this, originalValue );
    }

    protected virtual SqlPropertyChange<SqlConditionNode?> BeforeFilterChange(SqlConditionNode? newValue)
    {
        if ( ReferenceEquals( Filter, newValue ) )
            return SqlPropertyChange.Cancel<SqlConditionNode?>();

        if ( newValue is not null )
        {
            if ( Filter is null )
                ThrowIfCannotBePartial();

            var filterColumns = ValidateFilterCondition( newValue );
            ClearFilterColumnReferences();
            SetFilterColumnReferences( filterColumns );
        }
        else
            ClearFilterColumnReferences();

        return newValue;
    }

    protected virtual void AfterFilterChange(SqlConditionNode? originalValue)
    {
        AddFilterChange( this, originalValue );
    }

    protected virtual void AfterPrimaryKeyChange(SqlPrimaryKeyBuilder? originalValue)
    {
        AddPrimaryKeyChange( this, originalValue );
    }

    protected void ThrowIfMustRemainUnique()
    {
        var errors = Chain<string>.Empty;

        if ( PrimaryKey is not null )
            errors = errors.Extend( ExceptionResources.PrimaryKeyIndexMustRemainUnique );

        foreach ( var reference in ReferencingObjects )
        {
            if ( reference.Source.Object.Type != SqlObjectType.ForeignKey || reference.Source.Property is not null )
                continue;

            var foreignKey = ReinterpretCast.To<SqlForeignKeyBuilder>( reference.Source.Object );
            if ( ReferenceEquals( foreignKey.ReferencedIndex, this ) )
                errors = errors.Extend( ExceptionResources.IndexMustRemainUniqueBecauseItIsReferencedByForeignKey( foreignKey ) );
        }

        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( Database, errors );
    }

    protected void ThrowIfCannotBeUnique()
    {
        if ( IsVirtual )
            ExceptionThrower.Throw( SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.VirtualIndexCannotBeUnique ) );
    }

    protected void ThrowIfMustRemainVirtual()
    {
        if ( PrimaryKey is not null )
            ExceptionThrower.Throw(
                SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.PrimaryKeyIndexMustRemainVirtual ) );
    }

    protected void ThrowIfCannotBeVirtual()
    {
        var errors = Chain<string>.Empty;

        if ( IsUnique )
            errors = errors.Extend( ExceptionResources.UniqueIndexCannotBeVirtual );

        if ( Filter is not null )
            errors = errors.Extend( ExceptionResources.PartialIndexCannotBeVirtual );

        foreach ( var reference in ReferencingObjects )
        {
            if ( reference.Source.Object.Type != SqlObjectType.ForeignKey || reference.Source.Property is not null )
                continue;

            var foreignKey = ReinterpretCast.To<SqlForeignKeyBuilder>( reference.Source.Object );
            if ( ReferenceEquals( foreignKey.ReferencedIndex, this ) )
                errors = errors.Extend( ExceptionResources.IndexMustRemainNonVirtualBecauseItIsReferencedByForeignKey( foreignKey ) );
        }

        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( Database, errors );
    }

    protected void ThrowIfCannotBePartial()
    {
        var errors = Chain<string>.Empty;

        if ( PrimaryKey is not null )
            errors = errors.Extend( ExceptionResources.PrimaryKeyIndexCannotBePartial );

        if ( IsVirtual )
            errors = errors.Extend( ExceptionResources.VirtualIndexCannotBePartial );

        foreach ( var reference in ReferencingObjects )
        {
            if ( reference.Source.Object.Type != SqlObjectType.ForeignKey || reference.Source.Property is not null )
                continue;

            var foreignKey = ReinterpretCast.To<SqlForeignKeyBuilder>( reference.Source.Object );
            if ( ReferenceEquals( foreignKey.ReferencedIndex, this ) )
                errors = errors.Extend( ExceptionResources.IndexMustRemainNonPartialBecauseItIsReferencedByForeignKey( foreignKey ) );
        }

        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( Database, errors );
    }

    [Pure]
    protected ReadOnlyArray<SqlColumnBuilder> ValidateFilterCondition(SqlConditionNode condition)
    {
        // TODO:
        // move to configurable db builder interface (low priority, later)
        var validator = new SqlTableScopeExpressionValidator( Table );
        validator.Visit( condition );

        var errors = validator.GetErrors();
        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( Database, errors );

        return validator.GetReferencedColumns();
    }

    protected void SetColumnReferences(ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns)
    {
        _columns = columns;
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var c in _columns )
            AddReference( c.UnsafeReinterpretAs<SqlColumnBuilder>().Column, refSource );
    }

    protected void ClearColumnReferences()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var c in _columns )
            RemoveReference( c.UnsafeReinterpretAs<SqlColumnBuilder>().Column, refSource );

        _columns = ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>>.Empty;
    }

    protected void SetFilterColumnReferences(ReadOnlyArray<SqlColumnBuilder> columns)
    {
        _referencedFilterColumns = columns;
        var refSource = SqlObjectBuilderReferenceSource.Create( this, property: nameof( Filter ) );
        foreach ( var column in _referencedFilterColumns )
            AddReference( column, refSource );
    }

    protected void ClearFilterColumnReferences()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this, property: nameof( Filter ) );
        foreach ( var column in _referencedFilterColumns )
            RemoveReference( column, refSource );

        _referencedFilterColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
    }

    protected override void BeforeRemove()
    {
        if ( PrimaryKey is not null )
        {
            PrimaryKey.ThrowIfReferenced();
            ThrowIfReferenced();
            RemoveFromCollection( Table.Constraints, PrimaryKey );
            RemoveFromCollection( Table.Constraints, this );
        }
        else
            base.BeforeRemove();

        ClearColumnReferences();
        if ( Filter is null )
            return;

        ClearFilterColumnReferences();
        var filter = Filter;
        Filter = null;
        AfterFilterChange( filter );
    }

    protected override void AfterRemove()
    {
        var pk = PrimaryKey;
        PrimaryKey = null;

        if ( pk is not null )
        {
            AfterPrimaryKeyChange( pk );
            pk.Remove();
            AddRemoval( Table, pk );
        }

        base.AfterRemove();
    }

    protected override void QuickRemoveCore()
    {
        base.QuickRemoveCore();
        _columns = ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>>.Empty;
        _referencedFilterColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
        PrimaryKey = null;
        Filter = null;
    }

    internal void AssignPrimaryKey(SqlPrimaryKeyBuilder primaryKey)
    {
        Assume.Equals( primaryKey.Table, Table );
        Assume.IsNull( PrimaryKey );
        Assume.True( IsUnique );
        Assume.False( IsVirtual );
        Assume.IsNull( Filter );

        IsVirtual = true;
        AfterIsVirtualChange( false );
        PrimaryKey = primaryKey;
        AfterPrimaryKeyChange( null );
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

    ISqlIndexBuilder ISqlIndexBuilder.MarkAsVirtual(bool enabled)
    {
        return MarkAsVirtual( enabled );
    }

    ISqlIndexBuilder ISqlIndexBuilder.SetFilter(SqlConditionNode? filter)
    {
        return SetFilter( filter );
    }
}
