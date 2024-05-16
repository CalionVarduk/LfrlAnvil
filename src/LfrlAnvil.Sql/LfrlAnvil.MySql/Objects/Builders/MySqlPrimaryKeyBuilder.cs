using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlPrimaryKeyBuilder : SqlPrimaryKeyBuilder
{
    internal MySqlPrimaryKeyBuilder(MySqlIndexBuilder index, string name)
        : base( index, name ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlConstraintBuilder.Table" />
    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlPrimaryKeyBuilder.Index" />
    public new MySqlIndexBuilder Index => ReinterpretCast.To<MySqlIndexBuilder>( base.Index );

    /// <inheritdoc cref="SqlPrimaryKeyBuilder.SetName(string)" />
    public new MySqlPrimaryKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlPrimaryKeyBuilder.SetDefaultName()" />
    public new MySqlPrimaryKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }
}
