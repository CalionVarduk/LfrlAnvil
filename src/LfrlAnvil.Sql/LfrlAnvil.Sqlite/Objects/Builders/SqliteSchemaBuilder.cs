using System.Collections.Generic;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteSchemaBuilder : SqliteObjectBuilder, ISqlSchemaBuilder
{
    internal SqliteSchemaBuilder(SqliteDatabaseBuilder database, string name)
        : base( database.GetNextId(), name, SqlObjectType.Schema )
    {
        Database = database;
        Objects = new SqliteObjectBuilderCollection( this );
    }

    public SqliteObjectBuilderCollection Objects { get; }
    public override SqliteDatabaseBuilder Database { get; }
    public override string FullName => Name;

    internal override bool CanRemove
    {
        get
        {
            if ( ReferenceEquals( this, Database.Schemas.Default ) )
                return false;

            foreach ( var obj in Objects )
            {
                switch ( obj.Type )
                {
                    case SqlObjectType.Table:
                    {
                        var table = ReinterpretCast.To<SqliteTableBuilder>( obj );
                        foreach ( var v in table.ReferencingViews )
                        {
                            if ( ! ReferenceEquals( v.Schema, this ) )
                                return false;
                        }

                        break;
                    }
                    case SqlObjectType.Index:
                    {
                        var ix = ReinterpretCast.To<SqliteIndexBuilder>( obj );
                        foreach ( var fk in ix.ReferencingForeignKeys )
                        {
                            if ( ! ReferenceEquals( fk.Index.Table.Schema, this ) )
                                return false;
                        }

                        break;
                    }
                    case SqlObjectType.View:
                    {
                        var view = ReinterpretCast.To<SqliteViewBuilder>( obj );
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

    public SqliteSchemaBuilder SetName(string name)
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

        foreach ( var obj in Objects )
        {
            switch ( obj.Type )
            {
                case SqlObjectType.Table:
                {
                    var table = ReinterpretCast.To<SqliteTableBuilder>( obj );
                    foreach ( var v in table.ReferencingViews )
                    {
                        if ( ! ReferenceEquals( v.Schema, this ) )
                            errors = errors.Extend( ExceptionResources.DetectedExternalReferencingView( v, table ) );
                    }

                    break;
                }
                case SqlObjectType.Index:
                {
                    var ix = ReinterpretCast.To<SqliteIndexBuilder>( obj );
                    foreach ( var fk in ix.ReferencingForeignKeys )
                    {
                        if ( ! ReferenceEquals( fk.Index.Table.Schema, this ) )
                            errors = errors.Extend( ExceptionResources.DetectedExternalForeignKey( fk ) );
                    }

                    break;
                }
                case SqlObjectType.View:
                {
                    var view = ReinterpretCast.To<SqliteViewBuilder>( obj );
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
            throw new SqliteObjectBuilderException( errors );
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true, nameof( CanRemove ) );

        using var tableBuffer = Objects.CopyTablesIntoBuffer();
        using var viewBuffer = Objects.CopyViewsIntoBuffer();
        Objects.Clear();

        foreach ( var obj in viewBuffer )
            ReinterpretCast.To<SqliteViewBuilder>( obj ).ForceRemove();

        if ( tableBuffer.Length > 0 )
        {
            var reachedTables = new HashSet<ulong>();
            foreach ( var obj in tableBuffer )
                RemoveTable( ReinterpretCast.To<SqliteTableBuilder>( obj ), reachedTables );

            Assume.ContainsExactly( reachedTables, tableBuffer.Length, nameof( reachedTables ) );
        }

        Database.Schemas.Remove( Name );

        static void RemoveTable(SqliteTableBuilder table, HashSet<ulong> reachedTables)
        {
            if ( table.IsRemoved || ! reachedTables.Add( table.Id ) )
                return;

            foreach ( var ix in table.Indexes )
            {
                foreach ( var fk in ix.ReferencingForeignKeys )
                {
                    if ( ! fk.IsSelfReference() )
                        RemoveTable( fk.Index.Table, reachedTables );
                }
            }

            if ( ! table.IsRemoved )
                table.ForceRemove();
        }
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        SqliteHelpers.AssertName( name );
        Database.Schemas.ChangeName( this, name );
        Name = name;

        using ( var buffer = Objects.CopyViewsIntoBuffer() )
        {
            foreach ( var obj in buffer )
                ReinterpretCast.To<SqliteViewBuilder>( obj ).OnSchemaNameChange();
        }

        using ( var buffer = Objects.CopyTablesIntoBuffer() )
        {
            foreach ( var obj in buffer )
                ReinterpretCast.To<SqliteTableBuilder>( obj ).OnSchemaNameChange();
        }
    }

    ISqlSchemaBuilder ISqlSchemaBuilder.SetName(string name)
    {
        return SetName( name );
    }
}
