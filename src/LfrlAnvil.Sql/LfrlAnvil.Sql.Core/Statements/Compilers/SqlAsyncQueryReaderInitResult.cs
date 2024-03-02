namespace LfrlAnvil.Sql.Statements.Compilers;

public readonly record struct SqlAsyncQueryReaderInitResult(int[] Ordinals, SqlResultSetField[]? Fields);
