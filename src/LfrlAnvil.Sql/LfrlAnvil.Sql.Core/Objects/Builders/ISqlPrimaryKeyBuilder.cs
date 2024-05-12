namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL primary key constraint builder.
/// </summary>
public interface ISqlPrimaryKeyBuilder : ISqlConstraintBuilder
{
    /// <summary>
    /// Underlying index that defines this primary key.
    /// </summary>
    ISqlIndexBuilder Index { get; }

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    new ISqlPrimaryKeyBuilder SetName(string name);

    /// <inheritdoc cref="ISqlConstraintBuilder.SetDefaultName()" />
    new ISqlPrimaryKeyBuilder SetDefaultName();
}
