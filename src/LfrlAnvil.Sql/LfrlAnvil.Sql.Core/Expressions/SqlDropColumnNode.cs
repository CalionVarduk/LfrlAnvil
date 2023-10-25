namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDropColumnNode : SqlNodeBase
{
    internal SqlDropColumnNode(SqlRecordSetInfo table, string name)
        : base( SqlNodeType.DropColumn )
    {
        Table = table;
        Name = name;
    }

    public SqlRecordSetInfo Table { get; }
    public string Name { get; }
}
