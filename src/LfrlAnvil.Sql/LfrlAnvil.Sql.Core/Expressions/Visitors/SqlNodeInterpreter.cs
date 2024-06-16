// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

/// <summary>
/// Represents an object capable of recursive traversal over an SQL syntax tree and translating that tree into an SQL statement.
/// </summary>
/// <remarks>
/// SQL nodes that cannot be handled by an <see cref="SqlNodeInterpreter"/> instance may cause it to throw an exception of
/// <see cref="SqlNodeVisitorException"/> or <see cref="UnrecognizedSqlNodeException"/> type.
/// </remarks>
public abstract class SqlNodeInterpreter : ISqlNodeVisitor
{
    /// <summary>
    /// Specifies the beginning name delimiter symbol.
    /// </summary>
    public readonly char BeginNameDelimiter;

    /// <summary>
    /// Specifies the ending name delimiter symbol.
    /// </summary>
    public readonly char EndNameDelimiter;

    /// <summary>
    /// Creates a new <see cref="SqlNodeInterpreter"/> instance.
    /// </summary>
    /// <param name="context">Underlying <see cref="SqlNodeInterpreterContext"/> instance.</param>
    /// <param name="beginNameDelimiter">Specifies the beginning name delimiter symbol.</param>
    /// <param name="endNameDelimiter">Specifies the ending name delimiter symbol.</param>
    protected SqlNodeInterpreter(SqlNodeInterpreterContext context, char beginNameDelimiter, char endNameDelimiter)
    {
        Context = context;
        BeginNameDelimiter = beginNameDelimiter;
        EndNameDelimiter = endNameDelimiter;
        RecordSetNodeBehavior = null;
    }

    /// <summary>
    /// Underlying <see cref="SqlNodeInterpreterContext"/> instance.
    /// </summary>
    public SqlNodeInterpreterContext Context { get; }

    /// <summary>
    /// Specifies the current <see cref="RecordSetNodeBehaviorRule"/> instance.
    /// </summary>
    public RecordSetNodeBehaviorRule? RecordSetNodeBehavior { get; private set; }

    /// <inheritdoc />
    public virtual void VisitRawExpression(SqlRawExpressionNode node)
    {
        AppendMultilineSql( node.Sql );

        foreach ( var parameter in node.Parameters )
            AddContextParameter( parameter );
    }

    /// <inheritdoc />
    public virtual void VisitRawDataField(SqlRawDataFieldNode node)
    {
        TryAppendDataFieldRecordSetNameBasedOnNodeBehavior( node );
        AppendDelimitedName( node.Name );
    }

    /// <inheritdoc />
    public virtual void VisitNull(SqlNullNode node)
    {
        Context.Sql.Append( "NULL" );
    }

    /// <inheritdoc />
    public abstract void VisitLiteral(SqlLiteralNode node);

    /// <inheritdoc />
    public abstract void VisitParameter(SqlParameterNode node);

    /// <inheritdoc />
    public virtual void VisitColumn(SqlColumnNode node)
    {
        TryAppendDataFieldRecordSetNameBasedOnNodeBehavior( node );
        AppendDelimitedName( node.Name );
    }

