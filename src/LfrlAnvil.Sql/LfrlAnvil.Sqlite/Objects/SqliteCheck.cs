﻿using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteCheck : SqliteConstraint, ISqlCheck
{
    internal SqliteCheck(SqliteTable table, SqliteCheckBuilder builder)
        : base( table, builder ) { }

    public override SqliteDatabase Database => Table.Database;
}