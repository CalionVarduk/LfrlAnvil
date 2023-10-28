using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteForeignKeyBuilder : SqliteObjectBuilder, ISqlForeignKeyBuilder
{
    private string _fullName;

    internal SqliteForeignKeyBuilder(SqliteIndexBuilder index, SqliteIndexBuilder referencedIndex, string name)
        : base( index.Database.GetNextId(), name, SqlObjectType.ForeignKey )
    {
        Index = index;
        ReferencedIndex = referencedIndex;
        OnDeleteBehavior = ReferenceBehavior.Restrict;
        OnUpdateBehavior = ReferenceBehavior.Restrict;
        Index.AddForeignKey( this );
        ReferencedIndex.AddReferencingForeignKey( this );
        _fullName = string.Empty;
        UpdateFullName();
    }

    public SqliteIndexBuilder Index { get; }
    public SqliteIndexBuilder ReferencedIndex { get; }
    public ReferenceBehavior OnDeleteBehavior { get; private set; }
    public ReferenceBehavior OnUpdateBehavior { get; private set; }
    public override string FullName => _fullName;
    public override SqliteDatabaseBuilder Database => Index.Database;

    ISqlIndexBuilder ISqlForeignKeyBuilder.Index => Index;
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
        return SetName( SqliteHelpers.GetDefaultForeignKeyName( Index, ReferencedIndex ) );
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
        _fullName = SqliteHelpers.GetFullName( Index.Table.Schema.Name, Name );
    }

    internal void Reactivate()
    {
        Assume.Equals( IsRemoved, true, nameof( IsRemoved ) );
        IsRemoved = false;

        Index.AddForeignKey( this );
        ReferencedIndex.AddReferencingForeignKey( this );

        Index.Table.ForeignKeys.Reactivate( this );
        Index.Table.Schema.Objects.Reactivate( this );

        Database.ChangeTracker.ObjectCreated( Index.Table, this );
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true, nameof( CanRemove ) );

        Index.RemoveForeignKey( this );
        ReferencedIndex.RemoveReferencingForeignKey( this );

        Index.Table.Schema.Objects.Remove( Name );
        Index.Table.ForeignKeys.Remove( Index, ReferencedIndex );

        Database.ChangeTracker.ObjectRemoved( Index.Table, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        SqliteHelpers.AssertName( name );
        Index.Table.Schema.Objects.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        UpdateFullName();
        Database.ChangeTracker.NameUpdated( Index.Table, this, oldName );
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
