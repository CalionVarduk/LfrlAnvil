using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteCheckBuilder : SqliteObjectBuilder, ISqlCheckBuilder
{
    private readonly Dictionary<ulong, SqliteColumnBuilder> _referencedColumns;
    private string _fullName;

    internal SqliteCheckBuilder(string name, SqlConditionNode condition, SqliteTableScopeExpressionValidator visitor)
        : base( visitor.Table.Database.GetNextId(), name, SqlObjectType.Check )
    {
        Table = visitor.Table;
        Condition = condition;
        _referencedColumns = visitor.ReferencedColumns;
        _fullName = string.Empty;
        UpdateFullName();
        AddSelfToReferencedColumns();
    }

    public SqliteTableBuilder Table { get; }
    public SqlConditionNode Condition { get; }
    public override string FullName => _fullName;
    public IReadOnlyCollection<SqliteColumnBuilder> ReferencedColumns => _referencedColumns.Values;
    public override SqliteDatabaseBuilder Database => Table.Database;
    IReadOnlyCollection<ISqlColumnBuilder> ISqlCheckBuilder.ReferencedColumns => ReferencedColumns;
    ISqlTableBuilder ISqlCheckBuilder.Table => Table;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public SqliteCheckBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    internal void UpdateFullName()
    {
        _fullName = SqliteHelpers.GetFullName( Table.Schema.Name, Name );
    }

    [Pure]
    internal static SqliteTableScopeExpressionValidator AssertConditionNode(SqliteTableBuilder table, SqlConditionNode condition)
    {
        var visitor = new SqliteTableScopeExpressionValidator( table );
        visitor.Visit( condition );

        var errors = visitor.GetErrors();
        if ( errors.Count > 0 )
            throw new SqliteObjectBuilderException( errors );

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

        SqliteHelpers.AssertName( name );
        Table.Schema.Objects.ChangeName( this, name );
        Table.Checks.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        UpdateFullName();
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
