using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlColumn : ISqlObject
{
    ISqlTable Table { get; }
    ISqlColumnTypeDefinition TypeDefinition { get; }
    bool IsNullable { get; }

    [Pure]
    ISqlIndexColumn Asc();

    [Pure]
    ISqlIndexColumn Desc();
}
