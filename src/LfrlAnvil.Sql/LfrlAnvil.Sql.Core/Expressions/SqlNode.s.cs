using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Expressions;

public static partial class SqlNode
{
    private static SqlNullNode? _null;
    private static SqlTrueNode? _true;
    private static SqlFalseNode? _false;
    private static SqlDistinctTraitNode? _distinct;
    private static SqlDummyDataSourceNode? _dummyDataSource;
    private static SqlCommitTransactionNode? _commitTransaction;
    private static SqlRollbackTransactionNode? _rollbackTransaction;

    [Pure]
    public static SqlExpressionNode Literal<T>(T? value)
        where T : notnull
    {
        return Generic<T>.IsNull( value ) ? Null() : new SqlLiteralNode<T>( value );
    }

    [Pure]
    public static SqlExpressionNode Literal<T>(T? value)
        where T : struct
    {
        return value is null ? Null() : new SqlLiteralNode<T>( value.Value );
    }

    [Pure]
    public static SqlNullNode Null()
    {
        return _null ??= new SqlNullNode();
    }

    [Pure]
    public static SqlParameterNode Parameter<T>(string name, bool isNullable = false)
    {
        return Parameter( name, TypeNullability.Create( typeof( T ), isNullable ) );
    }

    [Pure]
    public static SqlParameterNode Parameter(string name, TypeNullability? type = null)
    {
        return new SqlParameterNode( name, type );
    }

    [Pure]
    public static SqlParameterNode[] ParameterRange<T>(string name, int count, bool isNullable = false)
    {
        return ParameterRange( name, count, TypeNullability.Create( typeof( T ), isNullable ) );
    }

    [Pure]
    public static SqlParameterNode[] ParameterRange(string name, int count, TypeNullability? type = null)
    {
        Ensure.IsGreaterThanOrEqualTo( count, 0 );
        if ( count == 0 )
            return Array.Empty<SqlParameterNode>();

        var result = new SqlParameterNode[count];
        for ( var i = 0; i < count; ++i )
            result[i] = Parameter( $"{name}{i + 1}", type );

        return result;
    }

    [Pure]
    public static SqlTypeCastExpressionNode TypeCast(SqlExpressionNode expression, Type type)
    {
        return new SqlTypeCastExpressionNode( expression, type );
    }

    [Pure]
    public static SqlTypeCastExpressionNode TypeCast(SqlExpressionNode expression, ISqlColumnTypeDefinition typeDefinition)
    {
        return new SqlTypeCastExpressionNode( expression, typeDefinition );
    }

    [Pure]
    public static SqlNegateExpressionNode Negate(SqlExpressionNode value)
    {
        return new SqlNegateExpressionNode( value );
    }

    [Pure]
    public static SqlBitwiseNotExpressionNode BitwiseNot(SqlExpressionNode value)
    {
        return new SqlBitwiseNotExpressionNode( value );
    }

    [Pure]
    public static SqlAddExpressionNode Add(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlAddExpressionNode( left, right );
    }

    [Pure]
    public static SqlConcatExpressionNode Concat(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlConcatExpressionNode( left, right );
    }

    [Pure]
    public static SqlSubtractExpressionNode Subtract(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlSubtractExpressionNode( left, right );
    }

    [Pure]
    public static SqlMultiplyExpressionNode Multiply(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlMultiplyExpressionNode( left, right );
    }

    [Pure]
    public static SqlDivideExpressionNode Divide(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlDivideExpressionNode( left, right );
    }

