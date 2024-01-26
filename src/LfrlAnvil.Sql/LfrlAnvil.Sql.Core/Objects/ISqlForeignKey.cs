namespace LfrlAnvil.Sql.Objects;

public interface ISqlForeignKey : ISqlConstraint
{
    ISqlIndex ReferencedIndex { get; }
    ISqlIndex OriginIndex { get; }
    ReferenceBehavior OnDeleteBehavior { get; }
    ReferenceBehavior OnUpdateBehavior { get; }
}
