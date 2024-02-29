﻿using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlPrimaryKeyBuilder : MySqlConstraintBuilder, ISqlPrimaryKeyBuilder
{
    internal MySqlPrimaryKeyBuilder(MySqlIndexBuilder index, string name)
        : base( index.Table, name, SqlObjectType.PrimaryKey )
    {
        Index = index;
    }

    public MySqlIndexBuilder Index { get; }
    public override MySqlDatabaseBuilder Database => Index.Database;
    public override bool CanRemove => Index.CanRemove;

    ISqlIndexBuilder ISqlPrimaryKeyBuilder.Index => Index;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public new MySqlPrimaryKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new MySqlPrimaryKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    internal override void MarkAsRemoved()
    {
        IsRemoved = true;
    }

    [Pure]
    protected override string GetDefaultName()
    {
        return MySqlHelpers.GetDefaultPrimaryKeyName( Table );
    }

    protected override void RemoveCore()
    {
        Index.Remove();
        Index.Table.Schema.Objects.Remove( Name );
        Index.Table.Constraints.Remove( Name );
        Database.Changes.ObjectRemoved( Index.Table, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        MySqlHelpers.AssertName( name );
        Index.Table.Schema.Objects.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        Database.Changes.NameUpdated( Index.Table, this, oldName );
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