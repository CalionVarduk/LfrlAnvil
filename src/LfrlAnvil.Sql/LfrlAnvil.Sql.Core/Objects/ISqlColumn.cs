using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlColumn : ISqlObject
{
    ISqlTable Table { get; }
    ISqlColumnTypeDefinition TypeDefinition { get; }
    bool IsNullable { get; }
    bool HasDefaultValue { get; }
    SqlColumnNode Node { get; }

    [Pure]
    SqlIndexColumn<ISqlColumn> Asc();

    [Pure]
    SqlIndexColumn<ISqlColumn> Desc();
}
