﻿using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqlitePrimaryKeyBuilder : SqliteObjectBuilder, ISqlPrimaryKeyBuilder
{
    private string _fullName;

    internal SqlitePrimaryKeyBuilder(SqliteIndexBuilder index, string name)
        : base( index.Database.GetNextId(), name, SqlObjectType.PrimaryKey )
    {
        Index = index;
        _fullName = string.Empty;
        UpdateFullName();
    }

    public SqliteIndexBuilder Index { get; }
    public override string FullName => _fullName;
    public override SqliteDatabaseBuilder Database => Index.Database;
    internal override bool CanRemove => Index.CanRemove;

    ISqlIndexBuilder ISqlPrimaryKeyBuilder.Index => Index;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public SqlitePrimaryKeyBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    public SqlitePrimaryKeyBuilder SetDefaultName()
    {
        return SetName( SqliteHelpers.GetDefaultPrimaryKeyName( Index.Table ) );
    }

    internal void UpdateFullName()
    {
        _fullName = SqliteHelpers.GetFullName( Index.Table.Schema.Name, Name );
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

        SqliteHelpers.AssertName( name );
        Index.Table.Schema.Objects.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        UpdateFullName();
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
