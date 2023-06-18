﻿using System.Text;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlSelectAllNode : SqlSelectNode
{
    internal SqlSelectAllNode(SqlDataSourceNode dataSource)
        : base( SqlNodeType.SelectAll )
    {
        DataSource = dataSource;
    }

    public SqlDataSourceNode DataSource { get; }
    public override SqlExpressionType? Type => null;

    internal override void Convert(ISqlSelectNodeConverter converter)
    {
        foreach ( var recordSet in DataSource.RecordSets )
        {
            foreach ( var field in recordSet.GetKnownFields() )
                converter.Add( field.Name, field.Type );
        }
    }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( '*' );
    }
}
