namespace LfrlAnvil.Sql.Objects;

public interface ISqlForeignKey : ISqlObject
{
    ISqlIndex ReferencedIndex { get; }
    ISqlIndex Index { get; }
    ReferenceBehavior OnDeleteBehavior { get; }
    ReferenceBehavior OnUpdateBehavior { get; }
}
