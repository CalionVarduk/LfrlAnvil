using System;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlStatementBatchNode : SqlNodeBase
{
    internal SqlStatementBatchNode(SqlNodeBase[] statements)
        : base( SqlNodeType.StatementBatch )
    {
        Statements = statements;
    }

    public ReadOnlyMemory<SqlNodeBase> Statements { get; }
}
