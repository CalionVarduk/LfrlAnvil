using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlCheck" />
public abstract class SqlCheck : SqlConstraint, ISqlCheck
{
    /// <summary>
    /// Creates a new <see cref="SqlCheck"/> instance.
    /// </summary>
    /// <param name="table">Table that this check constraint is attached to.</param>
    /// <param name="builder">Source builder.</param>
    protected SqlCheck(SqlTable table, SqlCheckBuilder builder)
        : base( table, builder ) { }
}
