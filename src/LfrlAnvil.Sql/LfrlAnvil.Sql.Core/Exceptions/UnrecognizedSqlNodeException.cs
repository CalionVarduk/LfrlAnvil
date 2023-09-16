using System;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Exceptions;

public class UnrecognizedSqlNodeException : NotSupportedException
{
    public UnrecognizedSqlNodeException(ISqlNodeVisitor visitor, SqlNodeBase node)
        : base( ExceptionResources.UnrecognizedSqlNode( visitor.GetType(), node ) )
    {
        Visitor = visitor;
        Node = node;
    }

    public ISqlNodeVisitor Visitor { get; }
    public SqlNodeBase Node { get; }
}
