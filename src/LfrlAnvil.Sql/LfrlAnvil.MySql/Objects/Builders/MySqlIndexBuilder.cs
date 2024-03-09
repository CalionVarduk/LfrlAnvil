using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlIndexBuilder : SqlIndexBuilder
{
    internal MySqlIndexBuilder(
        MySqlTableBuilder table,
        string name,
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns,
        bool isUnique)
        : base( table, name, columns, isUnique ) { }

    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );
    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );
    public new MySqlPrimaryKeyBuilder? PrimaryKey => ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.PrimaryKey );
    public new SqlIndexColumnBuilderArray<MySqlColumnBuilder> Columns => base.Columns.UnsafeReinterpretAs<MySqlColumnBuilder>();

    public new SqlObjectBuilderArray<MySqlColumnBuilder> ReferencedFilterColumns =>
        base.ReferencedFilterColumns.UnsafeReinterpretAs<MySqlColumnBuilder>();

    public new MySqlIndexBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new MySqlIndexBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public new MySqlIndexBuilder MarkAsUnique(bool enabled = true)
    {
        base.MarkAsUnique( enabled );
        return this;
    }

    public new MySqlIndexBuilder MarkAsVirtual(bool enabled = true)
    {
        base.MarkAsVirtual( enabled );
        return this;
    }

    public new MySqlIndexBuilder SetFilter(SqlConditionNode? filter)
    {
        base.SetFilter( filter );
        return this;
    }
}
