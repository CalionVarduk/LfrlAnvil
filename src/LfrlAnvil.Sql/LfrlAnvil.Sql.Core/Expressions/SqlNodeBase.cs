using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree node.
/// </summary>
public abstract class SqlNodeBase
{
    /// <summary>
    /// Creates a new <see cref="SqlNodeBase"/> instance of <see cref="SqlNodeType.Unknown"/> type.
    /// </summary>
    protected SqlNodeBase()
        : this( SqlNodeType.Unknown ) { }

    internal SqlNodeBase(SqlNodeType nodeType)
    {
        Assume.IsDefined( nodeType );
        NodeType = nodeType;
    }

    /// <summary>
    /// Specifies the type of this node.
    /// </summary>
    public SqlNodeType NodeType { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="SqlNodeBase"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public sealed override string ToString()
    {
        var interpreter = new SqlNodeDebugInterpreter();
        ToString( interpreter );
        return interpreter.Context.Sql.ToString();
    }

    /// <summary>
    /// Interpreters this node in order to create its string representation.
    /// </summary>
    /// <param name="interpreter">SQL node debug interpreter to use.</param>
    protected virtual void ToString(SqlNodeDebugInterpreter interpreter)
    {
        interpreter.Visit( this );
    }
}
