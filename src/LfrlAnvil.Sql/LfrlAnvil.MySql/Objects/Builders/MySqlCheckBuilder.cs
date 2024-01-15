using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlCheckBuilder : MySqlObjectBuilder, ISqlCheckBuilder
{
    private readonly Dictionary<ulong, MySqlColumnBuilder> _referencedColumns;
    private string? _fullName;

    internal MySqlCheckBuilder(string name, SqlConditionNode condition, MySqlTableScopeExpressionValidator visitor)
        : base( visitor.Table.Database.GetNextId(), name, SqlObjectType.Check )
    {
        Table = visitor.Table;
        Condition = condition;
        _referencedColumns = visitor.ReferencedColumns;
        _fullName = null;
        AddSelfToReferencedColumns();
    }

    public MySqlTableBuilder Table { get; }
    public SqlConditionNode Condition { get; }
    public override string FullName => _fullName ??= MySqlHelpers.GetFullName( Table.Schema.Name, Name );
    public IReadOnlyCollection<MySqlColumnBuilder> ReferencedColumns => _referencedColumns.Values;
    public override MySqlDatabaseBuilder Database => Table.Database;
    IReadOnlyCollection<ISqlColumnBuilder> ISqlCheckBuilder.ReferencedColumns => ReferencedColumns;
    ISqlTableBuilder ISqlCheckBuilder.Table => Table;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public MySqlCheckBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    internal void ResetFullName()
    {
        _fullName = null;
    }

    internal void MarkAsRemoved()
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

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true );

        foreach ( var column in _referencedColumns.Values )
            column.RemoveReferencingCheck( this );

        _referencedColumns.Clear();
        Table.Schema.Objects.Remove( Name );
        Table.Checks.Remove( Name );

        Database.ChangeTracker.ObjectRemoved( Table, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        MySqlHelpers.AssertName( name );
        Table.Schema.Objects.ChangeName( this, name );
        Table.Checks.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        ResetFullName();
        Database.ChangeTracker.NameUpdated( Table, this, oldName );
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
}
