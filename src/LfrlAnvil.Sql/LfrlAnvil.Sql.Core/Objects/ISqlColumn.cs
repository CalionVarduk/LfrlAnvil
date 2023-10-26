using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlColumn : ISqlObject
{
    ISqlTable Table { get; }
    ISqlColumnTypeDefinition TypeDefinition { get; }
    bool IsNullable { get; }
    SqlColumnNode Node { get; }

    [Pure]
    ISqlIndexColumn Asc();

    [Pure]
    ISqlIndexColumn Desc();
}
