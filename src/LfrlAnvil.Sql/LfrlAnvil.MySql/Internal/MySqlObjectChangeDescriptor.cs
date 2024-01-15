namespace LfrlAnvil.MySql.Internal;

internal enum MySqlObjectChangeDescriptor : byte
{
    Exists = 0,
    Name = 1,
    SchemaName = 2,
    IsNullable = 3,
    DataType = 4,
    DefaultValue = 5,
    IsUnique = 6,
    Filter = 7,
    PrimaryKey = 8,
    OnDeleteBehavior = 9,
    OnUpdateBehavior = 10
}
