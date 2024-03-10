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
        SqlIndexBuilderColumns<SqliteColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, name, new SqlIndexBuilderColumns<SqlColumnBuilder>( columns.Expressions ), isUnique, referencedColumns ) { }

    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );
    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );
    public new SqlitePrimaryKeyBuilder? PrimaryKey => ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.PrimaryKey );

    public new SqlIndexBuilderColumns<SqliteColumnBuilder> Columns =>
        new SqlIndexBuilderColumns<SqliteColumnBuilder>( base.Columns.Expressions );

    public new SqlObjectBuilderArray<SqliteColumnBuilder> ReferencedColumns =>
        base.ReferencedColumns.UnsafeReinterpretAs<SqliteColumnBuilder>();

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

    public new SqliteIndexBuilder MarkAsVirtual(bool enabled = true)
    {
        base.MarkAsVirtual( enabled );
        return this;
    }

    public new SqliteIndexBuilder SetFilter(SqlConditionNode? filter)
    {
        base.SetFilter( filter );
        return this;
    }
}
