﻿using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
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
            Context.AddParameter( parameter.Name, parameter.Type );
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

    public virtual void VisitParameter(SqlParameterNode node)
    {
        Context.Sql.Append( '@' ).Append( node.Name );
        Context.AddParameter( node.Name, node.Type );
    }

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
        VisitFunctionArguments( node.Arguments.Span );
    }

    public abstract void VisitRecordsAffectedFunction(SqlRecordsAffectedFunctionExpressionNode node);

    public virtual void VisitCoalesceFunction(SqlCoalesceFunctionExpressionNode node)
    {
        if ( node.Arguments.Length == 1 )
            VisitChild( node.Arguments.Span[0] );
        else
            VisitSimpleFunction( "COALESCE", node );
    }

    public abstract void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node);
    public abstract void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node);
    public abstract void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node);
    public abstract void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node);
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
            Context.AddParameter( parameter.Name, parameter.Type );
    }

    public virtual void VisitTrue(SqlTrueNode node)
    {
        Context.Sql.Append( '1' ).AppendSpace().Append( '=' ).AppendSpace().Append( '1' );
    }

    public virtual void VisitFalse(SqlFalseNode node)
    {
        Context.Sql.Append( '1' ).AppendSpace().Append( '=' ).AppendSpace().Append( '0' );
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
        Context.Sql.Append( "CASE" ).AppendSpace().Append( "WHEN" ).AppendSpace();
        this.Visit( node.Condition );
        Context.Sql.AppendSpace().Append( "THEN" ).AppendSpace().Append( '1' );
        Context.Sql.AppendSpace().Append( "ELSE" ).AppendSpace().Append( '0' ).AppendSpace().Append( "END" );
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

        Context.Sql.Append( joinType ).AppendSpace().Append( "JOIN" ).AppendSpace();
        this.Visit( node.InnerRecordSet );

        if ( node.JoinType == SqlJoinType.Cross )
            return;

        Context.Sql.AppendSpace().Append( "ON" ).AppendSpace();
        this.Visit( node.OnExpression );
    }

    public abstract void VisitDataSource(SqlDataSourceNode node);

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
            Context.AddParameter( parameter.Name, parameter.Type );

        if ( Context.ChildDepth > 0 )
        {
            Context.AppendIndent();
            AppendMultilineSql( node.Sql );
            Context.AppendShortIndent();
        }
        else
            AppendMultilineSql( node.Sql );
    }

    public abstract void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node);
    public abstract void VisitCompoundQuery(SqlCompoundQueryExpressionNode node);

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
        VisitChild( node.Query );
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
        if ( node.Expressions.Length == 0 )
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
        if ( node.Ordering.Length == 0 )
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
        if ( node.CommonTableExpressions.Length == 0 )
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
        if ( node.Windows.Length == 0 )
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

        if ( node.Partitioning.Length > 0 )
        {
            Context.Sql.Append( "PARTITION" ).AppendSpace().Append( "BY" ).AppendSpace();
            foreach ( var partition in node.Partitioning )
            {
                VisitChild( partition );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }

        if ( node.Ordering.Length > 0 )
        {
            if ( node.Partitioning.Length > 0 )
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
            if ( node.Partitioning.Length > 0 || node.Ordering.Length > 0 )
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
            Context.AddParameter( parameter.Name, parameter.Type );

        AppendMultilineSql( node.Sql );
    }

    public abstract void VisitInsertInto(SqlInsertIntoNode node);
    public abstract void VisitUpdate(SqlUpdateNode node);

    public virtual void VisitValueAssignment(SqlValueAssignmentNode node)
    {
        AppendDelimitedName( node.DataField.Name );
        Context.Sql.AppendSpace().Append( '=' ).AppendSpace();
        VisitChild( node.Value );
    }

    public abstract void VisitDeleteFrom(SqlDeleteFromNode node);
    public abstract void VisitTruncate(SqlTruncateNode node);
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
        if ( node.Statements.Length == 0 )
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

    public abstract void AppendDelimitedRecordSetName(SqlRecordSetNode node);

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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void AppendDelimitedSchemaObjectName(SqlSchemaObjectName name)
    {
        AppendDelimitedSchemaObjectName( name.Schema, name.Object );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void AppendDelimitedRecordSetInfo(SqlRecordSetInfo info)
    {
        if ( info.IsTemporary )
            AppendDelimitedTemporaryObjectName( info.Name.Object );
        else
            AppendDelimitedSchemaObjectName( info.Name );
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

    public virtual void AppendDelimitedTemporaryObjectName(string name)
    {
        Context.Sql.Append( "TEMP" ).AppendDot();
        AppendDelimitedName( name );
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
                    if ( aggregationTrait.Expressions.Length > 0 )
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
                    if ( sortTrait.Ordering.Length > 0 )
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
                    if ( cteTrait.CommonTableExpressions.Length > 0 )
                    {
                        commonTableExpressions = commonTableExpressions.Extend( cteTrait.CommonTableExpressions );
                        containsRecursiveCommonTableExpression = containsRecursiveCommonTableExpression || cteTrait.ContainsRecursive;
                    }

                    break;
                }
                case SqlNodeType.WindowDefinitionTrait:
                {
                    var windowTrait = ReinterpretCast.To<SqlWindowDefinitionTraitNode>( trait );
                    if ( windowTrait.Windows.Length > 0 )
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
                    if ( sortTrait.Ordering.Length > 0 )
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
                    if ( cteTrait.CommonTableExpressions.Length > 0 )
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
                default:
                {
                    custom = custom.Extend( trait );
                    break;
                }
            }
        }

        return new SqlAggregateFunctionTraits( distinct, filter, window, custom );
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
        VisitFunctionArguments( node.Arguments.Span );
    }

    protected void VisitFunctionArguments(ReadOnlySpan<SqlExpressionNode> arguments)
    {
        Context.Sql.Append( '(' );

        if ( arguments.Length > 0 )
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
        Chain<ReadOnlyMemory<SqlCommonTableExpressionNode>> commonTableExpressions,
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

    protected void VisitOptionalAggregationRange(Chain<ReadOnlyMemory<SqlExpressionNode>> aggregations)
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

    protected void VisitOptionalWindowRange(Chain<ReadOnlyMemory<SqlWindowDefinitionNode>> windows)
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

    protected void VisitOptionalOrderingRange(Chain<ReadOnlyMemory<SqlOrderByNode>> ordering)
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

    protected void VisitUpdateAssignmentRange(ReadOnlySpan<SqlValueAssignmentNode> assignments)
    {
        Context.Sql.AppendSpace().Append( "SET" );

        if ( assignments.Length > 0 )
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
}
