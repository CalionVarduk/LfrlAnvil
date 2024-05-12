using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL foreign key constraint builder.
/// </summary>
public interface ISqlForeignKeyBuilder : ISqlConstraintBuilder
{
    /// <summary>
    /// SQL index referenced by this foreign key.
    /// </summary>
    ISqlIndexBuilder ReferencedIndex { get; }

    /// <summary>
    /// SQL index that this foreign key originates from.
    /// </summary>
    ISqlIndexBuilder OriginIndex { get; }

    /// <summary>
    /// Specifies this foreign key's on delete behavior.
    /// </summary>
    ReferenceBehavior OnDeleteBehavior { get; }

    /// <summary>
    /// Specifies this foreign key's on update behavior.
    /// </summary>
    ReferenceBehavior OnUpdateBehavior { get; }

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    new ISqlForeignKeyBuilder SetName(string name);

    /// <inheritdoc cref="ISqlConstraintBuilder.SetDefaultName()" />
    new ISqlForeignKeyBuilder SetDefaultName();

    /// <summary>
    /// Changes <see cref="OnDeleteBehavior"/> value of this foreign key.
    /// </summary>
    /// <param name="behavior">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When behavior cannot be changed.</exception>
    ISqlForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior);

    /// <summary>
    /// Changes <see cref="OnUpdateBehavior"/> value of this foreign key.
    /// </summary>
    /// <param name="behavior">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When behavior cannot be changed.</exception>
    ISqlForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior);
}
