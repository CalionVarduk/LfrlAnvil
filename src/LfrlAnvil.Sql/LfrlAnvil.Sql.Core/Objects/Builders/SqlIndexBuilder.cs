using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlIndexBuilder" />
public abstract class SqlIndexBuilder : SqlConstraintBuilder, ISqlIndexBuilder
{
    private ReadOnlyArray<SqlColumnBuilder> _referencedColumns;
    private ReadOnlyArray<SqlColumnBuilder> _referencedFilterColumns;

    /// <summary>
    /// Creates a new <see cref="SqlIndexBuilder"/> instance.
    /// </summary>
    /// <param name="table">Table that this constraint is attached to.</param>
    /// <param name="name">Object's name.</param>
    /// <param name="columns">Collection of columns that define this index.</param>
    /// <param name="isUnique">Specifies whether or not this index is unique.</param>
    /// <param name="referencedColumns">Collection of columns referenced by this index's <see cref="Columns"/>.</param>
    protected SqlIndexBuilder(
        SqlTableBuilder table,
        string name,
        SqlIndexBuilderColumns<SqlColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, SqlObjectType.Index, name )
    {
        _referencedColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
        _referencedFilterColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
        Columns = SqlIndexBuilderColumns<SqlColumnBuilder>.Empty;
        IsUnique = isUnique;
        IsVirtual = false;
        PrimaryKey = null;
        Filter = null;
        SetColumnReferences( columns, referencedColumns );
    }

    /// <inheritdoc cref="ISqlIndexBuilder.PrimaryKey" />
    public SqlPrimaryKeyBuilder? PrimaryKey { get; private set; }

    /// <inheritdoc />
    public bool IsUnique { get; private set; }

    /// <inheritdoc />
    public bool IsVirtual { get; private set; }

    /// <inheritdoc />
    public SqlConditionNode? Filter { get; private set; }

    /// <inheritdoc cref="ISqlIndexBuilder.Columns" />
    public SqlIndexBuilderColumns<SqlColumnBuilder> Columns { get; private set; }

    /// <inheritdoc />
    public override bool CanRemove => base.CanRemove && (PrimaryKey?.ReferencedTargets is null || PrimaryKey.ReferencedTargets.Count == 0);

    /// <inheritdoc cref="ISqlIndexBuilder.ReferencedColumns" />
    public SqlObjectBuilderArray<SqlColumnBuilder> ReferencedColumns => SqlObjectBuilderArray<SqlColumnBuilder>.From( _referencedColumns );

    /// <inheritdoc cref="ISqlIndexBuilder.ReferencedFilterColumns" />
    public SqlObjectBuilderArray<SqlColumnBuilder> ReferencedFilterColumns =>
        SqlObjectBuilderArray<SqlColumnBuilder>.From( _referencedFilterColumns );

    ISqlPrimaryKeyBuilder? ISqlIndexBuilder.PrimaryKey => PrimaryKey;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    SqlIndexBuilderColumns<ISqlColumnBuilder> ISqlIndexBuilder.Columns =>
        new SqlIndexBuilderColumns<ISqlColumnBuilder>( Columns.Expressions );

    IReadOnlyCollection<ISqlColumnBuilder> ISqlIndexBuilder.ReferencedColumns => _referencedColumns.GetUnderlyingArray();
    IReadOnlyCollection<ISqlColumnBuilder> ISqlIndexBuilder.ReferencedFilterColumns => _referencedFilterColumns.GetUnderlyingArray();

    /// <inheritdoc cref="SqlConstraintBuilder.SetName(string)" />
    public new SqlIndexBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlConstraintBuilder.SetDefaultName()" />
    public new SqlIndexBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    /// <inheritdoc cref="ISqlIndexBuilder.MarkAsUnique(bool)" />
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

    /// <inheritdoc cref="ISqlIndexBuilder.MarkAsVirtual(bool)" />
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

    /// <inheritdoc cref="ISqlIndexBuilder.SetFilter(SqlConditionNode)" />
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

