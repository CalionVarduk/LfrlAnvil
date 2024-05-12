using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlPrimaryKey" />
public abstract class SqlPrimaryKey : SqlConstraint, ISqlPrimaryKey
{
    /// <summary>
    /// Creates a new <see cref="SqlPrimaryKey"/> instance.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="builder">Source builder.</param>
    protected SqlPrimaryKey(SqlIndex index, SqlPrimaryKeyBuilder builder)
        : base( index.Table, builder )
    {
        Index = index;
    }

    /// <inheritdoc cref="ISqlPrimaryKey.Index" />
    public SqlIndex Index { get; }

    ISqlIndex ISqlPrimaryKey.Index => Index;
}
