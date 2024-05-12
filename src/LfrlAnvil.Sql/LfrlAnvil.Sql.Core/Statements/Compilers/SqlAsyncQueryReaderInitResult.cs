namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents a result of an action that prepares a compiled asynchronous query reader for reading rows.
/// </summary>
/// <param name="Ordinals">Collection of field ordinals.</param>
/// <param name="Fields">Collection of definitions of row fields.</param>
public readonly record struct SqlAsyncQueryReaderInitResult(int[] Ordinals, SqlResultSetField[]? Fields);
