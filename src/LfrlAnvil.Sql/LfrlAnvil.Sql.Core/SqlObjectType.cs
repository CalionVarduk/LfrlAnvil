namespace LfrlAnvil.Sql;

public enum SqlObjectType : byte
{
    Unknown = 0,
    Schema = 1,
    Table = 2,
    Column = 3,
    PrimaryKey = 4,
    ForeignKey = 5,
    Index = 6
}
