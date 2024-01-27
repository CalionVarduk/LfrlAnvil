using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public abstract class MySqlConstraint : MySqlObject, ISqlConstraint
{
    protected MySqlConstraint(MySqlTable table, MySqlConstraintBuilder builder)
        : base( builder )
    {
        Table = table;
    }

    public MySqlTable Table { get; }
    ISqlTable ISqlConstraint.Table => Table;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {MySqlHelpers.GetFullName( Table.Schema.Name, Name )}";
    }
}