    /// <inheritdoc />
    [Pure]
    protected sealed override string GetDefaultName()
    {
        return Database.DefaultNames.GetForIndex(
            Table,
            new SqlIndexBuilderColumns<ISqlColumnBuilder>( Columns.Expressions ),
            IsUnique );
    }

    /// <summary>
    /// Callback invoked just before <see cref="IsUnique"/> change is processed.
    /// </summary>
    /// <param name="newValue">Value to set.</param>
    /// <returns><see cref="SqlPropertyChange{T}"/> instance associated with <see cref="IsUnique"/> change attempt.</returns>
    /// <exception cref="SqlObjectBuilderException">When <see cref="IsUnique"/> of this index cannot be changed.</exception>
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

    /// <summary>
    /// Callback invoked just after <see cref="IsUnique"/> change has been processed.
    /// </summary>
    /// <param name="originalValue">Original value.</param>
    protected virtual void AfterIsUniqueChange(bool originalValue)
    {
        AddIsUniqueChange( this, originalValue );
    }

    /// <summary>
    /// Callback invoked just before <see cref="IsVirtual"/> change is processed.
    /// </summary>
    /// <param name="newValue">Value to set.</param>
    /// <returns><see cref="SqlPropertyChange{T}"/> instance associated with <see cref="IsVirtual"/> change attempt.</returns>
    /// <exception cref="SqlObjectBuilderException">When <see cref="IsVirtual"/> of this index cannot be changed.</exception>
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

    /// <summary>
    /// Callback invoked just after <see cref="IsVirtual"/> change has been processed.
    /// </summary>
    /// <param name="originalValue">Original value.</param>
    protected virtual void AfterIsVirtualChange(bool originalValue)
    {
        AddIsVirtualChange( this, originalValue );
    }

    /// <summary>
    /// Callback invoked just before <see cref="Filter"/> change is processed.
    /// </summary>
    /// <param name="newValue">Value to set.</param>
    /// <returns><see cref="SqlPropertyChange{T}"/> instance associated with <see cref="Filter"/> change attempt.</returns>
    /// <exception cref="SqlObjectBuilderException">When <see cref="Filter"/> of this index cannot be changed.</exception>
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

    /// <summary>
    /// Callback invoked just after <see cref="Filter"/> change has been processed.
    /// </summary>
    /// <param name="originalValue">Original value.</param>
    protected virtual void AfterFilterChange(SqlConditionNode? originalValue)
    {
        AddFilterChange( this, originalValue );
    }

    /// <summary>
    /// Callback invoked just after <see cref="PrimaryKey"/> change has been processed.
    /// </summary>
    /// <param name="originalValue">Original value.</param>
    protected virtual void AfterPrimaryKeyChange(SqlPrimaryKeyBuilder? originalValue)
    {
        AddPrimaryKeyChange( this, originalValue );
    }

    /// <summary>
    /// Throws an exception when this index cannot be non-unique.
    /// </summary>
    /// <exception cref="SqlObjectBuilderException">When index cannot be non-unique.</exception>
    /// <remarks>Index cannot be assigned to a primary key and cannot be referenced by any foreign key.</remarks>
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

    /// <summary>
    /// Throws an exception when this index cannot be unique.
    /// </summary>
    /// <exception cref="SqlObjectBuilderException">When index cannot be unique.</exception>
    /// <remarks>Index cannot be virtual and all columns must be single column expressions.</remarks>
    protected void ThrowIfCannotBeUnique()
    {
        var errors = Chain<string>.Empty;

        if ( IsVirtual )
            errors = errors.Extend( ExceptionResources.VirtualIndexCannotBeUnique );

        foreach ( var column in Columns )
        {
            if ( column is null )
            {
                errors = errors.Extend( ExceptionResources.UniqueIndexCannotContainExpressions );
                break;
            }
        }

        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( Database, errors );
    }

