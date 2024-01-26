﻿using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public abstract class SqliteConstraint : SqliteObject, ISqlConstraint
{
    protected SqliteConstraint(SqliteTable table, SqliteConstraintBuilder builder)
        : base( builder )
    {
        Table = table;
    }

    public SqliteTable Table { get; }
    ISqlTable ISqlConstraint.Table => Table;
}
