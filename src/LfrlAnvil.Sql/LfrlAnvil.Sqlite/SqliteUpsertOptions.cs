using System;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Persistence;

namespace LfrlAnvil.Sqlite;

/// <summary>
/// Specifies available options for interpreting <see cref="SqlUpsertNode"/> instances.
/// </summary>
[Flags]
public enum SqliteUpsertOptions : byte
{
    /// <summary>
    /// Specifies that the upsert statement is not supported.
    /// </summary>
    /// <remarks>
    /// This setting will cause an interpreter to throw an <see cref="UnrecognizedSqlNodeException"/>
    /// whenever an <see cref="SqlUpsertNode"/> is visited.
    /// </remarks>
    Disabled = 0,

    /// <summary>
    /// Specifies that the upsert statement is supported.
    /// </summary>
    /// <remarks>
    /// Unless the <see cref="AllowEmptyConflictTarget"/> setting is also included,
    /// this setting requires that the <see cref="SqlUpsertNode.ConflictTarget"/> is either explicitly specified or
    /// is possible to be extracted from the target table.
    /// </remarks>
    Supported = 1,

    /// <summary>
    /// Specifies that the upsert statement is supported and that the <see cref="SqlUpsertNode.ConflictTarget"/> can be empty.
    /// </summary>
    AllowEmptyConflictTarget = 2
}
