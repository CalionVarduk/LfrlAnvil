using System;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlSchemaBuilder : SqlSchemaBuilder
{
    internal MySqlSchemaBuilder(MySqlDatabaseBuilder database, string name)
        : base( database, name, new MySqlObjectBuilderCollection() ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlSchemaBuilder.Objects" />
    public new MySqlObjectBuilderCollection Objects => ReinterpretCast.To<MySqlObjectBuilderCollection>( base.Objects );

    /// <inheritdoc />
    public override bool CanRemove => base.CanRemove && ! Name.Equals( Database.CommonSchemaName, StringComparison.OrdinalIgnoreCase );

    /// <inheritdoc cref="SqlSchemaBuilder.SetName(string)" />
    public new MySqlSchemaBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc />
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
