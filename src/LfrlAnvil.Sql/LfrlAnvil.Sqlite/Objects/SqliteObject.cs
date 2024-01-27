using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public abstract class SqliteObject : ISqlObject
{
    protected SqliteObject(SqliteObjectBuilder builder)
        : this( builder.Name, builder.Type ) { }

    protected SqliteObject(string name, SqlObjectType type)
    {
        Name = name;
        Type = type;
    }

    public SqlObjectType Type { get; }
    public string Name { get; }
    public abstract SqliteDatabase Database { get; }

    ISqlDatabase ISqlObject.Database => Database;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {Name}";
    }
}
