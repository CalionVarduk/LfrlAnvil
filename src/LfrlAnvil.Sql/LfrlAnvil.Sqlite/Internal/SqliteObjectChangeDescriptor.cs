namespace LfrlAnvil.Sqlite.Internal;

internal enum SqliteObjectChangeDescriptor : byte
{
    Exists = 0,
    Name = 1,
    IsNullable = 2,
    DataType = 3,
    DefaultValue = 4,
    IsUnique = 5,
    Filter = 6,
    PrimaryKey = 7,
    OnDeleteBehavior = 8,
    OnUpdateBehavior = 9,
    Reconstruct = 10
}
