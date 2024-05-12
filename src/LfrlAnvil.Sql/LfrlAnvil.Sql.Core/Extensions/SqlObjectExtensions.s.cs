using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Extensions;

/// <summary>
/// Contains various <see cref="ISqlObjectBuilder"/> and <see cref="ISqlObject"/> extension methods.
/// </summary>
public static class SqlObjectExtensions
{
    /// <summary>
    /// Changes <see cref="ISqlIndexBuilder.Filter"/> value of the provided <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Source index.</param>
    /// <param name="filter">Value to set.</param>
    /// <typeparam name="T">SQL index builder type.</typeparam>
    /// <returns><paramref name="index"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When filter cannot be changed.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T SetFilter<T>(this T index, Func<SqlTableBuilderNode, SqlConditionNode?> filter)
        where T : ISqlIndexBuilder
    {
        index.SetFilter( filter( index.Table.Node ) );
        return index;
    }

    /// <summary>
    /// Creates a new unique index builder with a default name and sets a new primary key builder with a default name based on that index.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="columns">Collection of columns that define the underlying index.</param>
    /// <returns>New <see cref="ISqlPrimaryKeyBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When unique index constraint or primary key constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlPrimaryKeyBuilder SetPrimaryKey(this ISqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( index );
    }

    /// <summary>
    /// Creates a new unique index builder with a default name and sets a new primary key builder based on that index.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="name">Name of the primary key constraint.</param>
    /// <param name="columns">Collection of columns that define the underlying index.</param>
    /// <returns>New <see cref="ISqlPrimaryKeyBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When unique index constraint or primary key constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlPrimaryKeyBuilder SetPrimaryKey(
        this ISqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( name, index );
    }

    /// <summary>
    /// Creates a new index builder with a default name.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <returns>New <see cref="ISqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder CreateIndex(this ISqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( columns );
    }

    /// <summary>
    /// Creates a new index builder.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="name">Name of the index constraint.</param>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <returns>New <see cref="ISqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder CreateIndex(
        this ISqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns );
    }

    /// <summary>
    /// Creates a new unique index builder with a default name.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <returns>New <see cref="ISqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder CreateUniqueIndex(this ISqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( columns, isUnique: true );
    }

    /// <summary>
    /// Creates a new unique index builder.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="name">Name of the index constraint.</param>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <returns>New <see cref="ISqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder CreateUniqueIndex(
        this ISqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns, isUnique: true );
    }

    /// <summary>
    /// Changes <see cref="ISqlColumnBuilder.TypeDefinition"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="dataType"><see cref="ISqlDataType"/> to use for retrieving a default type definition associated with it.</param>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When type definition cannot be changed.</exception>
    /// <remarks>Changing the type will reset the <see cref="ISqlColumnBuilder.DefaultValue"/> to null.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlColumnBuilder SetType(this ISqlColumnBuilder column, ISqlDataType dataType)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByDataType( dataType ) );
    }

    /// <summary>
    /// Changes <see cref="ISqlColumnBuilder.TypeDefinition"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="type">Runtime type to use for retrieving a type definition associated with it.</param>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When type definition cannot be changed.</exception>
    /// <remarks>Changing the type will reset the <see cref="ISqlColumnBuilder.DefaultValue"/> to null.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlColumnBuilder SetType(this ISqlColumnBuilder column, Type type)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByType( type ) );
    }

    /// <summary>
    /// Changes <see cref="ISqlColumnBuilder.TypeDefinition"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <typeparam name="T">Runtime type to use for retrieving a type definition associated with it.</typeparam>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When type definition cannot be changed.</exception>
    /// <remarks>Changing the type will reset the <see cref="ISqlColumnBuilder.DefaultValue"/> to null.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlColumnBuilder SetType<T>(this ISqlColumnBuilder column)
    {
        return column.SetType( typeof( T ) );
    }

    /// <summary>
    /// Changes <see cref="ISqlColumnBuilder.DefaultValue"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="value">Value to set.</param>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When default value cannot be changed.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlColumnBuilder SetDefaultValue<T>(this ISqlColumnBuilder column, T? value)
        where T : notnull
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    /// <summary>
    /// Changes <see cref="ISqlColumnBuilder.DefaultValue"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="value">Value to set.</param>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When default value cannot be changed.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlColumnBuilder SetDefaultValue<T>(this ISqlColumnBuilder column, T? value)
        where T : struct
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    /// <summary>
    /// Checks whether or not <see cref="ISqlForeignKeyBuilder.OriginIndex"/> and <see cref="ISqlForeignKeyBuilder.ReferencedIndex"/>
    /// of the provided <paramref name="foreignKey"/> belong to the same table.
    /// </summary>
    /// <param name="foreignKey">Foreign key to check.</param>
    /// <returns><b>true</b> when foreign key is a self-reference, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsSelfReference(this ISqlForeignKeyBuilder foreignKey)
    {
        return ReferenceEquals( foreignKey.OriginIndex.Table, foreignKey.ReferencedIndex.Table );
    }

    /// <summary>
    /// Checks whether or not <see cref="ISqlForeignKey.OriginIndex"/> and <see cref="ISqlForeignKey.ReferencedIndex"/>
    /// of the provided <paramref name="foreignKey"/> belong to the same table.
    /// </summary>
    /// <param name="foreignKey">Foreign key to check.</param>
    /// <returns><b>true</b> when foreign key is a self-reference, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsSelfReference(this ISqlForeignKey foreignKey)
    {
        return ReferenceEquals( foreignKey.OriginIndex.Table, foreignKey.ReferencedIndex.Table );
    }

    /// <summary>
    /// Creates a new unique index builder with a default name and sets a new primary key builder with a default name based on that index.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="columns">Collection of columns that define the underlying index.</param>
    /// <returns>New <see cref="SqlPrimaryKeyBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When unique index constraint or primary key constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlPrimaryKeyBuilder SetPrimaryKey(this SqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( index );
    }

    /// <summary>
    /// Creates a new unique index builder with a default name and sets a new primary key builder based on that index.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="name">Name of the primary key constraint.</param>
    /// <param name="columns">Collection of columns that define the underlying index.</param>
    /// <returns>New <see cref="SqlPrimaryKeyBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When unique index constraint or primary key constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlPrimaryKeyBuilder SetPrimaryKey(
        this SqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( name, index );
    }

    /// <summary>
    /// Creates a new index builder with a default name.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <returns>New <see cref="SqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexBuilder CreateIndex(this SqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( columns );
    }

    /// <summary>
    /// Creates a new index builder.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="name">Name of the index constraint.</param>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <returns>New <see cref="SqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexBuilder CreateIndex(
        this SqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns );
    }

    /// <summary>
    /// Creates a new unique index builder with a default name.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <returns>New <see cref="SqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexBuilder CreateUniqueIndex(this SqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( columns, isUnique: true );
    }

    /// <summary>
    /// Creates a new unique index builder.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="name">Name of the index constraint.</param>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <returns>New <see cref="SqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexBuilder CreateUniqueIndex(
        this SqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns, isUnique: true );
    }

    /// <summary>
    /// Changes <see cref="SqlColumnBuilder.TypeDefinition"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="dataType"><see cref="ISqlDataType"/> to use for retrieving a default type definition associated with it.</param>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When type definition cannot be changed.</exception>
    /// <remarks>Changing the type will reset the <see cref="SqlColumnBuilder.DefaultValue"/> to null.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnBuilder SetType(this SqlColumnBuilder column, ISqlDataType dataType)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByDataType( dataType ) );
    }

    /// <summary>
    /// Changes <see cref="SqlColumnBuilder.TypeDefinition"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="type">Runtime type to use for retrieving a type definition associated with it.</param>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When type definition cannot be changed.</exception>
    /// <remarks>Changing the type will reset the <see cref="SqlColumnBuilder.DefaultValue"/> to null.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnBuilder SetType(this SqlColumnBuilder column, Type type)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByType( type ) );
    }

    /// <summary>
    /// Changes <see cref="SqlColumnBuilder.TypeDefinition"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <typeparam name="T">Runtime type to use for retrieving a type definition associated with it.</typeparam>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When type definition cannot be changed.</exception>
    /// <remarks>Changing the type will reset the <see cref="SqlColumnBuilder.DefaultValue"/> to null.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnBuilder SetType<T>(this SqlColumnBuilder column)
    {
        return column.SetType( typeof( T ) );
    }

    /// <summary>
    /// Changes <see cref="SqlColumnBuilder.DefaultValue"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="value">Value to set.</param>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When default value cannot be changed.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnBuilder SetDefaultValue<T>(this SqlColumnBuilder column, T? value)
        where T : notnull
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    /// <summary>
    /// Changes <see cref="SqlColumnBuilder.DefaultValue"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="value">Value to set.</param>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When default value cannot be changed.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnBuilder SetDefaultValue<T>(this SqlColumnBuilder column, T? value)
        where T : struct
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    /// <summary>
    /// Adds a custom SQL <paramref name="statement"/> to the provided <paramref name="changeTracker"/>.
    /// </summary>
    /// <param name="changeTracker">Source change tracker.</param>
    /// <param name="statement">SQL statement to add.</param>
    /// <returns><paramref name="changeTracker"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T AddStatement<T>(this T changeTracker, string statement)
        where T : ISqlDatabaseChangeTracker
    {
        changeTracker.AddStatement( SqlNode.RawStatement( statement ) );
        return changeTracker;
    }

    /// <summary>
    /// Changes <see cref="ISqlDatabaseChangeTracker.IsAttached"/> value for the provided <paramref name="changeTracker"/>.
    /// </summary>
    /// <param name="changeTracker">Source change tracker.</param>
    /// <param name="enabled">
    /// Value to unset. <b>true</b> means that the change tracker will be detached. Equal to <b>true</b> by default.
    /// </param>
    /// <returns><paramref name="changeTracker"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T Detach<T>(this T changeTracker, bool enabled = true)
        where T : ISqlDatabaseChangeTracker
    {
        changeTracker.Attach( ! enabled );
        return changeTracker;
    }

    /// <summary>
    /// Creates a new <see cref="SqlCreateViewNode"/> instance from the provided <paramref name="view"/>.
    /// </summary>
    /// <param name="view">Source view.</param>
    /// <param name="replaceIfExists">
    /// Specifies the <see cref="SqlCreateViewNode.ReplaceIfExists"/> value. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="SqlCreateViewNode"/> instance.</returns>
    [Pure]
    public static SqlCreateViewNode ToCreateNode(this ISqlViewBuilder view, bool replaceIfExists = false)
    {
        return SqlNode.CreateView( view.Info, view.Source, replaceIfExists );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCreateTableNode"/> instance from the provided <paramref name="table"/>.
    /// </summary>
    /// <param name="table">Source table.</param>
    /// <param name="customInfo">
    /// Custom <see cref="SqlRecordSetInfo"/> to use as <see cref="SqlCreateTableNode.Info"/> of the result. Equal to null by default.
    /// </param>
    /// <param name="includeForeignKeys">
    /// Specifies whether or not foreign key constrains should be included in the result. Equal to <b>true</b> by default.
    /// </param>
    /// <param name="sortComputedColumns">
    /// Specifies whether or not columns with non-null <see cref="ISqlColumnBuilder.Computation"/> should be sorted
    /// by their computation reference depth and added to the end of the resulting collection of columns. Equal to <b>false</b> by default.
    /// </param>
    /// <param name="ifNotExists">
    /// Specifies the <see cref="SqlCreateTableNode.IfNotExists"/> value. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="SqlCreateTableNode"/> instance.</returns>
    [Pure]
    public static SqlCreateTableNode ToCreateNode(
        this ISqlTableBuilder table,
        SqlRecordSetInfo? customInfo = null,
        bool includeForeignKeys = true,
        bool sortComputedColumns = false,
        bool ifNotExists = false)
    {
        return SqlNode.CreateTable(
            customInfo ?? table.Info,
            table.Columns.ToDefinitionRange( sortComputedColumns ),
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

    /// <summary>
    /// Creates a new <see cref="SqlCreateIndexNode"/> instance from the provided <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Source index.</param>
    /// <param name="replaceIfExists">
    /// Specifies the <see cref="SqlCreateIndexNode.ReplaceIfExists"/> value. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="SqlCreateIndexNode"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="SqlColumnDefinitionNode"/> instance from the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <returns>New <see cref="SqlColumnDefinitionNode"/> instance.</returns>
    [Pure]
    public static SqlColumnDefinitionNode ToDefinitionNode(this ISqlColumnBuilder column)
    {
        return SqlNode.Column( column.Name, column.TypeDefinition, column.IsNullable, column.DefaultValue, column.Computation );
    }

    /// <summary>
    /// Creates a new <see cref="SqlPrimaryKeyDefinitionNode"/> instance from the provided <paramref name="primaryKey"/>.
    /// </summary>
    /// <param name="primaryKey">Source primary key.</param>
    /// <returns>New <see cref="SqlPrimaryKeyDefinitionNode"/> instance.</returns>
    [Pure]
    public static SqlPrimaryKeyDefinitionNode ToDefinitionNode(this ISqlPrimaryKeyBuilder primaryKey)
    {
        return SqlNode.PrimaryKey(
            SqlSchemaObjectName.Create( primaryKey.Table.Schema.Name, primaryKey.Name ),
            primaryKey.Index.Columns.Expressions );
    }

    /// <summary>
    /// Creates a new <see cref="SqlForeignKeyDefinitionNode"/> instance from the provided <paramref name="foreignKey"/>.
    /// </summary>
    /// <param name="foreignKey">Source foreign key.</param>
    /// <param name="table">SQL record set node that represents the foreign key's <see cref="ISqlConstraintBuilder.Table"/>.</param>
    /// <returns>New <see cref="SqlForeignKeyDefinitionNode"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="SqlCheckDefinitionNode"/> instance from the provided <paramref name="check"/>.
    /// </summary>
    /// <param name="check">Source check.</param>
    /// <returns>New <see cref="SqlCheckDefinitionNode"/> instance.</returns>
    [Pure]
    public static SqlCheckDefinitionNode ToDefinitionNode(this ISqlCheckBuilder check)
    {
        return SqlNode.Check( SqlSchemaObjectName.Create( check.Table.Schema.Name, check.Name ), check.Condition );
    }

    /// <summary>
    /// Creates a new collection of <see cref="SqlColumnDefinitionNode"/> instances from the provided <paramref name="columns"/>.
    /// </summary>
    /// <param name="columns">Source collection of columns.</param>
    /// <param name="sortComputedColumns">
    /// Specifies whether or not columns with non-null <see cref="ISqlColumnBuilder.Computation"/> should be sorted
    /// by their computation reference depth and added to the end of the resulting collection. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New collection of <see cref="SqlColumnDefinitionNode"/> instances.</returns>
    [Pure]
    public static SqlColumnDefinitionNode[] ToDefinitionRange(
        this IReadOnlyCollection<ISqlColumnBuilder> columns,
        bool sortComputedColumns = false)
    {
        if ( columns.Count == 0 )
            return Array.Empty<SqlColumnDefinitionNode>();

        var result = new SqlColumnDefinitionNode[columns.Count];

        if ( sortComputedColumns )
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

    /// <summary>
    /// Creates a new <see cref="SqlObjectOriginalValue{T}"/> instance associated with the given <paramref name="target"/>
    /// and its property's <paramref name="descriptor"/>.
    /// </summary>
    /// <param name="changeTracker">Source change tracker.</param>
    /// <param name="target">Object to check.</param>
    /// <param name="descriptor">Property change descriptor.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlObjectOriginalValue{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectOriginalValue<T> GetOriginalValue<T>(
        this ISqlDatabaseChangeTracker changeTracker,
        SqlObjectBuilder target,
        SqlObjectChangeDescriptor<T> descriptor)
    {
        return changeTracker.TryGetOriginalValue( target, descriptor, out var result )
            ? SqlObjectOriginalValue<T>.Create( ( T )result! )
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
