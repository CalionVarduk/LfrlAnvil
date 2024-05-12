using System.Data;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines a start of a DB transaction.
/// </summary>
public sealed class SqlBeginTransactionNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlBeginTransactionNode(IsolationLevel isolationLevel)
        : base( SqlNodeType.BeginTransaction )
    {
        IsolationLevel = isolationLevel;
    }

    /// <summary>
    /// Transaction's <see cref="System.Data.IsolationLevel"/>.
    /// </summary>
    public IsolationLevel IsolationLevel { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
