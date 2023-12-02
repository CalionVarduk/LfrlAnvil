namespace LfrlAnvil.Sql.Tests.Helpers.Data;

public readonly record struct ResultSet(string[] FieldNames, object?[][] Rows);
