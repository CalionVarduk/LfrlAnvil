using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteSchemaBuilder : SqlSchemaBuilder
{
    internal SqliteSchemaBuilder(SqliteDatabaseBuilder database, string name)
        : base( database, name, new SqliteObjectBuilderCollection() ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlSchemaBuilder.Objects" />
    public new SqliteObjectBuilderCollection Objects => ReinterpretCast.To<SqliteObjectBuilderCollection>( base.Objects );

    /// <inheritdoc cref="SqlSchemaBuilder.SetName(string)" />
    public new SqliteSchemaBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc />
    protected override void BeforeRemove()
    {
        ThrowIfDefault();
        ThrowIfReferenced();
        RemoveFromCollection( Database.Schemas, this );
    }

    /// <inheritdoc />
    protected override void AfterRemove()
    {
        base.AfterRemove();
        QuickRemoveObjects();
    }
}
