using System;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCreateTableNode : SqlNodeBase
{
    internal SqlCreateTableNode(
        string schemaName,
        string name,
        bool ifNotExists,
        bool isTemporary,
        SqlColumnDefinitionNode[] columns,
        Func<SqlNewTableNode, SqlCreateTableConstraints>? constraintsProvider)
        : base( SqlNodeType.CreateTable )
    {
        SchemaName = schemaName;
        Name = name;
        IfNotExists = ifNotExists;
        IsTemporary = isTemporary;
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

    public string SchemaName { get; }
    public string Name { get; }
    public bool IfNotExists { get; }
    public bool IsTemporary { get; }
    public ReadOnlyMemory<SqlColumnDefinitionNode> Columns { get; }
    public SqlPrimaryKeyDefinitionNode? PrimaryKey { get; }
    public ReadOnlyMemory<SqlForeignKeyDefinitionNode> ForeignKeys { get; }
    public ReadOnlyMemory<SqlCheckDefinitionNode> Checks { get; }
    public SqlNewTableNode RecordSet { get; }
}
