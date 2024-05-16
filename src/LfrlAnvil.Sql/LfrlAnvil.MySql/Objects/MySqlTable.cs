using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlTable : SqlTable
{
    internal MySqlTable(MySqlSchema schema, MySqlTableBuilder builder)
        : base( schema, builder, new MySqlColumnCollection( builder.Columns ), new MySqlConstraintCollection( builder.Constraints ) ) { }

    /// <inheritdoc cref="SqlTable.Columns" />
    public new MySqlColumnCollection Columns => ReinterpretCast.To<MySqlColumnCollection>( base.Columns );

    /// <inheritdoc cref="SqlTable.Constraints" />
    public new MySqlConstraintCollection Constraints => ReinterpretCast.To<MySqlConstraintCollection>( base.Constraints );

    /// <inheritdoc cref="SqlTable.Schema" />
    public new MySqlSchema Schema => ReinterpretCast.To<MySqlSchema>( base.Schema );

    /// <inheritdoc cref="SqlObject.Database" />
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
