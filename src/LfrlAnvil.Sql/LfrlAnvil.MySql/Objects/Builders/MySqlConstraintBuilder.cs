using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public abstract class MySqlConstraintBuilder : MySqlObjectBuilder, ISqlConstraintBuilder
{
    protected MySqlConstraintBuilder(MySqlTableBuilder table, string name, SqlObjectType type)
        : base( table.Database.GetNextId(), name, type )
    {
        Table = table;
    }

    public MySqlTableBuilder Table { get; }
    ISqlTableBuilder ISqlConstraintBuilder.Table => Table;

    public MySqlConstraintBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    public MySqlConstraintBuilder SetDefaultName()
    {
        return SetName( GetDefaultName() );
    }

    [Pure]
    protected abstract string GetDefaultName();

    internal abstract void ResetFullName();
    internal abstract void MarkAsRemoved();

    ISqlConstraintBuilder ISqlConstraintBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlConstraintBuilder ISqlConstraintBuilder.SetDefaultName()
    {
        return SetName( GetDefaultName() );
    }
}
