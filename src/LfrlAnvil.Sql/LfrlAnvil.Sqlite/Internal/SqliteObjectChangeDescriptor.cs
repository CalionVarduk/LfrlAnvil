namespace LfrlAnvil.Sqlite.Internal;

internal enum SqliteObjectChangeDescriptor : byte
{
    Exists = 0,
    Name = 1,
    IsNullable = 2,
    DataType = 3,
    DefaultValue = 4,
    IsUnique = 5,
    PrimaryKey = 6,
    OnDeleteBehavior = 7,
    OnUpdateBehavior = 8,
    Reconstruct = 9
}
