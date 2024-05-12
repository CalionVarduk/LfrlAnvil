namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL schema builder.
/// </summary>
public interface ISqlSchemaBuilder : ISqlObjectBuilder
{
    /// <summary>
    /// Collection of objects that belong to this schema.
    /// </summary>
    ISqlObjectBuilderCollection Objects { get; }

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    new ISqlSchemaBuilder SetName(string name);
}
