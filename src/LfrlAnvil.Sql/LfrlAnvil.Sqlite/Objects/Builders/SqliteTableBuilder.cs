using System;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteTableBuilder : SqliteObjectBuilder, ISqlTableBuilder
{
    private string _fullName;

    internal SqliteTableBuilder(SqliteSchemaBuilder schema, string name)
        : base( schema.Database.GetNextId(), name, SqlObjectType.Table )
    {
        Schema = schema;
        PrimaryKey = null;
        Columns = new SqliteColumnBuilderCollection( this );
        Indexes = new SqliteIndexBuilderCollection( this );
        ForeignKeys = new SqliteForeignKeyBuilderCollection( this );
        _fullName = string.Empty;
        UpdateFullName();
    }

    public SqliteSchemaBuilder Schema { get; }
    public SqliteColumnBuilderCollection Columns { get; }
    public SqliteIndexBuilderCollection Indexes { get; }
    public SqliteForeignKeyBuilderCollection ForeignKeys { get; }
    public SqlitePrimaryKeyBuilder? PrimaryKey { get; private set; }
    public override string FullName => _fullName;
    public override SqliteDatabaseBuilder Database => Schema.Database;

    internal override bool CanRemove
    {
        get
        {
            foreach ( var ix in Indexes )
            {
                if ( ! ix.CanRemove )
                    return false;
            }

            return true;
        }
    }

    ISqlSchemaBuilder ISqlTableBuilder.Schema => Schema;
    ISqlPrimaryKeyBuilder? ISqlTableBuilder.PrimaryKey => PrimaryKey;
    ISqlColumnBuilderCollection ISqlTableBuilder.Columns => Columns;
    ISqlIndexBuilderCollection ISqlTableBuilder.Indexes => Indexes;
    ISqlForeignKeyBuilderCollection ISqlTableBuilder.ForeignKeys => ForeignKeys;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    public SqliteTableBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    public SqlitePrimaryKeyBuilder SetPrimaryKey(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        EnsureNotRemoved();

        if ( PrimaryKey is null )
        {
            var indexColumns = SqliteHelpers.CreateIndexColumns( this, columns, allowNullableColumns: false );
            PrimaryKey = Schema.Objects.CreatePrimaryKey( this, indexColumns );
            Database.ChangeTracker.PrimaryKeyUpdated( this, null );
        }
        else if ( ! PrimaryKey.Index.AreColumnsEqual( columns ) )
        {
            var oldPrimaryKey = PrimaryKey;
            var indexColumns = SqliteHelpers.CreateIndexColumns( this, columns, allowNullableColumns: false );
            PrimaryKey = Schema.Objects.ReplacePrimaryKey( this, indexColumns, PrimaryKey );
            Database.ChangeTracker.PrimaryKeyUpdated( this, oldPrimaryKey );
        }

        return PrimaryKey;
    }

    internal void UnassignPrimaryKey()
    {
        Assume.IsNotNull( PrimaryKey, nameof( PrimaryKey ) );

        var oldPrimaryKey = PrimaryKey;
        PrimaryKey = null;
        Database.ChangeTracker.PrimaryKeyUpdated( this, oldPrimaryKey );
    }

    internal void ForceRemove()
    {
        Assume.Equals( IsRemoved, false, nameof( IsRemoved ) );
        IsRemoved = true;

        using ( var buffer = Database.ObjectPool.GreedyRent() )
        {
            foreach ( var ix in Indexes )
            {
                var count = ix.ReferencingForeignKeys.Count;
                buffer.Expand( count );
                ix.ClearReferencingForeignKeysInto( buffer.Slice( buffer.Length - count ) );
            }

            SqliteDatabaseBuilder.RemoveReferencingForeignKeys( this, buffer );
        }

        RemoveCore();
    }

    protected override void AssertRemoval()
    {
        var errors = Chain<string>.Empty;

        foreach ( var ix in Indexes )
        {
            foreach ( var fk in ix.ReferencingForeignKeys )
            {
                if ( ! fk.IsSelfReference() )
                    errors = errors.Extend( ExceptionResources.DetectedExternalForeignKey( fk ) );
            }
        }

        if ( errors.Count > 0 )
            throw new SqliteObjectBuilderException( errors );
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true, nameof( CanRemove ) );

        ForeignKeys.Clear();

        using var buffer = Database.ObjectPool.GreedyRent( Columns.Count + Indexes.Count );
        var columns = buffer.Slice( 0, Columns.Count );
        var indexes = buffer.Slice( columns.Length, Indexes.Count );

        Columns.ClearInto( columns );
        Indexes.ClearInto( indexes );

        foreach ( var obj in indexes )
        {
            var index = ReinterpretCast.To<SqliteIndexBuilder>( obj );
            var count = index.ForeignKeys.Count;
            buffer.Expand( count );
            index.ClearForeignKeysInto( buffer.Slice( buffer.Length - count ) );
        }

        var foreignKeys = buffer.Slice( indexes.StartIndex + indexes.Length );
        foreach ( var fk in foreignKeys )
            fk.Remove();

        foreach ( var index in indexes )
            index.Remove();

        foreach ( var column in columns )
            column.Remove();

        Schema.Objects.Remove( Name );
        Database.ChangeTracker.ObjectRemoved( this, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        SqliteHelpers.AssertName( name );
        if ( Schema.Objects.TryGet( name, out var obj ) )
            throw new SqliteObjectBuilderException( ExceptionResources.NameIsAlreadyTaken( obj, name ) );

        Rename(
            name,
            static (t, n) =>
            {
                t.Schema.Objects.ChangeName( t, n );
                var oldName = t.FullName;
                t.Name = n;
                t.UpdateFullName();
                t.Database.ChangeTracker.FullNameUpdated( t, t, oldName );
            } );
    }

    internal void OnSchemaNameChange()
    {
        Rename(
            Name,
            static (t, _) =>
            {
                var oldName = t.FullName;
                t.UpdateFullName();
                t.Database.ChangeTracker.FullNameUpdated( t, t, oldName );

                if ( t.PrimaryKey is not null )
                {
                    oldName = t.PrimaryKey.FullName;
                    t.PrimaryKey.UpdateFullName();
                    t.Database.ChangeTracker.FullNameUpdated( t, t.PrimaryKey, oldName );
                }

                foreach ( var fk in t.ForeignKeys )
                {
                    oldName = fk.FullName;
                    fk.UpdateFullName();
                    t.Database.ChangeTracker.FullNameUpdated( t, fk, oldName );
                }

                foreach ( var ix in t.Indexes )
                {
                    oldName = ix.FullName;
                    ix.UpdateFullName();
                    t.Database.ChangeTracker.FullNameUpdated( t, ix, oldName );
                }
            } );
    }

    internal void UpdateFullName()
    {
        _fullName = SqliteHelpers.GetFullName( Schema.Name, Name );
        foreach ( var column in Columns )
            column.ResetFullName();
    }

    private void Rename(string newName, Action<SqliteTableBuilder, string> update)
    {
        var hasSelfRefForeignKeys = false;

        using var buffer = Database.ObjectPool.GreedyRent();
        foreach ( var index in Indexes )
        {
            foreach ( var fk in index.ReferencingForeignKeys )
            {
                if ( fk.IsSelfReference() )
                {
                    hasSelfRefForeignKeys = true;
                    continue;
                }

                buffer.Push( fk );
            }
        }

        SqliteDatabaseBuilder.RemoveReferencingForeignKeys( this, buffer );

        update( this, newName );

        if ( hasSelfRefForeignKeys )
            Database.ChangeTracker.ReconstructionRequested( this );

        foreach ( var obj in buffer )
            ReinterpretCast.To<SqliteForeignKeyBuilder>( obj ).Reactivate();
    }

    ISqlTableBuilder ISqlTableBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlPrimaryKeyBuilder ISqlTableBuilder.SetPrimaryKey(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        return SetPrimaryKey( columns );
    }
}
