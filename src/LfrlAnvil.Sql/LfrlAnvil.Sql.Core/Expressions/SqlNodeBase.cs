﻿using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Expressions;

public abstract class SqlNodeBase
{
    protected SqlNodeBase(SqlNodeType nodeType)
    {
        Assume.IsDefined( nodeType, nameof( nodeType ) );
        NodeType = nodeType;
    }

    public SqlNodeType NodeType { get; }

    [Pure]
    public sealed override string ToString()
    {
        var interpreter = new SqlNodeDebugInterpreter();
        ToString( interpreter );
        return interpreter.Context.Sql.ToString();
    }

    protected virtual void ToString(SqlNodeDebugInterpreter interpreter)
    {
        interpreter.Visit( this );
    }
}