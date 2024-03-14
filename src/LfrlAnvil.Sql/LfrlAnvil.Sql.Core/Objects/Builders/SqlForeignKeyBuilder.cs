using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlForeignKeyBuilder : SqlConstraintBuilder, ISqlForeignKeyBuilder
{
    protected SqlForeignKeyBuilder(SqlIndexBuilder originIndex, SqlIndexBuilder referencedIndex, string name)
        : base( originIndex.Table, SqlObjectType.ForeignKey, name )
    {
        OriginIndex = originIndex;
        ReferencedIndex = referencedIndex;
        OnDeleteBehavior = ReferenceBehavior.Restrict;
        OnUpdateBehavior = ReferenceBehavior.Restrict;
        AddIndexReferences();
    }

    public SqlIndexBuilder OriginIndex { get; }
    public SqlIndexBuilder ReferencedIndex { get; }
    public ReferenceBehavior OnDeleteBehavior { get; private set; }
    public ReferenceBehavior OnUpdateBehavior { get; private set; }

    ISqlIndexBuilder ISqlForeignKeyBuilder.OriginIndex => OriginIndex;
    ISqlIndexBuilder ISqlForeignKeyBuilder.ReferencedIndex => ReferencedIndex;

    public new SqlForeignKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqlForeignKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public SqlForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        ThrowIfRemoved();
        var change = BeforeOnDeleteBehaviorChange( behavior );
        if ( change.IsCancelled )
            return this;

        var originalValue = OnDeleteBehavior;
        OnDeleteBehavior = change.NewValue;
        AfterOnDeleteBehaviorChange( originalValue );
        return this;
    }

    public SqlForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        ThrowIfRemoved();
        var change = BeforeOnUpdateBehaviorChange( behavior );
        if ( change.IsCancelled )
            return this;

        var originalValue = OnUpdateBehavior;
        OnUpdateBehavior = change.NewValue;
        AfterOnUpdateBehaviorChange( originalValue );
        return this;
    }

    [Pure]
    protected sealed override string GetDefaultName()
    {
        return Database.DefaultNames.GetForForeignKey( OriginIndex, ReferencedIndex );
    }

    protected virtual SqlPropertyChange<ReferenceBehavior> BeforeOnDeleteBehaviorChange(ReferenceBehavior newValue)
    {
        return OnDeleteBehavior == newValue ? SqlPropertyChange.Cancel<ReferenceBehavior>() : newValue;
    }

    protected virtual void AfterOnDeleteBehaviorChange(ReferenceBehavior originalValue)
    {
        AddOnDeleteBehaviorChange( this, originalValue );
    }

    protected virtual SqlPropertyChange<ReferenceBehavior> BeforeOnUpdateBehaviorChange(ReferenceBehavior newValue)
    {
        return OnUpdateBehavior == newValue ? SqlPropertyChange.Cancel<ReferenceBehavior>() : newValue;
    }

    protected virtual void AfterOnUpdateBehaviorChange(ReferenceBehavior originalValue)
    {
        AddOnUpdateBehaviorChange( this, originalValue );
    }

    protected void AddIndexReferences()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        AddReference( OriginIndex, refSource );
        AddReference( ReferencedIndex, refSource );
        if ( ReferenceEquals( OriginIndex.Table, ReferencedIndex.Table ) )
            return;

        AddReference( ReferencedIndex.Table, refSource, ReferencedIndex );
        if ( ! ReferenceEquals( OriginIndex.Table.Schema, ReferencedIndex.Table.Schema ) )
            AddReference( ReferencedIndex.Table.Schema, refSource, ReferencedIndex );
    }

    protected void RemoveIndexReferences()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        RemoveReference( OriginIndex, refSource );
        RemoveReference( ReferencedIndex, refSource );
        if ( ReferenceEquals( OriginIndex.Table, ReferencedIndex.Table ) )
            return;

        RemoveReference( ReferencedIndex.Table, refSource );
        if ( ! ReferenceEquals( OriginIndex.Table.Schema, ReferencedIndex.Table.Schema ) )
            RemoveReference( ReferencedIndex.Table.Schema, refSource );
    }

    protected void QuickRemoveReferencedIndexReferences()
    {
        if ( ReferenceEquals( OriginIndex.Table, ReferencedIndex.Table ) )
            return;

        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        RemoveReference( ReferencedIndex, refSource );
        RemoveReference( ReferencedIndex.Table, refSource );
        if ( ! ReferenceEquals( OriginIndex.Table.Schema, ReferencedIndex.Table.Schema ) )
            RemoveReference( ReferencedIndex.Table.Schema, refSource );
    }

    protected override void BeforeRemove()
    {
        base.BeforeRemove();
        RemoveIndexReferences();
    }

    protected override void QuickRemoveCore()
    {
        base.QuickRemoveCore();
        QuickRemoveReferencedIndexReferences();
    }

    ISqlForeignKeyBuilder ISqlForeignKeyBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlForeignKeyBuilder ISqlForeignKeyBuilder.SetDefaultName()
    {
        return SetDefaultName();
    }

    ISqlForeignKeyBuilder ISqlForeignKeyBuilder.SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        return SetOnDeleteBehavior( behavior );
    }

    ISqlForeignKeyBuilder ISqlForeignKeyBuilder.SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        return SetOnUpdateBehavior( behavior );
    }
}
