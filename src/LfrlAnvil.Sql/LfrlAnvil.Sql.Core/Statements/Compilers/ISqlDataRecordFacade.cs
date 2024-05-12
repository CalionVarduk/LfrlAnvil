using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents a facade over an <see cref="IDataRecord"/> instance.
/// </summary>
/// <typeparam name="TDataRecord">DB data record type.</typeparam>
/// <remarks>
/// Do not use this interface directly. It should only be used in query reader creation options, for custom value reading expressions.
/// See <see cref="SqlQueryMemberConfiguration"/> for more information.
/// </remarks>
public interface ISqlDataRecordFacade<out TDataRecord>
    where TDataRecord : IDataRecord
{
    /// <summary>
    /// Underlying DB data record instance. Usage of this property will be included in a query reader as-is, without any translation.
    /// </summary>
    TDataRecord Record { get; }

    /// <summary>
    /// Represents reading of a generic non-null value by field name from the <see cref="Record"/>.
    /// </summary>
    /// <param name="name">Field name.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Read value.</returns>
    /// <remarks>
    /// Usage of this method will be translated so that the field name will be replaced with an equivalent cached ordinal.
    /// </remarks>
    [Pure]
    T Get<T>(string name)
        where T : notnull;

    /// <summary>
    /// Represents reading of a generic nullable value by field name from the <see cref="Record"/>.
    /// </summary>
    /// <param name="name">Field name.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Read value.</returns>
    /// <remarks>
    /// Usage of this method will be translated so that the field name will be replaced with an equivalent cached ordinal.
    /// </remarks>
    [Pure]
    T? GetNullable<T>(string name);

    /// <summary>
    /// Represents reading of a generic nullable value by field name from the <see cref="Record"/>.
    /// </summary>
    /// <param name="name">Field name.</param>
    /// <param name="default">Default value to return instead of null.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Read value or <paramref name="default"/> when read value is null.</returns>
    /// <remarks>
    /// Usage of this method will be translated so that the field name will be replaced with an equivalent cached ordinal.
    /// </remarks>
    [Pure]
    T GetNullable<T>(string name, T @default)
        where T : notnull;

    /// <summary>
    /// Checks whether or not a value associated with the provided field name is null in the <see cref="Record"/>.
    /// </summary>
    /// <param name="name">Field name.</param>
    /// <returns><b>true</b> when value is null, otherwise <b>false</b>.</returns>
    /// <remarks>
    /// Usage of this method will be translated so that the field name will be replaced with an equivalent cached ordinal.
    /// </remarks>
    [Pure]
    bool IsNull(string name);

    /// <summary>
    /// Returns an ordinal value associated with the provided field name in the <see cref="Record"/>.
    /// </summary>
    /// <param name="name">Field name.</param>
    /// <returns>Ordinal value associated with the provided field name.</returns>
    /// <remarks>
    /// Usage of this method will be translated so that the cached ordinal is returned.
    /// </remarks>
    [Pure]
    int GetOrdinal(string name);
}
