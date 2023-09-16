namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDropTemporaryTableNode : SqlNodeBase
{
    internal SqlDropTemporaryTableNode(string name)
        : base( SqlNodeType.DropTemporaryTable )
    {
        Name = name;
    }

    public string Name { get; }
}
