using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public abstract class SqlNodeInterpreter : ISqlNodeVisitor
{
    public readonly char BeginNameDelimiter;
    public readonly char EndNameDelimiter;

    protected SqlNodeInterpreter(SqlNodeInterpreterContext context, char beginNameDelimiter, char endNameDelimiter)
    {
        Context = context;
        BeginNameDelimiter = beginNameDelimiter;
        EndNameDelimiter = endNameDelimiter;
        RecordSetNameBehavior = null;
    }

    public SqlNodeInterpreterContext Context { get; }
    public RecordSetNameBehaviorRule? RecordSetNameBehavior { get; private set; }

    public virtual void VisitRawExpression(SqlRawExpressionNode node)
    {
        AppendMultilineSql( node.Sql );

        foreach ( var parameter in node.Parameters )
            AddContextParameter( parameter );
    }

    public virtual void VisitRawDataField(SqlRawDataFieldNode node)
    {
        TryAppendDataFieldRecordSetNameBasedOnNameBehavior( node );
        AppendDelimitedName( node.Name );
    }

    public virtual void VisitNull(SqlNullNode node)
    {
        Context.Sql.Append( "NULL" );
    }

    public abstract void VisitLiteral(SqlLiteralNode node);
    public abstract void VisitParameter(SqlParameterNode node);

    public virtual void VisitColumn(SqlColumnNode node)
    {
        TryAppendDataFieldRecordSetNameBasedOnNameBehavior( node );
        AppendDelimitedName( node.Name );
    }

    public virtual void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        TryAppendDataFieldRecordSetNameBasedOnNameBehavior( node );
        AppendDelimitedName( node.Name );
    }

    public virtual void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        TryAppendDataFieldRecordSetNameBasedOnNameBehavior( node );
        AppendDelimitedName( node.Name );
    }

    public virtual void VisitViewDataField(SqlViewDataFieldNode node)
    {
        TryAppendDataFieldRecordSetNameBasedOnNameBehavior( node );
        AppendDelimitedName( node.Name );
    }

    public virtual void VisitNegate(SqlNegateExpressionNode node)
    {
        VisitPrefixUnaryOperator( node.Value, symbol: "-" );
    }

    public virtual void VisitAdd(SqlAddExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "+", node.Right );
    }

    public virtual void VisitConcat(SqlConcatExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "||", node.Right );
    }

    public virtual void VisitSubtract(SqlSubtractExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "-", node.Right );
    }

    public virtual void VisitMultiply(SqlMultiplyExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "*", node.Right );
    }

    public virtual void VisitDivide(SqlDivideExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "/", node.Right );
    }

    public virtual void VisitModulo(SqlModuloExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "%", node.Right );
    }

    public virtual void VisitBitwiseNot(SqlBitwiseNotExpressionNode node)
    {
        VisitPrefixUnaryOperator( node.Value, symbol: "~" );
    }

    public virtual void VisitBitwiseAnd(SqlBitwiseAndExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "&", node.Right );
    }

    public virtual void VisitBitwiseOr(SqlBitwiseOrExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "|", node.Right );
    }

    public virtual void VisitBitwiseXor(SqlBitwiseXorExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "^", node.Right );
    }

    public virtual void VisitBitwiseLeftShift(SqlBitwiseLeftShiftExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "<<", node.Right );
    }

    public virtual void VisitBitwiseRightShift(SqlBitwiseRightShiftExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: ">>", node.Right );
    }

    public virtual void VisitSwitchCase(SqlSwitchCaseNode node)
    {
        Context.Sql.Append( "WHEN" ).AppendSpace();
        this.Visit( node.Condition );

        using ( Context.TempIndentIncrease() )
        {
            Context.AppendIndent().Append( "THEN" ).AppendSpace();
            VisitChild( node.Expression );
        }
    }

    public virtual void VisitSwitch(SqlSwitchExpressionNode node)
    {
        var isChild = Context.ChildDepth > 0;
        if ( isChild )
            Context.AppendIndent();

        Context.Sql.Append( "CASE" );

        using ( Context.TempIndentIncrease() )
        {
            foreach ( var @case in node.Cases )
            {
                Context.AppendIndent();
                VisitSwitchCase( @case );
            }

            Context.AppendIndent().Append( "ELSE" ).AppendSpace();
            VisitChild( node.Default );
        }

        Context.AppendIndent().Append( "END" );

        if ( isChild )
            Context.AppendShortIndent();
    }

    public virtual void VisitNamedFunction(SqlNamedFunctionExpressionNode node)
    {
        AppendDelimitedSchemaObjectName( node.Name );
        VisitFunctionArguments( node.Arguments );
    }

    public virtual void VisitCoalesceFunction(SqlCoalesceFunctionExpressionNode node)
    {
        if ( node.Arguments.Count == 1 )
            VisitChild( node.Arguments[0] );
        else
            VisitSimpleFunction( "COALESCE", node );
    }

    public abstract void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node);
    public abstract void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node);
    public abstract void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node);
    public abstract void VisitCurrentUtcDateTimeFunction(SqlCurrentUtcDateTimeFunctionExpressionNode node);
    public abstract void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node);
    public abstract void VisitExtractDateFunction(SqlExtractDateFunctionExpressionNode node);
    public abstract void VisitExtractTimeOfDayFunction(SqlExtractTimeOfDayFunctionExpressionNode node);
    public abstract void VisitExtractDayFunction(SqlExtractDayFunctionExpressionNode node);
    public abstract void VisitExtractTemporalUnitFunction(SqlExtractTemporalUnitFunctionExpressionNode node);
    public abstract void VisitTemporalAddFunction(SqlTemporalAddFunctionExpressionNode node);
    public abstract void VisitTemporalDiffFunction(SqlTemporalDiffFunctionExpressionNode node);
    public abstract void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node);
    public abstract void VisitLengthFunction(SqlLengthFunctionExpressionNode node);
    public abstract void VisitByteLengthFunction(SqlByteLengthFunctionExpressionNode node);
    public abstract void VisitToLowerFunction(SqlToLowerFunctionExpressionNode node);
    public abstract void VisitToUpperFunction(SqlToUpperFunctionExpressionNode node);
    public abstract void VisitTrimStartFunction(SqlTrimStartFunctionExpressionNode node);
    public abstract void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node);
    public abstract void VisitTrimFunction(SqlTrimFunctionExpressionNode node);
    public abstract void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node);
    public abstract void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node);
    public abstract void VisitReverseFunction(SqlReverseFunctionExpressionNode node);
    public abstract void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node);
    public abstract void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node);
    public abstract void VisitSignFunction(SqlSignFunctionExpressionNode node);
    public abstract void VisitAbsFunction(SqlAbsFunctionExpressionNode node);
    public abstract void VisitCeilingFunction(SqlCeilingFunctionExpressionNode node);
    public abstract void VisitFloorFunction(SqlFloorFunctionExpressionNode node);
    public abstract void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node);
    public abstract void VisitRoundFunction(SqlRoundFunctionExpressionNode node);
    public abstract void VisitPowerFunction(SqlPowerFunctionExpressionNode node);
    public abstract void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node);
    public abstract void VisitMinFunction(SqlMinFunctionExpressionNode node);
    public abstract void VisitMaxFunction(SqlMaxFunctionExpressionNode node);

    public virtual void VisitCustomFunction(SqlFunctionExpressionNode node)
    {
        throw new UnrecognizedSqlNodeException( this, node );
    }

    public abstract void VisitNamedAggregateFunction(SqlNamedAggregateFunctionExpressionNode node);
    public abstract void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node);
    public abstract void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node);
    public abstract void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node);
    public abstract void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node);
    public abstract void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node);
    public abstract void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node);
    public abstract void VisitRowNumberWindowFunction(SqlRowNumberWindowFunctionExpressionNode node);
    public abstract void VisitRankWindowFunction(SqlRankWindowFunctionExpressionNode node);
    public abstract void VisitDenseRankWindowFunction(SqlDenseRankWindowFunctionExpressionNode node);
    public abstract void VisitCumulativeDistributionWindowFunction(SqlCumulativeDistributionWindowFunctionExpressionNode node);
    public abstract void VisitNTileWindowFunction(SqlNTileWindowFunctionExpressionNode node);
    public abstract void VisitLagWindowFunction(SqlLagWindowFunctionExpressionNode node);
    public abstract void VisitLeadWindowFunction(SqlLeadWindowFunctionExpressionNode node);
    public abstract void VisitFirstValueWindowFunction(SqlFirstValueWindowFunctionExpressionNode node);
    public abstract void VisitLastValueWindowFunction(SqlLastValueWindowFunctionExpressionNode node);
    public abstract void VisitNthValueWindowFunction(SqlNthValueWindowFunctionExpressionNode node);

    public virtual void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        throw new UnrecognizedSqlNodeException( this, node );
    }

    public virtual void VisitRawCondition(SqlRawConditionNode node)
    {
        AppendMultilineSql( node.Sql );

        foreach ( var parameter in node.Parameters )
            AddContextParameter( parameter );
    }

    public virtual void VisitTrue(SqlTrueNode node)
    {
        Context.Sql.Append( "TRUE" );
    }

    public virtual void VisitFalse(SqlFalseNode node)
    {
        Context.Sql.Append( "FALSE" );
    }

    public virtual void VisitEqualTo(SqlEqualToConditionNode node)
    {
        if ( node.Right.NodeType == SqlNodeType.Null )
        {
            VisitChild( node.Left );
            Context.Sql.AppendSpace().Append( "IS" ).AppendSpace();
            VisitNull( ReinterpretCast.To<SqlNullNode>( node.Right ) );
        }
        else
            VisitInfixBinaryOperator( node.Left, symbol: "=", node.Right );
    }

    public virtual void VisitNotEqualTo(SqlNotEqualToConditionNode node)
    {
        if ( node.Right.NodeType == SqlNodeType.Null )
        {
            VisitChild( node.Left );
            Context.Sql.AppendSpace().Append( "IS" ).AppendSpace().Append( "NOT" ).AppendSpace();
            VisitNull( ReinterpretCast.To<SqlNullNode>( node.Right ) );
        }
        else
            VisitInfixBinaryOperator( node.Left, symbol: "<>", node.Right );
    }

    public virtual void VisitGreaterThan(SqlGreaterThanConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: ">", node.Right );
    }

    public virtual void VisitLessThan(SqlLessThanConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "<", node.Right );
    }

    public virtual void VisitGreaterThanOrEqualTo(SqlGreaterThanOrEqualToConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: ">=", node.Right );
    }

    public virtual void VisitLessThanOrEqualTo(SqlLessThanOrEqualToConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "<=", node.Right );
    }

    public virtual void VisitAnd(SqlAndConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "AND", node.Right );
    }

    public virtual void VisitOr(SqlOrConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "OR", node.Right );
    }

    public virtual void VisitConditionValue(SqlConditionValueNode node)
    {
        this.Visit( node.Condition );
    }

    public virtual void VisitBetween(SqlBetweenConditionNode node)
    {
        VisitChild( node.Value );

        Context.Sql.AppendSpace();
        if ( node.IsNegated )
            Context.Sql.Append( "NOT" ).AppendSpace();

        Context.Sql.Append( "BETWEEN" ).AppendSpace();

        VisitChild( node.Min );
        Context.Sql.AppendSpace().Append( "AND" ).AppendSpace();
        VisitChild( node.Max );
    }

    public virtual void VisitExists(SqlExistsConditionNode node)
    {
        if ( node.IsNegated )
            Context.Sql.Append( "NOT" ).AppendSpace();

        Context.Sql.Append( "EXISTS" ).AppendSpace();
        VisitChild( node.Query );
    }

    public virtual void VisitLike(SqlLikeConditionNode node)
    {
        VisitChild( node.Value );

        Context.Sql.AppendSpace();
        if ( node.IsNegated )
            Context.Sql.Append( "NOT" ).AppendSpace();

        Context.Sql.Append( "LIKE" ).AppendSpace();

        VisitChild( node.Pattern );

        if ( node.Escape is not null )
        {
            Context.Sql.AppendSpace().Append( "ESCAPE" ).AppendSpace();
            VisitChild( node.Escape );
        }
    }

    public virtual void VisitIn(SqlInConditionNode node)
    {
        VisitChild( node.Value );

        Context.Sql.AppendSpace();
        if ( node.IsNegated )
            Context.Sql.Append( "NOT" ).AppendSpace();

        Context.Sql.Append( "IN" ).AppendSpace().Append( '(' );

        foreach ( var expr in node.Expressions )
        {
            VisitChild( expr );
            Context.Sql.AppendComma().AppendSpace();
        }

        Context.Sql.ShrinkBy( 2 ).Append( ')' );
    }

    public virtual void VisitInQuery(SqlInQueryConditionNode node)
    {
        VisitChild( node.Value );

        Context.Sql.AppendSpace();
        if ( node.IsNegated )
            Context.Sql.Append( "NOT" ).AppendSpace();

        Context.Sql.Append( "IN" ).AppendSpace();
        VisitChild( node.Query );
    }

    public virtual void VisitRawRecordSet(SqlRawRecordSetNode node)
    {
        if ( node.IsInfoRaw )
            Context.Sql.Append( node.Info.Name.Object );
        else
            AppendDelimitedRecordSetInfo( node.Info );

        AppendDelimitedAlias( node.Alias );
    }

    public virtual void VisitNamedFunctionRecordSet(SqlNamedFunctionRecordSetNode node)
    {
        VisitNamedFunction( node.Function );
        AppendDelimitedAlias( node.Alias );
    }

    public virtual void VisitTable(SqlTableNode node)
    {
        AppendDelimitedSchemaObjectName( node.Table.Info.Name );
        AppendDelimitedAlias( node.Alias );
    }

    public virtual void VisitTableBuilder(SqlTableBuilderNode node)
    {
        AppendDelimitedSchemaObjectName( node.Table.Info.Name );
        AppendDelimitedAlias( node.Alias );
    }

    public virtual void VisitView(SqlViewNode node)
    {
        AppendDelimitedSchemaObjectName( node.View.Info.Name );
        AppendDelimitedAlias( node.Alias );
    }

    public virtual void VisitViewBuilder(SqlViewBuilderNode node)
    {
        AppendDelimitedSchemaObjectName( node.View.Info.Name );
        AppendDelimitedAlias( node.Alias );
    }

    public virtual void VisitQueryRecordSet(SqlQueryRecordSetNode node)
    {
        VisitChild( node.Query );
        AppendDelimitedAlias( node.Alias );
    }

    public virtual void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node)
    {
        AppendDelimitedName( node.CommonTableExpression.Name );
        AppendDelimitedAlias( node.Alias );
    }

    public virtual void VisitNewTable(SqlNewTableNode node)
    {
        AppendDelimitedRecordSetInfo( node.CreationNode.Info );
        AppendDelimitedAlias( node.Alias );
    }

    public virtual void VisitNewView(SqlNewViewNode node)
    {
        AppendDelimitedRecordSetInfo( node.CreationNode.Info );
        AppendDelimitedAlias( node.Alias );
    }

    public virtual void VisitJoinOn(SqlDataSourceJoinOnNode node)
    {
        var joinType = node.JoinType switch
        {
            SqlJoinType.Left => "LEFT",
            SqlJoinType.Right => "RIGHT",
            SqlJoinType.Full => "FULL",
            SqlJoinType.Cross => "CROSS",
            _ => "INNER"
        };

        AppendJoin( joinType, node );
    }

    public virtual void VisitDataSource(SqlDataSourceNode node)
    {
        if ( node is SqlDummyDataSourceNode )
            return;

        Context.Sql.Append( "FROM" ).AppendSpace();
        this.Visit( node.From );

        foreach ( var join in node.Joins )
        {
            Context.AppendIndent();
            VisitJoinOn( join );
        }
    }

    public virtual void VisitSelectField(SqlSelectFieldNode node)
    {
        if ( node.Alias is not null )
        {
            VisitChild( node.Expression );
            AppendDelimitedAlias( node.Alias );
        }
        else
            this.Visit( node.Expression );
    }

    public virtual void VisitSelectCompoundField(SqlSelectCompoundFieldNode node)
    {
        AppendDelimitedName( node.Name );
    }

    public virtual void VisitSelectRecordSet(SqlSelectRecordSetNode node)
    {
        AppendDelimitedRecordSetName( node.RecordSet );
        Context.Sql.AppendDot().Append( '*' );
    }

    public virtual void VisitSelectAll(SqlSelectAllNode node)
    {
        Context.Sql.Append( '*' );
    }

    public virtual void VisitSelectExpression(SqlSelectExpressionNode node)
    {
        if ( node.Selection.NodeType == SqlNodeType.SelectField )
            AppendDelimitedName( ReinterpretCast.To<SqlSelectFieldNode>( node.Selection ).FieldName );
        else
            this.Visit( node.Selection );
    }

    public virtual void VisitRawQuery(SqlRawQueryExpressionNode node)
    {
        foreach ( var parameter in node.Parameters )
            AddContextParameter( parameter );

        if ( Context.ChildDepth > 0 )
        {
            Context.AppendIndent();
            AppendMultilineSql( node.Sql );
            Context.AppendShortIndent();
        }
        else
            AppendMultilineSql( node.Sql );
    }

    public virtual void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        var isChild = Context.ChildDepth > 0;
        if ( isChild )
            Context.AppendIndent();

        var traits = ExtractDataSourceTraits( ExtractDataSourceTraits( node.DataSource.Traits ), node.Traits );
        VisitDataSourceBeforeTraits( in traits );
        VisitDataSourceQuerySelection( node, in traits );
        VisitDataSource( node.DataSource );
        VisitDataSourceAfterTraits( in traits );

        if ( isChild )
            Context.AppendShortIndent();
    }

    public virtual void VisitCompoundQuery(SqlCompoundQueryExpressionNode node)
    {
        var isChild = Context.ChildDepth > 0;
        if ( isChild )
            Context.AppendIndent();

        var traits = ExtractQueryTraits( default, node.Traits );
        VisitQueryBeforeTraits( in traits );
        VisitCompoundQueryComponents( node, in traits );
        VisitQueryAfterTraits( in traits );

        if ( isChild )
            Context.AppendShortIndent();
    }

    public virtual void VisitCompoundQueryComponent(SqlCompoundQueryComponentNode node)
    {
        var operatorText = node.Operator switch
        {
            SqlCompoundQueryOperator.UnionAll => "UNION ALL",
            SqlCompoundQueryOperator.Intersect => "INTERSECT",
            SqlCompoundQueryOperator.Except => "EXCEPT",
            _ => "UNION"
        };

        Context.Sql.Append( operatorText );
        Context.AppendIndent();
        this.Visit( node.Query );
    }

    public virtual void VisitDistinctTrait(SqlDistinctTraitNode node)
    {
        Context.Sql.Append( "DISTINCT" );
    }

    public virtual void VisitFilterTrait(SqlFilterTraitNode node)
    {
        Context.Sql.Append( "WHERE" ).AppendSpace();
        this.Visit( node.Filter );
    }

    public virtual void VisitAggregationTrait(SqlAggregationTraitNode node)
    {
        Context.Sql.Append( "GROUP BY" );
        if ( node.Expressions.Count == 0 )
            return;

        Context.Sql.AppendSpace();
        foreach ( var expr in node.Expressions )
        {
            VisitChild( expr );
            Context.Sql.AppendComma().AppendSpace();
        }

        Context.Sql.ShrinkBy( 2 );
    }

    public virtual void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node)
    {
        Context.Sql.Append( "HAVING" ).AppendSpace();
        this.Visit( node.Filter );
    }

    public virtual void VisitSortTrait(SqlSortTraitNode node)
    {
        Context.Sql.Append( "ORDER BY" );
        if ( node.Ordering.Count == 0 )
            return;

        Context.Sql.AppendSpace();
        foreach ( var orderBy in node.Ordering )
        {
            VisitOrderBy( orderBy );
            Context.Sql.AppendComma().AppendSpace();
        }

        Context.Sql.ShrinkBy( 2 );
    }

    public abstract void VisitLimitTrait(SqlLimitTraitNode node);
    public abstract void VisitOffsetTrait(SqlOffsetTraitNode node);

    public virtual void VisitCommonTableExpressionTrait(SqlCommonTableExpressionTraitNode node)
    {
        Context.Sql.Append( "WITH" );
        if ( node.CommonTableExpressions.Count == 0 )
            return;

        Context.Sql.AppendSpace();
        if ( node.ContainsRecursive )
            Context.Sql.Append( "RECURSIVE" ).AppendSpace();

        foreach ( var cte in node.CommonTableExpressions )
        {
            VisitCommonTableExpression( cte );
            Context.Sql.AppendComma();
            Context.AppendIndent();
        }

        Context.Sql.ShrinkBy( Environment.NewLine.Length + Context.Indent + 1 );
    }

    public virtual void VisitWindowDefinitionTrait(SqlWindowDefinitionTraitNode node)
    {
        Context.Sql.Append( "WINDOW" );
        if ( node.Windows.Count == 0 )
            return;

        Context.Sql.AppendSpace();
        using ( Context.TempIndentIncrease() )
        {
            foreach ( var definition in node.Windows )
            {
                VisitWindowDefinition( definition );
                Context.Sql.AppendComma();
                Context.AppendIndent();
            }

            Context.Sql.ShrinkBy( Environment.NewLine.Length + Context.Indent + 1 );
        }
    }

    public virtual void VisitWindowTrait(SqlWindowTraitNode node)
    {
        Context.Sql.Append( "OVER" ).AppendSpace();
        AppendDelimitedName( node.Definition.Name );
    }

    public virtual void VisitOrderBy(SqlOrderByNode node)
    {
        VisitChild( node.Expression );
        Context.Sql.AppendSpace().Append( node.Ordering.Name );
    }

    public virtual void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        AppendDelimitedName( node.Name );
        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace();
        VisitChild( node.Query );
    }

    public virtual void VisitWindowDefinition(SqlWindowDefinitionNode node)
    {
        AppendDelimitedName( node.Name );
        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace().Append( '(' );

        if ( node.Partitioning.Count > 0 )
        {
            Context.Sql.Append( "PARTITION" ).AppendSpace().Append( "BY" ).AppendSpace();
            foreach ( var partition in node.Partitioning )
            {
                VisitChild( partition );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }

        if ( node.Ordering.Count > 0 )
        {
            if ( node.Partitioning.Count > 0 )
                Context.Sql.AppendSpace();

            Context.Sql.Append( "ORDER" ).AppendSpace().Append( "BY" ).AppendSpace();
            foreach ( var orderBy in node.Ordering )
            {
                VisitOrderBy( orderBy );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }

        if ( node.Frame is not null )
        {
            if ( node.Partitioning.Count > 0 || node.Ordering.Count > 0 )
                Context.Sql.AppendSpace();

            VisitWindowFrame( node.Frame );
        }

        Context.Sql.Append( ')' );
    }

    public void VisitWindowFrame(SqlWindowFrameNode node)
    {
        switch ( node.FrameType )
        {
            case SqlWindowFrameType.Rows:
                VisitRowsWindowFrame( node );
                break;
            case SqlWindowFrameType.Range:
                VisitRangeWindowFrame( node );
                break;
            default:
                VisitCustomWindowFrame( node );
                break;
        }
    }

    public abstract void VisitTypeCast(SqlTypeCastExpressionNode node);

    public virtual void VisitValues(SqlValuesNode node)
    {
        Context.Sql.Append( "VALUES" );

        for ( var i = 0; i < node.RowCount; ++i )
        {
            Context.AppendIndent().Append( '(' );

            var row = node[i];
            foreach ( var expr in row )
            {
                VisitChild( expr );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 ).Append( ')' ).AppendComma();
        }

        Context.Sql.ShrinkBy( 1 );
    }

    public virtual void VisitRawStatement(SqlRawStatementNode node)
    {
        foreach ( var parameter in node.Parameters )
            AddContextParameter( parameter );

        AppendMultilineSql( node.Sql );
    }

    public abstract void VisitInsertInto(SqlInsertIntoNode node);
    public abstract void VisitUpdate(SqlUpdateNode node);
    public abstract void VisitUpsert(SqlUpsertNode node);

    public virtual void VisitValueAssignment(SqlValueAssignmentNode node)
    {
        AppendDelimitedName( node.DataField.Name );
        Context.Sql.AppendSpace().Append( '=' ).AppendSpace();
        VisitChild( node.Value );
    }

    public abstract void VisitDeleteFrom(SqlDeleteFromNode node);

    public virtual void VisitTruncate(SqlTruncateNode node)
    {
        Context.Sql.Append( "TRUNCATE" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedRecordSetName( node.Table );
    }

    public abstract void VisitColumnDefinition(SqlColumnDefinitionNode node);
    public abstract void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node);
    public abstract void VisitForeignKeyDefinition(SqlForeignKeyDefinitionNode node);
    public abstract void VisitCheckDefinition(SqlCheckDefinitionNode node);
    public abstract void VisitCreateTable(SqlCreateTableNode node);
    public abstract void VisitCreateView(SqlCreateViewNode node);
    public abstract void VisitCreateIndex(SqlCreateIndexNode node);
    public abstract void VisitRenameTable(SqlRenameTableNode node);
    public abstract void VisitRenameColumn(SqlRenameColumnNode node);
    public abstract void VisitAddColumn(SqlAddColumnNode node);
    public abstract void VisitDropColumn(SqlDropColumnNode node);
    public abstract void VisitDropTable(SqlDropTableNode node);
    public abstract void VisitDropView(SqlDropViewNode node);
    public abstract void VisitDropIndex(SqlDropIndexNode node);

    public virtual void VisitStatementBatch(SqlStatementBatchNode node)
    {
        if ( node.Statements.Count == 0 )
            return;

        foreach ( var statement in node.Statements )
        {
            this.Visit( statement.Node );
            if ( statement.Node.NodeType != SqlNodeType.StatementBatch )
                Context.Sql.AppendSemicolon();

            Context.Sql.AppendLine();
            Context.AppendIndent();
        }

        Context.Sql.ShrinkBy( (Environment.NewLine.Length << 1) + Context.Indent );
    }

    public abstract void VisitBeginTransaction(SqlBeginTransactionNode node);

    public virtual void VisitCommitTransaction(SqlCommitTransactionNode node)
    {
        Context.Sql.Append( "COMMIT" );
    }

    public virtual void VisitRollbackTransaction(SqlRollbackTransactionNode node)
    {
        Context.Sql.Append( "ROLLBACK" );
    }

    public virtual void VisitCustom(SqlNodeBase node)
    {
        throw new UnrecognizedSqlNodeException( this, node );
    }

    public virtual void AppendDelimitedRecordSetName(SqlRecordSetNode node)
    {
        if ( node.IsAliased )
        {
            AppendDelimitedName( node.Alias );
            return;
        }

        if ( node.NodeType == SqlNodeType.RawRecordSet && ReinterpretCast.To<SqlRawRecordSetNode>( node ).IsInfoRaw )
        {
            Context.Sql.Append( node.Info.Name.Object );
            return;
        }

        AppendDelimitedRecordSetInfo( node.Info );
    }

    public void AppendDelimitedName(string name)
    {
        Context.Sql.Append( BeginNameDelimiter ).Append( name ).Append( EndNameDelimiter );
    }

    public void AppendDelimitedAlias(string? alias)
    {
        if ( alias is null )
            return;

        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace();
        AppendDelimitedName( alias );
    }

    public virtual void AppendDelimitedTemporaryObjectName(string name)
    {
        Context.Sql.Append( "TEMP" ).AppendDot();
        AppendDelimitedName( name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void AppendDelimitedSchemaObjectName(SqlSchemaObjectName name)
    {
        AppendDelimitedSchemaObjectName( name.Schema, name.Object );
    }

    public virtual void AppendDelimitedSchemaObjectName(string schemaName, string objName)
    {
        if ( schemaName.Length > 0 )
        {
            AppendDelimitedName( schemaName );
            Context.Sql.AppendDot();
        }

        AppendDelimitedName( objName );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void AppendDelimitedRecordSetInfo(SqlRecordSetInfo info)
    {
        if ( info.IsTemporary )
            AppendDelimitedTemporaryObjectName( info.Name.Object );
        else
            AppendDelimitedSchemaObjectName( info.Name );
    }

    public void AppendDelimitedDataFieldName(SqlRecordSetInfo recordSet, string name)
    {
        AppendDelimitedRecordSetInfo( recordSet );
        Context.Sql.AppendDot();
        AppendDelimitedName( name );
    }

    public void AppendMultilineSql(ReadOnlySpan<char> sql)
    {
        if ( Context.Indent <= 0 )
        {
            Context.Sql.Append( sql );
            return;
        }

        var slice = sql;
        while ( slice.Length > 0 )
        {
            var newLineIndex = slice.IndexOf( Environment.NewLine, StringComparison.Ordinal );
            if ( newLineIndex < 0 )
            {
                Context.Sql.Append( slice );
                break;
            }

            Context.Sql.Append( slice.Slice( 0, newLineIndex ) );
            Context.AppendIndent();
            slice = slice.Slice( newLineIndex + Environment.NewLine.Length );
        }
    }

    public void VisitChild(SqlNodeBase node)
    {
        if ( DoesChildNodeRequireParentheses( node ) )
            VisitChildWrappedInParentheses( node );
        else
            this.Visit( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceTraits ExtractDataSourceTraits(Chain<SqlTraitNode> traits)
    {
        return ExtractDataSourceTraits( default, traits );
    }

    [Pure]
    public static SqlDataSourceTraits ExtractDataSourceTraits(SqlDataSourceTraits @base, Chain<SqlTraitNode> traits)
    {
        var commonTableExpressions = @base.CommonTableExpressions.ToExtendable();
        var containsRecursiveCommonTableExpression = @base.ContainsRecursiveCommonTableExpression;
        var distinct = @base.Distinct;
        var filter = @base.Filter;
        var aggregations = @base.Aggregations.ToExtendable();
        var aggregationFilter = @base.AggregationFilter;
        var windows = @base.Windows.ToExtendable();
        var ordering = @base.Ordering.ToExtendable();
        var limit = @base.Limit;
        var offset = @base.Offset;
        var custom = @base.Custom.ToExtendable();

        foreach ( var trait in traits )
        {
            switch ( trait.NodeType )
            {
                case SqlNodeType.DistinctTrait:
                {
                    distinct = ReinterpretCast.To<SqlDistinctTraitNode>( trait );
                    break;
                }
                case SqlNodeType.FilterTrait:
                {
                    var filterTrait = ReinterpretCast.To<SqlFilterTraitNode>( trait );

                    filter = filter is null
                        ? filterTrait.Filter
                        : filterTrait.IsConjunction
                            ? filter.And( filterTrait.Filter )
                            : filter.Or( filterTrait.Filter );

                    break;
                }
                case SqlNodeType.AggregationTrait:
                {
                    var aggregationTrait = ReinterpretCast.To<SqlAggregationTraitNode>( trait );
                    if ( aggregationTrait.Expressions.Count > 0 )
                        aggregations = aggregations.Extend( aggregationTrait.Expressions );

                    break;
                }
                case SqlNodeType.AggregationFilterTrait:
                {
                    var aggregationFilterTrait = ReinterpretCast.To<SqlAggregationFilterTraitNode>( trait );

                    aggregationFilter = aggregationFilter is null
                        ? aggregationFilterTrait.Filter
                        : aggregationFilterTrait.IsConjunction
                            ? aggregationFilter.And( aggregationFilterTrait.Filter )
                            : aggregationFilter.Or( aggregationFilterTrait.Filter );

                    break;
                }
                case SqlNodeType.SortTrait:
                {
                    var sortTrait = ReinterpretCast.To<SqlSortTraitNode>( trait );
                    if ( sortTrait.Ordering.Count > 0 )
                        ordering = ordering.Extend( sortTrait.Ordering );

                    break;
                }
                case SqlNodeType.LimitTrait:
                {
                    limit = ReinterpretCast.To<SqlLimitTraitNode>( trait ).Value;
                    break;
                }
                case SqlNodeType.OffsetTrait:
                {
                    offset = ReinterpretCast.To<SqlOffsetTraitNode>( trait ).Value;
                    break;
                }
                case SqlNodeType.CommonTableExpressionTrait:
                {
                    var cteTrait = ReinterpretCast.To<SqlCommonTableExpressionTraitNode>( trait );
                    if ( cteTrait.CommonTableExpressions.Count > 0 )
                    {
                        commonTableExpressions = commonTableExpressions.Extend( cteTrait.CommonTableExpressions );
                        containsRecursiveCommonTableExpression = containsRecursiveCommonTableExpression || cteTrait.ContainsRecursive;
                    }

                    break;
                }
                case SqlNodeType.WindowDefinitionTrait:
                {
                    var windowTrait = ReinterpretCast.To<SqlWindowDefinitionTraitNode>( trait );
                    if ( windowTrait.Windows.Count > 0 )
                        windows = windows.Extend( windowTrait.Windows );

                    break;
                }
                default:
                {
                    custom = custom.Extend( trait );
                    break;
                }
            }
        }

        return new SqlDataSourceTraits(
            commonTableExpressions,
            containsRecursiveCommonTableExpression,
            distinct,
            filter,
            aggregations,
            aggregationFilter,
            windows,
            ordering,
            limit,
            offset,
            custom );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryTraits ExtractQueryTraits(Chain<SqlTraitNode> traits)
    {
        return ExtractQueryTraits( default, traits );
    }

    [Pure]
    public static SqlQueryTraits ExtractQueryTraits(SqlQueryTraits @base, Chain<SqlTraitNode> traits)
    {
        var commonTableExpressions = @base.CommonTableExpressions.ToExtendable();
        var containsRecursiveCommonTableExpression = @base.ContainsRecursiveCommonTableExpression;
        var ordering = @base.Ordering.ToExtendable();
        var limit = @base.Limit;
        var offset = @base.Offset;
        var custom = @base.Custom.ToExtendable();

        foreach ( var trait in traits )
        {
            switch ( trait.NodeType )
            {
                case SqlNodeType.SortTrait:
                {
                    var sortTrait = ReinterpretCast.To<SqlSortTraitNode>( trait );
                    if ( sortTrait.Ordering.Count > 0 )
                        ordering = ordering.Extend( sortTrait.Ordering );

                    break;
                }
                case SqlNodeType.LimitTrait:
                {
                    limit = ReinterpretCast.To<SqlLimitTraitNode>( trait ).Value;
                    break;
                }
                case SqlNodeType.OffsetTrait:
                {
                    offset = ReinterpretCast.To<SqlOffsetTraitNode>( trait ).Value;
                    break;
                }
                case SqlNodeType.CommonTableExpressionTrait:
                {
                    var cteTrait = ReinterpretCast.To<SqlCommonTableExpressionTraitNode>( trait );
                    if ( cteTrait.CommonTableExpressions.Count > 0 )
                    {
                        commonTableExpressions = commonTableExpressions.Extend( cteTrait.CommonTableExpressions );
                        containsRecursiveCommonTableExpression = containsRecursiveCommonTableExpression || cteTrait.ContainsRecursive;
                    }

                    break;
                }
                default:
                {
                    custom = custom.Extend( trait );
                    break;
                }
            }
        }

        return new SqlQueryTraits( commonTableExpressions, containsRecursiveCommonTableExpression, ordering, limit, offset, custom );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAggregateFunctionTraits ExtractAggregateFunctionTraits(Chain<SqlTraitNode> traits)
    {
        return ExtractAggregateFunctionTraits( default, traits );
    }

    [Pure]
    public static SqlAggregateFunctionTraits ExtractAggregateFunctionTraits(SqlAggregateFunctionTraits @base, Chain<SqlTraitNode> traits)
    {
        var distinct = @base.Distinct;
        var filter = @base.Filter;
        var window = @base.Window;
        var ordering = @base.Ordering.ToExtendable();
        var custom = @base.Custom.ToExtendable();

        foreach ( var trait in traits )
        {
            switch ( trait.NodeType )
            {
                case SqlNodeType.DistinctTrait:
                {
                    distinct = ReinterpretCast.To<SqlDistinctTraitNode>( trait );
                    break;
                }
                case SqlNodeType.FilterTrait:
                {
                    var filterTrait = ReinterpretCast.To<SqlFilterTraitNode>( trait );

                    filter = filter is null
                        ? filterTrait.Filter
                        : filterTrait.IsConjunction
                            ? filter.And( filterTrait.Filter )
                            : filter.Or( filterTrait.Filter );

                    break;
                }
                case SqlNodeType.WindowTrait:
                {
                    window = ReinterpretCast.To<SqlWindowTraitNode>( trait ).Definition;
                    break;
                }
                case SqlNodeType.SortTrait:
                {
                    var sortTrait = ReinterpretCast.To<SqlSortTraitNode>( trait );
                    if ( sortTrait.Ordering.Count > 0 )
                        ordering = ordering.Extend( sortTrait.Ordering );

                    break;
                }
                default:
                {
                    custom = custom.Extend( trait );
                    break;
                }
            }
        }

        return new SqlAggregateFunctionTraits( distinct, filter, window, ordering, custom );
    }

    [Pure]
    public static Chain<SqlTraitNode> FilterTraits(Chain<SqlTraitNode> traits, Func<SqlTraitNode, bool> predicate)
    {
        var result = Chain<SqlTraitNode>.Empty;
        foreach ( var trait in traits )
        {
            if ( predicate( trait ) )
                result = result.Extend( trait );
        }

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TemporaryRecordSetNameBehaviorRuleChange TempIgnoreRecordSet(SqlRecordSetNode node)
    {
        return new TemporaryRecordSetNameBehaviorRuleChange( this, new RecordSetNameBehaviorRule( node, null ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TemporaryRecordSetNameBehaviorRuleChange TempReplaceRecordSet(SqlRecordSetNode node, SqlRecordSetNode replacementNode)
    {
        return new TemporaryRecordSetNameBehaviorRuleChange( this, new RecordSetNameBehaviorRule( node, replacementNode ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TemporaryRecordSetNameBehaviorRuleChange TempIgnoreAllRecordSets()
    {
        return new TemporaryRecordSetNameBehaviorRuleChange( this, new RecordSetNameBehaviorRule( null, null ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TemporaryRecordSetNameBehaviorRuleChange TempIncludeAllRecordSets()
    {
        return new TemporaryRecordSetNameBehaviorRuleChange( this, null );
    }

    public readonly struct RecordSetNameBehaviorRule
    {
        public readonly SqlRecordSetNode? Node;
        public readonly SqlRecordSetNode? ReplacementNode;

        internal RecordSetNameBehaviorRule(SqlRecordSetNode? node, SqlRecordSetNode? replacementNode)
        {
            Node = node;
            ReplacementNode = replacementNode;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public SqlRecordSetNode? GetRecordSet(SqlRecordSetNode recordSet)
        {
            return Node is null || ReferenceEquals( Node, recordSet ) ? ReplacementNode : recordSet;
        }
    }

    public readonly struct TemporaryRecordSetNameBehaviorRuleChange : IDisposable
    {
        private readonly RecordSetNameBehaviorRule? _previous;
        private readonly SqlNodeInterpreter _interpreter;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal TemporaryRecordSetNameBehaviorRuleChange(SqlNodeInterpreter interpreter, RecordSetNameBehaviorRule? rule)
        {
            _interpreter = interpreter;
            _previous = interpreter.RecordSetNameBehavior;
            interpreter.RecordSetNameBehavior = rule;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            _interpreter.RecordSetNameBehavior = _previous;
        }
    }

    protected virtual void AddContextParameter(SqlParameterNode node)
    {
        Context.AddParameter( node.Name, node.Type, node.Index );
    }

    protected void VisitPrefixUnaryOperator(SqlNodeBase value, string symbol)
    {
        Context.Sql.Append( symbol );
        VisitChildWrappedInParentheses( value );
    }

    protected void VisitInfixBinaryOperator(SqlNodeBase left, string symbol, SqlNodeBase right)
    {
        VisitChild( left );
        Context.Sql.AppendSpace().Append( symbol ).AppendSpace();
        VisitChild( right );
    }

    protected void VisitChildWrappedInParentheses(SqlNodeBase node)
    {
        Context.Sql.Append( '(' );

        using ( Context.TempChildDepthIncrease() )
        using ( Context.TempIndentIncrease() )
            this.Visit( node );

        Context.Sql.Append( ')' );
    }

    protected void TryAppendDataFieldRecordSetNameBasedOnNameBehavior(SqlDataFieldNode node)
    {
        var recordSet = node.RecordSet;
        if ( RecordSetNameBehavior is not null )
            recordSet = RecordSetNameBehavior.Value.GetRecordSet( recordSet );

        if ( recordSet is null )
            return;

        AppendDelimitedRecordSetName( recordSet );
        Context.Sql.AppendDot();
    }

    protected void VisitSimpleFunction(string functionName, SqlFunctionExpressionNode node)
    {
        Context.Sql.Append( functionName );
        VisitFunctionArguments( node.Arguments );
    }

    protected void VisitFunctionArguments(ReadOnlyArray<SqlExpressionNode> arguments)
    {
        Context.Sql.Append( '(' );

        if ( arguments.Count > 0 )
        {
            using ( Context.TempIndentIncrease() )
            {
                foreach ( var arg in arguments )
                {
                    VisitChild( arg );
                    Context.Sql.AppendComma().AppendSpace();
                }
            }

            Context.Sql.ShrinkBy( 2 );
        }

        Context.Sql.Append( ')' );
    }

    protected void AppendJoin(string joinType, SqlDataSourceJoinOnNode node)
    {
        Context.Sql.Append( joinType ).AppendSpace().Append( "JOIN" ).AppendSpace();
        this.Visit( node.InnerRecordSet );

        if ( node.JoinType == SqlJoinType.Cross )
            return;

        Context.Sql.AppendSpace().Append( "ON" ).AppendSpace();
        this.Visit( node.OnExpression );
    }

    protected virtual void VisitDataSourceQuerySelection(SqlDataSourceQueryExpressionNode node, in SqlDataSourceTraits traits)
    {
        Context.Sql.Append( "SELECT" );
        VisitOptionalDistinctMarker( traits.Distinct );

        using ( Context.TempIndentIncrease() )
        {
            foreach ( var selection in node.Selection )
            {
                Context.AppendIndent();
                this.Visit( selection );
                Context.Sql.AppendComma();
            }
        }

        Context.Sql.ShrinkBy( 1 );
        Context.AppendIndent();
    }

    protected virtual void VisitCompoundQueryComponents(SqlCompoundQueryExpressionNode node, in SqlQueryTraits traits)
    {
        this.Visit( node.FirstQuery );
        foreach ( var component in node.FollowingQueries )
        {
            Context.AppendIndent();
            VisitCompoundQueryComponent( component );
        }
    }

    protected void VisitInsertIntoFields(SqlInsertIntoNode node)
    {
        VisitInsertIntoFields( node.RecordSet, node.DataFields );
    }

    protected void VisitInsertIntoFields(SqlRecordSetNode recordSet, ReadOnlyArray<SqlDataFieldNode> dataFields)
    {
        Context.Sql.Append( "INSERT INTO" ).AppendSpace();
        AppendDelimitedRecordSetName( recordSet );
        Context.Sql.AppendSpace().Append( '(' );

        if ( dataFields.Count > 0 )
        {
            foreach ( var dataField in dataFields )
            {
                AppendDelimitedName( dataField.Name );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }

        Context.Sql.Append( ')' );
    }

    protected virtual void VisitDataSourceBeforeTraits(in SqlDataSourceTraits traits)
    {
        VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions, traits.ContainsRecursiveCommonTableExpression );
    }

    protected virtual void VisitDataSourceAfterTraits(in SqlDataSourceTraits traits)
    {
        VisitOptionalFilterCondition( traits.Filter );
        VisitOptionalAggregationRange( traits.Aggregations );
        VisitOptionalAggregationFilterCondition( traits.AggregationFilter );
        VisitOptionalWindowRange( traits.Windows );
        VisitOptionalOrderingRange( traits.Ordering );
    }

    protected virtual void VisitQueryBeforeTraits(in SqlQueryTraits traits)
    {
        VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions, traits.ContainsRecursiveCommonTableExpression );
    }

    protected virtual void VisitQueryAfterTraits(in SqlQueryTraits traits)
    {
        VisitOptionalOrderingRange( traits.Ordering );
    }

    protected virtual void VisitRowsWindowFrame(SqlWindowFrameNode node)
    {
        Context.Sql.Append( "ROWS" ).AppendSpace().Append( "BETWEEN" ).AppendSpace();
        AppendWindowFrameBoundary( node.Start );
        Context.Sql.AppendSpace().Append( "AND" ).AppendSpace();
        AppendWindowFrameBoundary( node.End );
    }

    protected virtual void VisitRangeWindowFrame(SqlWindowFrameNode node)
    {
        Context.Sql.Append( "RANGE" ).AppendSpace().Append( "BETWEEN" ).AppendSpace();
        AppendWindowFrameBoundary( node.Start );
        Context.Sql.AppendSpace().Append( "AND" ).AppendSpace();
        AppendWindowFrameBoundary( node.End );
    }

    protected virtual void AppendWindowFrameBoundary(SqlWindowFrameBoundary boundary)
    {
        switch ( boundary.Direction )
        {
            case SqlWindowFrameBoundaryDirection.Preceding:
                if ( boundary.Expression is null )
                    Context.Sql.Append( "UNBOUNDED" );
                else
                    VisitChild( boundary.Expression );

                Context.Sql.AppendSpace().Append( "PRECEDING" );
                break;

            case SqlWindowFrameBoundaryDirection.Following:
                if ( boundary.Expression is null )
                    Context.Sql.Append( "UNBOUNDED" );
                else
                    VisitChild( boundary.Expression );

                Context.Sql.AppendSpace().Append( "FOLLOWING" );
                break;

            default:
                Context.Sql.Append( "CURRENT" ).AppendSpace().Append( "ROW" );
                break;
        }
    }

    protected virtual void VisitCustomWindowFrame(SqlWindowFrameNode node)
    {
        throw new UnrecognizedSqlNodeException( this, node );
    }

    protected void VisitOptionalCommonTableExpressionRange(
        Chain<ReadOnlyArray<SqlCommonTableExpressionNode>> commonTableExpressions,
        bool addRecursiveKeyword)
    {
        if ( commonTableExpressions.Count == 0 )
            return;

        Context.Sql.Append( "WITH" ).AppendSpace();
        if ( addRecursiveKeyword )
            Context.Sql.Append( "RECURSIVE" ).AppendSpace();

        foreach ( var cteRange in commonTableExpressions )
        {
            foreach ( var cte in cteRange )
            {
                VisitCommonTableExpression( cte );
                Context.Sql.AppendComma();
                Context.AppendIndent();
            }
        }

        Context.Sql.ShrinkBy( Environment.NewLine.Length + Context.Indent + 1 );
        Context.AppendIndent();
    }

    protected void VisitOptionalDistinctMarker(SqlDistinctTraitNode? distinct)
    {
        if ( distinct is null )
            return;

        Context.Sql.AppendSpace();
        VisitDistinctTrait( distinct );
    }

    protected void VisitOptionalFilterCondition(SqlConditionNode? filter)
    {
        if ( filter is null )
            return;

        Context.AppendIndent().Append( "WHERE" ).AppendSpace();
        this.Visit( filter );
    }

    protected void VisitOptionalAggregationRange(Chain<ReadOnlyArray<SqlExpressionNode>> aggregations)
    {
        if ( aggregations.Count == 0 )
            return;

        Context.AppendIndent().Append( "GROUP BY" ).AppendSpace();

        foreach ( var aggregationRange in aggregations )
        {
            foreach ( var aggregation in aggregationRange )
            {
                VisitChild( aggregation );
                Context.Sql.AppendComma().AppendSpace();
            }
        }

        Context.Sql.ShrinkBy( 2 );
    }

    protected void VisitOptionalAggregationFilterCondition(SqlConditionNode? filter)
    {
        if ( filter is null )
            return;

        Context.AppendIndent().Append( "HAVING" ).AppendSpace();
        this.Visit( filter );
    }

    protected void VisitOptionalWindowRange(Chain<ReadOnlyArray<SqlWindowDefinitionNode>> windows)
    {
        if ( windows.Count == 0 )
            return;

        Context.AppendIndent().Append( "WINDOW" ).AppendSpace();
        using ( Context.TempIndentIncrease() )
        {
            foreach ( var windowRange in windows )
            {
                foreach ( var window in windowRange )
                {
                    VisitWindowDefinition( window );
                    Context.Sql.AppendComma();
                    Context.AppendIndent();
                }
            }

            Context.Sql.ShrinkBy( Environment.NewLine.Length + Context.Indent + 1 );
        }
    }

    protected void VisitOptionalOrderingRange(Chain<ReadOnlyArray<SqlOrderByNode>> ordering)
    {
        if ( ordering.Count == 0 )
            return;

        Context.AppendIndent().Append( "ORDER BY" ).AppendSpace();

        foreach ( var orderByRange in ordering )
        {
            foreach ( var orderBy in orderByRange )
            {
                VisitOrderBy( orderBy );
                Context.Sql.AppendComma().AppendSpace();
            }
        }

        Context.Sql.ShrinkBy( 2 );
    }

    protected void VisitUpdateAssignmentRange(ReadOnlyArray<SqlValueAssignmentNode> assignments)
    {
        Context.Sql.AppendSpace().Append( "SET" );

        if ( assignments.Count > 0 )
        {
            using ( Context.TempIndentIncrease() )
            {
                foreach ( var assignment in assignments )
                {
                    Context.AppendIndent();
                    VisitValueAssignment( assignment );
                    Context.Sql.AppendComma();
                }
            }

            Context.Sql.ShrinkBy( 1 );
        }
    }

    [Pure]
    protected abstract bool DoesChildNodeRequireParentheses(SqlNodeBase node);

    protected void VisitCreateTableDefinition(SqlCreateTableNode node)
    {
        using ( Context.TempIndentIncrease() )
        {
            foreach ( var column in node.Columns )
            {
                Context.AppendIndent();
                VisitColumnDefinition( column );
                Context.Sql.AppendComma();
            }

            if ( node.PrimaryKey is not null )
            {
                Context.AppendIndent();
                VisitPrimaryKeyDefinition( node.PrimaryKey );
                Context.Sql.AppendComma();
            }

            foreach ( var foreignKey in node.ForeignKeys )
            {
                Context.AppendIndent();
                VisitForeignKeyDefinition( foreignKey );
                Context.Sql.AppendComma();
            }

            foreach ( var check in node.Checks )
            {
                Context.AppendIndent();
                VisitCheckDefinition( check );
                Context.Sql.AppendComma();
            }

            if ( node.Columns.Count > 0 || node.PrimaryKey is not null || node.ForeignKeys.Count > 0 || node.Checks.Count > 0 )
                Context.Sql.ShrinkBy( 1 );
        }
    }

    [Pure]
    protected static SqlFilterTraitNode CreateComplexDeleteOrUpdateFilter(ChangeTargetInfo targetInfo, SqlDataSourceNode dataSource)
    {
        var traits = Chain<SqlTraitNode>.Empty;
        foreach ( var trait in dataSource.Traits )
        {
            if ( trait.NodeType != SqlNodeType.CommonTableExpressionTrait )
                traits = traits.Extend( trait );
        }

        var pkBaseColumnNode = targetInfo.BaseTarget.GetUnsafeField( targetInfo.IdentityColumnNames[0] );
        var pkColumnNode = targetInfo.Target.GetUnsafeField( targetInfo.IdentityColumnNames[0] );

        if ( targetInfo.IdentityColumnNames.Length == 1 )
        {
            dataSource = dataSource.SetTraits( traits );
            return SqlNode.FilterTrait( pkBaseColumnNode.InQuery( dataSource.Select( pkColumnNode.AsSelf() ) ), isConjunction: true );
        }

        var pkFilter = pkBaseColumnNode == pkColumnNode;
        foreach ( var name in targetInfo.IdentityColumnNames.AsSpan( 1 ) )
        {
            pkBaseColumnNode = targetInfo.BaseTarget.GetUnsafeField( name );
            pkColumnNode = targetInfo.Target.GetUnsafeField( name );
            pkFilter = pkFilter.And( pkBaseColumnNode == pkColumnNode );
        }

        traits = traits.Extend( SqlNode.FilterTrait( pkFilter, isConjunction: true ) );
        dataSource = dataSource.SetTraits( traits );
        return SqlNode.FilterTrait( dataSource.Exists(), isConjunction: true );
    }

    [Pure]
    protected static SqlUpdateNode CreateSimplifiedUpdateFrom(
        ChangeTargetInfo targetInfo,
        SqlUpdateNode node,
        ComplexUpdateAssignmentsVisitor? updateAssignmentsVisitor)
    {
        Ensure.IsNotNull( targetInfo.Target.Alias );
        var indexesOfComplexAssignments = updateAssignmentsVisitor is not null
            ? updateAssignmentsVisitor.GetIndexesOfComplexAssignments()
            : ReadOnlySpan<int>.Empty;

        var (updateTraits, cteTraits) = SeparateCommonTableExpressionTraits( node.DataSource.Traits );
        var cteSelection = new SqlSelectNode[targetInfo.IdentityColumnNames.Length + indexesOfComplexAssignments.Length];
        var cteIdentityFieldNames = PrepareCommonTableExpressionIdentitySelection( targetInfo, cteSelection );

        var assignments = node.Assignments.AsSpan().ToArray();
        var cteComplexAssignmentFieldNames = indexesOfComplexAssignments.Length > 0
            ? new string[indexesOfComplexAssignments.Length]
            : Array.Empty<string>();

        var i = cteIdentityFieldNames.Length;
        foreach ( var assignmentIndex in indexesOfComplexAssignments )
        {
            var assignment = assignments[assignmentIndex];
            var fieldName = $"VAL_{assignment.DataField.Name}_{assignmentIndex}";
            cteComplexAssignmentFieldNames[i - cteIdentityFieldNames.Length] = fieldName;
            cteSelection[i++] = assignment.Value.As( fieldName );
        }

        var cteDataSource = CreateCommonTableExpressionForComplexDeleteOrUpdate( node.DataSource, cteTraits, cteSelection );
        var pkFilter = CreatePrimaryKeyFilterFromCommonTableExpression( targetInfo, cteDataSource.From, cteIdentityFieldNames );

        i = 0;
        foreach ( var assignmentIndex in indexesOfComplexAssignments )
        {
            var assignment = assignments[assignmentIndex];
            var cteField = cteDataSource.From.GetUnsafeField( cteComplexAssignmentFieldNames[i++] );
            assignments[assignmentIndex] = assignment.DataField.Assign( cteField );
        }

        updateTraits = updateTraits.Extend( SqlNode.CommonTableExpressionTrait( cteDataSource.From.CommonTableExpression ) );
        return targetInfo.BaseTarget.Join( cteDataSource.From.InnerOn( pkFilter ) ).SetTraits( updateTraits ).ToUpdate( assignments );
    }

    [Pure]
    protected static SqlDeleteFromNode CreateSimplifiedDeleteFrom(ChangeTargetInfo targetInfo, SqlDeleteFromNode node)
    {
        Ensure.IsNotNull( targetInfo.Target.Alias );
        var (deleteTraits, cteTraits) = SeparateCommonTableExpressionTraits( node.DataSource.Traits );
        var cteSelection = new SqlSelectNode[targetInfo.IdentityColumnNames.Length];
        var cteIdentityFieldNames = PrepareCommonTableExpressionIdentitySelection( targetInfo, cteSelection );

        var cteDataSource = CreateCommonTableExpressionForComplexDeleteOrUpdate( node.DataSource, cteTraits, cteSelection );
        var pkFilter = CreatePrimaryKeyFilterFromCommonTableExpression( targetInfo, cteDataSource.From, cteIdentityFieldNames );

        deleteTraits = deleteTraits.Extend( SqlNode.CommonTableExpressionTrait( cteDataSource.From.CommonTableExpression ) );
        return targetInfo.BaseTarget.Join( cteDataSource.From.InnerOn( pkFilter ) ).SetTraits( deleteTraits ).ToDeleteFrom();
    }

    [Pure]
    protected static (Chain<SqlTraitNode> CommonTableExpressions, Chain<SqlTraitNode> Other) SeparateCommonTableExpressionTraits(
        Chain<SqlTraitNode> traits)
    {
        var cte = Chain<SqlTraitNode>.Empty;
        var other = Chain<SqlTraitNode>.Empty;
        foreach ( var trait in traits )
        {
            if ( trait.NodeType == SqlNodeType.CommonTableExpressionTrait )
                cte = cte.Extend( trait );
            else
                other = other.Extend( trait );
        }

        return (cte, other);
    }

    [Pure]
    protected static Chain<SqlTraitNode> RemoveCommonTableExpressionTraits(Chain<SqlTraitNode> traits)
    {
        var result = Chain<SqlTraitNode>.Empty;
        foreach ( var trait in traits )
        {
            if ( trait.NodeType != SqlNodeType.CommonTableExpressionTrait )
                result = result.Extend( trait );
        }

        return result;
    }

    [Pure]
    protected static string[] ExtractIdentityColumnNames(SqlTableNode node)
    {
        var i = 0;
        var identityColumns = node.Table.Constraints.PrimaryKey.Index.Columns;
        var identityColumnNames = new string[identityColumns.Count];
        foreach ( var c in identityColumns )
        {
            Ensure.IsNotNull( c.Column );
            identityColumnNames[i++] = c.Column.Name;
        }

        return identityColumnNames;
    }

    [Pure]
    protected static string[]? TryExtractIdentityColumnNames(SqlTableBuilderNode node)
    {
        string[] identityColumnNames;
        var primaryKey = node.Table.Constraints.TryGetPrimaryKey();

        if ( primaryKey is not null )
        {
            var identityColumns = primaryKey.Index.Columns;
            identityColumnNames = new string[identityColumns.Expressions.Count];

            var i = 0;
            foreach ( var column in identityColumns )
            {
                Ensure.IsNotNull( column );
                identityColumnNames[i++] = column.Name;
            }
        }
        else
        {
            var index = 0;
            var identityColumns = node.Table.Columns;
            if ( identityColumns.Count == 0 )
                return null;

            identityColumnNames = new string[identityColumns.Count];
            foreach ( var column in identityColumns )
                identityColumnNames[index++] = column.Name;
        }

        return identityColumnNames;
    }

    [Pure]
    protected static string[]? TryExtractIdentityColumnNames(SqlNewTableNode node)
    {
        string[] identityColumnNames;
        var primaryKey = node.CreationNode.PrimaryKey;

        if ( primaryKey is not null )
        {
            var identityColumns = primaryKey.Columns;
            if ( identityColumns.Count == 0 )
                return null;

            identityColumnNames = new string[identityColumns.Count];
            for ( var i = 0; i < identityColumns.Count; ++i )
            {
                if ( identityColumns[i].Expression is not SqlDataFieldNode dataField )
                    return null;

                identityColumnNames[i] = dataField.Name;
            }
        }
        else
        {
            var index = 0;
            var identityColumns = node.CreationNode.Columns;
            if ( identityColumns.Count == 0 )
                return null;

            identityColumnNames = new string[identityColumns.Count];
            foreach ( var column in identityColumns )
                identityColumnNames[index++] = column.Name;
        }

        return identityColumnNames;
    }

    [Pure]
    protected static ChangeTargetInfo ExtractTableDeleteOrUpdateInfo(SqlTableNode node)
    {
        var identityColumnNames = ExtractIdentityColumnNames( node );
        return new ChangeTargetInfo( node, node.MarkAsOptional( false ).AsSelf(), identityColumnNames );
    }

    [Pure]
    protected ChangeTargetInfo ExtractTableBuilderDeleteInfo(SqlDeleteFromNode node, SqlTableBuilderNode target)
    {
        var identityColumnNames = TryExtractIdentityColumnNames( target );
        if ( identityColumnNames is null )
            throw new SqlNodeVisitorException( ExceptionResources.TargetDoesNotContainValidIdentityColumns, this, node );

        return new ChangeTargetInfo( target, target.MarkAsOptional( false ).AsSelf(), identityColumnNames );
    }

    [Pure]
    protected ChangeTargetInfo ExtractTableBuilderUpdateInfo(SqlUpdateNode node, SqlTableBuilderNode target)
    {
        var identityColumnNames = TryExtractIdentityColumnNames( target );
        if ( identityColumnNames is null )
            throw new SqlNodeVisitorException( ExceptionResources.TargetDoesNotContainValidIdentityColumns, this, node );

        return new ChangeTargetInfo( target, target.MarkAsOptional( false ).AsSelf(), identityColumnNames );
    }

    [Pure]
    protected ChangeTargetInfo ExtractNewTableDeleteInfo(
        SqlDeleteFromNode node,
        SqlNewTableNode target)
    {
        var identityColumnNames = TryExtractIdentityColumnNames( target );
        if ( identityColumnNames is null )
            throw new SqlNodeVisitorException( ExceptionResources.TargetDoesNotContainValidIdentityColumns, this, node );

        return new ChangeTargetInfo( target, target.MarkAsOptional( false ).AsSelf(), identityColumnNames );
    }

    [Pure]
    protected ChangeTargetInfo ExtractNewTableUpdateInfo(SqlUpdateNode node, SqlNewTableNode target)
    {
        var identityColumnNames = TryExtractIdentityColumnNames( target );
        if ( identityColumnNames is null )
            throw new SqlNodeVisitorException( ExceptionResources.TargetDoesNotContainValidIdentityColumns, this, node );

        return new ChangeTargetInfo( target, target.MarkAsOptional( false ).AsSelf(), identityColumnNames );
    }

    [Pure]
    protected ChangeTargetInfo ExtractTargetInfo(SqlDeleteFromNode node)
    {
        var from = node.DataSource.From;
        var info = from.NodeType switch
        {
            SqlNodeType.Table => ExtractTableDeleteOrUpdateInfo( ReinterpretCast.To<SqlTableNode>( from ) ),
            SqlNodeType.TableBuilder => ExtractTableBuilderDeleteInfo( node, ReinterpretCast.To<SqlTableBuilderNode>( from ) ),
            SqlNodeType.NewTable => ExtractNewTableDeleteInfo( node, ReinterpretCast.To<SqlNewTableNode>( from ) ),
            _ => throw new SqlNodeVisitorException( ExceptionResources.TargetIsNotValidRecordSet, this, node )
        };

        if ( ! from.IsAliased )
            throw new SqlNodeVisitorException( ExceptionResources.TargetIsNotAliased, this, node );

        return info;
    }

    [Pure]
    protected ChangeTargetInfo ExtractTargetInfo(SqlUpdateNode node)
    {
        var from = node.DataSource.From;
        var info = from.NodeType switch
        {
            SqlNodeType.Table => ExtractTableDeleteOrUpdateInfo( ReinterpretCast.To<SqlTableNode>( from ) ),
            SqlNodeType.TableBuilder => ExtractTableBuilderUpdateInfo( node, ReinterpretCast.To<SqlTableBuilderNode>( from ) ),
            SqlNodeType.NewTable => ExtractNewTableUpdateInfo( node, ReinterpretCast.To<SqlNewTableNode>( from ) ),
            _ => throw new SqlNodeVisitorException( ExceptionResources.TargetIsNotValidRecordSet, this, node )
        };

        if ( ! from.IsAliased )
            throw new SqlNodeVisitorException( ExceptionResources.TargetIsNotAliased, this, node );

        return info;
    }

    [Pure]
    protected SqlDataFieldNode[] ExtractUpsertConflictTargets(SqlUpsertNode node)
    {
        var target = node.RecordSet;
        var identityColumnNames = target.NodeType switch
        {
            SqlNodeType.Table => ExtractIdentityColumnNames( ReinterpretCast.To<SqlTableNode>( target ) ),
            SqlNodeType.TableBuilder => TryExtractIdentityColumnNames( ReinterpretCast.To<SqlTableBuilderNode>( target ) ),
            SqlNodeType.NewTable => TryExtractIdentityColumnNames( ReinterpretCast.To<SqlNewTableNode>( target ) ),
            _ => throw new SqlNodeVisitorException( ExceptionResources.TargetIsNotValidRecordSet, this, node )
        };

        if ( identityColumnNames is null )
            throw new SqlNodeVisitorException( ExceptionResources.TargetDoesNotContainValidIdentityColumns, this, node );

        var result = new SqlDataFieldNode[identityColumnNames.Length];
        for ( var i = 0; i < result.Length; ++i )
            result[i] = target.GetUnsafeField( identityColumnNames[i] );

        return result;
    }

    [Pure]
    protected static ComplexUpdateAssignmentsVisitor? CreateUpdateAssignmentsVisitor(SqlUpdateNode node)
    {
        if ( node.DataSource.Joins.Count == 0 )
            return null;

        var result = new ComplexUpdateAssignmentsVisitor( node.DataSource );
        result.VisitAssignmentRange( node.Assignments );
        return result;
    }

    protected readonly record struct ChangeTargetInfo(SqlRecordSetNode Target, SqlRecordSetNode BaseTarget, string[] IdentityColumnNames);

    protected sealed class ComplexUpdateAssignmentsVisitor : SqlNodeVisitor
    {
        private readonly SqlRecordSetNode[] _joinedRecordSets;
        private List<int>? _indexesOfComplexAssignments;
        private int _nextAssignmentIndex;

        internal ComplexUpdateAssignmentsVisitor(SqlDataSourceNode dataSource)
        {
            Assume.IsGreaterThan( dataSource.Joins.Count, 0 );

            var index = 0;
            _joinedRecordSets = new SqlRecordSetNode[dataSource.Joins.Count];
            foreach ( var join in dataSource.Joins )
                _joinedRecordSets[index++] = join.InnerRecordSet;

            _indexesOfComplexAssignments = null;
            _nextAssignmentIndex = 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool ContainsComplexAssignments()
        {
            return _indexesOfComplexAssignments is not null;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ReadOnlySpan<int> GetIndexesOfComplexAssignments()
        {
            return CollectionsMarshal.AsSpan( _indexesOfComplexAssignments );
        }

        public override void VisitRawDataField(SqlRawDataFieldNode node)
        {
            VisitDataField( node );
        }

        public override void VisitColumn(SqlColumnNode node)
        {
            VisitDataField( node );
        }

        public override void VisitColumnBuilder(SqlColumnBuilderNode node)
        {
            VisitDataField( node );
        }

        public override void VisitQueryDataField(SqlQueryDataFieldNode node)
        {
            VisitDataField( node );
        }

        public override void VisitViewDataField(SqlViewDataFieldNode node)
        {
            VisitDataField( node );
        }

        public override void VisitCustom(SqlNodeBase node)
        {
            if ( node is SqlDataFieldNode dataField )
                VisitDataField( dataField );
        }

        internal void VisitAssignmentRange(ReadOnlyArray<SqlValueAssignmentNode> assignments)
        {
            Assume.Equals( _nextAssignmentIndex, 0 );
            foreach ( var assignment in assignments )
            {
                this.Visit( assignment.Value );
                ++_nextAssignmentIndex;
            }
        }

        private void VisitDataField(SqlDataFieldNode node)
        {
            if ( Array.IndexOf( _joinedRecordSets, node.RecordSet ) == -1 )
                return;

            if ( _indexesOfComplexAssignments is null )
            {
                _indexesOfComplexAssignments = new List<int> { _nextAssignmentIndex };
                return;
            }

            if ( _indexesOfComplexAssignments[^1] != _nextAssignmentIndex )
                _indexesOfComplexAssignments.Add( _nextAssignmentIndex );
        }
    }

    private static string[] PrepareCommonTableExpressionIdentitySelection(ChangeTargetInfo targetInfo, SqlSelectNode[] selection)
    {
        Assume.ContainsAtLeast( selection, targetInfo.IdentityColumnNames.Length );
        var fieldNames = new string[targetInfo.IdentityColumnNames.Length];
        for ( var i = 0; i < targetInfo.IdentityColumnNames.Length; ++i )
        {
            var name = targetInfo.IdentityColumnNames[i];
            var fieldName = $"ID_{name}_{i}";
            fieldNames[i] = fieldName;
            selection[i] = targetInfo.Target.GetUnsafeField( name ).As( fieldName );
        }

        return fieldNames;
    }

    [Pure]
    private static SqlSingleDataSourceNode<SqlCommonTableExpressionRecordSetNode> CreateCommonTableExpressionForComplexDeleteOrUpdate(
        SqlDataSourceNode dataSource,
        Chain<SqlTraitNode> cteTraits,
        SqlSelectNode[] cteSelection)
    {
        var cte = dataSource.SetTraits( cteTraits ).Select( cteSelection ).ToCte( $"_{Guid.NewGuid():N}" );
        return cte.RecordSet.ToDataSource();
    }

    [Pure]
    private static SqlConditionNode CreatePrimaryKeyFilterFromCommonTableExpression(
        ChangeTargetInfo targetInfo,
        SqlCommonTableExpressionRecordSetNode cte,
        ReadOnlySpan<string> cteIdentityFieldNames)
    {
        var pkBaseColumnNode = targetInfo.BaseTarget.GetUnsafeField( targetInfo.IdentityColumnNames[0] );
        var pkCteColumnNode = cte.GetUnsafeField( cteIdentityFieldNames[0] );
        var pkFilter = pkBaseColumnNode == pkCteColumnNode;

        var i = 1;
        foreach ( var name in targetInfo.IdentityColumnNames.AsSpan( 1 ) )
        {
            pkBaseColumnNode = targetInfo.BaseTarget.GetUnsafeField( name );
            pkCteColumnNode = cte.GetUnsafeField( cteIdentityFieldNames[i++] );
            pkFilter = pkFilter.And( pkBaseColumnNode == pkCteColumnNode );
        }

        return pkFilter;
    }
}
