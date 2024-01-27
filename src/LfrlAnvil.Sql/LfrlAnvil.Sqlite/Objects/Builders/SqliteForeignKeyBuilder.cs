using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteForeignKeyBuilder : SqliteConstraintBuilder, ISqlForeignKeyBuilder
{
    internal SqliteForeignKeyBuilder(SqliteIndexBuilder originIndex, SqliteIndexBuilder referencedIndex, string name)
        : base( originIndex.Table, name, SqlObjectType.ForeignKey )
    {
        OriginIndex = originIndex;
        ReferencedIndex = referencedIndex;
        OnDeleteBehavior = ReferenceBehavior.Restrict;
        OnUpdateBehavior = ReferenceBehavior.Restrict;
        OriginIndex.AddOriginatingForeignKey( this );
        ReferencedIndex.AddReferencingForeignKey( this );
    }

    public SqliteIndexBuilder OriginIndex { get; }
    public SqliteIndexBuilder ReferencedIndex { get; }
    public ReferenceBehavior OnDeleteBehavior { get; private set; }
    public ReferenceBehavior OnUpdateBehavior { get; private set; }
    public override SqliteDatabaseBuilder Database => OriginIndex.Database;

    ISqlIndexBuilder ISqlForeignKeyBuilder.OriginIndex => OriginIndex;
    ISqlIndexBuilder ISqlForeignKeyBuilder.ReferencedIndex => ReferencedIndex;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public new SqliteForeignKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new SqliteForeignKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public SqliteForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        EnsureNotRemoved();

        if ( OnDeleteBehavior != behavior )
        {
            var oldBehavior = OnDeleteBehavior;
            OnDeleteBehavior = behavior;
            Database.ChangeTracker.OnDeleteBehaviorUpdated( this, oldBehavior );
        }

        return this;
    }

    public SqliteForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        EnsureNotRemoved();

        if ( OnUpdateBehavior != behavior )
        {
            var oldBehavior = OnUpdateBehavior;
            OnUpdateBehavior = behavior;
            Database.ChangeTracker.OnUpdateBehaviorUpdated( this, oldBehavior );
        }

        return this;
    }

    internal void Reactivate()
    {
        Assume.Equals( IsRemoved, true );
        IsRemoved = false;

        OriginIndex.AddOriginatingForeignKey( this );
        ReferencedIndex.AddReferencingForeignKey( this );

        OriginIndex.Table.Schema.Objects.Reactivate( this );
        OriginIndex.Table.Constraints.Reactivate( this );

        Database.ChangeTracker.ObjectCreated( OriginIndex.Table, this );
    }

    [Pure]
    protected override string GetDefaultName()
    {
        return SqliteHelpers.GetDefaultForeignKeyName( OriginIndex, ReferencedIndex );
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true );

        OriginIndex.RemoveOriginatingForeignKey( this );
        ReferencedIndex.RemoveReferencingForeignKey( this );

        OriginIndex.Table.Schema.Objects.Remove( Name );
        OriginIndex.Table.Constraints.Remove( Name );

        Database.ChangeTracker.ObjectRemoved( OriginIndex.Table, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        SqliteHelpers.AssertName( name );
        OriginIndex.Table.Schema.Objects.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        Database.ChangeTracker.NameUpdated( OriginIndex.Table, this, oldName );
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
