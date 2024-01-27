﻿using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteForeignKey : SqliteConstraint, ISqlForeignKey
{
    internal SqliteForeignKey(SqliteIndex originIndex, SqliteIndex referencedIndex, SqliteForeignKeyBuilder builder)
        : base( originIndex.Table, builder )
    {
        OriginIndex = originIndex;
        ReferencedIndex = referencedIndex;
        OnUpdateBehavior = builder.OnUpdateBehavior;
        OnDeleteBehavior = builder.OnDeleteBehavior;
    }

    public SqliteIndex OriginIndex { get; }
    public SqliteIndex ReferencedIndex { get; }
    public ReferenceBehavior OnUpdateBehavior { get; }
    public ReferenceBehavior OnDeleteBehavior { get; }
    public override SqliteDatabase Database => OriginIndex.Database;

    ISqlIndex ISqlForeignKey.OriginIndex => OriginIndex;
    ISqlIndex ISqlForeignKey.ReferencedIndex => ReferencedIndex;
}