    [Pure]
    public static SqlModuloExpressionNode Modulo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlModuloExpressionNode( left, right );
    }

    [Pure]
    public static SqlBitwiseAndExpressionNode BitwiseAnd(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseAndExpressionNode( left, right );
    }

    [Pure]
    public static SqlBitwiseOrExpressionNode BitwiseOr(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseOrExpressionNode( left, right );
    }

    [Pure]
    public static SqlBitwiseXorExpressionNode BitwiseXor(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseXorExpressionNode( left, right );
    }

    [Pure]
    public static SqlBitwiseLeftShiftExpressionNode BitwiseLeftShift(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseLeftShiftExpressionNode( left, right );
    }

    [Pure]
    public static SqlBitwiseRightShiftExpressionNode BitwiseRightShift(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseRightShiftExpressionNode( left, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRawExpressionNode RawExpression(string sql, params SqlParameterNode[] parameters)
    {
        return RawExpression( sql, type: null, parameters );
    }

    [Pure]
    public static SqlRawExpressionNode RawExpression(string sql, TypeNullability? type, params SqlParameterNode[] parameters)
    {
        return new SqlRawExpressionNode( sql, type, parameters );
    }

    [Pure]
    public static SqlEqualToConditionNode EqualTo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlEqualToConditionNode( left, right );
    }

    [Pure]
    public static SqlNotEqualToConditionNode NotEqualTo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlNotEqualToConditionNode( left, right );
    }

    [Pure]
    public static SqlGreaterThanConditionNode GreaterThan(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlGreaterThanConditionNode( left, right );
    }

    [Pure]
    public static SqlLessThanConditionNode LessThan(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlLessThanConditionNode( left, right );
    }

    [Pure]
    public static SqlGreaterThanOrEqualToConditionNode GreaterThanOrEqualTo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlGreaterThanOrEqualToConditionNode( left, right );
    }

    [Pure]
    public static SqlLessThanOrEqualToConditionNode LessThanOrEqualTo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlLessThanOrEqualToConditionNode( left, right );
    }

    [Pure]
    public static SqlBetweenConditionNode Between(SqlExpressionNode left, SqlExpressionNode min, SqlExpressionNode max)
    {
        return new SqlBetweenConditionNode( left, min, max, isNegated: false );
    }

    [Pure]
    public static SqlBetweenConditionNode NotBetween(SqlExpressionNode left, SqlExpressionNode min, SqlExpressionNode max)
    {
        return new SqlBetweenConditionNode( left, min, max, isNegated: true );
    }

    [Pure]
    public static SqlLikeConditionNode Like(SqlExpressionNode value, SqlExpressionNode pattern, SqlExpressionNode? escape = null)
    {
        return new SqlLikeConditionNode( value, pattern, escape, isNegated: false );
    }

    [Pure]
    public static SqlLikeConditionNode NotLike(SqlExpressionNode value, SqlExpressionNode pattern, SqlExpressionNode? escape = null)
    {
        return new SqlLikeConditionNode( value, pattern, escape, isNegated: true );
    }

    [Pure]
    public static SqlExistsConditionNode Exists(SqlQueryExpressionNode query)
    {
        return new SqlExistsConditionNode( query, isNegated: false );
    }

    [Pure]
    public static SqlExistsConditionNode NotExists(SqlQueryExpressionNode query)
    {
        return new SqlExistsConditionNode( query, isNegated: true );
    }

    [Pure]
    public static SqlConditionNode In(SqlExpressionNode value, params SqlExpressionNode[] expressions)
    {
        return expressions.Length == 0 ? False() : new SqlInConditionNode( value, expressions, isNegated: false );
    }

    [Pure]
    public static SqlConditionNode NotIn(SqlExpressionNode value, params SqlExpressionNode[] expressions)
    {
        return expressions.Length == 0 ? True() : new SqlInConditionNode( value, expressions, isNegated: true );
    }

    [Pure]
    public static SqlInQueryConditionNode InQuery(SqlExpressionNode value, SqlQueryExpressionNode query)
    {
        return new SqlInQueryConditionNode( value, query, isNegated: false );
    }

    [Pure]
    public static SqlInQueryConditionNode NotInQuery(SqlExpressionNode value, SqlQueryExpressionNode query)
    {
        return new SqlInQueryConditionNode( value, query, isNegated: true );
    }

    [Pure]
    public static SqlTrueNode True()
    {
        return _true ??= new SqlTrueNode();
    }

    [Pure]
    public static SqlFalseNode False()
    {
        return _false ??= new SqlFalseNode();
    }

    [Pure]
    public static SqlRawConditionNode RawCondition(string sql, params SqlParameterNode[] parameters)
    {
        return new SqlRawConditionNode( sql, parameters );
    }

    [Pure]
    public static SqlConditionValueNode Value(SqlConditionNode condition)
    {
        return new SqlConditionValueNode( condition );
    }

    [Pure]
    public static SqlAndConditionNode And(SqlConditionNode left, SqlConditionNode right)
    {
        return new SqlAndConditionNode( left, right );
    }

    [Pure]
    public static SqlOrConditionNode Or(SqlConditionNode left, SqlConditionNode right)
    {
        return new SqlOrConditionNode( left, right );
    }

    [Pure]
    public static SqlTableNode Table(ISqlTable value, string? alias = null)
    {
        return new SqlTableNode( value, alias, isOptional: false );
    }

    [Pure]
    public static SqlTableBuilderNode Table(ISqlTableBuilder value, string? alias = null)
    {
        return new SqlTableBuilderNode( value, alias, isOptional: false );
    }

    [Pure]
    public static SqlViewNode View(ISqlView value, string? alias = null)
    {
        return new SqlViewNode( value, alias, isOptional: false );
    }

    [Pure]
    public static SqlViewBuilderNode View(ISqlViewBuilder value, string? alias = null)
    {
        return new SqlViewBuilderNode( value, alias, isOptional: false );
    }

    [Pure]
    public static SqlRawRecordSetNode RawRecordSet(string name, string? alias = null)
    {
        return new SqlRawRecordSetNode( name, alias, isOptional: false );
    }

    [Pure]
    public static SqlRawRecordSetNode RawRecordSet(SqlRecordSetInfo info, string? alias = null)
    {
        return new SqlRawRecordSetNode( info, alias, isOptional: false );
    }

    [Pure]
    public static SqlRawDataFieldNode RawDataField(SqlRecordSetNode recordSet, string name, TypeNullability? type = null)
    {
        return new SqlRawDataFieldNode( recordSet, name, type );
    }

    [Pure]
    public static SqlRawQueryExpressionNode RawQuery(string sql, params SqlParameterNode[] parameters)
    {
        return new SqlRawQueryExpressionNode( sql, parameters );
    }

    [Pure]
    public static SqlFilterTraitNode FilterTrait(SqlConditionNode filter, bool isConjunction)
    {
        return new SqlFilterTraitNode( filter, isConjunction );
    }

    [Pure]
    public static SqlAggregationTraitNode AggregationTrait(params SqlExpressionNode[] expressions)
    {
        return new SqlAggregationTraitNode( expressions );
    }

    [Pure]
    public static SqlAggregationFilterTraitNode AggregationFilterTrait(SqlConditionNode filter, bool isConjunction)
    {
        return new SqlAggregationFilterTraitNode( filter, isConjunction );
    }

    [Pure]
    public static SqlDistinctTraitNode DistinctTrait()
    {
        return _distinct ??= new SqlDistinctTraitNode();
    }

    [Pure]
    public static SqlSortTraitNode SortTrait(params SqlOrderByNode[] ordering)
    {
        return new SqlSortTraitNode( ordering );
    }

    [Pure]
    public static SqlLimitTraitNode LimitTrait(SqlExpressionNode value)
    {
        return new SqlLimitTraitNode( value );
    }

    [Pure]
    public static SqlOffsetTraitNode OffsetTrait(SqlExpressionNode value)
    {
        return new SqlOffsetTraitNode( value );
    }

    [Pure]
    public static SqlCommonTableExpressionTraitNode CommonTableExpressionTrait(
        params SqlCommonTableExpressionNode[] commonTableExpressions)
    {
        return new SqlCommonTableExpressionTraitNode( commonTableExpressions );
    }

    [Pure]
    public static SqlWindowDefinitionTraitNode WindowDefinitionTrait(params SqlWindowDefinitionNode[] windows)
    {
        return new SqlWindowDefinitionTraitNode( windows );
    }

    [Pure]
    public static SqlWindowTraitNode WindowTrait(SqlWindowDefinitionNode definition)
    {
        return new SqlWindowTraitNode( definition );
    }

    [Pure]
    public static SqlWindowDefinitionNode WindowDefinition(string name, SqlOrderByNode[] ordering, SqlWindowFrameNode? frame = null)
    {
        return WindowDefinition( name, Array.Empty<SqlExpressionNode>(), ordering, frame );
    }

    [Pure]
    public static SqlWindowDefinitionNode WindowDefinition(
        string name,
        SqlExpressionNode[] partitioning,
        SqlOrderByNode[] ordering,
        SqlWindowFrameNode? frame = null)
    {
        return new SqlWindowDefinitionNode( name, partitioning, ordering, frame );
    }

    [Pure]
    public static SqlWindowFrameNode RowsWindowFrame(SqlWindowFrameBoundary start, SqlWindowFrameBoundary end)
    {
        return new SqlWindowFrameNode( SqlWindowFrameType.Rows, start, end );
    }

    [Pure]
    public static SqlWindowFrameNode RangeWindowFrame(SqlWindowFrameBoundary start, SqlWindowFrameBoundary end)
    {
        return new SqlWindowFrameNode( SqlWindowFrameType.Range, start, end );
    }

    [Pure]
    public static SqlSelectFieldNode Select(SqlExpressionNode expression, string alias)
    {
        return new SqlSelectFieldNode( expression, alias );
    }

    [Pure]
    public static SqlSelectFieldNode Select(SqlDataFieldNode dataField, string? alias = null)
    {
        return new SqlSelectFieldNode( dataField, alias );
    }

    [Pure]
    public static SqlSelectRecordSetNode SelectAll(SqlRecordSetNode recordSet)
    {
        return new SqlSelectRecordSetNode( recordSet );
    }

    [Pure]
    public static SqlSelectAllNode SelectAll(SqlDataSourceNode dataSource)
    {
        return new SqlSelectAllNode( dataSource );
    }

    [Pure]
    public static SqlSelectExpressionNode SelectExpression(SqlSelectNode selectNode)
    {
        return new SqlSelectExpressionNode( selectNode );
    }

    [Pure]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Query<TDataSourceNode>(
        TDataSourceNode dataSource,
        params SqlSelectNode[] selection)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlDataSourceQueryExpressionNode<TDataSourceNode>( dataSource, selection );
    }

    [Pure]
    public static SqlCompoundQueryExpressionNode CompoundQuery(
        SqlQueryExpressionNode firstQuery,
        params SqlCompoundQueryComponentNode[] followingQueries)
    {
        return new SqlCompoundQueryExpressionNode( firstQuery, followingQueries );
    }

    [Pure]
    public static SqlNamedFunctionRecordSetNode NamedFunctionRecordSet(SqlNamedFunctionExpressionNode function, string alias)
    {
        return new SqlNamedFunctionRecordSetNode( function, alias, isOptional: false );
    }

    [Pure]
    public static SqlQueryRecordSetNode QueryRecordSet(SqlQueryExpressionNode query, string alias)
    {
        return new SqlQueryRecordSetNode( query, alias, isOptional: false );
    }

    [Pure]
    public static SqlOrdinalCommonTableExpressionNode OrdinalCommonTableExpression(SqlQueryExpressionNode query, string name)
    {
        return new SqlOrdinalCommonTableExpressionNode( query, name );
    }

    [Pure]
    public static SqlRecursiveCommonTableExpressionNode RecursiveCommonTableExpression(SqlCompoundQueryExpressionNode query, string name)
    {
        return new SqlRecursiveCommonTableExpressionNode( query, name );
    }

    [Pure]
    public static SqlSingleDataSourceNode<TRecordSetNode> SingleDataSource<TRecordSetNode>(TRecordSetNode from)
        where TRecordSetNode : SqlRecordSetNode
    {
        return new SqlSingleDataSourceNode<TRecordSetNode>( from );
    }

    [Pure]
    public static SqlDummyDataSourceNode DummyDataSource()
    {
        return _dummyDataSource ??= new SqlDummyDataSourceNode( Chain<SqlTraitNode>.Empty );
    }

    [Pure]
    public static SqlMultiDataSourceNode Join(SqlRecordSetNode from, params SqlDataSourceJoinOnNode[] joins)
    {
        return new SqlMultiDataSourceNode( from, joins );
    }

    [Pure]
    public static SqlMultiDataSourceNode Join(SqlRecordSetNode from, params SqlJoinDefinition[] definitions)
    {
        return new SqlMultiDataSourceNode( from, definitions );
    }

    [Pure]
    public static SqlMultiDataSourceNode Join(SqlDataSourceNode from, params SqlDataSourceJoinOnNode[] joins)
    {
        return new SqlMultiDataSourceNode( from, joins );
    }

    [Pure]
    public static SqlMultiDataSourceNode Join(SqlDataSourceNode from, params SqlJoinDefinition[] definitions)
    {
        return new SqlMultiDataSourceNode( from, definitions );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode OrderByAsc(SqlExpressionNode expression)
    {
        return OrderBy( expression, Sql.OrderBy.Asc );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode OrderByDesc(SqlExpressionNode expression)
    {
        return OrderBy( expression, Sql.OrderBy.Desc );
    }

    [Pure]
    public static SqlOrderByNode OrderBy(SqlExpressionNode expression, OrderBy ordering)
    {
        return new SqlOrderByNode( expression, ordering );
    }

    [Pure]
    public static SqlDataSourceJoinOnNode InnerJoinOn(SqlRecordSetNode innerRecordSet, SqlConditionNode onExpression)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Inner, innerRecordSet, onExpression );
    }

    [Pure]
    public static SqlDataSourceJoinOnNode LeftJoinOn(SqlRecordSetNode innerRecordSet, SqlConditionNode onExpression)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Left, innerRecordSet, onExpression );
    }

    [Pure]
    public static SqlDataSourceJoinOnNode RightJoinOn(SqlRecordSetNode innerRecordSet, SqlConditionNode onExpression)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Right, innerRecordSet, onExpression );
    }

    [Pure]
    public static SqlDataSourceJoinOnNode FullJoinOn(SqlRecordSetNode innerRecordSet, SqlConditionNode onExpression)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Full, innerRecordSet, onExpression );
    }

    [Pure]
    public static SqlDataSourceJoinOnNode CrossJoin(SqlRecordSetNode innerRecordSet)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Cross, innerRecordSet, True() );
    }

    [Pure]
    public static SqlCompoundQueryComponentNode UnionWith(SqlQueryExpressionNode query)
    {
        return new SqlCompoundQueryComponentNode( query, SqlCompoundQueryOperator.Union );
    }

    [Pure]
    public static SqlCompoundQueryComponentNode UnionAllWith(SqlQueryExpressionNode query)
    {
        return new SqlCompoundQueryComponentNode( query, SqlCompoundQueryOperator.UnionAll );
    }

    [Pure]
    public static SqlCompoundQueryComponentNode IntersectWith(SqlQueryExpressionNode query)
    {
        return new SqlCompoundQueryComponentNode( query, SqlCompoundQueryOperator.Intersect );
    }

    [Pure]
    public static SqlCompoundQueryComponentNode ExceptWith(SqlQueryExpressionNode query)
    {
        return new SqlCompoundQueryComponentNode( query, SqlCompoundQueryOperator.Except );
    }

    [Pure]
    public static SqlCompoundQueryComponentNode CompoundWith(SqlCompoundQueryOperator @operator, SqlQueryExpressionNode query)
    {
        Ensure.IsDefined( @operator );
        return new SqlCompoundQueryComponentNode( query, @operator );
    }

    [Pure]
    public static SqlSwitchExpressionNode Iif(SqlConditionNode condition, SqlExpressionNode whenTrue, SqlExpressionNode whenFalse)
    {
        return Switch( new[] { SwitchCase( condition, whenTrue ) }, whenFalse );
    }

    [Pure]
    public static SqlSwitchExpressionNode Switch(IEnumerable<SqlSwitchCaseNode> cases, SqlExpressionNode defaultExpression)
    {
        return new SqlSwitchExpressionNode( cases.ToArray(), defaultExpression );
    }

    [Pure]
    public static SqlSwitchCaseNode SwitchCase(SqlConditionNode condition, SqlExpressionNode expression)
    {
        return new SqlSwitchCaseNode( condition, expression );
    }

    [Pure]
    public static SqlValuesNode Values(SqlExpressionNode[,] expressions)
    {
        return new SqlValuesNode( expressions );
    }

    [Pure]
    public static SqlValuesNode Values(params SqlExpressionNode[] expressions)
    {
        return new SqlValuesNode( expressions );
    }

    [Pure]
    public static SqlRawStatementNode RawStatement(string sql, params SqlParameterNode[] parameters)
    {
        return new SqlRawStatementNode( sql, parameters );
    }

    [Pure]
    public static SqlDeleteFromNode DeleteFrom(SqlDataSourceNode dataSource)
    {
        return new SqlDeleteFromNode( dataSource );
    }

    [Pure]
    public static SqlTruncateNode Truncate(SqlRecordSetNode table)
    {
        return new SqlTruncateNode( table );
    }

    [Pure]
    public static SqlValueAssignmentNode ValueAssignment(SqlDataFieldNode dataField, SqlExpressionNode value)
    {
        return new SqlValueAssignmentNode( dataField, value );
    }

    [Pure]
    public static SqlUpdateNode Update(SqlDataSourceNode dataSource, params SqlValueAssignmentNode[] assignments)
    {
        return new SqlUpdateNode( dataSource, assignments );
    }

    [Pure]
    public static SqlInsertIntoNode InsertInto(
        SqlQueryExpressionNode query,
        SqlRecordSetNode recordSet,
        params SqlDataFieldNode[] dataFields)
    {
        return new SqlInsertIntoNode( query, recordSet, dataFields );
    }

    [Pure]
    public static SqlInsertIntoNode InsertInto(SqlValuesNode values, SqlRecordSetNode recordSet, params SqlDataFieldNode[] dataFields)
    {
        return new SqlInsertIntoNode( values, recordSet, dataFields );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnDefinitionNode Column<T>(string name, bool isNullable = false, SqlExpressionNode? defaultValue = null)
        where T : notnull
    {
        return Column( name, TypeNullability.Create<T>( isNullable ), defaultValue );
    }

    [Pure]
    public static SqlColumnDefinitionNode Column(string name, TypeNullability type, SqlExpressionNode? defaultValue = null)
    {
        return new SqlColumnDefinitionNode( name, type, defaultValue );
    }

    [Pure]
    public static SqlColumnDefinitionNode Column(
        string name,
        ISqlColumnTypeDefinition typeDefinition,
        bool isNullable = false,
        SqlExpressionNode? defaultValue = null)
    {
        return new SqlColumnDefinitionNode( name, typeDefinition, isNullable, defaultValue );
    }

    [Pure]
    public static SqlPrimaryKeyDefinitionNode PrimaryKey(SqlSchemaObjectName name, params SqlOrderByNode[] columns)
    {
        return new SqlPrimaryKeyDefinitionNode( name, columns );
    }

    [Pure]
    public static SqlForeignKeyDefinitionNode ForeignKey(
        SqlSchemaObjectName name,
        SqlDataFieldNode[] columns,
        SqlRecordSetNode referencedTable,
        SqlDataFieldNode[] referencedColumns,
        ReferenceBehavior? onDeleteBehavior = null,
        ReferenceBehavior? onUpdateBehavior = null)
    {
        return new SqlForeignKeyDefinitionNode(
            name,
            columns,
            referencedTable,
            referencedColumns,
            onDeleteBehavior ?? ReferenceBehavior.Restrict,
            onUpdateBehavior ?? ReferenceBehavior.Restrict );
    }

    [Pure]
    public static SqlCheckDefinitionNode Check(SqlSchemaObjectName name, SqlConditionNode condition)
    {
        return new SqlCheckDefinitionNode( name, condition );
    }

    [Pure]
    public static SqlCreateTableNode CreateTable(
        SqlRecordSetInfo info,
        SqlColumnDefinitionNode[] columns,
        bool ifNotExists = false,
        Func<SqlNewTableNode, SqlCreateTableConstraints>? constraintsProvider = null)
    {
        return new SqlCreateTableNode( info, ifNotExists, columns, constraintsProvider );
    }

    [Pure]
    public static SqlCreateViewNode CreateView(SqlRecordSetInfo info, SqlQueryExpressionNode source, bool replaceIfExists = false)
    {
        return new SqlCreateViewNode( info, replaceIfExists, source );
    }

    [Pure]
    public static SqlCreateIndexNode CreateIndex(
        SqlSchemaObjectName name,
        bool isUnique,
        SqlRecordSetNode table,
        SqlOrderByNode[] columns,
        bool replaceIfExists = false,
        SqlConditionNode? filter = null)
    {
        return new SqlCreateIndexNode( name, isUnique, replaceIfExists, table, columns, filter );
    }

    [Pure]
    public static SqlRenameTableNode RenameTable(SqlRecordSetInfo info, SqlSchemaObjectName newName)
    {
        return new SqlRenameTableNode( info, newName );
    }

    [Pure]
    public static SqlRenameColumnNode RenameColumn(SqlRecordSetInfo table, string oldName, string newName)
    {
        return new SqlRenameColumnNode( table, oldName, newName );
    }

    [Pure]
    public static SqlAddColumnNode AddColumn(SqlRecordSetInfo table, SqlColumnDefinitionNode definition)
    {
        return new SqlAddColumnNode( table, definition );
    }

    [Pure]
    public static SqlDropColumnNode DropColumn(SqlRecordSetInfo table, string name)
    {
        return new SqlDropColumnNode( table, name );
    }

    [Pure]
    public static SqlDropTableNode DropTable(SqlRecordSetInfo table, bool ifExists = false)
    {
        return new SqlDropTableNode( table, ifExists );
    }

    [Pure]
    public static SqlDropViewNode DropView(SqlRecordSetInfo view, bool ifExists = false)
    {
        return new SqlDropViewNode( view, ifExists );
    }

    [Pure]
    public static SqlDropIndexNode DropIndex(SqlRecordSetInfo table, SqlSchemaObjectName name, bool ifExists = false)
    {
        return new SqlDropIndexNode( table, name, ifExists );
    }

    [Pure]
    public static SqlStatementBatchNode Batch(params ISqlStatementNode[] statements)
    {
        return new SqlStatementBatchNode( statements );
    }

    [Pure]
    public static SqlBeginTransactionNode BeginTransaction(IsolationLevel isolationLevel)
    {
        return new SqlBeginTransactionNode( isolationLevel );
    }

    [Pure]
    public static SqlCommitTransactionNode CommitTransaction()
    {
        return _commitTransaction ??= new SqlCommitTransactionNode();
    }

    [Pure]
    public static SqlRollbackTransactionNode RollbackTransaction()
    {
        return _rollbackTransaction ??= new SqlRollbackTransactionNode();
    }
}
