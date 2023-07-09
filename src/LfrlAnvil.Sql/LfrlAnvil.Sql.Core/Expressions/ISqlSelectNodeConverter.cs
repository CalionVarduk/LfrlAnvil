namespace LfrlAnvil.Sql.Expressions;

internal interface ISqlSelectNodeConverter
{
    void Add(string name, SqlExpressionNode? expression);
}
