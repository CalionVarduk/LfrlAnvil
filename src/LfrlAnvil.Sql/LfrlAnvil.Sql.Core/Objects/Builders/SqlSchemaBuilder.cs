using System;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Internal;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlSchemaBuilder : SqlObjectBuilder, ISqlSchemaBuilder
{
    protected SqlSchemaBuilder(SqlDatabaseBuilder database, string name, SqlObjectBuilderCollection objects)
        : base( database, SqlObjectType.Schema, name )
    {
        Objects = objects;
        Objects.SetSchema( this );
    }

    public SqlObjectBuilderCollection Objects { get; }
    public override bool CanRemove => ! ReferenceEquals( this, Database.Schemas.Default ) && base.CanRemove;

    ISqlObjectBuilderCollection ISqlSchemaBuilder.Objects => Objects;

    public new SqlSchemaBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void ThrowIfDefault()
    {
        if ( ReferenceEquals( this, Database.Schemas.Default ) )
            ExceptionThrower.Throw( SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.DefaultSchemaCannotBeRemoved ) );
    }

    protected override SqlPropertyChange<string> BeforeNameChange(string newValue)
    {
        var change = base.BeforeNameChange( newValue );
        if ( change.IsCancelled )
            return change;

        ChangeNameInCollection( Database.Schemas, this, newValue );
        return change;
    }

    protected override void AfterNameChange(string originalValue)
    {
        ResetAllTableAndViewInfoCache();
        AddNameChange( this, this, originalValue );
    }

    protected override void BeforeRemove()
    {
        ThrowIfDefault();
        base.BeforeRemove();
        QuickRemoveObjects();
        RemoveFromCollection( Database.Schemas, this );
    }

    protected override void AfterRemove()
    {
        AddRemoval( this, this );
    }

    protected override void QuickRemoveCore()
    {
        throw new NotSupportedException( ExceptionResources.SchemaQuickRemovalIsUnsupported );
    }

    protected void QuickRemoveObjects()
    {
        foreach ( var obj in Objects )
            QuickRemove( obj );

        ClearCollection( Objects );
    }

    protected void ResetAllTableAndViewInfoCache()
    {
        foreach ( var obj in Objects )
        {
            switch ( obj.Type )
            {
                case SqlObjectType.Table:
                    ResetInfo( ReinterpretCast.To<SqlTableBuilder>( obj ) );
                    break;

                case SqlObjectType.View:
                    ResetInfo( ReinterpretCast.To<SqlViewBuilder>( obj ) );
                    break;
            }
        }
    }

    ISqlSchemaBuilder ISqlSchemaBuilder.SetName(string name)
    {
        return SetName( name );
    }
}
