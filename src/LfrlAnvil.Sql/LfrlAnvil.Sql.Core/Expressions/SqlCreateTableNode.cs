using System;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCreateTableNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlCreateTableNode(
        SqlRecordSetInfo info,
        bool ifNotExists,
        SqlColumnDefinitionNode[] columns,
        Func<SqlNewTableNode, SqlCreateTableConstraints>? constraintsProvider)
        : base( SqlNodeType.CreateTable )
    {
        Info = info;
        IfNotExists = ifNotExists;
        Columns = columns;
        RecordSet = new SqlNewTableNode( this, alias: null, isOptional: false );
        if ( constraintsProvider is not null )
        {
            var constraints = constraintsProvider( RecordSet );
            PrimaryKey = constraints.PrimaryKey;
            ForeignKeys = constraints.ForeignKeys;
            Checks = constraints.Checks;
        }
        else
        {
            PrimaryKey = null;
            ForeignKeys = ReadOnlyMemory<SqlForeignKeyDefinitionNode>.Empty;
            Checks = ReadOnlyMemory<SqlCheckDefinitionNode>.Empty;
        }
    }

    public SqlRecordSetInfo Info { get; }
    public bool IfNotExists { get; }
    public ReadOnlyMemory<SqlColumnDefinitionNode> Columns { get; }
    public SqlPrimaryKeyDefinitionNode? PrimaryKey { get; }
    public ReadOnlyMemory<SqlForeignKeyDefinitionNode> ForeignKeys { get; }
    public ReadOnlyMemory<SqlCheckDefinitionNode> Checks { get; }
    public SqlNewTableNode RecordSet { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
