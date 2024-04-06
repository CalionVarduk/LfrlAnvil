using System;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Exceptions;

public class SqlNodeMutatorException : InvalidOperationException
{
    public SqlNodeMutatorException(SqlNodeBase parent, SqlNodeBase node, SqlNodeBase result, Type expectedType, string description)
        : base( ExceptionResources.InvalidNodeMutatorResult( parent, node, result, expectedType, description ) )
    {
        Parent = parent;
        Node = node;
        Result = result;
        ExpectedType = expectedType;
    }

    public SqlNodeBase Parent { get; }
    public SqlNodeBase Node { get; }
    public SqlNodeBase Result { get; }
    public Type ExpectedType { get; }
}
