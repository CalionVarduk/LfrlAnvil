namespace LfrlAnvil.Sql.Expressions.Visitors;

public readonly record struct SqlNodeInterpreterContextParameter(string Name, TypeNullability? Type, int? Index);
