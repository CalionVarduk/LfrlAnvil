using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlForeignKeyBuilder : MySqlConstraintBuilder, ISqlForeignKeyBuilder
{
    internal MySqlForeignKeyBuilder(MySqlIndexBuilder originIndex, MySqlIndexBuilder referencedIndex, string name)
        : base( originIndex.Table, name, SqlObjectType.ForeignKey )
    {
        OriginIndex = originIndex;
        ReferencedIndex = referencedIndex;
        OnDeleteBehavior = ReferenceBehavior.Restrict;
        OnUpdateBehavior = ReferenceBehavior.Restrict;
        OriginIndex.AddOriginatingForeignKey( this );
        ReferencedIndex.AddReferencingForeignKey( this );
    }

    public MySqlIndexBuilder OriginIndex { get; }
    public MySqlIndexBuilder ReferencedIndex { get; }
    public ReferenceBehavior OnDeleteBehavior { get; private set; }
    public ReferenceBehavior OnUpdateBehavior { get; private set; }
    public override MySqlDatabaseBuilder Database => OriginIndex.Database;

    ISqlIndexBuilder ISqlForeignKeyBuilder.OriginIndex => OriginIndex;
    ISqlIndexBuilder ISqlForeignKeyBuilder.ReferencedIndex => ReferencedIndex;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public new MySqlForeignKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new MySqlForeignKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public MySqlForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        EnsureNotRemoved();

        if ( OnDeleteBehavior != behavior )
        {
            var oldBehavior = OnDeleteBehavior;
            OnDeleteBehavior = behavior;
            Database.Changes.OnDeleteBehaviorUpdated( this, oldBehavior );
        }

        return this;
    }

    public MySqlForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        EnsureNotRemoved();

        if ( OnUpdateBehavior != behavior )
        {
            var oldBehavior = OnUpdateBehavior;
            OnUpdateBehavior = behavior;
            Database.Changes.OnUpdateBehaviorUpdated( this, oldBehavior );
        }

        return this;
    }

    internal override void MarkAsRemoved()
    {
        Assume.Equals( IsRemoved, false );
        IsRemoved = true;
        OriginIndex.RemoveOriginatingForeignKey( this );
        ReferencedIndex.RemoveReferencingForeignKey( this );
    }

    [Pure]
    protected override string GetDefaultName()
    {
        return MySqlHelpers.GetDefaultForeignKeyName( OriginIndex, ReferencedIndex );
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true );

        OriginIndex.RemoveOriginatingForeignKey( this );
        ReferencedIndex.RemoveReferencingForeignKey( this );

        OriginIndex.Table.Schema.Objects.Remove( Name );
        OriginIndex.Table.Constraints.Remove( Name );

        Database.Changes.ObjectRemoved( OriginIndex.Table, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        MySqlHelpers.AssertName( name );
        OriginIndex.Table.Schema.Objects.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        Database.Changes.NameUpdated( OriginIndex.Table, this, oldName );
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
