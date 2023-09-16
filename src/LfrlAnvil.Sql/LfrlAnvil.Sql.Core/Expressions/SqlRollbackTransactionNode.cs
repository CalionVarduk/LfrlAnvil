namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRollbackTransactionNode : SqlNodeBase
{
    internal SqlRollbackTransactionNode()
        : base( SqlNodeType.RollbackTransaction ) { }
}
