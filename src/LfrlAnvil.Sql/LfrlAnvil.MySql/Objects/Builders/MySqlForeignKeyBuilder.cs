using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlForeignKeyBuilder : SqlForeignKeyBuilder
{
    internal MySqlForeignKeyBuilder(MySqlIndexBuilder originIndex, MySqlIndexBuilder referencedIndex, string name)
        : base( originIndex, referencedIndex, name ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlConstraintBuilder.Table" />
    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetName(string)" />
    public new MySqlForeignKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetDefaultName()" />
    public new MySqlForeignKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetOnDeleteBehavior(ReferenceBehavior)" />
    public new MySqlForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        base.SetOnDeleteBehavior( behavior );
        return this;
    }

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetOnUpdateBehavior(ReferenceBehavior)" />
    public new MySqlForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        base.SetOnUpdateBehavior( behavior );
        return this;
    }
}
