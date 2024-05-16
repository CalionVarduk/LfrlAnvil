namespace LfrlAnvil.Sqlite;

/// <summary>
/// Represents an SQLite DB encoding.
/// </summary>
public enum SqliteDatabaseEncoding : byte
{
    /// <summary>
    /// <b>UTF-8</b> encoding.
    /// </summary>
    UTF_8 = 0,

    /// <summary>
    /// <b>UTF-16</b> encoding.
    /// </summary>
    UTF_16 = 1,

    /// <summary>
    /// <b>UTF-16le</b> encoding.
    /// </summary>
    UTF_16_LE = 2,

    /// <summary>
    /// <b>UTF-16be</b> encoding.
    /// </summary>
    UTF_16_BE = 3
}
