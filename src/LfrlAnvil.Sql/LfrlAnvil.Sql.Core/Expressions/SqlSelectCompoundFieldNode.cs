using System;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlSelectCompoundFieldNode : SqlSelectNode
{
    internal SqlSelectCompoundFieldNode(string name, Origin[] origins)
        : base( SqlNodeType.SelectCompoundField )
    {
        Name = name;
        Origins = origins;
    }

    public string Name { get; }
    public ReadOnlyMemory<Origin> Origins { get; }

    internal override void Convert(ISqlSelectNodeConverter converter)
    {
        converter.Add( Name, null );
    }

    public readonly record struct Origin(int QueryIndex, SqlSelectNode Selection, SqlExpressionNode? Expression);
}
