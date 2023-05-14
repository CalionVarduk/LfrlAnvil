namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlForeignKeyBuilder : ISqlObjectBuilder
{
    ISqlIndexBuilder ReferencedIndex { get; }
    ISqlIndexBuilder Index { get; }
    ReferenceBehavior OnDeleteBehavior { get; }
    ReferenceBehavior OnUpdateBehavior { get; }

    new ISqlForeignKeyBuilder SetName(string name);
    ISqlForeignKeyBuilder SetDefaultName();
    ISqlForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior);
    ISqlForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior);
}
