using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlSchema" />
public abstract class SqlSchema : SqlObject, ISqlSchema
{
    /// <summary>
    /// Creates a new <see cref="SqlSchema"/> instance.
    /// </summary>
    /// <param name="database">Database that this schema belongs to.</param>
    /// <param name="builder">Source builder.</param>
    /// <param name="objects">Collection of objects that belong to this schema.</param>
    protected SqlSchema(SqlDatabase database, SqlSchemaBuilder builder, SqlObjectCollection objects)
        : base( database, builder )
    {
        Objects = objects;
        Objects.SetSchema( this );
    }

    /// <inheritdoc cref="ISqlSchema.Objects" />
    public SqlObjectCollection Objects { get; }

    ISqlObjectCollection ISqlSchema.Objects => Objects;
}
