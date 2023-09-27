namespace LfrlAnvil.Sql.Expressions;

internal interface ISqlSelectNodeExpressionVisitor
{
    void Handle(string name, SqlExpressionNode? expression);
}
