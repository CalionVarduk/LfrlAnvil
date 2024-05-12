using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a collection of SQL table column builders.
/// </summary>
public interface ISqlColumnBuilderCollection : IReadOnlyCollection<ISqlColumnBuilder>
{
    /// <summary>
    /// Table that this collection belongs to.
    /// </summary>
    ISqlTableBuilder Table { get; }

    /// <summary>
    /// Specifies the default <see cref="ISqlColumnTypeDefinition"/> of newly created columns.
    /// </summary>
    ISqlColumnTypeDefinition DefaultTypeDefinition { get; }

    /// <summary>
    /// Checks whether or not a column with the provided <paramref name="name"/> exists.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns><b>true</b> when column exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(string name);

    /// <summary>
    /// Returns a column with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the column to return.</param>
    /// <returns>Existing column.</returns>
    /// <exception cref="KeyNotFoundException">When column does not exist.</exception>
    [Pure]
    ISqlColumnBuilder Get(string name);

    /// <summary>
    /// Attempts to return a column with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the column to return.</param>
    /// <returns>Existing column or null when column does not exist.</returns>
    [Pure]
    ISqlColumnBuilder? TryGet(string name);

    /// <summary>
    /// Changes <see cref="DefaultTypeDefinition"/> value of this collection.
    /// </summary>
    /// <param name="definition">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When <paramref name="definition"/> is not valid.</exception>
    ISqlColumnBuilderCollection SetDefaultTypeDefinition(ISqlColumnTypeDefinition definition);

    /// <summary>
    /// Creates a new column builder.
    /// </summary>
    /// <param name="name">Name of the column.</param>
    /// <returns>New <see cref="ISqlColumnBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When column could not be created.</exception>
    ISqlColumnBuilder Create(string name);

    /// <summary>
    /// Creates a new column builder or returns an existing column builder.
    /// </summary>
    /// <param name="name">Name of the column.</param>
    /// <returns>New <see cref="ISqlColumnBuilder"/> instance or an existing column builder.</returns>
    /// <exception cref="SqlObjectBuilderException">When column does not exist and could not be created.</exception>
    ISqlColumnBuilder GetOrCreate(string name);

    /// <summary>
    /// Attempts to remove a column by its <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the column to remove.</param>
    /// <returns><b>true</b> when column was removed, otherwise <b>false</b>.</returns>
    bool Remove(string name);
}
