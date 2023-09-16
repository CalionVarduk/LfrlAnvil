using System.Data;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlBeginTransactionNode : SqlNodeBase
{
    internal SqlBeginTransactionNode(IsolationLevel isolationLevel)
        : base( SqlNodeType.BeginTransaction )
    {
        IsolationLevel = isolationLevel;
    }

    public IsolationLevel IsolationLevel { get; }
}
