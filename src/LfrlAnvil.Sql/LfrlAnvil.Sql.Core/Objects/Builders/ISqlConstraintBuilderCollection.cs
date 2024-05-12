using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a collection of SQL table constraint builders.
/// </summary>
public interface ISqlConstraintBuilderCollection : IReadOnlyCollection<ISqlConstraintBuilder>
{
    /// <summary>
    /// Table that this collection belongs to.
    /// </summary>
    ISqlTableBuilder Table { get; }

    /// <summary>
    /// Checks whether or not a constraint with the provided <paramref name="name"/> exists.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns><b>true</b> when constraint exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(string name);

    /// <summary>
    /// Returns a constraint with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the constraint to return.</param>
    /// <returns>Existing constraint.</returns>
    /// <exception cref="KeyNotFoundException">When constraint does not exist.</exception>
    [Pure]
    ISqlConstraintBuilder Get(string name);

    /// <summary>
    /// Attempts to return a constraint with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the constraint to return.</param>
    /// <returns>Existing constraint or null when constraint does not exist.</returns>
    [Pure]
    ISqlConstraintBuilder? TryGet(string name);

    /// <summary>
    /// Retrieves a primary key builder.
    /// </summary>
    /// <returns>Existing primary key builder.</returns>
    /// <exception cref="SqlObjectBuilderException">When primary key builder does not exist.</exception>
    [Pure]
    ISqlPrimaryKeyBuilder GetPrimaryKey();

    /// <summary>
    /// Attempts to retrieve a primary key builder.
    /// </summary>
    /// <returns>Existing primary key builder or null when it does not exist.</returns>
    [Pure]
    ISqlPrimaryKeyBuilder? TryGetPrimaryKey();

    /// <summary>
    /// Returns an index with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the index to return.</param>
    /// <returns>Existing index.</returns>
    /// <exception cref="KeyNotFoundException">When index does not exist.</exception>
    /// <exception cref="SqlObjectCastException">When constraint exists but is not an index.</exception>
    [Pure]
    ISqlIndexBuilder GetIndex(string name);

    /// <summary>
    /// Attempts to return an index with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the index to return.</param>
    /// <returns>Existing index or null when index does not exist.</returns>
    [Pure]
    ISqlIndexBuilder? TryGetIndex(string name);

    /// <summary>
    /// Returns a foreign key with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the foreign key to return.</param>
    /// <returns>Existing foreign key.</returns>
    /// <exception cref="KeyNotFoundException">When foreign key does not exist.</exception>
    /// <exception cref="SqlObjectCastException">When constraint exists but is not a foreign key.</exception>
    [Pure]
    ISqlForeignKeyBuilder GetForeignKey(string name);

    /// <summary>
    /// Attempts to return a foreign key with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the foreign key to return.</param>
    /// <returns>Existing foreign key or null when foreign key does not exist.</returns>
    [Pure]
    ISqlForeignKeyBuilder? TryGetForeignKey(string name);

    /// <summary>
    /// Returns a check with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the check to return.</param>
    /// <returns>Existing check.</returns>
    /// <exception cref="KeyNotFoundException">When check does not exist.</exception>
    /// <exception cref="SqlObjectCastException">When constraint exists but is not a check.</exception>
    [Pure]
    ISqlCheckBuilder GetCheck(string name);

    /// <summary>
    /// Attempts to return a check with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the check to return.</param>
    /// <returns>Existing check or null when check does not exist.</returns>
    [Pure]
    ISqlCheckBuilder? TryGetCheck(string name);

    /// <summary>
    /// Sets a new primary key builder with a default name.
    /// </summary>
    /// <param name="index">Underlying index that defines the primary key.</param>
    /// <returns>New <see cref="ISqlPrimaryKeyBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When primary key constraint could not be created.</exception>
    ISqlPrimaryKeyBuilder SetPrimaryKey(ISqlIndexBuilder index);

    /// <summary>
    /// Sets a new primary key builder.
    /// </summary>
    /// <param name="name">Name of the primary key constraint.</param>
    /// <param name="index">Underlying index that defines the primary key.</param>
    /// <returns>New <see cref="ISqlPrimaryKeyBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When primary key constraint could not be created.</exception>
    ISqlPrimaryKeyBuilder SetPrimaryKey(string name, ISqlIndexBuilder index);

    /// <summary>
    /// Creates a new index builder with a default name.
    /// </summary>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <param name="isUnique">Specifies whether or not the index should start as unique. Equal to <b>false</b> by default.</param>
    /// <returns>New <see cref="ISqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    ISqlIndexBuilder CreateIndex(ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false);

    /// <summary>
    /// Creates a new index builder.
    /// </summary>
    /// <param name="name">Name of the index constraint.</param>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <param name="isUnique">Specifies whether or not the index should start as unique. Equal to <b>false</b> by default.</param>
    /// <returns>New <see cref="ISqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    ISqlIndexBuilder CreateIndex(string name, ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false);

    /// <summary>
    /// Creates a new foreign key builder with a default name.
    /// </summary>
    /// <param name="originIndex">SQL index that the foreign key originates from.</param>
    /// <param name="referencedIndex">SQL index referenced by the foreign key.</param>
    /// <returns>New <see cref="ISqlForeignKeyBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When foreign key constraint could not be created.</exception>
    ISqlForeignKeyBuilder CreateForeignKey(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex);

    /// <summary>
    /// Creates a new foreign key builder.
    /// </summary>
    /// <param name="name">Name of the foreign key constraint.</param>
    /// <param name="originIndex">SQL index that the foreign key originates from.</param>
    /// <param name="referencedIndex">SQL index referenced by the foreign key.</param>
    /// <returns>New <see cref="ISqlForeignKeyBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When foreign key constraint could not be created.</exception>
    ISqlForeignKeyBuilder CreateForeignKey(string name, ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex);

    /// <summary>
    /// Creates a new check builder with a default name.
    /// </summary>
    /// <param name="condition">Underlying condition of the check constraint.</param>
    /// <returns>New <see cref="ISqlCheckBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When check constraint could not be created.</exception>
    ISqlCheckBuilder CreateCheck(SqlConditionNode condition);

    /// <summary>
    /// Creates a new check builder.
    /// </summary>
    /// <param name="name">Name of the check constraint.</param>
    /// <param name="condition">Underlying condition of the check constraint.</param>
    /// <returns>New <see cref="ISqlCheckBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When check constraint could not be created.</exception>
    ISqlCheckBuilder CreateCheck(string name, SqlConditionNode condition);

    /// <summary>
    /// Attempts to remove a constraint by its <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the constraint to remove.</param>
    /// <returns><b>true</b> when constraint was removed, otherwise <b>false</b>.</returns>
    bool Remove(string name);
}
