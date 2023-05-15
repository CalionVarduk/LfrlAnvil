namespace LfrlAnvil.Sql.Objects;

public interface ISqlColumn : ISqlObject
{
    ISqlTable Table { get; }
    ISqlColumnTypeDefinition TypeDefinition { get; }
    bool IsNullable { get; }
    object? DefaultValue { get; }
}
