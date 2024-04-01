using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

public sealed class PostgreSqlCheckBuilder : SqlCheckBuilder
{
    internal PostgreSqlCheckBuilder(
        PostgreSqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, name, condition, referencedColumns ) { }

    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );
    public new PostgreSqlTableBuilder Table => ReinterpretCast.To<PostgreSqlTableBuilder>( base.Table );

    public new SqlObjectBuilderArray<PostgreSqlColumnBuilder> ReferencedColumns =>
        base.ReferencedColumns.UnsafeReinterpretAs<PostgreSqlColumnBuilder>();

    public new PostgreSqlCheckBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new PostgreSqlCheckBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }
}
