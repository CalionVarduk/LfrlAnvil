using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteIndexBuilder : SqlIndexBuilder
{
    internal SqliteIndexBuilder(
        SqliteTableBuilder table,
        string name,
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns,
        bool isUnique)
        : base( table, name, columns, isUnique ) { }

    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );
    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );
    public new SqlitePrimaryKeyBuilder? PrimaryKey => ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.PrimaryKey );
    public new SqlIndexColumnBuilderArray<SqliteColumnBuilder> Columns => base.Columns.UnsafeReinterpretAs<SqliteColumnBuilder>();

    public new SqlObjectBuilderArray<SqliteColumnBuilder> ReferencedFilterColumns =>
        base.ReferencedFilterColumns.UnsafeReinterpretAs<SqliteColumnBuilder>();

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Name )}";
    }

    public new SqliteIndexBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqliteIndexBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public new SqliteIndexBuilder MarkAsUnique(bool enabled = true)
    {
        base.MarkAsUnique( enabled );
        return this;
    }

    public new SqliteIndexBuilder SetFilter(SqlConditionNode? filter)
    {
        base.SetFilter( filter );
        return this;
    }
}
