namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCommitTransactionNode : SqlNodeBase
{
    internal SqlCommitTransactionNode()
        : base( SqlNodeType.CommitTransaction ) { }
}
