using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteSchemaBuilder : SqlSchemaBuilder
{
    internal SqliteSchemaBuilder(SqliteDatabaseBuilder database, string name)
        : base( database, name, new SqliteObjectBuilderCollection() ) { }

    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );
    public new SqliteObjectBuilderCollection Objects => ReinterpretCast.To<SqliteObjectBuilderCollection>( base.Objects );

    public new SqliteSchemaBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    protected override void BeforeRemove()
    {
        ThrowIfDefault();
        ThrowIfReferenced();
        RemoveFromCollection( Database.Schemas, this );
    }

    protected override void AfterRemove()
    {
        base.AfterRemove();
        QuickRemoveObjects();
    }
}
