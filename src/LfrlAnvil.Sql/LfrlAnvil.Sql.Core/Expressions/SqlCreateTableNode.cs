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
            ForeignKeys = constraints.ForeignKeys ?? ReadOnlyArray<SqlForeignKeyDefinitionNode>.Empty;
            Checks = constraints.Checks ?? ReadOnlyArray<SqlCheckDefinitionNode>.Empty;
        }
        else
        {
            PrimaryKey = null;
            ForeignKeys = ReadOnlyArray<SqlForeignKeyDefinitionNode>.Empty;
            Checks = ReadOnlyArray<SqlCheckDefinitionNode>.Empty;
        }
    }

    public SqlRecordSetInfo Info { get; }
    public bool IfNotExists { get; }
    public ReadOnlyArray<SqlColumnDefinitionNode> Columns { get; }
    public SqlPrimaryKeyDefinitionNode? PrimaryKey { get; }
    public ReadOnlyArray<SqlForeignKeyDefinitionNode> ForeignKeys { get; }
    public ReadOnlyArray<SqlCheckDefinitionNode> Checks { get; }
    public SqlNewTableNode RecordSet { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
