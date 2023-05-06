using System.Collections.Generic;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Builders;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Builders;

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
                if ( obj.Type != SqlObjectType.Index )
                    continue;

                var ix = ReinterpretCast.To<SqliteIndexBuilder>( obj );
                foreach ( var fk in ix.ReferencingForeignKeys )
                {
                    if ( ! ReferenceEquals( fk.Index.Table.Schema, this ) )
                        return false;
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
            if ( obj.Type != SqlObjectType.Index )
                continue;

            var ix = ReinterpretCast.To<SqliteIndexBuilder>( obj );
            foreach ( var fk in ix.ReferencingForeignKeys )
            {
                if ( ! ReferenceEquals( fk.Index.Table.Schema, this ) )
                    errors = errors.Extend( ExceptionResources.DetectedExternalForeignKey( fk ) );
            }
        }

        if ( errors.Count > 0 )
            throw new SqliteObjectBuilderException( errors );
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true, nameof( CanRemove ) );

        using var buffer = Objects.ClearIntoBuffer();
        if ( buffer.Length > 0 )
        {
            var reachedTables = new HashSet<ulong>();
            foreach ( var obj in buffer )
                RemoveTable( ReinterpretCast.To<SqliteTableBuilder>( obj ), reachedTables );

            Assume.ContainsExactly( reachedTables, buffer.Length, nameof( reachedTables ) );
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

        using var buffer = Objects.CopyTablesIntoBuffer();
        foreach ( var obj in buffer )
        {
            var table = ReinterpretCast.To<SqliteTableBuilder>( obj );
            table.OnSchemaNameChange();
        }
    }

    ISqlSchemaBuilder ISqlSchemaBuilder.SetName(string name)
    {
        return SetName( name );
    }
}
