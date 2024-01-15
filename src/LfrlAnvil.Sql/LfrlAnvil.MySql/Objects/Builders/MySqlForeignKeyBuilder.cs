using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlForeignKeyBuilder : MySqlObjectBuilder, ISqlForeignKeyBuilder
{
    private string? _fullName;

    internal MySqlForeignKeyBuilder(MySqlIndexBuilder originIndex, MySqlIndexBuilder referencedIndex, string name)
        : base( originIndex.Database.GetNextId(), name, SqlObjectType.ForeignKey )
    {
        OriginIndex = originIndex;
        ReferencedIndex = referencedIndex;
        OnDeleteBehavior = ReferenceBehavior.Restrict;
        OnUpdateBehavior = ReferenceBehavior.Restrict;
        OriginIndex.AddOriginatingForeignKey( this );
        ReferencedIndex.AddReferencingForeignKey( this );
        _fullName = null;
    }

    public MySqlIndexBuilder OriginIndex { get; }
    public MySqlIndexBuilder ReferencedIndex { get; }
    public ReferenceBehavior OnDeleteBehavior { get; private set; }
    public ReferenceBehavior OnUpdateBehavior { get; private set; }
    public override string FullName => _fullName ??= MySqlHelpers.GetFullName( OriginIndex.Table.Schema.Name, Name );
    public override MySqlDatabaseBuilder Database => OriginIndex.Database;

    ISqlIndexBuilder ISqlForeignKeyBuilder.OriginIndex => OriginIndex;
    ISqlIndexBuilder ISqlForeignKeyBuilder.ReferencedIndex => ReferencedIndex;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public MySqlForeignKeyBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    public MySqlForeignKeyBuilder SetDefaultName()
    {
        return SetName( MySqlHelpers.GetDefaultForeignKeyName( OriginIndex, ReferencedIndex ) );
    }

    public MySqlForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior)
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

    public MySqlForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior)
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

    internal void ResetFullName()
    {
        _fullName = null;
    }

    internal void MarkAsRemoved()
    {
        Assume.Equals( IsRemoved, false );
        IsRemoved = true;
        OriginIndex.RemoveOriginatingForeignKey( this );
        ReferencedIndex.RemoveReferencingForeignKey( this );
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

        MySqlHelpers.AssertName( name );
        OriginIndex.Table.Schema.Objects.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        ResetFullName();
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
