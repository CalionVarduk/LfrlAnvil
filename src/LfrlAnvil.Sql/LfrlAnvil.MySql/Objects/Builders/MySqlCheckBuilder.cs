using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlCheckBuilder : MySqlConstraintBuilder, ISqlCheckBuilder
{
    private readonly Dictionary<ulong, MySqlColumnBuilder> _referencedColumns;

    internal MySqlCheckBuilder(string name, SqlConditionNode condition, MySqlTableScopeExpressionValidator visitor)
        : base( visitor.Table, name, SqlObjectType.Check )
    {
        Condition = condition;
        _referencedColumns = visitor.ReferencedColumns;
        AddSelfToReferencedColumns();
    }

    public SqlConditionNode Condition { get; }
    public IReadOnlyCollection<MySqlColumnBuilder> ReferencedColumns => _referencedColumns.Values;
    public override MySqlDatabaseBuilder Database => Table.Database;
    IReadOnlyCollection<ISqlColumnBuilder> ISqlCheckBuilder.ReferencedColumns => ReferencedColumns;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public new MySqlCheckBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new MySqlCheckBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    internal override void MarkAsRemoved()
    {
        Assume.Equals( IsRemoved, false );
        IsRemoved = true;

        foreach ( var column in _referencedColumns.Values )
            column.RemoveReferencingCheck( this );

        _referencedColumns.Clear();
    }

    [Pure]
    internal static MySqlTableScopeExpressionValidator AssertConditionNode(MySqlTableBuilder table, SqlConditionNode condition)
    {
        var visitor = new MySqlTableScopeExpressionValidator( table );
        visitor.Visit( condition );

        var errors = visitor.GetErrors();
        if ( errors.Count > 0 )
            throw new MySqlObjectBuilderException( errors );

        return visitor;
    }

    [Pure]
    protected override string GetDefaultName()
    {
        return MySqlHelpers.GetDefaultCheckName( Table );
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true );

        foreach ( var column in _referencedColumns.Values )
            column.RemoveReferencingCheck( this );

        _referencedColumns.Clear();
        Table.Schema.Objects.Remove( Name );
        Table.Constraints.Remove( Name );

        Database.Changes.ObjectRemoved( Table, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        MySqlHelpers.AssertName( name );
        Table.Schema.Objects.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        Database.Changes.NameUpdated( Table, this, oldName );
    }

    private void AddSelfToReferencedColumns()
    {
        foreach ( var column in _referencedColumns.Values )
            column.AddReferencingCheck( this );
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
