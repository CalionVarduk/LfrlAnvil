using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlObject : ISqlObject
{
    protected SqlObject(SqlDatabase database, SqlObjectBuilder builder)
        : this( database, builder.Type, builder.Name ) { }

    protected SqlObject(SqlDatabase database, SqlObjectType type, string name)
    {
        Assume.IsDefined( type );
        Database = database;
        Type = type;
        Name = name;
    }

    public SqlDatabase Database { get; }
    public SqlObjectType Type { get; }
    public string Name { get; }

    ISqlDatabase ISqlObject.Database => Database;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {Name}";
    }
}
