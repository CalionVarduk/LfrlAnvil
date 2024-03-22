﻿using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlQueryDataFieldNode : SqlDataFieldNode
{
    internal SqlQueryDataFieldNode(SqlRecordSetNode recordSet, string name, SqlSelectNode selection, SqlExpressionNode? expression)
        : base( recordSet, SqlNodeType.QueryDataField )
    {
        Name = name;
        Selection = selection;
        Expression = expression;
    }

    public SqlSelectNode Selection { get; }
    public SqlExpressionNode? Expression { get; }
    public override string Name { get; }

    [Pure]
    public override SqlQueryDataFieldNode ReplaceRecordSet(SqlRecordSetNode recordSet)
    {
        return new SqlQueryDataFieldNode( recordSet, Name, Selection, Expression );
    }
}
