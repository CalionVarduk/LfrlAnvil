using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqlitePrimaryKeyBuilder : SqlPrimaryKeyBuilder
{
    internal SqlitePrimaryKeyBuilder(SqliteIndexBuilder index, string name)
        : base( index, name ) { }

    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );
    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );
    public new SqliteIndexBuilder Index => ReinterpretCast.To<SqliteIndexBuilder>( base.Index );

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Name )}";
    }

    public new SqlitePrimaryKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqlitePrimaryKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }
}
