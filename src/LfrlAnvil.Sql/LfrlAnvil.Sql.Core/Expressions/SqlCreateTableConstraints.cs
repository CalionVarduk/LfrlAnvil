using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Expressions;

public readonly record struct SqlCreateTableConstraints(
    SqlPrimaryKeyDefinitionNode? PrimaryKey,
    ReadOnlyMemory<SqlForeignKeyDefinitionNode> ForeignKeys,
    ReadOnlyMemory<SqlCheckDefinitionNode> Checks)
{
    public static readonly SqlCreateTableConstraints Empty = new SqlCreateTableConstraints();

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlCreateTableConstraints WithPrimaryKey(SqlPrimaryKeyDefinitionNode node)
    {
        return new SqlCreateTableConstraints( node, ForeignKeys, Checks );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlCreateTableConstraints WithForeignKeys(params SqlForeignKeyDefinitionNode[] nodes)
    {
        return new SqlCreateTableConstraints( PrimaryKey, nodes, Checks );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlCreateTableConstraints WithChecks(params SqlCheckDefinitionNode[] nodes)
    {
        return new SqlCreateTableConstraints( PrimaryKey, ForeignKeys, nodes );
    }
}
