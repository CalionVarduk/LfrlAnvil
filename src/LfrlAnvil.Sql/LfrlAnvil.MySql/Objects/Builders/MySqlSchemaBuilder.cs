using System;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlSchemaBuilder : SqlSchemaBuilder
{
    internal MySqlSchemaBuilder(MySqlDatabaseBuilder database, string name)
        : base( database, name, new MySqlObjectBuilderCollection() ) { }

    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );
    public new MySqlObjectBuilderCollection Objects => ReinterpretCast.To<MySqlObjectBuilderCollection>( base.Objects );
    public override bool CanRemove => base.CanRemove && ! Name.Equals( Database.CommonSchemaName, StringComparison.OrdinalIgnoreCase );

    public new MySqlSchemaBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    protected override void BeforeRemove()
    {
        ThrowIfDefault();

        if ( Name.Equals( Database.CommonSchemaName, StringComparison.OrdinalIgnoreCase ) )
            ExceptionThrower.Throw( SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.CommonSchemaCannotBeRemoved ) );

        ThrowIfReferenced();
        QuickRemoveObjects();
        RemoveFromCollection( Database.Schemas, this );
    }
}
