using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlForeignKey : MySqlObject, ISqlForeignKey
{
    internal MySqlForeignKey(MySqlIndex originIndex, MySqlIndex referencedIndex, MySqlForeignKeyBuilder builder)
        : base( builder )
    {
        OriginIndex = originIndex;
        ReferencedIndex = referencedIndex;
        OnUpdateBehavior = builder.OnUpdateBehavior;
        OnDeleteBehavior = builder.OnDeleteBehavior;
        FullName = builder.FullName;
    }

    public MySqlIndex OriginIndex { get; }
    public MySqlIndex ReferencedIndex { get; }
    public ReferenceBehavior OnUpdateBehavior { get; }
    public ReferenceBehavior OnDeleteBehavior { get; }
    public override string FullName { get; }
    public override MySqlDatabase Database => OriginIndex.Database;

    ISqlIndex ISqlForeignKey.OriginIndex => OriginIndex;
    ISqlIndex ISqlForeignKey.ReferencedIndex => ReferencedIndex;
}
