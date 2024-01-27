using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public abstract class MySqlObject : ISqlObject
{
    protected MySqlObject(MySqlObjectBuilder builder)
        : this( builder.Name, builder.Type ) { }

    protected MySqlObject(string name, SqlObjectType type)
    {
        Name = name;
        Type = type;
    }

    public SqlObjectType Type { get; }
    public string Name { get; }
    public abstract MySqlDatabase Database { get; }

    ISqlDatabase ISqlObject.Database => Database;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {Name}";
    }
}
