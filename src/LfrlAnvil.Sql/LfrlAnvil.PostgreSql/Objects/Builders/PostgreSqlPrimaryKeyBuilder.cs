using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlPrimaryKeyBuilder : SqlPrimaryKeyBuilder
{
    internal PostgreSqlPrimaryKeyBuilder(PostgreSqlIndexBuilder index, string name)
        : base( index, name ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlConstraintBuilder.Table" />
    public new PostgreSqlTableBuilder Table => ReinterpretCast.To<PostgreSqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlPrimaryKeyBuilder.Index" />
    public new PostgreSqlIndexBuilder Index => ReinterpretCast.To<PostgreSqlIndexBuilder>( base.Index );

    /// <inheritdoc cref="SqlPrimaryKeyBuilder.SetName(string)" />
    public new PostgreSqlPrimaryKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlPrimaryKeyBuilder.SetDefaultName()" />
    public new PostgreSqlPrimaryKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }
}
