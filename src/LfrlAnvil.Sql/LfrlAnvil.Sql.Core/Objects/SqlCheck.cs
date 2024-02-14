using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlCheck : SqlConstraint, ISqlCheck
{
    protected SqlCheck(SqlTable table, SqlCheckBuilder builder)
        : base( table, builder ) { }
}
