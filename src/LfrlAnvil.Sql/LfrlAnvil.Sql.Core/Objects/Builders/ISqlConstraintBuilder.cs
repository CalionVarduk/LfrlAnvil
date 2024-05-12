using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL constraint builder attached to a table.
/// </summary>
public interface ISqlConstraintBuilder : ISqlObjectBuilder
{
    /// <summary>
    /// Table that this constraint is attached to.
    /// </summary>
    ISqlTableBuilder Table { get; }

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    new ISqlConstraintBuilder SetName(string name);

    /// <summary>
    /// Changes the name of this object to a default name.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When name cannot be changed.</exception>
    /// <remarks>See <see cref="ISqlDefaultObjectNameProvider"/> for more information.</remarks>
    ISqlConstraintBuilder SetDefaultName();
}
