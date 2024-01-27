using System;
using System.Collections.Generic;
using LfrlAnvil.Memory;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlSchemaBuilder : MySqlObjectBuilder, ISqlSchemaBuilder
{
    internal MySqlSchemaBuilder(MySqlDatabaseBuilder database, string name)
        : base( database.GetNextId(), name, SqlObjectType.Schema )
    {
        Database = database;
        Objects = new MySqlObjectBuilderCollection( this );
    }

    public MySqlObjectBuilderCollection Objects { get; }
    public override MySqlDatabaseBuilder Database { get; }

    internal override bool CanRemove
    {
        get
        {
            if ( ReferenceEquals( this, Database.Schemas.Default ) ||
                Name.Equals( Database.CommonSchemaName, StringComparison.OrdinalIgnoreCase ) )
                return false;

            foreach ( var obj in Objects )
            {
                switch ( obj.Type )
                {
                    case SqlObjectType.Table:
                    {
                        var table = ReinterpretCast.To<MySqlTableBuilder>( obj );
                        foreach ( var v in table.ReferencingViews )
                        {
                            if ( ! ReferenceEquals( v.Schema, this ) )
                                return false;
                        }

                        break;
                    }
                    case SqlObjectType.Index:
                    {
                        var ix = ReinterpretCast.To<MySqlIndexBuilder>( obj );
                        foreach ( var fk in ix.ReferencingForeignKeys )
                        {
                            if ( ! ReferenceEquals( fk.OriginIndex.Table.Schema, this ) )
                                return false;
                        }

                        break;
                    }
                    case SqlObjectType.View:
                    {
                        var view = ReinterpretCast.To<MySqlViewBuilder>( obj );
                        foreach ( var v in view.ReferencingViews )
                        {
                            if ( ! ReferenceEquals( v.Schema, this ) )
                                return false;
                        }

                        break;
                    }
                }
            }

            return true;
        }
    }

    ISqlObjectBuilderCollection ISqlSchemaBuilder.Objects => Objects;

    public MySqlSchemaBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    protected override void AssertRemoval()
    {
        var errors = Chain<string>.Empty;
        if ( ReferenceEquals( this, Database.Schemas.Default ) )
            errors = errors.Extend( ExceptionResources.DefaultSchemaCannotBeRemoved );

        if ( Name.Equals( Database.CommonSchemaName, StringComparison.OrdinalIgnoreCase ) )
            errors = errors.Extend( ExceptionResources.CommonSchemaCannotBeRemoved );

        foreach ( var obj in Objects )
        {
            switch ( obj.Type )
            {
                case SqlObjectType.Table:
                {
                    var table = ReinterpretCast.To<MySqlTableBuilder>( obj );
                    foreach ( var v in table.ReferencingViews )
                    {
                        if ( ! ReferenceEquals( v.Schema, this ) )
                            errors = errors.Extend( ExceptionResources.DetectedExternalReferencingView( v, table ) );
                    }

                    break;
                }
                case SqlObjectType.Index:
                {
                    var ix = ReinterpretCast.To<MySqlIndexBuilder>( obj );
                    foreach ( var fk in ix.ReferencingForeignKeys )
                    {
                        if ( ! ReferenceEquals( fk.OriginIndex.Table.Schema, this ) )
                            errors = errors.Extend( ExceptionResources.DetectedExternalForeignKey( fk ) );
                    }

                    break;
                }
                case SqlObjectType.View:
                {
                    var view = ReinterpretCast.To<MySqlViewBuilder>( obj );
                    foreach ( var v in view.ReferencingViews )
                    {
                        if ( ! ReferenceEquals( v.Schema, this ) )
                            errors = errors.Extend( ExceptionResources.DetectedExternalReferencingView( v, view ) );
                    }

                    break;
                }
            }
        }

        if ( errors.Count > 0 )
            throw new MySqlObjectBuilderException( errors );
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true );

        foreach ( var obj in Objects )
        {
            switch ( obj.Type )
            {
                case SqlObjectType.View:
                    ReinterpretCast.To<MySqlViewBuilder>( obj ).MarkAsRemoved();
                    break;

                case SqlObjectType.Table:
                    ReinterpretCast.To<MySqlTableBuilder>( obj ).MarkAsRemoved();
                    break;
            }
        }

        Objects.Clear();
        Database.Schemas.Remove( Name );
        Database.ChangeTracker.SchemaDropped( Name );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        MySqlHelpers.AssertName( name );
        using var viewBuffer = GetViewsToRecreate();

        Database.Schemas.ChangeName( this, name );
        var oldName = Name;
        Name = name;

        if ( ! Name.Equals( Database.CommonSchemaName, StringComparison.OrdinalIgnoreCase ) )
            Database.ChangeTracker.SchemaCreated( Name );

        foreach ( var obj in Objects )
        {
            if ( obj.Type == SqlObjectType.Table )
                ReinterpretCast.To<MySqlTableBuilder>( obj ).OnSchemaNameChange( oldName );
        }

        if ( ! oldName.Equals( Database.CommonSchemaName, StringComparison.OrdinalIgnoreCase ) )
            Database.ChangeTracker.SchemaDropped( oldName );

        foreach ( var obj in viewBuffer )
        {
            var view = ReinterpretCast.To<MySqlViewBuilder>( obj );
            view.ResetInfoCache();
            view.Reactivate();
        }

        RentedMemorySequence<MySqlObjectBuilder> GetViewsToRecreate()
        {
            var views = new Dictionary<ulong, MySqlViewBuilder>();
            foreach ( var obj in Objects )
            {
                if ( obj.Type == SqlObjectType.View )
                {
                    var view = ReinterpretCast.To<MySqlViewBuilder>( obj );
                    views.Add( view.Id, view );
                }
            }

            return MySqlViewBuilder.RemoveReferencingViewsIntoBuffer( Database, views );
        }
    }

    ISqlSchemaBuilder ISqlSchemaBuilder.SetName(string name)
    {
        return SetName( name );
    }
}
