namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlAddColumnNode : SqlNodeBase
{
    internal SqlAddColumnNode(SqlRecordSetInfo table, SqlColumnDefinitionNode definition)
        : base( SqlNodeType.AddColumn )
    {
        Table = table;
        Definition = definition;
    }

    public SqlRecordSetInfo Table { get; }
    public SqlColumnDefinitionNode Definition { get; }
}
