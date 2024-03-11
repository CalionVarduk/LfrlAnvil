using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Extensions;

public static class SqlObjectExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T SetFilter<T>(this T index, Func<SqlTableBuilderNode, SqlConditionNode?> filter)
        where T : ISqlIndexBuilder
    {
        index.SetFilter( filter( index.Table.Node ) );
        return index;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlPrimaryKeyBuilder SetPrimaryKey(this ISqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlPrimaryKeyBuilder SetPrimaryKey(
        this ISqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( name, index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder CreateIndex(this ISqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder CreateIndex(
        this ISqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder CreateUniqueIndex(this ISqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( columns, isUnique: true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder CreateUniqueIndex(
        this ISqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns, isUnique: true );
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
    public static bool IsSelfReference(this ISqlForeignKey foreignKey)
    {
        return ReferenceEquals( foreignKey.OriginIndex.Table, foreignKey.ReferencedIndex.Table );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlPrimaryKeyBuilder SetPrimaryKey(this SqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlPrimaryKeyBuilder SetPrimaryKey(
        this SqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( name, index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexBuilder CreateIndex(this SqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexBuilder CreateIndex(
        this SqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexBuilder CreateUniqueIndex(this SqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( columns, isUnique: true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexBuilder CreateUniqueIndex(
        this SqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns, isUnique: true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnBuilder SetType(this SqlColumnBuilder column, ISqlDataType dataType)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByDataType( dataType ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnBuilder SetType(this SqlColumnBuilder column, Type type)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByType( type ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnBuilder SetType<T>(this SqlColumnBuilder column)
    {
        return column.SetType( typeof( T ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnBuilder SetDefaultValue<T>(this SqlColumnBuilder column, T? value)
        where T : notnull
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnBuilder SetDefaultValue<T>(this SqlColumnBuilder column, T? value)
        where T : struct
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T AddStatement<T>(this T changeTracker, string statement)
        where T : ISqlDatabaseChangeTracker
    {
        changeTracker.AddStatement( SqlNode.RawStatement( statement ) );
        return changeTracker;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T Detach<T>(this T changeTracker, bool enabled = true)
        where T : ISqlDatabaseChangeTracker
    {
        changeTracker.Attach( ! enabled );
        return changeTracker;
    }

    [Pure]
    public static SqlCreateViewNode ToCreateNode(this ISqlViewBuilder view, bool replaceIfExists = false)
    {
        return SqlNode.CreateView( view.Info, view.Source, replaceIfExists );
    }

    [Pure]
    public static SqlCreateTableNode ToCreateNode(
        this ISqlTableBuilder table,
        SqlRecordSetInfo? customInfo = null,
        bool includeForeignKeys = true,
        bool sortGeneratedColumns = false,
        bool ifNotExists = false)
    {
        return SqlNode.CreateTable(
            customInfo ?? table.Info,
            table.Columns.ToDefinitionRange( sortGeneratedColumns ),
            ifNotExists,
            t =>
            {
                var constraints = table.Constraints;
                var primaryKey = constraints.TryGetPrimaryKey()?.ToDefinitionNode();

                var result = primaryKey is not null
                    ? SqlCreateTableConstraints.Empty.WithPrimaryKey( primaryKey )
                    : SqlCreateTableConstraints.Empty;

                var checks = new List<SqlCheckDefinitionNode>();
                var foreignKeys = includeForeignKeys ? new List<SqlForeignKeyDefinitionNode>() : null;

                foreach ( var constraint in constraints )
                {
                    switch ( constraint.Type )
                    {
                        case SqlObjectType.Check:
                            checks.Add( ReinterpretCast.To<ISqlCheckBuilder>( constraint ).ToDefinitionNode() );
                            break;

                        case SqlObjectType.ForeignKey:
                            foreignKeys?.Add( ReinterpretCast.To<ISqlForeignKeyBuilder>( constraint ).ToDefinitionNode( t ) );
                            break;
                    }
                }

                result = result.WithChecks( checks.ToArray() );
                if ( foreignKeys is not null )
                    result = result.WithForeignKeys( foreignKeys.ToArray() );

                return result;
            } );
    }

    [Pure]
    public static SqlCreateIndexNode ToCreateNode(this ISqlIndexBuilder index, bool replaceIfExists = false)
    {
        var ixTable = index.Table;
        return SqlNode.CreateIndex(
            SqlSchemaObjectName.Create( ixTable.Schema.Name, index.Name ),
            index.IsUnique,
            ixTable.Node,
            index.Columns.Expressions,
            replaceIfExists,
            index.Filter );
    }

    [Pure]
    public static SqlColumnDefinitionNode ToDefinitionNode(this ISqlColumnBuilder column)
    {
        return SqlNode.Column( column.Name, column.TypeDefinition, column.IsNullable, column.DefaultValue, column.Computation );
    }

    [Pure]
    public static SqlPrimaryKeyDefinitionNode ToDefinitionNode(this ISqlPrimaryKeyBuilder primaryKey)
    {
        return SqlNode.PrimaryKey(
            SqlSchemaObjectName.Create( primaryKey.Table.Schema.Name, primaryKey.Name ),
            primaryKey.Index.Columns.Expressions );
    }

    [Pure]
    public static SqlForeignKeyDefinitionNode ToDefinitionNode(this ISqlForeignKeyBuilder foreignKey, SqlRecordSetNode table)
    {
        var refIndex = foreignKey.ReferencedIndex;
        var fkColumns = foreignKey.OriginIndex.Columns;
        var fkReferencedColumns = refIndex.Columns;
        var isSelfReference = foreignKey.IsSelfReference();

        var i = 0;
        var columns = Array.Empty<SqlDataFieldNode>();
        if ( fkColumns.Expressions.Count > 0 )
        {
            columns = new SqlDataFieldNode[fkColumns.Expressions.Count];
            foreach ( var column in fkColumns )
            {
                Ensure.IsNotNull( column );
                columns[i++] = table[column.Name];
            }
        }

        var referencedColumns = Array.Empty<SqlDataFieldNode>();
        if ( fkReferencedColumns.Expressions.Count > 0 )
        {
            i = 0;
            referencedColumns = new SqlDataFieldNode[fkReferencedColumns.Expressions.Count];
            if ( isSelfReference )
            {
                foreach ( var column in fkReferencedColumns )
                {
                    Ensure.IsNotNull( column );
                    referencedColumns[i++] = table[column.Name];
                }
            }
            else
            {
                foreach ( var column in fkReferencedColumns )
                {
                    Ensure.IsNotNull( column );
                    referencedColumns[i++] = column.Node;
                }
            }
        }

        return SqlNode.ForeignKey(
            SqlSchemaObjectName.Create( foreignKey.Table.Schema.Name, foreignKey.Name ),
            columns,
            refIndex.Table.Node,
            referencedColumns,
            foreignKey.OnDeleteBehavior,
            foreignKey.OnUpdateBehavior );
    }

    [Pure]
    public static SqlCheckDefinitionNode ToDefinitionNode(this ISqlCheckBuilder check)
    {
        return SqlNode.Check( SqlSchemaObjectName.Create( check.Table.Schema.Name, check.Name ), check.Condition );
    }

    [Pure]
    public static SqlColumnDefinitionNode[] ToDefinitionRange(
        this IReadOnlyCollection<ISqlColumnBuilder> columns,
        bool sortGeneratedColumns = false)
    {
        if ( columns.Count == 0 )
            return Array.Empty<SqlColumnDefinitionNode>();

        var result = new SqlColumnDefinitionNode[columns.Count];

        if ( sortGeneratedColumns )
        {
            var nonGeneratedColumnCount = 0;
            var sortedColumns = columns.ToArray();
            for ( var i = 0; i < sortedColumns.Length; ++i )
            {
                var column = sortedColumns[i];
                if ( column.Computation is not null )
                    continue;

                sortedColumns[i] = sortedColumns[nonGeneratedColumnCount];
                sortedColumns[nonGeneratedColumnCount++] = column;
            }

            var generatedColumns = sortedColumns.AsSpan( nonGeneratedColumnCount );
            if ( generatedColumns.Length > 1 )
            {
                var sortInfoByColumnName = new Dictionary<string, ColumnDefinitionSortInfo>(
                    comparer: SqlHelpers.NameComparer,
                    capacity: generatedColumns.Length );

                for ( var i = 0; i < generatedColumns.Length; ++i )
                {
                    var column = generatedColumns[i];
                    sortInfoByColumnName.Add( column.Name, new ColumnDefinitionSortInfo( column ) );
                }

                foreach ( var sortInfo in sortInfoByColumnName.Values )
                {
                    foreach ( var column in sortInfo.Column.ReferencedComputationColumns )
                    {
                        if ( sortInfoByColumnName.TryGetValue( column.Name, out var columnSortInfo ) )
                            columnSortInfo.AddParent( sortInfo );
                    }
                }

                var j = 0;
                foreach ( var sortInfo in sortInfoByColumnName.Values.OrderDescending() )
                    generatedColumns[j++] = sortInfo.Column;
            }

            for ( var i = 0; i < sortedColumns.Length; ++i )
                result[i] = sortedColumns[i].ToDefinitionNode();
        }
        else
        {
            var i = 0;
            foreach ( var column in columns )
                result[i++] = column.ToDefinitionNode();
        }

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectOriginalValue<T> GetOriginalValue<T>(
        this ISqlDatabaseChangeTracker changeTracker,
        SqlObjectBuilder target,
        SqlObjectChangeDescriptor<T> descriptor)
    {
        return changeTracker.TryGetOriginalValue( target, descriptor, out var result )
            ? SqlObjectOriginalValue<T>.Create( (T)result! )
            : SqlObjectOriginalValue<T>.CreateEmpty();
    }

    private sealed class ColumnDefinitionSortInfo : IComparable<ColumnDefinitionSortInfo>
    {
        internal readonly ISqlColumnBuilder Column;
        private Chain<ColumnDefinitionSortInfo> _parents;
        private int _depth;

        internal ColumnDefinitionSortInfo(ISqlColumnBuilder column)
        {
            Column = column;
            _parents = Chain<ColumnDefinitionSortInfo>.Empty;
            _depth = -1;
        }

        internal int Depth
        {
            get
            {
                if ( _depth != -1 )
                    return _depth;

                _depth = 0;
                foreach ( var parent in _parents )
                    _depth = Math.Max( _depth, parent.Depth + 1 );

                return _depth;
            }
        }

        [Pure]
        public int CompareTo(ColumnDefinitionSortInfo? other)
        {
            return other is null ? 1 : Depth.CompareTo( other.Depth );
        }

        internal void AddParent(ColumnDefinitionSortInfo info)
        {
            Assume.Equals( _depth, -1 );
            _parents = _parents.Extend( info );
        }
    }
}
