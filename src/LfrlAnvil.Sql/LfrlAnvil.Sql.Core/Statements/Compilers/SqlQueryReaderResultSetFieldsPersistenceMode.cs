namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents a strategy to use for extracting result set fields of a query.
/// </summary>
public enum SqlQueryReaderResultSetFieldsPersistenceMode : byte
{
    /// <summary>
    /// Specifies that result set fields should not be extracted at all.
    /// </summary>
    Ignore = 0,

    /// <summary>
    /// Specifies that result set fields should be extracted but without information about field types.
    /// </summary>
    Persist = 1,

    /// <summary>
    /// Specifies that result set fields should be extracted, including field types.
    /// </summary>
    PersistWithTypes = 2
}
