using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlCheckBuilder : SqlCheckBuilder
{
    internal MySqlCheckBuilder(
        MySqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, name, condition, referencedColumns ) { }

    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );
    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );

    public new SqlObjectBuilderArray<MySqlColumnBuilder> ReferencedColumns =>
        base.ReferencedColumns.UnsafeReinterpretAs<MySqlColumnBuilder>();

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
}