    /// <inheritdoc />
    public virtual void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        TryAppendDataFieldRecordSetNameBasedOnNodeBehavior( node );
        AppendDelimitedName( node.Name );
    }

    /// <inheritdoc />
    public virtual void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        TryAppendDataFieldRecordSetNameBasedOnNodeBehavior( node );
        AppendDelimitedName( node.Name );
    }

    /// <inheritdoc />
    public virtual void VisitViewDataField(SqlViewDataFieldNode node)
    {
        TryAppendDataFieldRecordSetNameBasedOnNodeBehavior( node );
        AppendDelimitedName( node.Name );
    }

    /// <inheritdoc />
    public virtual void VisitNegate(SqlNegateExpressionNode node)
    {
        VisitPrefixUnaryOperator( node.Value, symbol: "-" );
    }

    /// <inheritdoc />
    public virtual void VisitAdd(SqlAddExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "+", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitConcat(SqlConcatExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "||", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitSubtract(SqlSubtractExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "-", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitMultiply(SqlMultiplyExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "*", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitDivide(SqlDivideExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "/", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitModulo(SqlModuloExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "%", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitBitwiseNot(SqlBitwiseNotExpressionNode node)
    {
        VisitPrefixUnaryOperator( node.Value, symbol: "~" );
    }

    /// <inheritdoc />
    public virtual void VisitBitwiseAnd(SqlBitwiseAndExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "&", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitBitwiseOr(SqlBitwiseOrExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "|", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitBitwiseXor(SqlBitwiseXorExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "^", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitBitwiseLeftShift(SqlBitwiseLeftShiftExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "<<", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitBitwiseRightShift(SqlBitwiseRightShiftExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: ">>", node.Right );
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual void VisitNamedFunction(SqlNamedFunctionExpressionNode node)
    {
        AppendDelimitedSchemaObjectName( node.Name );
        VisitFunctionArguments( node.Arguments );
    }

    /// <inheritdoc />
    public virtual void VisitCoalesceFunction(SqlCoalesceFunctionExpressionNode node)
    {
        if ( node.Arguments.Count == 1 )
            VisitChild( node.Arguments[0] );
        else
            VisitSimpleFunction( "COALESCE", node );
    }

    /// <inheritdoc />
    public abstract void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitCurrentUtcDateTimeFunction(SqlCurrentUtcDateTimeFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitExtractDateFunction(SqlExtractDateFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitExtractTimeOfDayFunction(SqlExtractTimeOfDayFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitExtractDayFunction(SqlExtractDayFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitExtractTemporalUnitFunction(SqlExtractTemporalUnitFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitTemporalAddFunction(SqlTemporalAddFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitTemporalDiffFunction(SqlTemporalDiffFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitLengthFunction(SqlLengthFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitByteLengthFunction(SqlByteLengthFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitToLowerFunction(SqlToLowerFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitToUpperFunction(SqlToUpperFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitTrimStartFunction(SqlTrimStartFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitTrimFunction(SqlTrimFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitReverseFunction(SqlReverseFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitSignFunction(SqlSignFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitAbsFunction(SqlAbsFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitCeilingFunction(SqlCeilingFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitFloorFunction(SqlFloorFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitRoundFunction(SqlRoundFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitPowerFunction(SqlPowerFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitMinFunction(SqlMinFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitMaxFunction(SqlMaxFunctionExpressionNode node);

    /// <inheritdoc />
    /// <inheritdoc />
    public virtual void VisitCustomFunction(SqlFunctionExpressionNode node)
    {
        throw new UnrecognizedSqlNodeException( this, node );
    }

    /// <inheritdoc />
    public abstract void VisitNamedAggregateFunction(SqlNamedAggregateFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitRowNumberWindowFunction(SqlRowNumberWindowFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitRankWindowFunction(SqlRankWindowFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitDenseRankWindowFunction(SqlDenseRankWindowFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitCumulativeDistributionWindowFunction(SqlCumulativeDistributionWindowFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitNTileWindowFunction(SqlNTileWindowFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitLagWindowFunction(SqlLagWindowFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitLeadWindowFunction(SqlLeadWindowFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitFirstValueWindowFunction(SqlFirstValueWindowFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitLastValueWindowFunction(SqlLastValueWindowFunctionExpressionNode node);

    /// <inheritdoc />
    public abstract void VisitNthValueWindowFunction(SqlNthValueWindowFunctionExpressionNode node);

    /// <inheritdoc />
    /// <inheritdoc />
    public virtual void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        throw new UnrecognizedSqlNodeException( this, node );
    }

    /// <inheritdoc />
    public virtual void VisitRawCondition(SqlRawConditionNode node)
    {
        AppendMultilineSql( node.Sql );

        foreach ( var parameter in node.Parameters )
            AddContextParameter( parameter );
    }

    /// <inheritdoc />
    public virtual void VisitTrue(SqlTrueNode node)
    {
        Context.Sql.Append( "TRUE" );
    }

    /// <inheritdoc />
    public virtual void VisitFalse(SqlFalseNode node)
    {
        Context.Sql.Append( "FALSE" );
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual void VisitGreaterThan(SqlGreaterThanConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: ">", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitLessThan(SqlLessThanConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "<", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitGreaterThanOrEqualTo(SqlGreaterThanOrEqualToConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: ">=", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitLessThanOrEqualTo(SqlLessThanOrEqualToConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "<=", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitAnd(SqlAndConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "AND", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitOr(SqlOrConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "OR", node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitConditionValue(SqlConditionValueNode node)
    {
        this.Visit( node.Condition );
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual void VisitExists(SqlExistsConditionNode node)
    {
        if ( node.IsNegated )
            Context.Sql.Append( "NOT" ).AppendSpace();

        Context.Sql.Append( "EXISTS" ).AppendSpace();
        VisitChild( node.Query );
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual void VisitInQuery(SqlInQueryConditionNode node)
    {
        VisitChild( node.Value );

        Context.Sql.AppendSpace();
        if ( node.IsNegated )
            Context.Sql.Append( "NOT" ).AppendSpace();

        Context.Sql.Append( "IN" ).AppendSpace();
        VisitChild( node.Query );
    }

    /// <inheritdoc />
    public virtual void VisitRawRecordSet(SqlRawRecordSetNode node)
    {
        if ( node.IsInfoRaw )
            Context.Sql.Append( node.Info.Name.Object );
        else
            AppendDelimitedRecordSetInfo( node.Info );

        AppendDelimitedAlias( node.Alias );
    }

    /// <inheritdoc />
    public virtual void VisitNamedFunctionRecordSet(SqlNamedFunctionRecordSetNode node)
    {
        VisitNamedFunction( node.Function );
        AppendDelimitedAlias( node.Alias );
    }

    /// <inheritdoc />
    public virtual void VisitTable(SqlTableNode node)
    {
        AppendDelimitedSchemaObjectName( node.Table.Info.Name );
        AppendDelimitedAlias( node.Alias );
    }

    /// <inheritdoc />
    public virtual void VisitTableBuilder(SqlTableBuilderNode node)
    {
        AppendDelimitedSchemaObjectName( node.Table.Info.Name );
        AppendDelimitedAlias( node.Alias );
    }

    /// <inheritdoc />
    public virtual void VisitView(SqlViewNode node)
    {
        AppendDelimitedSchemaObjectName( node.View.Info.Name );
        AppendDelimitedAlias( node.Alias );
    }

    /// <inheritdoc />
    public virtual void VisitViewBuilder(SqlViewBuilderNode node)
    {
        AppendDelimitedSchemaObjectName( node.View.Info.Name );
        AppendDelimitedAlias( node.Alias );
    }

    /// <inheritdoc />
    public virtual void VisitQueryRecordSet(SqlQueryRecordSetNode node)
    {
        VisitChild( node.Query );
        AppendDelimitedAlias( node.Alias );
    }

    /// <inheritdoc />
    public virtual void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node)
    {
        AppendDelimitedName( node.CommonTableExpression.Name );
        AppendDelimitedAlias( node.Alias );
    }

    /// <inheritdoc />
    public virtual void VisitNewTable(SqlNewTableNode node)
    {
        AppendDelimitedRecordSetInfo( node.CreationNode.Info );
        AppendDelimitedAlias( node.Alias );
    }

    /// <inheritdoc />
    public virtual void VisitNewView(SqlNewViewNode node)
    {
        AppendDelimitedRecordSetInfo( node.CreationNode.Info );
        AppendDelimitedAlias( node.Alias );
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual void VisitSelectCompoundField(SqlSelectCompoundFieldNode node)
    {
        AppendDelimitedName( node.Name );
    }

    /// <inheritdoc />
    public virtual void VisitSelectRecordSet(SqlSelectRecordSetNode node)
    {
        AppendDelimitedRecordSetName( node.RecordSet );
        Context.Sql.AppendDot().Append( '*' );
    }

    /// <inheritdoc />
    public virtual void VisitSelectAll(SqlSelectAllNode node)
    {
        Context.Sql.Append( '*' );
    }

    /// <inheritdoc />
    public virtual void VisitSelectExpression(SqlSelectExpressionNode node)
    {
        if ( node.Selection.NodeType == SqlNodeType.SelectField )
            AppendDelimitedName( ReinterpretCast.To<SqlSelectFieldNode>( node.Selection ).FieldName );
        else
            this.Visit( node.Selection );
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual void VisitDistinctTrait(SqlDistinctTraitNode node)
    {
        Context.Sql.Append( "DISTINCT" );
    }

    /// <inheritdoc />
    public virtual void VisitFilterTrait(SqlFilterTraitNode node)
    {
        Context.Sql.Append( "WHERE" ).AppendSpace();
        this.Visit( node.Filter );
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node)
    {
        Context.Sql.Append( "HAVING" ).AppendSpace();
        this.Visit( node.Filter );
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public abstract void VisitLimitTrait(SqlLimitTraitNode node);

    /// <inheritdoc />
    public abstract void VisitOffsetTrait(SqlOffsetTraitNode node);

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual void VisitWindowTrait(SqlWindowTraitNode node)
    {
        Context.Sql.Append( "OVER" ).AppendSpace();
        AppendDelimitedName( node.Definition.Name );
    }

    /// <inheritdoc />
    public virtual void VisitOrderBy(SqlOrderByNode node)
    {
        VisitChild( node.Expression );
        Context.Sql.AppendSpace().Append( node.Ordering.Name );
    }

    /// <inheritdoc />
    public virtual void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        AppendDelimitedName( node.Name );
        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace();
        VisitChild( node.Query );
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public abstract void VisitTypeCast(SqlTypeCastExpressionNode node);

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual void VisitRawStatement(SqlRawStatementNode node)
    {
        foreach ( var parameter in node.Parameters )
            AddContextParameter( parameter );

        AppendMultilineSql( node.Sql );
    }

    /// <inheritdoc />
    public abstract void VisitInsertInto(SqlInsertIntoNode node);

    /// <inheritdoc />
    public abstract void VisitUpdate(SqlUpdateNode node);

    /// <inheritdoc />
    public abstract void VisitUpsert(SqlUpsertNode node);

    /// <inheritdoc />
    public virtual void VisitValueAssignment(SqlValueAssignmentNode node)
    {
        AppendDelimitedName( node.DataField.Name );
        Context.Sql.AppendSpace().Append( '=' ).AppendSpace();
        VisitChild( node.Value );
    }

    /// <inheritdoc />
    public abstract void VisitDeleteFrom(SqlDeleteFromNode node);

    /// <inheritdoc />
    public virtual void VisitTruncate(SqlTruncateNode node)
    {
        Context.Sql.Append( "TRUNCATE" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedRecordSetName( node.Table );
    }

    /// <inheritdoc />
    public abstract void VisitColumnDefinition(SqlColumnDefinitionNode node);

    /// <inheritdoc />
    public abstract void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node);

    /// <inheritdoc />
    public abstract void VisitForeignKeyDefinition(SqlForeignKeyDefinitionNode node);

    /// <inheritdoc />
    public abstract void VisitCheckDefinition(SqlCheckDefinitionNode node);

    /// <inheritdoc />
    public abstract void VisitCreateTable(SqlCreateTableNode node);

    /// <inheritdoc />
    public abstract void VisitCreateView(SqlCreateViewNode node);

    /// <inheritdoc />
    public abstract void VisitCreateIndex(SqlCreateIndexNode node);

    /// <inheritdoc />
    public abstract void VisitRenameTable(SqlRenameTableNode node);

    /// <inheritdoc />
    public abstract void VisitRenameColumn(SqlRenameColumnNode node);

    /// <inheritdoc />
    public abstract void VisitAddColumn(SqlAddColumnNode node);

    /// <inheritdoc />
    public abstract void VisitDropColumn(SqlDropColumnNode node);

    /// <inheritdoc />
    public abstract void VisitDropTable(SqlDropTableNode node);

    /// <inheritdoc />
    public abstract void VisitDropView(SqlDropViewNode node);

    /// <inheritdoc />
    public abstract void VisitDropIndex(SqlDropIndexNode node);

    /// <inheritdoc />
    /// <inheritdoc />
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

    /// <inheritdoc />
    public abstract void VisitBeginTransaction(SqlBeginTransactionNode node);

    /// <inheritdoc />
    public virtual void VisitCommitTransaction(SqlCommitTransactionNode node)
    {
        Context.Sql.Append( "COMMIT" );
    }

    /// <inheritdoc />
    public virtual void VisitRollbackTransaction(SqlRollbackTransactionNode node)
    {
        Context.Sql.Append( "ROLLBACK" );
    }

    /// <inheritdoc />
    public virtual void VisitCustom(SqlNodeBase node)
    {
        throw new UnrecognizedSqlNodeException( this, node );
    }

    /// <summary>
    /// Appends delimited name of the provided record set to <see cref="Context"/>.
    /// </summary>
    /// <param name="node">Record set whose name should be appended.</param>
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

    /// <summary>
    /// Appends delimited name to <see cref="Context"/>.
    /// </summary>
    /// <param name="name">Name to append.</param>
    public void AppendDelimitedName(string name)
    {
        Context.Sql.Append( BeginNameDelimiter ).Append( name ).Append( EndNameDelimiter );
    }

    /// <summary>
    /// Appends delimited alias to <see cref="Context"/>.
    /// </summary>
    /// <param name="alias">Alias to append.</param>
    public void AppendDelimitedAlias(string? alias)
    {
        if ( alias is null )
            return;

        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace();
        AppendDelimitedName( alias );
    }

    /// <summary>
    /// Appends delimited name of a temporary SQL object to <see cref="Context"/>.
    /// </summary>
    /// <param name="name">Name to append.</param>
    public virtual void AppendDelimitedTemporaryObjectName(string name)
    {
        Context.Sql.Append( "TEMP" ).AppendDot();
        AppendDelimitedName( name );
    }

    /// <summary>
    /// Appends delimited schema object's name to <see cref="Context"/>.
    /// </summary>
    /// <param name="name">Name to append.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void AppendDelimitedSchemaObjectName(SqlSchemaObjectName name)
    {
        AppendDelimitedSchemaObjectName( name.Schema, name.Object );
    }

    /// <summary>
    /// Appends delimited schema object's name to <see cref="Context"/>.
    /// </summary>
    /// <param name="schemaName">Schema name to append.</param>
    /// <param name="objName">Object name to append.</param>
    public virtual void AppendDelimitedSchemaObjectName(string schemaName, string objName)
    {
        if ( schemaName.Length > 0 )
        {
            AppendDelimitedName( schemaName );
            Context.Sql.AppendDot();
        }

        AppendDelimitedName( objName );
    }

    /// <summary>
    /// Appends delimited <see cref="SqlRecordSetInfo"/> to <see cref="Context"/>.
    /// </summary>
    /// <param name="info"><see cref="SqlRecordSetInfo"/> to append.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void AppendDelimitedRecordSetInfo(SqlRecordSetInfo info)
    {
        if ( info.IsTemporary )
            AppendDelimitedTemporaryObjectName( info.Name.Object );
        else
            AppendDelimitedSchemaObjectName( info.Name );
    }

    /// <summary>
    /// Appends delimited data field's name to <see cref="Context"/>.
    /// </summary>
    /// <param name="recordSet">Record set name to append.</param>
    /// <param name="name">Data field name to append.</param>
    public void AppendDelimitedDataFieldName(SqlRecordSetInfo recordSet, string name)
    {
        AppendDelimitedRecordSetInfo( recordSet );
        Context.Sql.AppendDot();
        AppendDelimitedName( name );
    }

    /// <summary>
    /// Appends raw, potentially multiline, <paramref name="sql"/> to <see cref="Context"/>
    /// while respecting its current <see cref="SqlNodeInterpreterContext.Indent"/>.
    /// </summary>
    /// <param name="sql">Raw sql to append.</param>
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

    /// <summary>
    /// Visits an <see cref="SqlNodeBase"/> as a child node.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    public void VisitChild(SqlNodeBase node)
    {
        if ( DoesChildNodeRequireParentheses( node ) )
            VisitChildWrappedInParentheses( node );
        else
            this.Visit( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceTraits"/> instance.
    /// </summary>
    /// <param name="traits">Collection of traits to parse.</param>
    /// <returns>New <see cref="SqlDataSourceTraits"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceTraits ExtractDataSourceTraits(Chain<SqlTraitNode> traits)
    {
        return ExtractDataSourceTraits( default, traits );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceTraits"/> instance.
    /// </summary>
    /// <param name="base"><see cref="SqlDataSourceTraits"/> instance to extend.</param>
    /// <param name="traits">Collection of traits to parse.</param>
    /// <returns>New <see cref="SqlDataSourceTraits"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="SqlQueryTraits"/> instance.
    /// </summary>
    /// <param name="traits">Collection of traits to parse.</param>
    /// <returns>New <see cref="SqlQueryTraits"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryTraits ExtractQueryTraits(Chain<SqlTraitNode> traits)
    {
        return ExtractQueryTraits( default, traits );
    }

    /// <summary>
    /// Creates a new <see cref="SqlQueryTraits"/> instance.
    /// </summary>
    /// <param name="base"><see cref="SqlQueryTraits"/> instance to extend.</param>
    /// <param name="traits">Collection of traits to parse.</param>
    /// <returns>New <see cref="SqlQueryTraits"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="SqlAggregateFunctionTraits"/> instance.
    /// </summary>
    /// <param name="traits">Collection of traits to parse.</param>
    /// <returns>New <see cref="SqlAggregateFunctionTraits"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAggregateFunctionTraits ExtractAggregateFunctionTraits(Chain<SqlTraitNode> traits)
    {
        return ExtractAggregateFunctionTraits( default, traits );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAggregateFunctionTraits"/> instance.
    /// </summary>
    /// <param name="base"><see cref="SqlAggregateFunctionTraits"/> instance to extend.</param>
    /// <param name="traits">Collection of traits to parse.</param>
    /// <returns>New <see cref="SqlAggregateFunctionTraits"/> instance.</returns>
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

    /// <summary>
    /// Filters a collection of traits with the provided <paramref name="predicate"/>.
    /// </summary>
    /// <param name="traits">Source collection of traits.</param>
    /// <param name="predicate">Filtering predicate. Traits that cause this predicate to return <b>false</b> will be filtered out.</param>
    /// <returns>New collection of traits.</returns>
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

    /// <summary>
    /// Updates the <see cref="RecordSetNodeBehavior"/> to ignore all occurrences of the provided record set
    /// and returns a new <see cref="TemporaryRecordSetNodeBehaviorRuleChange"/> instance.
    /// </summary>
    /// <param name="node">Record set to ignore.</param>
    /// <returns>New <see cref="TemporaryRecordSetNodeBehaviorRuleChange"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TemporaryRecordSetNodeBehaviorRuleChange TempIgnoreRecordSet(SqlRecordSetNode node)
    {
        return new TemporaryRecordSetNodeBehaviorRuleChange( this, new RecordSetNodeBehaviorRule( node, null ) );
    }

    /// <summary>
    /// Updates the <see cref="RecordSetNodeBehavior"/> to replace all occurrences of the provided record set with another
    /// and returns a new <see cref="TemporaryRecordSetNodeBehaviorRuleChange"/> instance.
    /// </summary>
    /// <param name="node">Record set to replace.</param>
    /// <param name="replacementNode">Replacement record set.</param>
    /// <returns>New <see cref="TemporaryRecordSetNodeBehaviorRuleChange"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TemporaryRecordSetNodeBehaviorRuleChange TempReplaceRecordSet(SqlRecordSetNode node, SqlRecordSetNode replacementNode)
    {
        return new TemporaryRecordSetNodeBehaviorRuleChange( this, new RecordSetNodeBehaviorRule( node, replacementNode ) );
    }

    /// <summary>
    /// Updates the <see cref="RecordSetNodeBehavior"/> to ignore all occurrences of all record sets
    /// and returns a new <see cref="TemporaryRecordSetNodeBehaviorRuleChange"/> instance.
    /// </summary>
    /// <returns>New <see cref="TemporaryRecordSetNodeBehaviorRuleChange"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TemporaryRecordSetNodeBehaviorRuleChange TempIgnoreAllRecordSets()
    {
        return new TemporaryRecordSetNodeBehaviorRuleChange( this, new RecordSetNodeBehaviorRule( null, null ) );
    }

    /// <summary>
    /// Updates the <see cref="RecordSetNodeBehavior"/> to include all occurrences of all record sets as they are
    /// and returns a new <see cref="TemporaryRecordSetNodeBehaviorRuleChange"/> instance.
    /// </summary>
    /// <returns>New <see cref="TemporaryRecordSetNodeBehaviorRuleChange"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TemporaryRecordSetNodeBehaviorRuleChange TempIncludeAllRecordSets()
    {
        return new TemporaryRecordSetNodeBehaviorRuleChange( this, null );
    }

    /// <summary>
    /// Represents a custom rule for handling specific <see cref="SqlRecordSetNode"/> instances.
    /// </summary>
    public readonly struct RecordSetNodeBehaviorRule
    {
        /// <summary>
        /// <see cref="SqlRecordSetNode"/> to replace. When this property is null, then all record sets will be replaced.
        /// </summary>
        public readonly SqlRecordSetNode? Node;

        /// <summary>
        /// Replacement <see cref="SqlRecordSetNode"/>. Null replacement means that replaced record sets will be ignored.
        /// </summary>
        public readonly SqlRecordSetNode? ReplacementNode;

        internal RecordSetNodeBehaviorRule(SqlRecordSetNode? node, SqlRecordSetNode? replacementNode)
        {
            Node = node;
            ReplacementNode = replacementNode;
        }

        /// <summary>
        /// Returns the <see cref="ReplacementNode"/> when the provided <paramref name="recordSet"/>
        /// and the current <see cref="Node"/> match.
        /// </summary>
        /// <param name="recordSet">Record set to check.</param>
        /// <returns>
        /// The <see cref="ReplacementNode"/> when the provided <paramref name="recordSet"/> and the current <see cref="Node"/> match,
        /// otherwise the provided <paramref name="recordSet"/>.
        /// </returns>
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public SqlRecordSetNode? GetRecordSet(SqlRecordSetNode recordSet)
        {
            return Node is null || ReferenceEquals( Node, recordSet ) ? ReplacementNode : recordSet;
        }
    }

    /// <summary>
    /// Represents a temporary disposable <see cref="RecordSetNodeBehaviorRule"/> change.
    /// </summary>
    public readonly struct TemporaryRecordSetNodeBehaviorRuleChange : IDisposable
    {
        private readonly RecordSetNodeBehaviorRule? _previous;
        private readonly SqlNodeInterpreter _interpreter;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal TemporaryRecordSetNodeBehaviorRuleChange(SqlNodeInterpreter interpreter, RecordSetNodeBehaviorRule? rule)
        {
            _interpreter = interpreter;
            _previous = interpreter.RecordSetNodeBehavior;
            interpreter.RecordSetNodeBehavior = rule;
        }

        /// <inheritdoc />
        /// <remarks>Brings back the previous <see cref="SqlNodeInterpreter.RecordSetNodeBehavior"/>.</remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            _interpreter.RecordSetNodeBehavior = _previous;
        }
    }

    /// <summary>
    /// Registers an SQL parameter in the <see cref="Context"/>.
    /// </summary>
    /// <param name="node">SQL parameter node to register.</param>
    protected virtual void AddContextParameter(SqlParameterNode node)
    {
        Context.AddParameter( node.Name, node.Type, node.Index );
    }

    /// <summary>
    /// Visits a prefix unary operator SQL node.
    /// </summary>
    /// <param name="value">Operand node.</param>
    /// <param name="symbol">Operator's symbol.</param>
    protected void VisitPrefixUnaryOperator(SqlNodeBase value, string symbol)
    {
        Context.Sql.Append( symbol );
        VisitChildWrappedInParentheses( value );
    }

    /// <summary>
    /// Visits an infix binary operator SQL node.
    /// </summary>
    /// <param name="left">Left operand node.</param>
    /// <param name="symbol">Operator's symbol.</param>
    /// <param name="right">Right operand node.</param>
    protected void VisitInfixBinaryOperator(SqlNodeBase left, string symbol, SqlNodeBase right)
    {
        VisitChild( left );
        Context.Sql.AppendSpace().Append( symbol ).AppendSpace();
        VisitChild( right );
    }

    /// <summary>
    /// Visits a child SQL node wrapped in parentheses.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    protected void VisitChildWrappedInParentheses(SqlNodeBase node)
    {
        Context.Sql.Append( '(' );

        using ( Context.TempChildDepthIncrease() )
        using ( Context.TempIndentIncrease() )
            this.Visit( node );

        Context.Sql.Append( ')' );
    }

    /// <summary>
    /// Attempts to append provided data field's record set's name to <see cref="Context"/>,
    /// based on the current <see cref="RecordSetNodeBehavior"/> rule.
    /// </summary>
    /// <param name="node">SQL data field node.</param>
    protected void TryAppendDataFieldRecordSetNameBasedOnNodeBehavior(SqlDataFieldNode node)
    {
        var recordSet = node.RecordSet;
        if ( RecordSetNodeBehavior is not null )
            recordSet = RecordSetNodeBehavior.Value.GetRecordSet( recordSet );

        if ( recordSet is null )
            return;

        AppendDelimitedRecordSetName( recordSet );
        Context.Sql.AppendDot();
    }

    /// <summary>
    /// Visits a simple SQL function node.
    /// </summary>
    /// <param name="functionName">Name of the function.</param>
    /// <param name="node">SQL function expression node.</param>
    protected void VisitSimpleFunction(string functionName, SqlFunctionExpressionNode node)
    {
        Context.Sql.Append( functionName );
        VisitFunctionArguments( node.Arguments );
    }

    /// <summary>
    /// Sequentially visits all arguments of a simple SQL function node.
    /// </summary>
    /// <param name="arguments">Collection of arguments to visit.</param>
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

    /// <summary>
    /// Appends an SQL join statement to <see cref="Context"/>.
    /// </summary>
    /// <param name="joinType">Join operation type.</param>
    /// <param name="node">SQL data source join node.</param>
    protected void AppendJoin(string joinType, SqlDataSourceJoinOnNode node)
    {
        Context.Sql.Append( joinType ).AppendSpace().Append( "JOIN" ).AppendSpace();
        this.Visit( node.InnerRecordSet );

        if ( node.JoinType == SqlJoinType.Cross )
            return;

        Context.Sql.AppendSpace().Append( "ON" ).AppendSpace();
        this.Visit( node.OnExpression );
    }

    /// <summary>
    /// Visits <see cref="SqlQueryExpressionNode.Selection"/> of the provided <paramref name="node"/>, including any relevant traits.
    /// </summary>
    /// <param name="node">SQL data source query expression node.</param>
    /// <param name="traits">Collection of traits.</param>
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

    /// <summary>
    /// Visits <see cref="SqlCompoundQueryExpressionNode.FirstQuery"/> and
    /// <see cref="SqlCompoundQueryExpressionNode.FollowingQueries"/> of the provided <paramref name="node"/>,
    /// including any relevant traits.
    /// </summary>
    /// <param name="node">SQL compound query expression node.</param>
    /// <param name="traits">Collection of traits.</param>
    protected virtual void VisitCompoundQueryComponents(SqlCompoundQueryExpressionNode node, in SqlQueryTraits traits)
    {
        this.Visit( node.FirstQuery );
        foreach ( var component in node.FollowingQueries )
        {
            Context.AppendIndent();
            VisitCompoundQueryComponent( component );
        }
    }

    /// <summary>
    /// Visits a part of an insert into SQL statement that includes the record set and its data fields.
    /// </summary>
    /// <param name="node">Source node.</param>
    protected void VisitInsertIntoFields(SqlInsertIntoNode node)
    {
        VisitInsertIntoFields( node.RecordSet, node.DataFields );
    }

    /// <summary>
    /// Visits a part of an insert into SQL statement that includes the record set and its data fields.
    /// </summary>
    /// <param name="recordSet">Target table.</param>
    /// <param name="dataFields">Collection of data fields that this insertion refers to.</param>
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

    /// <summary>
    /// Visits all relevant SQL data source traits that should be at the beginning of an SQL statement.
    /// </summary>
    /// <param name="traits">Collection of traits.</param>
    protected virtual void VisitDataSourceBeforeTraits(in SqlDataSourceTraits traits)
    {
        VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions, traits.ContainsRecursiveCommonTableExpression );
    }

    /// <summary>
    /// Visits all relevant SQL data source traits that should be at the end of an SQL statement.
    /// </summary>
    /// <param name="traits">Collection of traits.</param>
    protected virtual void VisitDataSourceAfterTraits(in SqlDataSourceTraits traits)
    {
        VisitOptionalFilterCondition( traits.Filter );
        VisitOptionalAggregationRange( traits.Aggregations );
        VisitOptionalAggregationFilterCondition( traits.AggregationFilter );
        VisitOptionalWindowRange( traits.Windows );
        VisitOptionalOrderingRange( traits.Ordering );
    }

    /// <summary>
    /// Visits all relevant SQL query traits that should be at the beginning of an SQL statement.
    /// </summary>
    /// <param name="traits">Collection of traits.</param>
    protected virtual void VisitQueryBeforeTraits(in SqlQueryTraits traits)
    {
        VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions, traits.ContainsRecursiveCommonTableExpression );
    }

    /// <summary>
    /// Visits all relevant SQL query traits that should be at the end of an SQL statement.
    /// </summary>
    /// <param name="traits">Collection of traits.</param>
    protected virtual void VisitQueryAfterTraits(in SqlQueryTraits traits)
    {
        VisitOptionalOrderingRange( traits.Ordering );
    }

    /// <summary>
    /// Visits an <see cref="SqlWindowFrameNode"/> with <see cref="SqlWindowFrameType.Rows"/> type.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    protected virtual void VisitRowsWindowFrame(SqlWindowFrameNode node)
    {
        Context.Sql.Append( "ROWS" ).AppendSpace().Append( "BETWEEN" ).AppendSpace();
        AppendWindowFrameBoundary( node.Start );
        Context.Sql.AppendSpace().Append( "AND" ).AppendSpace();
        AppendWindowFrameBoundary( node.End );
    }

    /// <summary>
    /// Visits an <see cref="SqlWindowFrameNode"/> with <see cref="SqlWindowFrameType.Range"/> type.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    protected virtual void VisitRangeWindowFrame(SqlWindowFrameNode node)
    {
        Context.Sql.Append( "RANGE" ).AppendSpace().Append( "BETWEEN" ).AppendSpace();
        AppendWindowFrameBoundary( node.Start );
        Context.Sql.AppendSpace().Append( "AND" ).AppendSpace();
        AppendWindowFrameBoundary( node.End );
    }

    /// <summary>
    /// Appends an SQL window frame boundary to <see cref="Context"/>.
    /// </summary>
    /// <param name="boundary">Boundary to append.</param>
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

    /// <summary>
    /// Visits an <see cref="SqlWindowFrameNode"/> with <see cref="SqlWindowFrameType.Custom"/> type.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    /// <exception cref="UnrecognizedSqlNodeException">Custom window frames are not supported by default.</exception>
    protected virtual void VisitCustomWindowFrame(SqlWindowFrameNode node)
    {
        throw new UnrecognizedSqlNodeException( this, node );
    }

    /// <summary>
    /// Visits an optional collection of <see cref="SqlCommonTableExpressionNode"/> instances.
    /// </summary>
    /// <param name="commonTableExpressions">Collection of nodes to visit.</param>
    /// <param name="addRecursiveKeyword">Specifies whether or not any of the common table expressions to visit is recursive.</param>
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

    /// <summary>
    /// Visits an optional <see cref="SqlDistinctTraitNode"/>.
    /// </summary>
    /// <param name="distinct">Node to visit.</param>
    protected void VisitOptionalDistinctMarker(SqlDistinctTraitNode? distinct)
    {
        if ( distinct is null )
            return;

        Context.Sql.AppendSpace();
        VisitDistinctTrait( distinct );
    }

    /// <summary>
    /// Visits an optional <see cref="SqlConditionNode"/> filter.
    /// </summary>
    /// <param name="filter">Node to visit.</param>
    protected void VisitOptionalFilterCondition(SqlConditionNode? filter)
    {
        if ( filter is null )
            return;

        Context.AppendIndent().Append( "WHERE" ).AppendSpace();
        this.Visit( filter );
    }

    /// <summary>
    /// Visits an optional collection of aggregating <see cref="SqlExpressionNode"/> instances.
    /// </summary>
    /// <param name="aggregations">Collection of nodes to visit.</param>
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

    /// <summary>
    /// Visits an optional <see cref="SqlConditionNode"/> aggregation filter.
    /// </summary>
    /// <param name="filter">Node to visit.</param>
    protected void VisitOptionalAggregationFilterCondition(SqlConditionNode? filter)
    {
        if ( filter is null )
            return;

        Context.AppendIndent().Append( "HAVING" ).AppendSpace();
        this.Visit( filter );
    }

    /// <summary>
    /// Visits an optional collection of <see cref="SqlWindowDefinitionNode"/> instances.
    /// </summary>
    /// <param name="windows">Collection of nodes to visit.</param>
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

    /// <summary>
    /// Visits an optional collection of ordering <see cref="SqlOrderByNode"/> instances.
    /// </summary>
    /// <param name="ordering">Collection of nodes to visit.</param>
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

    /// <summary>
    /// Visits a collection of <see cref="SqlValueAssignmentNode"/> instances.
    /// </summary>
    /// <param name="assignments">Collection of nodes to visit.</param>
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

    /// <summary>
    /// Specifies whether or not the provided <paramref name="node"/> should be interpreted as a child node.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <returns><b>true</b> when node should be interpreted as a child node, otherwise <b>false</b>.</returns>
    /// <remarks>
    /// See <see cref="VisitChild(SqlNodeBase)"/> and <see cref="VisitChildWrappedInParentheses(SqlNodeBase)"/> for more information.
    /// </remarks>
    [Pure]
    protected abstract bool DoesChildNodeRequireParentheses(SqlNodeBase node);

    /// <summary>
    /// Visits components of an <see cref="SqlCreateTableNode"/>.
    /// </summary>
    /// <param name="node">Node to visit.</param>
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

    /// <summary>
    /// Creates a complex <see cref="SqlDeleteFromNode"/> or <see cref="SqlUpdateNode"/> filter expression,
    /// that uses sub-queries for filtering records that should be modified, for a data source that does not represent a valid SQL
    /// statement on its own.
    /// </summary>
    /// <param name="targetInfo"><see cref="ChangeTargetInfo"/> instance associated with the operation.</param>
    /// <param name="dataSource">
    /// <see cref="SqlDataSourceNode"/> instance attached to <see cref="SqlDeleteFromNode"/> or <see cref="SqlUpdateNode"/>.
    /// </param>
    /// <returns>New <see cref="SqlFilterTraitNode"/> instance.</returns>
    [Pure]
    protected static SqlFilterTraitNode CreateComplexDeleteOrUpdateFilter(ChangeTargetInfo targetInfo, SqlDataSourceNode dataSource)
    {
        var traits = RemoveCommonTableExpressionTraits( dataSource.Traits );
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

    /// <summary>
    /// Creates a simplified version of an <see cref="SqlUpdateNode"/> that does not represent a valid SQL statement on its own,
    /// by using a common table expression that the update target joins to.
    /// </summary>
    /// <param name="targetInfo"><see cref="ChangeTargetInfo"/> instance associated with the operation.</param>
    /// <param name="node">Source update node.</param>
    /// <param name="updateAssignmentsVisitor">
    /// Optional <see cref="ComplexUpdateAssignmentsVisitor"/> that contains information about complex value assignments.
    /// </param>
    /// <returns>New <see cref="SqlUpdateNode"/> instance.</returns>
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

    /// <summary>
    /// Creates a simplified version of an <see cref="SqlDeleteFromNode"/> that does not represent a valid SQL statement on its own,
    /// by using a common table expression that the delete target joins to.
    /// </summary>
    /// <param name="targetInfo"><see cref="ChangeTargetInfo"/> instance associated with the operation.</param>
    /// <param name="node">Source delete node.</param>
    /// <returns>New <see cref="SqlDeleteFromNode"/> instance.</returns>
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

    /// <summary>
    /// Creates two new collections of traits be separating traits of <see cref="SqlNodeType.CommonTableExpressionTrait"/> type.
    /// </summary>
    /// <param name="traits">Source collection of traits.</param>
    /// <returns>
    /// A tuple whose first element is a collection of traits of only <see cref="SqlNodeType.CommonTableExpressionTrait"/> type
    /// and whose second element is a collection of traits without traits of <see cref="SqlNodeType.CommonTableExpressionTrait"/> type.
    /// </returns>
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

    /// <summary>
    /// Creates a new collection of traits be excluding traits of <see cref="SqlNodeType.CommonTableExpressionTrait"/> type.
    /// </summary>
    /// <param name="traits">Source collection of traits.</param>
    /// <returns>New collection of traits.</returns>
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

    /// <summary>
    /// Extracts names of columns that are part of the provided table's primary key constraint.
    /// </summary>
    /// <param name="node">Table to process.</param>
    /// <returns>Collection of names of columns that are part of the provided table's primary key constraint.</returns>
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

    /// <summary>
    /// Attempts to extract names of columns that are part of the provided table's primary key constraint.
    /// </summary>
    /// <param name="node">Table to process.</param>
    /// <returns>
    /// Collection of names of columns that are part of the provided table's primary key constraint or null when the extraction has failed.
    /// </returns>
    /// <remarks>
    /// If table does not currently have a primary key, then all of its columns will be extracted.
    /// Extraction will fail when table does not have any columns.
    /// </remarks>
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

    /// <summary>
    /// Attempts to extract names of columns that are part of the provided table's primary key constraint.
    /// </summary>
    /// <param name="node">Table to process.</param>
    /// <returns>
    /// Collection of names of columns that are part of the provided table's primary key constraint or null when the extraction has failed.
    /// </returns>
    /// <remarks>
    /// If table does not currently have a primary key, then all of its columns will be extracted.
    /// Extraction will fail when table does not have any columns or when its primary key does not have any columns,
    /// or any of primary key's columns is not a data field.
    /// </remarks>
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

    /// <summary>
    /// Creates a new <see cref="ChangeTargetInfo"/> instance.
    /// </summary>
    /// <param name="node">Target of the change.</param>
    /// <returns>New <see cref="ChangeTargetInfo"/> instance.</returns>
    [Pure]
    protected static ChangeTargetInfo ExtractTableDeleteOrUpdateInfo(SqlTableNode node)
    {
        var identityColumnNames = ExtractIdentityColumnNames( node );
        return new ChangeTargetInfo( node, node.MarkAsOptional( false ).AsSelf(), identityColumnNames );
    }

    /// <summary>
    /// Creates a new <see cref="ChangeTargetInfo"/> instance.
    /// </summary>
    /// <param name="node">Source delete node.</param>
    /// <param name="target">Target of the change.</param>
    /// <returns>New <see cref="ChangeTargetInfo"/> instance.</returns>
    /// <exception cref="SqlNodeVisitorException">
    /// When identity columns from the <paramref name="target"/> could not be extracted.
    /// </exception>
    [Pure]
    protected ChangeTargetInfo ExtractTableBuilderDeleteInfo(SqlDeleteFromNode node, SqlTableBuilderNode target)
    {
        var identityColumnNames = TryExtractIdentityColumnNames( target );
        if ( identityColumnNames is null )
            throw new SqlNodeVisitorException( ExceptionResources.TargetDoesNotContainValidIdentityColumns, this, node );

        return new ChangeTargetInfo( target, target.MarkAsOptional( false ).AsSelf(), identityColumnNames );
    }

    /// <summary>
    /// Creates a new <see cref="ChangeTargetInfo"/> instance.
    /// </summary>
    /// <param name="node">Source update node.</param>
    /// <param name="target">Target of the change.</param>
    /// <returns>New <see cref="ChangeTargetInfo"/> instance.</returns>
    /// <exception cref="SqlNodeVisitorException">
    /// When identity columns from the <paramref name="target"/> could not be extracted.
    /// </exception>
    [Pure]
    protected ChangeTargetInfo ExtractTableBuilderUpdateInfo(SqlUpdateNode node, SqlTableBuilderNode target)
    {
        var identityColumnNames = TryExtractIdentityColumnNames( target );
        if ( identityColumnNames is null )
            throw new SqlNodeVisitorException( ExceptionResources.TargetDoesNotContainValidIdentityColumns, this, node );

        return new ChangeTargetInfo( target, target.MarkAsOptional( false ).AsSelf(), identityColumnNames );
    }

    /// <summary>
    /// Creates a new <see cref="ChangeTargetInfo"/> instance.
    /// </summary>
    /// <param name="node">Source delete node.</param>
    /// <param name="target">Target of the change.</param>
    /// <returns>New <see cref="ChangeTargetInfo"/> instance.</returns>
    /// <exception cref="SqlNodeVisitorException">
    /// When identity columns from the <paramref name="target"/> could not be extracted.
    /// </exception>
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

    /// <summary>
    /// Creates a new <see cref="ChangeTargetInfo"/> instance.
    /// </summary>
    /// <param name="node">Source update node.</param>
    /// <param name="target">Target of the change.</param>
    /// <returns>New <see cref="ChangeTargetInfo"/> instance.</returns>
    /// <exception cref="SqlNodeVisitorException">
    /// When identity columns from the <paramref name="target"/> could not be extracted.
    /// </exception>
    [Pure]
    protected ChangeTargetInfo ExtractNewTableUpdateInfo(SqlUpdateNode node, SqlNewTableNode target)
    {
        var identityColumnNames = TryExtractIdentityColumnNames( target );
        if ( identityColumnNames is null )
            throw new SqlNodeVisitorException( ExceptionResources.TargetDoesNotContainValidIdentityColumns, this, node );

        return new ChangeTargetInfo( target, target.MarkAsOptional( false ).AsSelf(), identityColumnNames );
    }

    /// <summary>
    /// Creates a new <see cref="ChangeTargetInfo"/> instance.
    /// </summary>
    /// <param name="node">Source delete node.</param>
    /// <returns>New <see cref="ChangeTargetInfo"/> instance.</returns>
    /// <exception cref="SqlNodeVisitorException">
    /// When identity columns from the delete target could not be extracted or delete target is not aliased.
    /// </exception>
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

    /// <summary>
    /// Creates a new <see cref="ChangeTargetInfo"/> instance.
    /// </summary>
    /// <param name="node">Source update node.</param>
    /// <returns>New <see cref="ChangeTargetInfo"/> instance.</returns>
    /// <exception cref="SqlNodeVisitorException">
    /// When identity columns from the update target could not be extracted or update target is not aliased.
    /// </exception>
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

    /// <summary>
    /// Extracts a collection of data field nodes that define a default conflict target for an <see cref="SqlUpsertNode"/>.
    /// </summary>
    /// <param name="node">Source upsert node.</param>
    /// <returns>
    /// Collection of <see cref="SqlDataFieldNode"/> instances that define the conflict target for the provided <paramref name="node"/>.
    /// </returns>
    /// <exception cref="SqlNodeVisitorException">When identity columns from the upsert target could not be extracted.</exception>
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

    /// <summary>
    /// Creates a new <see cref="ComplexUpdateAssignmentsVisitor"/> instance or returns null when <see cref="SqlUpdateNode.DataSource"/>
    /// of the provided <paramref name="node"/> does not contain any joins.
    /// </summary>
    /// <param name="node">Source update node.</param>
    /// <returns>New <see cref="ComplexUpdateAssignmentsVisitor"/> instance or null.</returns>
    [Pure]
    protected static ComplexUpdateAssignmentsVisitor? CreateUpdateAssignmentsVisitor(SqlUpdateNode node)
    {
        if ( node.DataSource.Joins.Count == 0 )
            return null;

        var result = new ComplexUpdateAssignmentsVisitor( node.DataSource );
        result.VisitAssignmentRange( node.Assignments );
        return result;
    }

    /// <summary>
    /// Represents an information about a record set that will be replaced with its base, non-aliased version.
    /// </summary>
    /// <param name="Target">Target of the change.</param>
    /// <param name="BaseTarget">Base, non-aliased version of the target.</param>
    /// <param name="IdentityColumnNames">Collection of target's identity column names.</param>
    protected readonly record struct ChangeTargetInfo(SqlRecordSetNode Target, SqlRecordSetNode BaseTarget, string[] IdentityColumnNames);

    /// <summary>
    /// Represents an <see cref="SqlNodeVisitor"/> that extracts information from a collection of <see cref="SqlValueAssignmentNode"/>
    /// instances about data fields whose assigned expressions contain data fields from joined record sets.
    /// </summary>
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

        /// <summary>
        /// Checks whether or not this visitor has found at least one complex value assignment.
        /// </summary>
        /// <returns><b>true</b> when at least one complex assignment has been found, otherwise <b>false</b>.</returns>
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool ContainsComplexAssignments()
        {
            return _indexesOfComplexAssignments is not null;
        }

        /// <summary>
        /// Returns a collection of 0-based indexes of all complex value assignments.
        /// </summary>
        /// <returns>Collection of 0-based indexes of all complex value assignments.</returns>
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ReadOnlySpan<int> GetIndexesOfComplexAssignments()
        {
            return CollectionsMarshal.AsSpan( _indexesOfComplexAssignments );
        }

        /// <inheritdoc />
        public override void VisitRawDataField(SqlRawDataFieldNode node)
        {
            VisitDataField( node );
        }

        /// <inheritdoc />
        public override void VisitColumn(SqlColumnNode node)
        {
            VisitDataField( node );
        }

        /// <inheritdoc />
        public override void VisitColumnBuilder(SqlColumnBuilderNode node)
        {
            VisitDataField( node );
        }

        /// <inheritdoc />
        public override void VisitQueryDataField(SqlQueryDataFieldNode node)
        {
            VisitDataField( node );
        }

        /// <inheritdoc />
        public override void VisitViewDataField(SqlViewDataFieldNode node)
        {
            VisitDataField( node );
        }

        /// <inheritdoc />
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
