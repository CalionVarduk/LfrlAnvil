using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteCheckBuilder : SqliteConstraintBuilder, ISqlCheckBuilder
{
    private readonly Dictionary<ulong, SqliteColumnBuilder> _referencedColumns;

    internal SqliteCheckBuilder(string name, SqlConditionNode condition, SqliteTableScopeExpressionValidator visitor)
        : base( visitor.Table, name, SqlObjectType.Check )
    {
        Condition = condition;
        _referencedColumns = visitor.ReferencedColumns;
        AddSelfToReferencedColumns();
    }

    public SqlConditionNode Condition { get; }
    public IReadOnlyCollection<SqliteColumnBuilder> ReferencedColumns => _referencedColumns.Values;
    public override SqliteDatabaseBuilder Database => Table.Database;
    IReadOnlyCollection<ISqlColumnBuilder> ISqlCheckBuilder.ReferencedColumns => ReferencedColumns;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public new SqliteCheckBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqliteCheckBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
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

    [Pure]
    protected override string GetDefaultName()
    {
        return SqliteHelpers.GetDefaultCheckName( Table );
    }

    protected override void RemoveCore()
    {
        Assume.True( CanRemove );

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

        SqliteHelpers.AssertName( name );
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
