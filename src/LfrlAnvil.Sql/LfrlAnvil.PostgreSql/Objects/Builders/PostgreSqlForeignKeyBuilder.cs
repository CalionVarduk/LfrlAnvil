using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlForeignKeyBuilder : SqlForeignKeyBuilder
{
    internal PostgreSqlForeignKeyBuilder(PostgreSqlIndexBuilder originIndex, PostgreSqlIndexBuilder referencedIndex, string name)
        : base( originIndex, referencedIndex, name ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlConstraintBuilder.Table" />
    public new PostgreSqlTableBuilder Table => ReinterpretCast.To<PostgreSqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetName(string)" />
    public new PostgreSqlForeignKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetDefaultName()" />
    public new PostgreSqlForeignKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetOnDeleteBehavior(ReferenceBehavior)" />
    public new PostgreSqlForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        base.SetOnDeleteBehavior( behavior );
        return this;
    }

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetOnUpdateBehavior(ReferenceBehavior)" />
    public new PostgreSqlForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        base.SetOnUpdateBehavior( behavior );
        return this;
    }
}
