using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlForeignKey" />
public abstract class SqlForeignKey : SqlConstraint, ISqlForeignKey
{
    /// <summary>
    /// Creates a new <see cref="SqlForeignKey"/> instance.
    /// </summary>
    /// <param name="originIndex">SQL index that this foreign key originates from.</param>
    /// <param name="referencedIndex">SQL index referenced by this foreign key.</param>
    /// <param name="builder">Source builder.</param>
    protected SqlForeignKey(SqlIndex originIndex, SqlIndex referencedIndex, SqlForeignKeyBuilder builder)
        : base( originIndex.Table, builder )
    {
        OriginIndex = originIndex;
        ReferencedIndex = referencedIndex;
        OnDeleteBehavior = builder.OnDeleteBehavior;
        OnUpdateBehavior = builder.OnUpdateBehavior;
    }

    /// <inheritdoc cref="ISqlForeignKey.OriginIndex" />
    public SqlIndex OriginIndex { get; }

    /// <inheritdoc cref="ISqlForeignKey.ReferencedIndex" />
    public SqlIndex ReferencedIndex { get; }

    /// <inheritdoc />
    public ReferenceBehavior OnDeleteBehavior { get; }

    /// <inheritdoc />
    public ReferenceBehavior OnUpdateBehavior { get; }

    ISqlIndex ISqlForeignKey.OriginIndex => OriginIndex;
    ISqlIndex ISqlForeignKey.ReferencedIndex => ReferencedIndex;
}
