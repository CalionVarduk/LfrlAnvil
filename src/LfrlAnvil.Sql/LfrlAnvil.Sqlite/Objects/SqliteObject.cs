using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public abstract class SqliteObject : ISqlObject
{
    protected SqliteObject(SqliteObjectBuilder builder)
    {
        Name = builder.Name;
        Type = builder.Type;
    }

    public SqlObjectType Type { get; }
    public string Name { get; }
    public abstract string FullName { get; }
    public abstract SqliteDatabase Database { get; }

    ISqlDatabase ISqlObject.Database => Database;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {FullName}";
    }

    [Pure]
    public sealed override int GetHashCode()
    {
        return FullName.GetHashCode();
    }
}
