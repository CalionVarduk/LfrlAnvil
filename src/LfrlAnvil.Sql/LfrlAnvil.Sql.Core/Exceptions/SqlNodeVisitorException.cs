using System;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Exceptions;

/// <summary>
/// Represents an error that occurred due to an <see cref="ISqlNodeVisitor"/> instance being unable to handle a node.
/// </summary>
public class SqlNodeVisitorException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqlNodeVisitorException"/> instance.
    /// </summary>
    /// <param name="reason">Description of a reason behind this error.</param>
    /// <param name="visitor">SQL node visitor that failed.</param>
    /// <param name="node">Node that caused the failure.</param>
    public SqlNodeVisitorException(string reason, ISqlNodeVisitor visitor, SqlNodeBase node)
        : base( ExceptionResources.FailedWhileVisitingNode( reason, visitor.GetType(), node ) )
    {
        Visitor = visitor;
        Node = node;
    }

    /// <summary>
    /// SQL node visitor that failed.
    /// </summary>
    public ISqlNodeVisitor Visitor { get; }

    /// <summary>
    /// Node that caused the failure.
    /// </summary>
    public SqlNodeBase Node { get; }
}
