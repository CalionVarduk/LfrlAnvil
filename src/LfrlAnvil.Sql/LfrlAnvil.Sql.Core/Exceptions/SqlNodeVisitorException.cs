using System;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Exceptions;

public class SqlNodeVisitorException : InvalidOperationException
{
    public SqlNodeVisitorException(string reason, ISqlNodeVisitor visitor, SqlNodeBase node)
        : base( ExceptionResources.FailedWhileVisitingNode( reason, visitor.GetType(), node ) )
    {
        Visitor = visitor;
        Node = node;
    }

    public ISqlNodeVisitor Visitor { get; }
    public SqlNodeBase Node { get; }
}
