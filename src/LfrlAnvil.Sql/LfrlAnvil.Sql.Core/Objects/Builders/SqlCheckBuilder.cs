using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlCheckBuilder" />
public abstract class SqlCheckBuilder : SqlConstraintBuilder, ISqlCheckBuilder
{
    private ReadOnlyArray<SqlColumnBuilder> _referencedColumns;

    /// <summary>
    /// Creates a new <see cref="SqlCheckBuilder"/> instance.
    /// </summary>
    /// <param name="table">Table that this constraint is attached to.</param>
    /// <param name="name">Object's name.</param>
    /// <param name="condition">Underlying condition of this check constraint.</param>
    /// <param name="referencedColumns">Collection of columns referenced by this check constraint.</param>
    protected SqlCheckBuilder(
        SqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, SqlObjectType.Check, name )
    {
        _referencedColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
        Condition = condition;
        SetReferencedColumns( referencedColumns );
    }

    /// <inheritdoc />
    public SqlConditionNode Condition { get; }

    /// <inheritdoc cref="ISqlCheckBuilder.ReferencedColumns" />
    public SqlObjectBuilderArray<SqlColumnBuilder> ReferencedColumns => SqlObjectBuilderArray<SqlColumnBuilder>.From( _referencedColumns );

    IReadOnlyCollection<ISqlColumnBuilder> ISqlCheckBuilder.ReferencedColumns => _referencedColumns.GetUnderlyingArray();

    /// <inheritdoc cref="SqlConstraintBuilder.SetName(string)" />
    public new SqlCheckBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlConstraintBuilder.SetDefaultName()" />
    public new SqlCheckBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    /// <inheritdoc />
    [Pure]
    protected sealed override string GetDefaultName()
    {
        return Database.DefaultNames.GetForCheck( Table );
    }

    /// <summary>
    /// Adds a collection of <paramref name="columns"/> to <see cref="ReferencedColumns"/>
    /// and adds this check constraint to their reference sources.
    /// </summary>
    /// <param name="columns">Collection of columns to add.</param>
    protected void SetReferencedColumns(ReadOnlyArray<SqlColumnBuilder> columns)
    {
        _referencedColumns = columns;
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var column in _referencedColumns )
            AddReference( column, refSource );
    }

    /// <summary>
    /// Removes all columns from <see cref="ReferencedColumns"/>
    /// and removes this check constraint from their reference sources.
    /// </summary>
    protected void ClearReferencedColumns()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var column in _referencedColumns )
            RemoveReference( column, refSource );

        _referencedColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
    }

    /// <inheritdoc />
    protected override void BeforeRemove()
    {
        base.BeforeRemove();
        ClearReferencedColumns();
    }

    /// <inheritdoc />
    protected override void QuickRemoveCore()
    {
        base.QuickRemoveCore();
        _referencedColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
    }

    ISqlCheckBuilder ISqlCheckBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlCheckBuilder ISqlCheckBuilder.SetDefaultName()
    {
        return SetDefaultName();
    }
}
