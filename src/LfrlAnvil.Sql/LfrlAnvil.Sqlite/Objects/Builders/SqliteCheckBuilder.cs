using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteCheckBuilder : SqlCheckBuilder
{
    internal SqliteCheckBuilder(
        SqliteTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, name, condition, referencedColumns ) { }

    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );
    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );

    public new SqlObjectBuilderArray<SqliteColumnBuilder> ReferencedColumns =>
        base.ReferencedColumns.UnsafeReinterpretAs<SqliteColumnBuilder>();

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Name )}";
    }

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
}
