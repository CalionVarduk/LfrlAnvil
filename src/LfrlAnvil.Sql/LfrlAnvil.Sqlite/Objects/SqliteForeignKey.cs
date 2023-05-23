using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteForeignKey : SqliteObject, ISqlForeignKey
{
    internal SqliteForeignKey(SqliteIndex index, SqliteIndex referencedIndex, SqliteForeignKeyBuilder builder)
        : base( builder )
    {
        Index = index;
        ReferencedIndex = referencedIndex;
        OnUpdateBehavior = builder.OnUpdateBehavior;
        OnDeleteBehavior = builder.OnDeleteBehavior;
        FullName = builder.FullName;
    }

    public SqliteIndex Index { get; }
    public SqliteIndex ReferencedIndex { get; }
    public ReferenceBehavior OnUpdateBehavior { get; }
    public ReferenceBehavior OnDeleteBehavior { get; }
    public override string FullName { get; }
    public override SqliteDatabase Database => Index.Database;

    ISqlIndex ISqlForeignKey.Index => Index;
    ISqlIndex ISqlForeignKey.ReferencedIndex => ReferencedIndex;
}
