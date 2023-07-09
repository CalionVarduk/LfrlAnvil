﻿namespace LfrlAnvil.Sql.Expressions;

public abstract class SqlSelectNode : SqlNodeBase
{
    internal SqlSelectNode(SqlNodeType nodeType)
        : base( nodeType ) { }

    internal abstract void Convert(ISqlSelectNodeConverter converter);
}
