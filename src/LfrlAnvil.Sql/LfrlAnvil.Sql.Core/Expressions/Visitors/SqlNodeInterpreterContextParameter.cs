namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents a single SQL parameter registered in an <see cref="SqlNodeInterpreterContext"/>.
/// </summary>
/// <param name="Name">Parameter's name.</param>
/// <param name="Type">Optional runtime type of this parameter.</param>
/// <param name="Index">Optional 0-based position of this parameter.</param>
public readonly record struct SqlNodeInterpreterContextParameter(string Name, TypeNullability? Type, int? Index);
