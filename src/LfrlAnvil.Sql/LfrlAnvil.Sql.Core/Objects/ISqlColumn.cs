﻿using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlColumn : ISqlObject
{
    ISqlTable Table { get; }
    ISqlColumnTypeDefinition TypeDefinition { get; }
    bool IsNullable { get; }
    bool HasDefaultValue { get; }
    SqlColumnNode Node { get; }

    [Pure]
    SqlOrderByNode Asc();

    [Pure]
    SqlOrderByNode Desc();
}
