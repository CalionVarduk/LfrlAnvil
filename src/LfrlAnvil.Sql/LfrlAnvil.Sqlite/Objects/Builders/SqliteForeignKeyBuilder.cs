using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteForeignKeyBuilder : SqliteObjectBuilder, ISqlForeignKeyBuilder
{
    private string _fullName;

    internal SqliteForeignKeyBuilder(SqliteIndexBuilder originIndex, SqliteIndexBuilder referencedIndex, string name)
        : base( originIndex.Database.GetNextId(), name, SqlObjectType.ForeignKey )
    {
        OriginIndex = originIndex;
        ReferencedIndex = referencedIndex;
        OnDeleteBehavior = ReferenceBehavior.Restrict;
        OnUpdateBehavior = ReferenceBehavior.Restrict;
        OriginIndex.AddOriginatingForeignKey( this );
        ReferencedIndex.AddReferencingForeignKey( this );
        _fullName = string.Empty;
        UpdateFullName();
    }

    public SqliteIndexBuilder OriginIndex { get; }
    public SqliteIndexBuilder ReferencedIndex { get; }
    public ReferenceBehavior OnDeleteBehavior { get; private set; }
    public ReferenceBehavior OnUpdateBehavior { get; private set; }
    public override string FullName => _fullName;
    public override SqliteDatabaseBuilder Database => OriginIndex.Database;

    ISqlIndexBuilder ISqlForeignKeyBuilder.OriginIndex => OriginIndex;
    ISqlIndexBuilder ISqlForeignKeyBuilder.ReferencedIndex => ReferencedIndex;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public SqliteForeignKeyBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    public SqliteForeignKeyBuilder SetDefaultName()
    {
        return SetName( SqliteHelpers.GetDefaultForeignKeyName( OriginIndex, ReferencedIndex ) );
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

    internal void UpdateFullName()
    {
        _fullName = SqliteHelpers.GetFullName( OriginIndex.Table.Schema.Name, Name );
    }

    internal void Reactivate()
    {
        Assume.Equals( IsRemoved, true );
        IsRemoved = false;

        OriginIndex.AddOriginatingForeignKey( this );
        ReferencedIndex.AddReferencingForeignKey( this );

        OriginIndex.Table.ForeignKeys.Reactivate( this );
        OriginIndex.Table.Schema.Objects.Reactivate( this );

        Database.ChangeTracker.ObjectCreated( OriginIndex.Table, this );
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true );

        OriginIndex.RemoveOriginatingForeignKey( this );
        ReferencedIndex.RemoveReferencingForeignKey( this );

        OriginIndex.Table.Schema.Objects.Remove( Name );
        OriginIndex.Table.ForeignKeys.Remove( OriginIndex, ReferencedIndex );

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
        UpdateFullName();
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
