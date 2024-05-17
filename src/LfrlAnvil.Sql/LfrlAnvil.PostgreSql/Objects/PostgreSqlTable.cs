using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlTable : SqlTable
{
    internal PostgreSqlTable(PostgreSqlSchema schema, PostgreSqlTableBuilder builder)
        : base(
            schema,
            builder,
            new PostgreSqlColumnCollection( builder.Columns ),
            new PostgreSqlConstraintCollection( builder.Constraints ) ) { }

    /// <inheritdoc cref="SqlTable.Columns" />
    public new PostgreSqlColumnCollection Columns => ReinterpretCast.To<PostgreSqlColumnCollection>( base.Columns );

    /// <inheritdoc cref="SqlTable.Constraints" />
    public new PostgreSqlConstraintCollection Constraints => ReinterpretCast.To<PostgreSqlConstraintCollection>( base.Constraints );

    /// <inheritdoc cref="SqlTable.Schema" />
    public new PostgreSqlSchema Schema => ReinterpretCast.To<PostgreSqlSchema>( base.Schema );

    /// <inheritdoc cref="SqlObject.Database" />
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
