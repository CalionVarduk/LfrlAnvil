namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRenameTableNode : SqlNodeBase
{
    internal SqlRenameTableNode(SqlRecordSetInfo table, SqlSchemaObjectName newName)
        : base( SqlNodeType.RenameTable )
    {
        Table = table;
        NewName = newName;
    }

    public SqlRecordSetInfo Table { get; }
    public SqlSchemaObjectName NewName { get; }
}
