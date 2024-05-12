using System;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Exceptions;

/// <summary>
/// Represents an error that occurred due to an <see cref="ISqlNodeVisitor"/> instance encountering an unrecognized type of node.
/// </summary>
public class UnrecognizedSqlNodeException : NotSupportedException
{
    /// <summary>
    /// Creates a new <see cref="UnrecognizedSqlNodeException"/> instance.
    /// </summary>
    /// <param name="visitor">SQL node visitor that failed.</param>
    /// <param name="node">Unrecognized node.</param>
    public UnrecognizedSqlNodeException(ISqlNodeVisitor visitor, SqlNodeBase node)
        : base( ExceptionResources.UnrecognizedSqlNode( visitor.GetType(), node ) )
    {
        Visitor = visitor;
        Node = node;
    }

    /// <summary>
    /// SQL node visitor that failed.
    /// </summary>
    public ISqlNodeVisitor Visitor { get; }

    /// <summary>
    /// Unrecognized node.
    /// </summary>
    public SqlNodeBase Node { get; }
}
