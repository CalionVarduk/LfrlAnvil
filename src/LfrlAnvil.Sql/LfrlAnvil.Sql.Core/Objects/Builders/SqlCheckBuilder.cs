﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlCheckBuilder : SqlConstraintBuilder, ISqlCheckBuilder
{
    private ReadOnlyArray<SqlColumnBuilder> _referencedColumns;

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

    public SqlConditionNode Condition { get; }
    public SqlObjectBuilderArray<SqlColumnBuilder> ReferencedColumns => SqlObjectBuilderArray<SqlColumnBuilder>.From( _referencedColumns );

    IReadOnlyCollection<ISqlColumnBuilder> ISqlCheckBuilder.ReferencedColumns => _referencedColumns.GetUnderlyingArray();

    public new SqlCheckBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqlCheckBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    [Pure]
    internal static SqlTableScopeExpressionValidator AssertConditionNode(SqlTableBuilder table, SqlConditionNode condition)
    {
        // TODO:
        // move to configurable db builder interface (low priority, later)
        var visitor = new SqlTableScopeExpressionValidator( table );
        visitor.Visit( condition );

        var errors = visitor.GetErrors();
        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( table.Database, errors );

        return visitor;
    }

    [Pure]
    protected sealed override string GetDefaultName()
    {
        return SqlHelpers.GetDefaultCheckName( Table );
    }

    protected void SetReferencedColumns(ReadOnlyArray<SqlColumnBuilder> columns)
    {
        _referencedColumns = columns;
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var column in _referencedColumns )
            AddReference( column, refSource );
    }

    protected void ClearReferencedColumns()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var column in _referencedColumns )
            RemoveReference( column, refSource );

        _referencedColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
    }

    protected override void BeforeRemove()
    {
        base.BeforeRemove();
        ClearReferencedColumns();
    }

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