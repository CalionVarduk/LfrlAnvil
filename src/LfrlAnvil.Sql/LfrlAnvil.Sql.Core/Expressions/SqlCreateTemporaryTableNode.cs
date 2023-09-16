using System;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCreateTemporaryTableNode : SqlNodeBase
{
    internal SqlCreateTemporaryTableNode(string name, SqlColumnDefinitionNode[] columns)
        : base( SqlNodeType.CreateTemporaryTable )
    {
        Name = name;
        Columns = columns;
    }

    public string Name { get; }
    public ReadOnlyMemory<SqlColumnDefinitionNode> Columns { get; }
}
