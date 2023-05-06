using System;
using LfrlAnvil.Generators;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Builders;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Builders;

public sealed class SqliteDatabaseBuilder : ISqlDatabaseBuilder
{
    private readonly UlongSequenceGenerator _idGenerator;

    public SqliteDatabaseBuilder()
    {
        _idGenerator = new UlongSequenceGenerator();
        DataTypes = new SqliteDataTypeProvider();
        TypeDefinitions = new SqliteColumnTypeDefinitionProvider();
        Schemas = new SqliteSchemaBuilderCollection( this );
        ChangeTracker = new SqliteDatabaseChangeTracker();
        ObjectPool = new MemorySequencePool<SqliteObjectBuilder>( minSegmentLength: 32 );
    }

    public SqliteDataTypeProvider DataTypes { get; }
    public SqliteColumnTypeDefinitionProvider TypeDefinitions { get; }
    public SqliteSchemaBuilderCollection Schemas { get; }
    public SqlDialect Dialect => SqliteDialect.Instance;
    internal SqliteDatabaseChangeTracker ChangeTracker { get; }
    internal MemorySequencePool<SqliteObjectBuilder> ObjectPool { get; }

    ISqlDataTypeProvider ISqlDatabaseBuilder.DataTypes => DataTypes;
    ISqlColumnTypeDefinitionProvider ISqlDatabaseBuilder.TypeDefinitions => TypeDefinitions;
    ISqlSchemaBuilderCollection ISqlDatabaseBuilder.Schemas => Schemas;

    public ReadOnlySpan<string> GetPendingStatements()
    {
        return ChangeTracker.GetPendingStatements();
    }

    public void AddRawStatement(string statement)
    {
        ChangeTracker.AddRawStatement( statement );
    }

    internal ulong GetNextId()
    {
        return _idGenerator.Generate();
    }

    internal static void RemoveReferencingForeignKeys(SqliteTableBuilder table, RentedMemorySequenceSpan<SqliteObjectBuilder> foreignKeys)
    {
        if ( foreignKeys.Length == 0 )
            return;

        PrepareReferencingForeignKeysForRemoval( foreignKeys, table.Database.ChangeTracker.CurrentTable, table );

        foreach ( var obj in foreignKeys )
        {
            var fk = ReinterpretCast.To<SqliteForeignKeyBuilder>( obj );
            Assume.Equals( table, fk.ReferencedIndex.Table, nameof( table ) );
            if ( ! fk.IsSelfReference() )
                fk.Remove();
        }
    }

    internal static void PrepareReferencingForeignKeysForRemoval(
        RentedMemorySequenceSpan<SqliteObjectBuilder> foreignKeys,
        SqliteTableBuilder? tableWithOngoingChanges = null,
        SqliteTableBuilder? sourceTable = null)
    {
        Assume.IsGreaterThan( foreignKeys.Length, 0, nameof( foreignKeys.Length ) );

        if ( tableWithOngoingChanges is not null && ! ReferenceEquals( tableWithOngoingChanges, sourceTable ) )
        {
            var i = 0;
            while ( i < foreignKeys.Length )
            {
                var fk = ReinterpretCast.To<SqliteForeignKeyBuilder>( foreignKeys[i] );
                if ( ! ReferenceEquals( fk.Index.Table, tableWithOngoingChanges ) )
                    break;

                ++i;
            }

            var j = i++;
            while ( i < foreignKeys.Length )
            {
                var fk = ReinterpretCast.To<SqliteForeignKeyBuilder>( foreignKeys[i] );
                if ( ReferenceEquals( fk.Index.Table, tableWithOngoingChanges ) )
                {
                    foreignKeys[i] = foreignKeys[j];
                    foreignKeys[j++] = fk;
                }

                ++i;
            }

            foreignKeys = foreignKeys.Slice( j );
        }

        foreignKeys.Sort(
            static (a, b) =>
            {
                var fk1 = ReinterpretCast.To<SqliteForeignKeyBuilder>( a );
                var fk2 = ReinterpretCast.To<SqliteForeignKeyBuilder>( b );
                return fk1.Index.Table.Id.CompareTo( fk2.Index.Table.Id );
            } );
    }
}
