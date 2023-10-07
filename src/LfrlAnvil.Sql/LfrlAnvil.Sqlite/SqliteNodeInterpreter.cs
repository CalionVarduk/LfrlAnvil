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
        VisitSimpleFunction( "GET_CURRENT_DATE", node );
    }

    public override void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node)
    {
        VisitSimpleFunction( "GET_CURRENT_TIME", node );
    }

    public override void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node)
    {
        VisitSimpleFunction( "GET_CURRENT_DATETIME", node );
    }

    public override void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        VisitSimpleFunction( "GET_CURRENT_TIMESTAMP", node );
    }

    public override void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node)
    {
        VisitSimpleFunction( "NEW_GUID", node );
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
        // TODO
        // update does not work in more complex scenarios e.g.
        // UPDATE U SET
        //   B = U.B + X.B + 1
        // FROM T AS U
        // JOIN X ON X.A = U.A
        //
        // interpreter will produce sth like this:
        // UPDATE T SET
        //   B = T.B + X.B + 1 <= X.B doesn't exist in this scope
        // WHERE A IN (
        //   SELECT U.A
        //   FROM T AS U
        //   JOIN X ON X.A = U.A
        // )
        //
        // X.B should be replaced by some other expression
        //
        // CTEs could be used to improve this behavior:
        // WITH X AS ( <= CTEs could be grouped by joined record set & include all fields used in value assignments
        //   SELECT U.A, X.B <= requires T PK columns
        //   FROM T AS U
        //   JOIN X ON X.A = U.A
        // )
        // UPDATE T SET
        //   B = T.B + (SELECT X.B FROM X WHERE X.A = T.A) + 1 <= X.B replaced by selection from CTE filtered by T PK
        // WHERE A IN (
        //   SELECT U.A
        //   FROM T AS U
        //   JOIN X ON X.A = U.A
        // )
        //
        // more general form:
        // WITH <other-cte>
        // "_{GUID}" AS (
        //   SELECT <aliased-target-pk>, <non-target-columns-used-in-assignments>
        //   FROM <complex-data-source>
        // )
        // UPDATE <target> SET
        //   <value-assignments> <= all non-<target> columns need to be replaced by:
        //                          (SELECT <non-target-column> FROM "_{GUID}" WHERE <target-pk-comparison>)
        // WHERE <target-pk> IN (SELECT <aliased-target-pk> FROM "_{GUID}") <= or EXISTS (or row value comparison), if pk has multiple columns
        //
        // this approach requires pre-emptive scanning of value assignments in order to get all non-<target> columns (new visitor)
        // and to properly prepare the CTE

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

            var targetInfo = ExtractTargetUpdateInfo( node );
            AppendRecordSetName( targetInfo.BaseTarget );
            VisitUpdateAssignmentRange( node.Assignments.Span );
            VisitComplexDeleteOrUpdateDataSourceFilter( targetInfo, node.DataSource, traits );
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

            var targetInfo = ExtractTargetDeleteInfo( node );
            AppendRecordSetName( targetInfo.BaseTarget );
            VisitComplexDeleteOrUpdateDataSourceFilter( targetInfo, node.DataSource, traits );
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
            Context.Sql.Append( "CREATE TEMP TABLE" ).AppendSpace();
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
        TargetDeleteOrUpdateInfo targetInfo,
        SqlDataSourceNode dataSource,
        SqlDataSourceTraits traits)
    {
        Context.AppendIndent().Append( "WHERE" ).AppendSpace();

        if ( targetInfo.IdentityColumnNames.Length == 1 )
        {
            AppendRecordSetName( targetInfo.BaseTarget );
            Context.Sql.AppendDot();
            AppendDelimitedName( targetInfo.IdentityColumnNames[0] );
            Context.Sql.AppendSpace().Append( "IN" ).AppendSpace().Append( '(' );

            using ( Context.TempIndentIncrease() )
            {
                Context.AppendIndent().Append( "SELECT" );
                VisitOptionalDistinctMarker( traits.Distinct );
                Context.Sql.AppendSpace();
                AppendRecordSetName( targetInfo.Target );
                Context.Sql.AppendDot();
                AppendDelimitedName( targetInfo.IdentityColumnNames[0] );

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

                foreach ( var columnName in targetInfo.IdentityColumnNames )
                {
                    Context.Sql.Append( '(' );
                    AppendRecordSetName( targetInfo.BaseTarget );
                    Context.Sql.AppendDot();
                    AppendDelimitedName( columnName );
                    Context.Sql.AppendSpace().Append( '=' ).AppendSpace();
                    AppendRecordSetName( targetInfo.Target );
                    Context.Sql.AppendDot();
                    AppendDelimitedName( columnName );
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

    [Pure]
    protected static TargetDeleteOrUpdateInfo ExtractTableRecordSetDeleteOrUpdateInfo(SqlTableRecordSetNode node)
    {
        var identityColumns = node.Table.PrimaryKey.Index.Columns.Span;
        var identityColumnNames = new string[identityColumns.Length];
        for ( var i = 0; i < identityColumns.Length; ++i )
            identityColumnNames[i] = identityColumns[i].Column.Name;

        return new TargetDeleteOrUpdateInfo( node, node.AsSelf(), identityColumnNames );
    }

    [Pure]
    protected TargetDeleteOrUpdateInfo ExtractTableBuilderRecordSetDeleteInfo(SqlDeleteFromNode node, SqlTableBuilderRecordSetNode target)
    {
        return ExtractTableBuilderRecordSetDeleteOrUpdateInfo( node, target, Resources.DeleteTargetDoesNotHaveAnyColumns );
    }

    [Pure]
    protected TargetDeleteOrUpdateInfo ExtractTableBuilderRecordSetUpdateInfo(SqlUpdateNode node, SqlTableBuilderRecordSetNode target)
    {
        return ExtractTableBuilderRecordSetDeleteOrUpdateInfo( node, target, Resources.UpdateTargetDoesNotHaveAnyColumns );
    }

    [Pure]
    protected TargetDeleteOrUpdateInfo ExtractTemporaryTableRecordSetDeleteInfo(
        SqlDeleteFromNode node,
        SqlTemporaryTableRecordSetNode target)
    {
        return ExtractTemporaryTableRecordSetDeleteOrUpdateInfo( node, target, Resources.DeleteTargetDoesNotHaveAnyColumns );
    }

    [Pure]
    protected TargetDeleteOrUpdateInfo ExtractTemporaryTableRecordSetUpdateInfo(SqlUpdateNode node, SqlTemporaryTableRecordSetNode target)
    {
        return ExtractTemporaryTableRecordSetDeleteOrUpdateInfo( node, target, Resources.UpdateTargetDoesNotHaveAnyColumns );
    }

    [Pure]
    protected TargetDeleteOrUpdateInfo ExtractTargetDeleteInfo(SqlDeleteFromNode node)
    {
        var from = node.DataSource.From;
        var info = from.NodeType switch
        {
            SqlNodeType.TableRecordSet => ExtractTableRecordSetDeleteOrUpdateInfo( ReinterpretCast.To<SqlTableRecordSetNode>( from ) ),
            SqlNodeType.TableBuilderRecordSet => ExtractTableBuilderRecordSetDeleteInfo(
                node,
                ReinterpretCast.To<SqlTableBuilderRecordSetNode>( from ) ),
            SqlNodeType.TemporaryTableRecordSet => ExtractTemporaryTableRecordSetDeleteInfo(
                node,
                ReinterpretCast.To<SqlTemporaryTableRecordSetNode>( from ) ),
            _ => throw new SqlNodeVisitorException( Resources.DeleteTargetIsNotTableRecordSet, this, node )
        };

        if ( ! from.IsAliased )
            throw new SqlNodeVisitorException( Resources.DeleteTargetIsNotAliased, this, node );

        return info;
    }

    [Pure]
    protected TargetDeleteOrUpdateInfo ExtractTargetUpdateInfo(SqlUpdateNode node)
    {
        var from = node.DataSource.From;
        var info = from.NodeType switch
        {
            SqlNodeType.TableRecordSet => ExtractTableRecordSetDeleteOrUpdateInfo( ReinterpretCast.To<SqlTableRecordSetNode>( from ) ),
            SqlNodeType.TableBuilderRecordSet => ExtractTableBuilderRecordSetUpdateInfo(
                node,
                ReinterpretCast.To<SqlTableBuilderRecordSetNode>( from ) ),
            SqlNodeType.TemporaryTableRecordSet => ExtractTemporaryTableRecordSetUpdateInfo(
                node,
                ReinterpretCast.To<SqlTemporaryTableRecordSetNode>( from ) ),
            _ => throw new SqlNodeVisitorException( Resources.UpdateTargetIsNotTableRecordSet, this, node )
        };

        if ( ! from.IsAliased )
            throw new SqlNodeVisitorException( Resources.UpdateTargetIsNotAliased, this, node );

        return info;
    }

    protected readonly record struct TargetDeleteOrUpdateInfo(
        SqlRecordSetNode Target,
        SqlRecordSetNode BaseTarget,
        string[] IdentityColumnNames);

    [Pure]
    private TargetDeleteOrUpdateInfo ExtractTableBuilderRecordSetDeleteOrUpdateInfo(
        SqlNodeBase source,
        SqlTableBuilderRecordSetNode node,
        string noColumnsErrorReason)
    {
        string[] identityColumnNames;
        var primaryKey = node.Table.PrimaryKey;

        if ( primaryKey is not null )
        {
            var identityColumns = primaryKey.Index.Columns.Span;
            identityColumnNames = new string[identityColumns.Length];
            for ( var i = 0; i < identityColumns.Length; ++i )
                identityColumnNames[i] = identityColumns[i].Column.Name;
        }
        else
        {
            var index = 0;
            var identityColumns = node.Table.Columns;
            if ( identityColumns.Count == 0 )
                throw new SqlNodeVisitorException( noColumnsErrorReason, this, source );

            identityColumnNames = new string[identityColumns.Count];
            foreach ( var column in identityColumns )
                identityColumnNames[index++] = column.Name;
        }

        return new TargetDeleteOrUpdateInfo( node, node.AsSelf(), identityColumnNames );
    }

    [Pure]
    private TargetDeleteOrUpdateInfo ExtractTemporaryTableRecordSetDeleteOrUpdateInfo(
        SqlNodeBase source,
        SqlTemporaryTableRecordSetNode node,
        string noColumnsErrorReason)
    {
        // TODO: same as table builder, once an optional PK is added to CreationNode
        var identityColumns = node.CreationNode.Columns.Span;
        if ( identityColumns.Length == 0 )
            throw new SqlNodeVisitorException( noColumnsErrorReason, this, source );

        var identityColumnNames = new string[identityColumns.Length];
        for ( var i = 0; i < identityColumns.Length; ++i )
            identityColumnNames[i] = identityColumns[i].Name;

        return new TargetDeleteOrUpdateInfo( node, node.AsSelf(), identityColumnNames );
    }
}
