using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public abstract class SqliteConstraintBuilder : SqliteObjectBuilder, ISqlConstraintBuilder
{
    protected SqliteConstraintBuilder(SqliteTableBuilder table, string name, SqlObjectType type)
        : base( table.Database.GetNextId(), name, type )
    {
        Table = table;
    }

    public SqliteTableBuilder Table { get; }
    ISqlTableBuilder ISqlConstraintBuilder.Table => Table;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Name )}";
    }

    public SqliteConstraintBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    public SqliteConstraintBuilder SetDefaultName()
    {
        return SetName( GetDefaultName() );
    }

    [Pure]
    protected abstract string GetDefaultName();

    ISqlConstraintBuilder ISqlConstraintBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlConstraintBuilder ISqlConstraintBuilder.SetDefaultName()
    {
        return SetName( GetDefaultName() );
    }
}
