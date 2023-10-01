﻿using System;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlIndex : ISqlObject
{
    ISqlTable Table { get; }
    ReadOnlyMemory<ISqlIndexColumn> Columns { get; }
    bool IsUnique { get; }
    bool IsPartial { get; }
}
