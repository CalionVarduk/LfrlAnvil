using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Extensions;

public static class SqlObjectExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool Contains(this ISqlIndexBuilderCollection indexes, params ISqlIndexColumnBuilder[] columns)
    {
        return indexes.Contains( columns );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder Get(this ISqlIndexBuilderCollection indexes, params ISqlIndexColumnBuilder[] columns)
    {
        return indexes.Get( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder Create(this ISqlIndexBuilderCollection indexes, params ISqlIndexColumnBuilder[] columns)
    {
        return indexes.Create( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder GetOrCreate(this ISqlIndexBuilderCollection indexes, params ISqlIndexColumnBuilder[] columns)
    {
        return indexes.GetOrCreate( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool Remove(this ISqlIndexBuilderCollection indexes, params ISqlIndexColumnBuilder[] columns)
    {
        return indexes.Remove( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder SetFilter(this ISqlIndexBuilder index, Func<SqlTableBuilderNode, SqlConditionNode?> filter)
    {
        return index.SetFilter( filter( index.Table.RecordSet ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlPrimaryKeyBuilder SetPrimaryKey(this ISqlTableBuilder table, params ISqlIndexColumnBuilder[] columns)
    {
        return table.SetPrimaryKey( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlColumnBuilder SetType(this ISqlColumnBuilder column, ISqlDataType dataType)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByDataType( dataType ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlColumnBuilder SetType(this ISqlColumnBuilder column, Type type)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByType( type ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlColumnBuilder SetType<T>(this ISqlColumnBuilder column)
    {
        return column.SetType( typeof( T ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlColumnBuilder SetDefaultValue<T>(this ISqlColumnBuilder column, T? value)
        where T : notnull
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlColumnBuilder SetDefaultValue<T>(this ISqlColumnBuilder column, T? value)
        where T : struct
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsSelfReference(this ISqlForeignKeyBuilder foreignKey)
    {
        return ReferenceEquals( foreignKey.OriginIndex.Table, foreignKey.ReferencedIndex.Table );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool Contains(this ISqlIndexCollection indexes, params ISqlIndexColumn[] columns)
    {
        return indexes.Contains( columns );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndex Get(this ISqlIndexCollection indexes, params ISqlIndexColumn[] columns)
    {
        return indexes.Get( columns );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsSelfReference(this ISqlForeignKey foreignKey)
    {
        return ReferenceEquals( foreignKey.OriginIndex.Table, foreignKey.ReferencedIndex.Table );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void AddStatement(this ISqlDatabaseBuilder database, string statement)
    {
        database.AddStatement( SqlNode.RawStatement( statement ) );
    }

    [Pure]
    public static SqlCreateViewNode ToCreateNode(this ISqlViewBuilder view)
    {
        return SqlNode.CreateView( view.Info, view.Source );
    }

    [Pure]
    public static SqlCreateTableNode ToCreateNode(
        this ISqlTableBuilder table,
        SqlRecordSetInfo? customInfo = null,
        bool useFullConstraintNames = false,
        bool includeForeignKeys = true)
    {
        return SqlNode.CreateTable(
            customInfo ?? table.Info,
            table.Columns.ToDefinitionRange(),
            ifNotExists: false,
            t =>
            {
                var primaryKey = table.PrimaryKey?.ToDefinitionNode( t, useFullConstraintNames );
                var checks = table.Checks.ToDefinitionRange( useFullConstraintNames );
                var foreignKeys = includeForeignKeys
                    ? table.ForeignKeys.ToDefinitionRange( t, useFullConstraintNames )
                    : Array.Empty<SqlForeignKeyDefinitionNode>();

                var result = SqlCreateTableConstraints.Empty.WithForeignKeys( foreignKeys ).WithChecks( checks );
                return primaryKey is not null ? result.WithPrimaryKey( primaryKey ) : result;
            } );
    }

    [Pure]
    public static SqlCreateIndexNode ToCreateNode(this ISqlIndexBuilder index)
    {
        var ixTable = index.Table;
        var columns = index.Columns.ToDefinitionNode( ixTable.RecordSet );

        return SqlNode.CreateIndex(
            SqlSchemaObjectName.Create( ixTable.Schema.Name, index.Name ),
            index.IsUnique,
            ixTable.RecordSet,
            columns,
            replaceIfExists: false,
            index.Filter );
    }

    [Pure]
    public static SqlColumnDefinitionNode ToDefinitionNode(this ISqlColumnBuilder column)
    {
        return SqlNode.Column( column.Name, column.TypeDefinition, column.IsNullable, column.DefaultValue );
    }

    [Pure]
    public static SqlPrimaryKeyDefinitionNode ToDefinitionNode(
        this ISqlPrimaryKeyBuilder primaryKey,
        SqlRecordSetNode table,
        bool useFullName = false)
    {
        var columns = primaryKey.Index.Columns.ToDefinitionNode( table );
        return SqlNode.PrimaryKey( useFullName ? primaryKey.FullName : primaryKey.Name, columns );
    }

    [Pure]
    public static SqlForeignKeyDefinitionNode ToDefinitionNode(
        this ISqlForeignKeyBuilder foreignKey,
        SqlRecordSetNode table,
        bool useFullName)
    {
        var refIndex = foreignKey.ReferencedIndex;
        var fkColumns = foreignKey.OriginIndex.Columns;
        var fkReferencedColumns = refIndex.Columns;
        var isSelfReference = foreignKey.IsSelfReference();

        var i = 0;
        var columns = Array.Empty<SqlDataFieldNode>();
        if ( fkColumns.Length > 0 )
        {
            columns = new SqlDataFieldNode[fkColumns.Length];
            foreach ( var column in fkColumns )
                columns[i++] = table[column.Column.Name];
        }

        var referencedColumns = Array.Empty<SqlDataFieldNode>();
        if ( fkReferencedColumns.Length > 0 )
        {
            i = 0;
            referencedColumns = new SqlDataFieldNode[fkReferencedColumns.Length];
            if ( isSelfReference )
            {
                foreach ( var column in fkReferencedColumns )
                    referencedColumns[i++] = table[column.Column.Name];
            }
            else
            {
                foreach ( var column in fkReferencedColumns )
                    referencedColumns[i++] = column.Column.Node;
            }
        }

        return SqlNode.ForeignKey(
            useFullName ? foreignKey.FullName : foreignKey.Name,
            columns,
            refIndex.Table.RecordSet,
            referencedColumns,
            foreignKey.OnDeleteBehavior,
            foreignKey.OnUpdateBehavior );
    }

    [Pure]
    public static SqlCheckDefinitionNode ToDefinitionNode(this ISqlCheckBuilder check, bool useFullName)
    {
        return SqlNode.Check( useFullName ? check.FullName : check.Name, check.Condition );
    }

    [Pure]
    public static SqlColumnDefinitionNode[] ToDefinitionRange(this IReadOnlyCollection<ISqlColumnBuilder> columns)
    {
        if ( columns.Count == 0 )
            return Array.Empty<SqlColumnDefinitionNode>();

        var i = 0;
        var result = new SqlColumnDefinitionNode[columns.Count];
        foreach ( var column in columns )
            result[i++] = column.ToDefinitionNode();

        return result;
    }

    [Pure]
    public static SqlOrderByNode[] ToDefinitionNode(this ReadOnlyMemory<ISqlIndexColumnBuilder> columns, SqlRecordSetNode table)
    {
        if ( columns.Length == 0 )
            return Array.Empty<SqlOrderByNode>();

        var i = 0;
        var result = new SqlOrderByNode[columns.Length];
        foreach ( var column in columns )
            result[i++] = SqlNode.OrderBy( table[column.Column.Name], column.Ordering );

        return result;
    }

    [Pure]
    public static SqlForeignKeyDefinitionNode[] ToDefinitionRange(
        this IReadOnlyCollection<ISqlForeignKeyBuilder> foreignKeys,
        SqlRecordSetNode table,
        bool useFullName = false)
    {
        if ( foreignKeys.Count == 0 )
            return Array.Empty<SqlForeignKeyDefinitionNode>();

        var i = 0;
        var result = new SqlForeignKeyDefinitionNode[foreignKeys.Count];
        foreach ( var fk in foreignKeys )
            result[i++] = fk.ToDefinitionNode( table, useFullName );

        return result;
    }

    [Pure]
    public static SqlCheckDefinitionNode[] ToDefinitionRange(this IReadOnlyCollection<ISqlCheckBuilder> checks, bool useFullName = false)
    {
        if ( checks.Count == 0 )
            return Array.Empty<SqlCheckDefinitionNode>();

        var i = 0;
        var result = new SqlCheckDefinitionNode[checks.Count];
        foreach ( var chk in checks )
            result[i++] = chk.ToDefinitionNode( useFullName );

        return result;
    }
}
