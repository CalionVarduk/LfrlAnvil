using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlPrimaryKeyBuilder : MySqlObjectBuilder, ISqlPrimaryKeyBuilder
{
    private string? _fullName;

    internal MySqlPrimaryKeyBuilder(MySqlIndexBuilder index, string name)
        : base( index.Database.GetNextId(), name, SqlObjectType.PrimaryKey )
    {
        Index = index;
        _fullName = null;
    }

    public MySqlIndexBuilder Index { get; }
    public override string FullName => _fullName ??= MySqlHelpers.GetFullName( Index.Table.Schema.Name, Name );
    public override MySqlDatabaseBuilder Database => Index.Database;
    internal override bool CanRemove => Index.CanRemove;

    ISqlIndexBuilder ISqlPrimaryKeyBuilder.Index => Index;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public MySqlPrimaryKeyBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    public MySqlPrimaryKeyBuilder SetDefaultName()
    {
        return SetName( MySqlHelpers.GetDefaultPrimaryKeyName( Index.Table ) );
    }

    internal void ResetFullName()
    {
        _fullName = null;
    }

    internal void MarkAsRemoved()
    {
        Assume.Equals( IsRemoved, false );
        IsRemoved = true;
    }

    protected override void RemoveCore()
    {
        Index.Remove();
        Index.Table.UnassignPrimaryKey();
        Index.Table.Schema.Objects.Remove( Name );
        Database.ChangeTracker.ObjectRemoved( Index.Table, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        MySqlHelpers.AssertName( name );
        Index.Table.Schema.Objects.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        ResetFullName();
        Database.ChangeTracker.NameUpdated( Index.Table, this, oldName );
    }

    ISqlPrimaryKeyBuilder ISqlPrimaryKeyBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlPrimaryKeyBuilder ISqlPrimaryKeyBuilder.SetDefaultName()
    {
        return SetDefaultName();
    }
}
