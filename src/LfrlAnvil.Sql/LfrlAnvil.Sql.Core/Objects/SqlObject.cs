using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlObject" />
public abstract class SqlObject : ISqlObject
{
    /// <summary>
    /// Creates a new <see cref="SqlObject"/> instance.
    /// </summary>
    /// <param name="database">Database that this object belongs to.</param>
    /// <param name="builder">Source builder.</param>
    protected SqlObject(SqlDatabase database, SqlObjectBuilder builder)
        : this( database, builder.Type, builder.Name ) { }

    /// <summary>
    /// Creates a new <see cref="SqlObject"/> instance.
    /// </summary>
    /// <param name="database">Database that this object belongs to.</param>
    /// <param name="type">Object's type.</param>
    /// <param name="name">Object's name.</param>
    protected SqlObject(SqlDatabase database, SqlObjectType type, string name)
    {
        Assume.IsDefined( type );
        Database = database;
        Type = type;
        Name = name;
    }

    /// <inheritdoc cref="ISqlObject.Database" />
    public SqlDatabase Database { get; }

    /// <inheritdoc />
    public SqlObjectType Type { get; }

    /// <inheritdoc />
    public string Name { get; }

    ISqlDatabase ISqlObject.Database => Database;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlObject"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {Name}";
    }
}
