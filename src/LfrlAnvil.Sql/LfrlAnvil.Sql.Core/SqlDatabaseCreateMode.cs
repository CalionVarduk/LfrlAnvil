namespace LfrlAnvil.Sql;

/// <summary>
/// Represents the mode with which to create <see cref="ISqlDatabase"/> instances.
/// </summary>
public enum SqlDatabaseCreateMode : byte
{
    /// <summary>
    /// Specifies that versions that haven't been applied to the database yet should not be invoked at all.
    /// </summary>
    NoChanges = 0,

    /// <summary>
    /// Specifies that versions that haven't been applied to the database yet should be ran in read-only mode.
    /// Such versions won't actually be applied to the database itself, but this mode can be useful for debugging
    /// created SQL statements that would be executed in <see cref="Commit"/> mode.
    /// </summary>
    DryRun = 1,

    /// <summary>
    /// Specifies that versions that haven't been applied to the database yet should be applied.
    /// </summary>
    Commit = 2
}
