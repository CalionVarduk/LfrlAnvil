using System;
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
        return column.SetType( column.Database.TypeDefinitions.GetDefaultForDataType( dataType ) );
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
        bool useFullConstraintNames = false)
    {
        var i = 0;
        var tableColumns = table.Columns;
        var columns = Array.Empty<SqlColumnDefinitionNode>();
        if ( tableColumns.Count > 0 )
        {
            columns = new SqlColumnDefinitionNode[tableColumns.Count];
            foreach ( var column in tableColumns )
                columns[i++] = column.ToDefinitionNode();
        }

        return SqlNode.CreateTable(
            customInfo ?? table.Info,
            columns,
            ifNotExists: false,
            t =>
            {
                var tableForeignKeys = table.ForeignKeys;
                var tableChecks = table.Checks;
                var primaryKey = table.PrimaryKey?.ToDefinitionNode( t, useFullConstraintNames );
                var foreignKeys = Array.Empty<SqlForeignKeyDefinitionNode>();
                var checks = Array.Empty<SqlCheckDefinitionNode>();
                if ( tableForeignKeys.Count > 0 )
                {
                    var j = 0;
                    foreignKeys = new SqlForeignKeyDefinitionNode[tableForeignKeys.Count];
                    foreach ( var foreignKey in tableForeignKeys )
                        foreignKeys[j++] = foreignKey.ToDefinitionNode( t, useFullConstraintNames );
                }

                if ( tableChecks.Count > 0 )
                {
                    var j = 0;
                    checks = new SqlCheckDefinitionNode[tableChecks.Count];
                    foreach ( var check in tableChecks )
                        checks[j++] = check.ToDefinitionNode( useFullConstraintNames );
                }

                var result = SqlCreateTableConstraints.Empty.WithForeignKeys( foreignKeys ).WithChecks( checks );
                return primaryKey is not null ? result.WithPrimaryKey( primaryKey ) : result;
            } );
    }

    [Pure]
    public static SqlCreateIndexNode ToCreateNode(this ISqlIndexBuilder index)
    {
        var ixColumns = index.Columns;
        var columns = Array.Empty<SqlOrderByNode>();
        if ( ixColumns.Length > 0 )
        {
            var i = 0;
            columns = new SqlOrderByNode[ixColumns.Length];
            foreach ( var column in ixColumns )
                columns[i++] = SqlNode.OrderBy( column.Column.Node, column.Ordering );
        }

        var ixTable = index.Table;
        return SqlNode.CreateIndex(
            SqlSchemaObjectName.Create( ixTable.Schema.Name, index.Name ),
            index.IsUnique,
            ixTable.RecordSet,
            columns,
            ifNotExists: false,
            index.Filter );
    }

    [Pure]
    private static SqlColumnDefinitionNode ToDefinitionNode(this ISqlColumnBuilder column)
    {
        return SqlNode.Column(
            column.Name,
            TypeNullability.Create( column.TypeDefinition.RuntimeType, column.IsNullable ),
            column.DefaultValue );
    }

    [Pure]
    private static SqlPrimaryKeyDefinitionNode ToDefinitionNode(
        this ISqlPrimaryKeyBuilder primaryKey,
        SqlNewTableNode table,
        bool useFullName)
    {
        var pkColumns = primaryKey.Index.Columns;
        var columns = Array.Empty<SqlOrderByNode>();
        if ( pkColumns.Length > 0 )
        {
            var i = 0;
            columns = new SqlOrderByNode[pkColumns.Length];
            foreach ( var column in pkColumns )
                columns[i++] = SqlNode.OrderBy( table[column.Column.Name], column.Ordering );
        }

        return SqlNode.PrimaryKey( useFullName ? primaryKey.FullName : primaryKey.Name, columns );
    }

    [Pure]
    private static SqlForeignKeyDefinitionNode ToDefinitionNode(
        this ISqlForeignKeyBuilder foreignKey,
        SqlNewTableNode table,
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
    private static SqlCheckDefinitionNode ToDefinitionNode(this ISqlCheckBuilder check, bool useFullName)
    {
        return SqlNode.Check( useFullName ? check.FullName : check.Name, check.Condition );
    }
}
