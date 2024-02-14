using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlConstraint : SqlObject, ISqlConstraint
{
    protected SqlConstraint(SqlTable table, SqlConstraintBuilder builder)
        : base( table.Database, builder )
    {
        Table = table;
    }

    public SqlTable Table { get; }
    ISqlTable ISqlConstraint.Table => Table;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Table.Schema.Name, Name )}";
    }
}
