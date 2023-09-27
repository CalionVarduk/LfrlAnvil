using System.Data;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sqlite.Exceptions;

namespace LfrlAnvil.Sqlite;

public class SqliteNodeInterpreter : SqlNodeInterpreter
{
    public SqliteNodeInterpreter(SqliteColumnTypeDefinitionProvider columnTypeDefinitions, SqlNodeInterpreterContext context)
        : base( context, beginNameDelimiter: '"', endNameDelimiter: '"' )
    {
        ColumnTypeDefinitions = columnTypeDefinitions;
    }

    public SqliteColumnTypeDefinitionProvider ColumnTypeDefinitions { get; }

    public override void VisitLiteral(SqlLiteralNode node)
    {
        var sql = node.GetSql( ColumnTypeDefinitions );
        Context.Sql.Append( sql );
    }

    public override void VisitModulo(SqlModuloExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "MOD" ).Append( '(' );
            VisitChild( node.Left );
            Context.Sql.AppendComma().AppendSpace();
            VisitChild( node.Right );
            Context.Sql.Append( ')' );
        }
    }

    public override void VisitBitwiseXor(SqlBitwiseXorExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( '(' );
            VisitChild( node.Left );
            Context.Sql.AppendSpace().Append( '|' ).AppendSpace();
            VisitChild( node.Right );
            Context.Sql.Append( ')' ).AppendSpace().Append( '&' ).AppendSpace().Append( '~' ).Append( '(' );
            VisitChild( node.Left );
            Context.Sql.AppendSpace().Append( '&' ).AppendSpace();
            VisitChild( node.Right );
            Context.Sql.Append( ')' );
        }
    }

    public override void VisitRecordsAffectedFunction(SqlRecordsAffectedFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CHANGES", node );
    }

    public override void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CURRENT_DATE", node );
    }

    public override void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CURRENT_TIME", node );
    }

    public override void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CURRENT_DATETIME", node );
    }

    public override void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CURRENT_TIMESTAMP", node );
    }

    public override void VisitLengthFunction(SqlLengthFunctionExpressionNode node)
    {
        VisitSimpleFunction( "LENGTH", node );
    }

    public override void VisitToLowerFunction(SqlToLowerFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TO_LOWER", node );
    }

    public override void VisitToUpperFunction(SqlToUpperFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TO_UPPER", node );
    }

    public override void VisitTrimStartFunction(SqlTrimStartFunctionExpressionNode node)
    {
        VisitSimpleFunction( "LTRIM", node );
    }

    public override void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node)
    {
        VisitSimpleFunction( "RTRIM", node );
    }

    public override void VisitTrimFunction(SqlTrimFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TRIM", node );
    }

    public override void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SUBSTR", node );
    }

    public override void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node)
    {
        VisitSimpleFunction( "REPLACE", node );
    }

    public override void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node)
    {
        VisitSimpleFunction( "INSTR", node );
    }

    public override void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node)
    {
        VisitSimpleFunction( "INSTR_LAST", node );
    }

    public override void VisitSignFunction(SqlSignFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SIGN", node );
    }

    public override void VisitAbsFunction(SqlAbsFunctionExpressionNode node)
    {
        VisitSimpleFunction( "ABS", node );
    }

    public override void VisitCeilingFunction(SqlCeilingFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CEIL", node );
    }

    public override void VisitFloorFunction(SqlFloorFunctionExpressionNode node)
    {
        VisitSimpleFunction( "FLOOR", node );
    }

    public override void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TRUNC", node );
    }

    public override void VisitPowerFunction(SqlPowerFunctionExpressionNode node)
    {
        VisitSimpleFunction( "POW", node );
    }

    public override void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SQRT", node );
    }

    public override void VisitMinFunction(SqlMinFunctionExpressionNode node)
    {
        if ( node.Arguments.Length == 1 )
        {
            using ( Context.TempParentNodeUpdate( node ) )
                VisitChild( node.Arguments.Span[0] );
        }
        else
            VisitSimpleFunction( "MIN", node );
    }

    public override void VisitMaxFunction(SqlMaxFunctionExpressionNode node)
    {
        if ( node.Arguments.Length == 1 )
        {
            using ( Context.TempParentNodeUpdate( node ) )
                VisitChild( node.Arguments.Span[0] );
        }
        else
            VisitSimpleFunction( "MAX", node );
    }

    public override void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "MIN", node );
    }

    public override void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "MAX", node );
    }

    public override void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AVG", node );
    }

    public override void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "SUM", node );
    }

    public override void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "COUNT", node );
    }

    public override void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "GROUP_CONCAT", node );
    }

    public override void VisitTrue(SqlTrueNode node)
    {
        Context.Sql.Append( "TRUE" );
    }

    public override void VisitFalse(SqlFalseNode node)
    {
        Context.Sql.Append( "FALSE" );
    }

    public override void VisitConditionValue(SqlConditionValueNode node)
    {
        this.Visit( node.Condition );
    }

    public override void VisitTableRecordSet(SqlTableRecordSetNode node)
    {
        AppendDelimitedName( node.Table.FullName );
        if ( node.IsAliased )
            AppendDelimitedAlias( node.Name );
    }

    public override void VisitTableBuilderRecordSet(SqlTableBuilderRecordSetNode node)
    {
        AppendDelimitedName( node.Table.FullName );
        if ( node.IsAliased )
            AppendDelimitedAlias( node.Name );
    }

    public override void VisitViewRecordSet(SqlViewRecordSetNode node)
    {
        AppendDelimitedName( node.View.FullName );
        if ( node.IsAliased )
            AppendDelimitedAlias( node.Name );
    }

    public override void VisitViewBuilderRecordSet(SqlViewBuilderRecordSetNode node)
    {
        AppendDelimitedName( node.View.FullName );
        if ( node.IsAliased )
            AppendDelimitedAlias( node.Name );
    }

    public override void VisitTemporaryTableRecordSet(SqlTemporaryTableRecordSetNode node)
    {
        Context.Sql.Append( "temp" ).AppendDot();
        AppendDelimitedName( node.CreationNode.Name );
        if ( node.IsAliased )
            AppendDelimitedAlias( node.Name );
    }

    public override void VisitDataSource(SqlDataSourceNode node)
    {
        if ( node is SqlDummyDataSourceNode )
            return;

        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "FROM" ).AppendSpace();
            this.Visit( node.From );

            foreach ( var join in node.Joins.Span )
            {
                Context.AppendIndent();
                VisitJoinOn( join );
            }
        }
    }

    public override void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        var hasParentNode = Context.ParentNode is not null;
        if ( hasParentNode )
            Context.AppendIndent();

        using ( Context.TempParentNodeUpdate( node ) )
        {
            var traits = ExtractDataSourceTraits( ExtractDataSourceTraits( node.DataSource.Traits ), node.Traits );
            VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions );
            VisitDataSourceQuerySelection( node, traits.Distinct );
            VisitDataSource( node.DataSource );
            VisitOptionalFilterCondition( traits.Filter );
            VisitOptionalAggregationRange( traits.Aggregations );
            VisitOptionalAggregationFilterCondition( traits.AggregationFilter );
            VisitOptionalOrderingRange( traits.Ordering );
            VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
        }

        if ( hasParentNode )
            Context.AppendShortIndent();
    }

    public override void VisitCompoundQuery(SqlCompoundQueryExpressionNode node)
    {
        var hasParentNode = Context.ParentNode is not null;
        if ( hasParentNode )
            Context.AppendIndent();

        using ( Context.TempParentNodeUpdate( node ) )
        {
            var traits = ExtractQueryTraits( default, node.Traits );
            VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions );
            VisitCompoundQueryComponents( node );
            VisitOptionalOrderingRange( traits.Ordering );
            VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
        }

        if ( hasParentNode )
            Context.AppendShortIndent();
    }

    public override void VisitLimitTrait(SqlLimitTraitNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "LIMIT" ).AppendSpace();
            VisitChild( node.Value );
        }
    }

    public override void VisitOffsetTrait(SqlOffsetTraitNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "OFFSET" ).AppendSpace();
            VisitChild( node.Value );
        }
    }

    public override void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            if ( node.IsRecursive )
                Context.Sql.Append( "RECURSIVE" ).AppendSpace();

            AppendDelimitedName( node.Name );
            Context.Sql.AppendSpace().Append( "AS" ).AppendSpace();
            VisitChild( node.Query );
        }
    }

    public override void VisitTypeCast(SqlTypeCastExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "CAST" ).Append( '(' );
            VisitChild( node.Value );
            Context.Sql.AppendSpace().Append( "AS" ).AppendSpace();

            var typeDefinition = ColumnTypeDefinitions.GetByType( node.TargetType );
            Context.Sql.Append( typeDefinition.DbType.Name ).Append( ')' );
        }
    }

    public override void VisitInsertInto(SqlInsertIntoNode node)
    {
        switch ( node.Source.NodeType )
        {
            case SqlNodeType.DataSourceQuery:
            {
                using ( Context.TempParentNodeUpdate( node ) )
                {
                    var query = ReinterpretCast.To<SqlDataSourceQueryExpressionNode>( node.Source );
                    var traits = ExtractDataSourceTraits( ExtractDataSourceTraits( query.DataSource.Traits ), query.Traits );

                    VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions );
                    VisitInsertIntoFields( node );

                    Context.AppendIndent();
                    VisitDataSourceQuerySelection( query, traits.Distinct );
                    VisitDataSource( query.DataSource );
                    VisitOptionalFilterCondition( traits.Filter );
                    VisitOptionalAggregationRange( traits.Aggregations );
                    VisitOptionalAggregationFilterCondition( traits.AggregationFilter );
                    VisitOptionalOrderingRange( traits.Ordering );
                    VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
                }

                break;
            }
            case SqlNodeType.CompoundQuery:
            {
                using ( Context.TempParentNodeUpdate( node ) )
                {
                    var query = ReinterpretCast.To<SqlCompoundQueryExpressionNode>( node.Source );
                    var traits = ExtractQueryTraits( query.Traits );

                    VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions );
                    VisitInsertIntoFields( node );

                    Context.AppendIndent();
                    VisitCompoundQueryComponents( query );
                    VisitOptionalOrderingRange( traits.Ordering );
                    VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
                }

                break;
            }
            default:
            {
                using ( Context.TempParentNodeUpdate( node ) )
                    VisitInsertIntoFields( node );

                Context.AppendIndent();
                this.Visit( node.Source );
                break;
            }
        }
    }

    public override void VisitUpdate(SqlUpdateNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            var traits = ExtractDataSourceTraits( node.DataSource.Traits );
            VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions );
            Context.Sql.Append( "UPDATE" ).AppendSpace();

            if ( IsUpdateOrDeleteDataSourceSimple( node.DataSource, traits ) )
            {
                AppendRecordSetName( node.DataSource.From );
                VisitUpdateAssignmentRange( node.Assignments.Span );
                VisitOptionalFilterCondition( traits.Filter );
                return;
            }

            if ( node.DataSource.From is not SqlTableRecordSetNode from )
                throw new SqlNodeVisitorException( Resources.UpdateTargetIsNotTableRecordSet, this, node );

            if ( ! from.IsAliased )
                throw new SqlNodeVisitorException( Resources.UpdateTargetIsNotAliased, this, node );

            AppendDelimitedName( from.Table.FullName );
            VisitUpdateAssignmentRange( node.Assignments.Span );
            VisitComplexDeleteOrUpdateDataSourceFilter( from, node.DataSource, traits );
        }
    }

    public override void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            var traits = ExtractDataSourceTraits( node.DataSource.Traits );
            VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions );
            Context.Sql.Append( "DELETE FROM" ).AppendSpace();

            if ( IsUpdateOrDeleteDataSourceSimple( node.DataSource, traits ) )
            {
                AppendRecordSetName( node.DataSource.From );
                VisitOptionalFilterCondition( traits.Filter );
                return;
            }

            // TODO: (later) check by verifying node enum type, rather than node cli type
            if ( node.DataSource.From is not SqlTableRecordSetNode from )
                throw new SqlNodeVisitorException( Resources.DeleteTargetIsNotTableRecordSet, this, node );

            if ( ! from.IsAliased )
                throw new SqlNodeVisitorException( Resources.DeleteTargetIsNotAliased, this, node );

            AppendDelimitedName( from.Table.FullName );
            VisitComplexDeleteOrUpdateDataSourceFilter( from, node.DataSource, traits );
        }
    }

    public override void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        var typeDefinition = ColumnTypeDefinitions.GetByType( node.Type.BaseType );
        AppendDelimitedName( node.Name );
        Context.Sql.AppendSpace().Append( typeDefinition.DbType.Name );

        if ( ! node.Type.IsNullable )
            Context.Sql.AppendSpace().Append( "NOT" ).AppendSpace().Append( "NULL" );
    }

    public override void VisitCreateTemporaryTable(SqlCreateTemporaryTableNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "DROP TABLE IF EXISTS" ).AppendSpace().Append( "temp" ).AppendDot();
            AppendDelimitedName( node.Name );
            Context.Sql.AppendSemicolon();
            Context.AppendIndent().Append( "CREATE TEMP TABLE" ).AppendSpace();
            AppendDelimitedName( node.Name );
            Context.Sql.AppendSpace().Append( '(' );

            if ( node.Columns.Length > 0 )
            {
                using ( Context.TempIndentIncrease() )
                {
                    foreach ( var column in node.Columns.Span )
                    {
                        Context.AppendIndent();
                        VisitColumnDefinition( column );
                        Context.Sql.AppendComma();
                    }

                    Context.Sql.ShrinkBy( 1 );
                }

                Context.AppendIndent();
            }

            Context.Sql.Append( ')' ).AppendSpace().Append( "WITHOUT ROWID" );
        }
    }

    public override void VisitDropTemporaryTable(SqlDropTemporaryTableNode node)
    {
        Context.Sql.Append( "DROP TABLE" ).AppendSpace().Append( "temp" ).AppendDot();
        AppendDelimitedName( node.Name );
    }

    public override void VisitBeginTransaction(SqlBeginTransactionNode node)
    {
        if ( node.IsolationLevel == IsolationLevel.ReadUncommitted )
        {
            Context.Sql.Append( "PRAGMA read_uncommitted = 1;" );
            Context.AppendIndent().Append( "BEGIN" );
        }
        else
        {
            Context.Sql.Append( "PRAGMA read_uncommitted = 0;" );
            Context.AppendIndent().Append( "BEGIN IMMEDIATE" );
        }
    }

    public override void AppendRecordSetName(SqlRecordSetNode node)
    {
        switch ( node.NodeType )
        {
            case SqlNodeType.RawRecordSet:
                Context.Sql.Append( node.Name );
                break;

            case SqlNodeType.TemporaryTableRecordSet:
                if ( ! node.IsAliased )
                    Context.Sql.Append( "temp" ).AppendDot();

                AppendDelimitedName( node.Name );
                break;

            default:
                AppendDelimitedName( node.Name );
                break;
        }
    }

    [Pure]
    protected override bool DoesChildNodeRequireParentheses(SqlNodeBase node)
    {
        switch ( node.NodeType )
        {
            case SqlNodeType.RawDataField:
            case SqlNodeType.Null:
            case SqlNodeType.Literal:
            case SqlNodeType.Parameter:
            case SqlNodeType.Column:
            case SqlNodeType.ColumnBuilder:
            case SqlNodeType.QueryDataField:
            case SqlNodeType.ViewDataField:
            case SqlNodeType.Modulo:
            case SqlNodeType.FunctionExpression:
            case SqlNodeType.True:
            case SqlNodeType.False:
            case SqlNodeType.SelectField:
            case SqlNodeType.SelectCompoundField:
            case SqlNodeType.SelectRecordSet:
            case SqlNodeType.SelectAll:
            case SqlNodeType.SelectExpression:
            case SqlNodeType.CompoundQueryComponent:
            case SqlNodeType.TypeCast:
                return false;

            case SqlNodeType.AggregateFunctionExpression:
                foreach ( var trait in ReinterpretCast.To<SqlAggregateFunctionExpressionNode>( node ).Traits )
                {
                    if ( trait.NodeType != SqlNodeType.DistinctTrait )
                        return true;
                }

                return false;
        }

        return true;
    }

    protected void VisitSimpleAggregateFunction(string functionName, SqlAggregateFunctionExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            var traits = ExtractAggregateFunctionTraits( node.Traits );
            Context.Sql.Append( functionName ).Append( '(' );

            if ( node.Arguments.Length > 0 )
            {
                using ( Context.TempIndentIncrease() )
                {
                    if ( traits.Distinct is not null )
                    {
                        VisitDistinctTrait( traits.Distinct );
                        Context.Sql.AppendSpace();
                    }

                    foreach ( var arg in node.Arguments.Span )
                    {
                        VisitChild( arg );
                        Context.Sql.AppendComma().AppendSpace();
                    }
                }

                Context.Sql.ShrinkBy( 2 );
            }

            Context.Sql.Append( ')' );

            if ( traits.Filter is not null )
            {
                Context.Sql.AppendSpace().Append( "FILTER" ).AppendSpace().Append( '(' ).Append( "WHERE" ).AppendSpace();
                this.Visit( traits.Filter );
                Context.Sql.Append( ')' );
            }
        }
    }

    protected void VisitOptionalLimitAndOffsetExpressions(SqlExpressionNode? limit, SqlExpressionNode? offset)
    {
        if ( limit is not null )
        {
            Context.AppendIndent().Append( "LIMIT" ).AppendSpace();
            VisitChild( limit );

            if ( offset is not null )
            {
                Context.Sql.AppendSpace().Append( "OFFSET" ).AppendSpace();
                VisitChild( offset );
            }

            return;
        }

        if ( offset is null )
            return;

        Context.AppendIndent().Append( "LIMIT" ).AppendSpace().Append( '-' ).Append( '1' );
        Context.Sql.AppendSpace().Append( "OFFSET" ).AppendSpace();
        VisitChild( offset );
    }

    protected void VisitInsertIntoFields(SqlInsertIntoNode node)
    {
        Context.Sql.Append( "INSERT INTO" ).AppendSpace();
        AppendRecordSetName( node.RecordSet );
        Context.Sql.AppendSpace().Append( '(' );

        if ( node.DataFields.Length > 0 )
        {
            foreach ( var dataField in node.DataFields.Span )
            {
                AppendDelimitedName( dataField.Name );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }

        Context.Sql.Append( ')' );
    }

    protected void VisitDataSourceQuerySelection(SqlDataSourceQueryExpressionNode node, SqlDistinctTraitNode? distinct)
    {
        Context.Sql.Append( "SELECT" );
        VisitOptionalDistinctMarker( distinct );

        using ( Context.TempIndentIncrease() )
        {
            foreach ( var selection in node.Selection.Span )
            {
                Context.AppendIndent();
                this.Visit( selection );
                Context.Sql.AppendComma();
            }
        }

        Context.Sql.ShrinkBy( 1 );
        Context.AppendIndent();
    }

    protected void VisitCompoundQueryComponents(SqlCompoundQueryExpressionNode node)
    {
        VisitChild( node.FirstQuery );
        foreach ( var component in node.FollowingQueries.Span )
        {
            Context.AppendIndent();
            VisitCompoundQueryComponent( component );
        }
    }

    [Pure]
    protected static bool IsUpdateOrDeleteDataSourceSimple(SqlDataSourceNode node, SqlDataSourceTraits traits)
    {
        return ! node.From.IsAliased &&
            node.RecordSets.Count == 1 &&
            traits.Distinct is null &&
            traits.Aggregations.Count == 0 &&
            traits.AggregationFilter is null &&
            traits.Ordering.Count == 0 &&
            traits.Limit is null &&
            traits.Offset is null &&
            traits.Custom.Count == 0;
    }

    protected void VisitComplexDeleteOrUpdateDataSourceFilter(
        SqlTableRecordSetNode target,
        SqlDataSourceNode dataSource,
        SqlDataSourceTraits traits)
    {
        // TODO: (later)
        // - in order for this to work with TableBuilder & TemporaryTable nodes:
        //   - IsTemporary boolean
        //   - FullName string
        //   - collection of PK column names (or, if table lacks PK, collection of all column names)
        var primaryKeyColumns = target.Table.PrimaryKey.Index.Columns.Span;
        Context.AppendIndent().Append( "WHERE" ).AppendSpace();

        if ( primaryKeyColumns.Length == 1 )
        {
            AppendDelimitedName( primaryKeyColumns[0].Column.Name );
            Context.Sql.AppendSpace().Append( "IN" ).AppendSpace().Append( '(' );

            using ( Context.TempIndentIncrease() )
            {
                Context.AppendIndent().Append( "SELECT" );
                VisitOptionalDistinctMarker( traits.Distinct );
                Context.Sql.AppendSpace();
                AppendRecordSetName( target );
                Context.Sql.AppendDot();
                AppendDelimitedName( primaryKeyColumns[0].Column.Name );

                Context.AppendIndent();
                VisitDataSource( dataSource );
                VisitOptionalFilterCondition( traits.Filter );
                VisitOptionalAggregationRange( traits.Aggregations );
                VisitOptionalAggregationFilterCondition( traits.AggregationFilter );
                VisitOptionalOrderingRange( traits.Ordering );
                VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
            }
        }
        else
        {
            Context.Sql.Append( "EXISTS" ).AppendSpace().Append( '(' );

            using ( Context.TempIndentIncrease() )
            {
                Context.AppendIndent().Append( "SELECT" );
                VisitOptionalDistinctMarker( traits.Distinct );
                Context.Sql.AppendSpace().Append( '*' );

                Context.AppendIndent();
                VisitDataSource( dataSource );
                Context.AppendIndent().Append( "WHERE" ).AppendSpace();

                foreach ( var c in primaryKeyColumns )
                {
                    Context.Sql.Append( '(' );
                    AppendDelimitedName( target.Table.FullName );
                    Context.Sql.AppendDot();
                    AppendDelimitedName( c.Column.Name );
                    Context.Sql.AppendSpace().Append( '=' ).AppendSpace();
                    AppendRecordSetName( target );
                    Context.Sql.AppendDot();
                    AppendDelimitedName( c.Column.Name );
                    Context.Sql.Append( ')' ).AppendSpace().Append( "AND" ).AppendSpace();
                }

                if ( traits.Filter is null )
                    Context.Sql.ShrinkBy( 5 );
                else
                    VisitChild( traits.Filter );

                VisitOptionalAggregationRange( traits.Aggregations );
                VisitOptionalAggregationFilterCondition( traits.AggregationFilter );
                VisitOptionalOrderingRange( traits.Ordering );
                VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
            }
        }

        Context.AppendIndent().Append( ')' );
    }
}
