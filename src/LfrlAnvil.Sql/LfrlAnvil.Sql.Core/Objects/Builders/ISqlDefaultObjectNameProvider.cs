using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a provider of default SQL object names.
/// </summary>
public interface ISqlDefaultObjectNameProvider
{
    /// <summary>
    /// Creates a default primary key constraint name.
    /// </summary>
    /// <param name="table"><see cref="ISqlTableBuilder"/> that the primary key belongs to.</param>
    /// <returns>Default primary key constraint name.</returns>
    [Pure]
    string GetForPrimaryKey(ISqlTableBuilder table);

    /// <summary>
    /// Creates a default foreign key constraint name.
    /// </summary>
    /// <param name="originIndex"><see cref="ISqlIndexBuilder"/> from which the foreign key originates.</param>
    /// <param name="referencedIndex"><see cref="ISqlIndexBuilder"/> which the foreign key references.</param>
    /// <returns>Default foreign key constraint name.</returns>
    [Pure]
    string GetForForeignKey(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex);

    /// <summary>
    /// Creates a default check constraint name.
    /// </summary>
    /// <param name="table"><see cref="ISqlTableBuilder"/> that the check belongs to.</param>
    /// <returns>Default check constraint name.</returns>
    [Pure]
    string GetForCheck(ISqlTableBuilder table);

    /// <summary>
    /// Creates a default index constraint name.
    /// </summary>
    /// <param name="table"><see cref="ISqlTableBuilder"/> that the index belongs to.</param>
    /// <param name="columns">Collection of columns that belong to the index.</param>
    /// <param name="isUnique">Specifies whether or not the index is unique.</param>
    /// <returns>Default index constraint name.</returns>
    [Pure]
    string GetForIndex(ISqlTableBuilder table, SqlIndexBuilderColumns<ISqlColumnBuilder> columns, bool isUnique);
}
