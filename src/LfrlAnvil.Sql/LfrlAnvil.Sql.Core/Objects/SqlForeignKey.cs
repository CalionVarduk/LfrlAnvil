using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlForeignKey : SqlConstraint, ISqlForeignKey
{
    protected SqlForeignKey(SqlIndex originIndex, SqlIndex referencedIndex, SqlForeignKeyBuilder builder)
        : base( originIndex.Table, builder )
    {
        OriginIndex = originIndex;
        ReferencedIndex = referencedIndex;
        OnDeleteBehavior = builder.OnDeleteBehavior;
        OnUpdateBehavior = builder.OnUpdateBehavior;
    }

    public SqlIndex OriginIndex { get; }
    public SqlIndex ReferencedIndex { get; }
    public ReferenceBehavior OnDeleteBehavior { get; }
    public ReferenceBehavior OnUpdateBehavior { get; }

    ISqlIndex ISqlForeignKey.OriginIndex => OriginIndex;
    ISqlIndex ISqlForeignKey.ReferencedIndex => ReferencedIndex;
}
