namespace LfrlAnvil.Sql.Internal;

public readonly record struct SqlConnectionStringEntry(string Key, object Value, bool IsMutable);
