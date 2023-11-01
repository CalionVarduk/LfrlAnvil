using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlTable : ISqlObject
{
    ISqlSchema Schema { get; }
    ISqlPrimaryKey PrimaryKey { get; }
    ISqlColumnCollection Columns { get; }
    ISqlIndexCollection Indexes { get; }
    ISqlForeignKeyCollection ForeignKeys { get; }
    ISqlCheckCollection Checks { get; }
    SqlRecordSetInfo Info { get; }
    SqlTableNode RecordSet { get; }
}
