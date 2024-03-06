using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteForeignKeyBuilder : SqlForeignKeyBuilder
{
    internal SqliteForeignKeyBuilder(SqliteIndexBuilder originIndex, SqliteIndexBuilder referencedIndex, string name)
        : base( originIndex, referencedIndex, name ) { }

    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );
    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Name )}";
    }

    public new SqliteForeignKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqliteForeignKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public new SqliteForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        base.SetOnDeleteBehavior( behavior );
        return this;
    }

    public new SqliteForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        base.SetOnUpdateBehavior( behavior );
        return this;
    }
}
