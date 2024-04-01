using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlTable : SqlTable
{
    internal PostgreSqlTable(PostgreSqlSchema schema, PostgreSqlTableBuilder builder)
        : base( schema, builder, new PostgreSqlColumnCollection( builder.Columns ), new PostgreSqlConstraintCollection( builder.Constraints ) ) { }

    public new PostgreSqlColumnCollection Columns => ReinterpretCast.To<PostgreSqlColumnCollection>( base.Columns );
    public new PostgreSqlConstraintCollection Constraints => ReinterpretCast.To<PostgreSqlConstraintCollection>( base.Constraints );
    public new PostgreSqlSchema Schema => ReinterpretCast.To<PostgreSqlSchema>( base.Schema );
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