    /// <summary>
    /// Throws an exception when this index cannot be non-virtual.
    /// </summary>
    /// <exception cref="SqlObjectBuilderException">When index cannot be non-virtual.</exception>
    /// <remarks>Index cannot be assigned to a primary key.</remarks>
    protected void ThrowIfMustRemainVirtual()
    {
        if ( PrimaryKey is not null )
            ExceptionThrower.Throw(
                SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.PrimaryKeyIndexMustRemainVirtual ) );
    }

    /// <summary>
    /// Throws an exception when this index cannot be virtual.
    /// </summary>
    /// <exception cref="SqlObjectBuilderException">When index cannot be virtual.</exception>
    /// <remarks>Index cannot be unique, cannot be partial and cannot be referenced by any foreign key.</remarks>
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

    /// <summary>
    /// Throws an exception when this index cannot be partial.
    /// </summary>
    /// <exception cref="SqlObjectBuilderException">When index cannot be partial.</exception>
    /// <remarks>Index cannot be assigned to a primary key, cannot be virtual and cannot be referenced by any foreign key.</remarks>
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

    /// <summary>
    /// Checks if the provided condition is a valid <see cref="Filter"/> condition.
    /// </summary>
    /// <param name="condition">Condition to check.</param>
    /// <returns>Collection of columns referenced by the <paramref name="condition"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When <paramref name="condition"/> is not valid.</exception>
    /// <remarks><see cref="CreateFilterConditionValidator()"/> creates condition's validator.</remarks>
    [Pure]
    protected ReadOnlyArray<SqlColumnBuilder> ValidateFilterCondition(SqlConditionNode condition)
    {
        var validator = CreateFilterConditionValidator();
        validator.Visit( condition );

        var errors = validator.GetErrors();
        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( Database, errors );

        return validator.GetReferencedColumns();
    }

    /// <summary>
    /// Creates a new <see cref="SqlTableScopeExpressionValidator"/> used for <see cref="Filter"/> condition validation.
    /// </summary>
    /// <returns>New <see cref="SqlTableScopeExpressionValidator"/> instance.</returns>
    [Pure]
    protected virtual SqlTableScopeExpressionValidator CreateFilterConditionValidator()
    {
        return new SqlTableScopeExpressionValidator( Table );
    }

    /// <summary>
    /// Adds a collection of <paramref name="referencedColumns"/> to <see cref="ReferencedColumns"/>
    /// and adds this index to their reference sources.
    /// </summary>
    /// <param name="columns">Collection of columns that define this index.</param>
    /// <param name="referencedColumns">Collection of columns to add.</param>
    protected void SetColumnReferences(SqlIndexBuilderColumns<SqlColumnBuilder> columns, ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        Columns = columns;
        _referencedColumns = referencedColumns;
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var c in _referencedColumns )
            AddReference( c, refSource );
    }

    /// <summary>
    /// Removes all columns from <see cref="ReferencedColumns"/> and removes this index from their reference sources.
    /// </summary>
    protected void ClearColumnReferences()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var c in _referencedColumns )
            RemoveReference( c, refSource );

        Columns = SqlIndexBuilderColumns<SqlColumnBuilder>.Empty;
        _referencedColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
    }

    /// <summary>
    /// Adds a collection of <paramref name="columns"/> to <see cref="ReferencedFilterColumns"/>
    /// and adds this index's <see cref="Filter"/> to their reference sources.
    /// </summary>
    /// <param name="columns">Collection of columns to add.</param>
    protected void SetFilterColumnReferences(ReadOnlyArray<SqlColumnBuilder> columns)
    {
        _referencedFilterColumns = columns;
        var refSource = SqlObjectBuilderReferenceSource.Create( this, property: nameof( Filter ) );
        foreach ( var column in _referencedFilterColumns )
            AddReference( column, refSource );
    }

    /// <summary>
    /// Removes all columns from <see cref="ReferencedFilterColumns"/>
    /// and removes this index's <see cref="Filter"/> from their reference sources.
    /// </summary>
    protected void ClearFilterColumnReferences()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this, property: nameof( Filter ) );
        foreach ( var column in _referencedFilterColumns )
            RemoveReference( column, refSource );

        _referencedFilterColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    protected override void QuickRemoveCore()
    {
        base.QuickRemoveCore();
        _referencedColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
        _referencedFilterColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
        Columns = SqlIndexBuilderColumns<SqlColumnBuilder>.Empty;
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
