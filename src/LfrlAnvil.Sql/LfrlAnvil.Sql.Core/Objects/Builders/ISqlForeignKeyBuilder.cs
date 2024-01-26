namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlForeignKeyBuilder : ISqlConstraintBuilder
{
    ISqlIndexBuilder ReferencedIndex { get; }
    ISqlIndexBuilder OriginIndex { get; }
    ReferenceBehavior OnDeleteBehavior { get; }
    ReferenceBehavior OnUpdateBehavior { get; }

    new ISqlForeignKeyBuilder SetName(string name);
    new ISqlForeignKeyBuilder SetDefaultName();
    ISqlForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior);
    ISqlForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior);
}
