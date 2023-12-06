namespace LfrlAnvil.Sql.Statements.Compilers;

public readonly record struct SqlAsyncReaderInitResult(int[] Ordinals, SqlResultSetField[]? Fields);
