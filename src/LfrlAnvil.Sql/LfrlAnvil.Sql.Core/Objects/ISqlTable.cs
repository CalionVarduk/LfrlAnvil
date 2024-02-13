using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlTable : ISqlObject
{
    ISqlSchema Schema { get; }
    ISqlColumnCollection Columns { get; }
    ISqlConstraintCollection Constraints { get; }
    SqlRecordSetInfo Info { get; }
    SqlTableNode Node { get; }
}
