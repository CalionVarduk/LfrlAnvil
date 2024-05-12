namespace LfrlAnvil.Sql;

/// <summary>
/// Represents how an optional functionality should be handled.
/// </summary>
public enum SqlOptionalFunctionalityResolution : byte
{
    /// <summary>
    /// Specifies that the functionality should be ignored.
    /// </summary>
    Ignore = 0,

    /// <summary>
    /// Specifies that the functionality should be included.
    /// </summary>
    Include = 1,

    /// <summary>
    /// Specifies that the functionality should be forbidden and that using it may cause an exception to be thrown.
    /// </summary>
    Forbid = 2
}
