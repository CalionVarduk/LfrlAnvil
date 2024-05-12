using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents a collection of constraints for an <see cref="SqlCreateTableNode"/>.
/// </summary>
/// <param name="PrimaryKey">Primary key constraint.</param>
/// <param name="ForeignKeys">Collection of foreign key constraints.</param>
/// <param name="Checks">Collection of check constraints.</param>
public readonly record struct SqlCreateTableConstraints(
    SqlPrimaryKeyDefinitionNode? PrimaryKey,
    ReadOnlyArray<SqlForeignKeyDefinitionNode>? ForeignKeys,
    ReadOnlyArray<SqlCheckDefinitionNode>? Checks
)
{
    /// <summary>
    /// Represents an empty collection of constraints.
    /// </summary>
    public static readonly SqlCreateTableConstraints Empty = new SqlCreateTableConstraints();

    /// <summary>
    /// Creates a new <see cref="SqlCreateTableConstraints"/> instance with changed <see cref="PrimaryKey"/>.
    /// </summary>
    /// <param name="node">Primary key to set.</param>
    /// <returns>New <see cref="SqlCreateTableConstraints"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlCreateTableConstraints WithPrimaryKey(SqlPrimaryKeyDefinitionNode? node)
    {
        return new SqlCreateTableConstraints( node, ForeignKeys, Checks );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCreateTableConstraints"/> instance with changed <see cref="ForeignKeys"/>.
    /// </summary>
    /// <param name="nodes">Collection of foreign keys to set.</param>
    /// <returns>New <see cref="SqlCreateTableConstraints"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlCreateTableConstraints WithForeignKeys(params SqlForeignKeyDefinitionNode[] nodes)
    {
        return new SqlCreateTableConstraints( PrimaryKey, nodes, Checks );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCreateTableConstraints"/> instance with changed <see cref="Checks"/>.
    /// </summary>
    /// <param name="nodes">Collection of checks to set.</param>
    /// <returns>New <see cref="SqlCreateTableConstraints"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlCreateTableConstraints WithChecks(params SqlCheckDefinitionNode[] nodes)
    {
        return new SqlCreateTableConstraints( PrimaryKey, ForeignKeys, nodes );
    }
}
