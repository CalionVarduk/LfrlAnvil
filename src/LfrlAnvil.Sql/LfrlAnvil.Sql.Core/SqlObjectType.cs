namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a type of an SQL object.
/// </summary>
public enum SqlObjectType : byte
{
    /// <summary>
    /// Specifies an unknown type of object.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Specifies a schema object.
    /// </summary>
    Schema = 1,

    /// <summary>
    /// Specifies a table object.
    /// </summary>
    Table = 2,

    /// <summary>
    /// Specifies table's column object.
    /// </summary>
    Column = 3,

    /// <summary>
    /// Specifies table's primary key object.
    /// </summary>
    PrimaryKey = 4,

    /// <summary>
    /// Specifies table's foreign key object.
    /// </summary>
    ForeignKey = 5,

    /// <summary>
    /// Specifies table's check object.
    /// </summary>
    Check = 6,

    /// <summary>
    /// Specifies table's index object.
    /// </summary>
    Index = 7,

    /// <summary>
    /// Specifies a view object.
    /// </summary>
    View = 8,

    /// <summary>
    /// Specifies view's data field object.
    /// </summary>
    ViewDataField = 9
}
