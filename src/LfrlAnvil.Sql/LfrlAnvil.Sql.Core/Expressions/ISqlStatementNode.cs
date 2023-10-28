namespace LfrlAnvil.Sql.Expressions;

public interface ISqlStatementNode
{
    public SqlNodeBase Node { get; }
    public int QueryCount { get; }
}
