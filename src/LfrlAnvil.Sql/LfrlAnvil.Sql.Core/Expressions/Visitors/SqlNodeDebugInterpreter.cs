﻿// Copyright 2024 Łukasz Furlepa
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
using System.Diagnostics.Contracts;
using System.Globalization;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents an object capable of recursive traversal over an SQL syntax tree and translating that tree
/// into its debug text representation.
/// </summary>
/// <remarks>
/// This interpreter can be useful for debugging complex SQL syntax trees.
/// It is also used in the default <see cref="SqlNodeBase.ToString()"/> method implementation.
/// </remarks>
public sealed class SqlNodeDebugInterpreter : SqlNodeInterpreter
{
    /// <summary>
    /// Creates a new <see cref="SqlNodeInterpreter"/> instance with default context.
    /// </summary>
    public SqlNodeDebugInterpreter()
        : this( SqlNodeInterpreterContext.Create() ) { }

    /// <summary>
    /// Creates a new <see cref="SqlNodeInterpreter"/> instance.
    /// </summary>
    /// <param name="context">Underlying <see cref="SqlNodeInterpreterContext"/> instance.</param>
    public SqlNodeDebugInterpreter(SqlNodeInterpreterContext context)
        : base( context, beginNameDelimiter: '[', endNameDelimiter: ']' ) { }

    /// <inheritdoc />
    public override void VisitRawDataField(SqlRawDataFieldNode node)
    {
        base.VisitRawDataField( node );
        AppendExpressionType( node.Type );
    }

    /// <inheritdoc />
    public override void VisitLiteral(SqlLiteralNode node)
    {
        var value = node.GetValue();

        Context.Sql
            .Append( '"' )
            .Append( (value as IConvertible)?.ToString( CultureInfo.InvariantCulture ) ?? value.ToString() )
            .Append( '"' );

        AppendExpressionType( node.Type );
    }

    /// <inheritdoc />
    public override void VisitParameter(SqlParameterNode node)
    {
        Context.Sql.Append( '@' ).Append( node.Name );
        if ( node.Index is not null )
            Context.Sql.AppendSpace().Append( '(' ).Append( '#' ).Append( node.Index.Value ).Append( ')' );

        AppendExpressionType( node.Type );
        AddContextParameter( node );
    }

    /// <inheritdoc />
    public override void VisitColumn(SqlColumnNode node)
    {
        base.VisitColumn( node );
        AppendExpressionType( node.Type );
    }

    /// <inheritdoc />
    public override void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        base.VisitColumnBuilder( node );
        AppendExpressionType( node.Type );
    }

    /// <inheritdoc />
    public override void VisitCoalesceFunction(SqlCoalesceFunctionExpressionNode node)
    {
        VisitSimpleFunction( "COALESCE", node );
    }

    /// <inheritdoc />
    public override void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CURRENT_DATE", node );
    }

    /// <inheritdoc />
    public override void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CURRENT_TIME", node );
    }

    /// <inheritdoc />
    public override void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CURRENT_DATETIME", node );
    }

    /// <inheritdoc />
    public override void VisitCurrentUtcDateTimeFunction(SqlCurrentUtcDateTimeFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CURRENT_UTC_DATETIME", node );
    }

    /// <inheritdoc />
    public override void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CURRENT_TIMESTAMP", node );
    }

    /// <inheritdoc />
    public override void VisitExtractDateFunction(SqlExtractDateFunctionExpressionNode node)
    {
        VisitSimpleFunction( "EXTRACT_DATE", node );
    }

    /// <inheritdoc />
    public override void VisitExtractTimeOfDayFunction(SqlExtractTimeOfDayFunctionExpressionNode node)
    {
        VisitSimpleFunction( "EXTRACT_TIME_OF_DAY", node );
    }

    /// <inheritdoc />
    public override void VisitExtractDayFunction(SqlExtractDayFunctionExpressionNode node)
    {
        Context.Sql.Append( "EXTRACT_DAY_OF" ).Append( '_' ).Append( node.Unit.ToString().ToUpperInvariant() );
        VisitFunctionArguments( node.Arguments );
    }

    /// <inheritdoc />
    public override void VisitExtractTemporalUnitFunction(SqlExtractTemporalUnitFunctionExpressionNode node)
    {
        Context.Sql.Append( "EXTRACT_TEMPORAL" ).Append( '_' ).Append( node.Unit.ToString().ToUpperInvariant() );
        VisitFunctionArguments( node.Arguments );
    }

    /// <inheritdoc />
    public override void VisitTemporalAddFunction(SqlTemporalAddFunctionExpressionNode node)
    {
        Context.Sql.Append( "TEMPORAL_ADD" ).Append( '_' ).Append( node.Unit.ToString().ToUpperInvariant() );
        VisitFunctionArguments( node.Arguments );
    }

    /// <inheritdoc />
    public override void VisitTemporalDiffFunction(SqlTemporalDiffFunctionExpressionNode node)
    {
        Context.Sql.Append( "TEMPORAL_DIFF" ).Append( '_' ).Append( node.Unit.ToString().ToUpperInvariant() );
        VisitFunctionArguments( node.Arguments );
    }

    /// <inheritdoc />
    public override void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node)
    {
        VisitSimpleFunction( "NEW_GUID", node );
    }

    /// <inheritdoc />
    public override void VisitLengthFunction(SqlLengthFunctionExpressionNode node)
    {
        VisitSimpleFunction( "LENGTH", node );
    }

    /// <inheritdoc />
    public override void VisitByteLengthFunction(SqlByteLengthFunctionExpressionNode node)
    {
        VisitSimpleFunction( "BYTE_LENGTH", node );
    }

    /// <inheritdoc />
    public override void VisitToLowerFunction(SqlToLowerFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TO_LOWER", node );
    }

    /// <inheritdoc />
    public override void VisitToUpperFunction(SqlToUpperFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TO_UPPER", node );
    }

    /// <inheritdoc />
    public override void VisitTrimStartFunction(SqlTrimStartFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TRIM_START", node );
    }

    /// <inheritdoc />
    public override void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TRIM_END", node );
    }

    /// <inheritdoc />
    public override void VisitTrimFunction(SqlTrimFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TRIM", node );
    }

    /// <inheritdoc />
    public override void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SUBSTRING", node );
    }

    /// <inheritdoc />
    public override void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node)
    {
        VisitSimpleFunction( "REPLACE", node );
    }

    /// <inheritdoc />
    public override void VisitReverseFunction(SqlReverseFunctionExpressionNode node)
    {
        VisitSimpleFunction( "REVERSE", node );
    }

    /// <inheritdoc />
    public override void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node)
    {
        VisitSimpleFunction( "INDEX_OF", node );
    }

    /// <inheritdoc />
    public override void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node)
    {
        VisitSimpleFunction( "LAST_INDEX_OF", node );
    }

    /// <inheritdoc />
    public override void VisitSignFunction(SqlSignFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SIGN", node );
    }

    /// <inheritdoc />
    public override void VisitAbsFunction(SqlAbsFunctionExpressionNode node)
    {
        VisitSimpleFunction( "ABS", node );
    }

    /// <inheritdoc />
    public override void VisitCeilingFunction(SqlCeilingFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CEILING", node );
    }

    /// <inheritdoc />
    public override void VisitFloorFunction(SqlFloorFunctionExpressionNode node)
    {
        VisitSimpleFunction( "FLOOR", node );
    }

    /// <inheritdoc />
    public override void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TRUNCATE", node );
    }

    /// <inheritdoc />
    public override void VisitRoundFunction(SqlRoundFunctionExpressionNode node)
    {
        VisitSimpleFunction( "ROUND", node );
    }

    /// <inheritdoc />
    public override void VisitPowerFunction(SqlPowerFunctionExpressionNode node)
    {
        VisitSimpleFunction( "POWER", node );
    }

    /// <inheritdoc />
    public override void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SQUARE_ROOT", node );
    }

    /// <inheritdoc />
    public override void VisitMinFunction(SqlMinFunctionExpressionNode node)
    {
        VisitSimpleFunction( "MIN", node );
    }

    /// <inheritdoc />
    public override void VisitMaxFunction(SqlMaxFunctionExpressionNode node)
    {
        VisitSimpleFunction( "MAX", node );
    }

    /// <inheritdoc />
    public override void VisitCustomFunction(SqlFunctionExpressionNode node)
    {
        VisitSimpleFunction( $"{{{node.GetType().GetDebugString()}}}", node );
    }

    /// <inheritdoc />
    public override void VisitNamedAggregateFunction(SqlNamedAggregateFunctionExpressionNode node)
    {
        Context.Sql.Append( "AGG" ).Append( '_' );
        AppendDelimitedSchemaObjectName( node.Name );
        VisitFunctionArguments( node.Arguments );

        using ( Context.TempIndentIncrease() )
            VisitTraits( node.Traits );
    }

    /// <inheritdoc />
    public override void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", "MIN", node );
    }

    /// <inheritdoc />
    public override void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", "MAX", node );
    }

    /// <inheritdoc />
    public override void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", "AVERAGE", node );
    }

    /// <inheritdoc />
    public override void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", "SUM", node );
    }

    /// <inheritdoc />
    public override void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", "COUNT", node );
    }

    /// <inheritdoc />
    public override void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", "STRING_CONCAT", node );
    }

    /// <inheritdoc />
    public override void VisitRowNumberWindowFunction(SqlRowNumberWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "ROW_NUMBER", node );
    }

    /// <inheritdoc />
    public override void VisitRankWindowFunction(SqlRankWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "RANK", node );
    }

    /// <inheritdoc />
    public override void VisitDenseRankWindowFunction(SqlDenseRankWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "DENSE_RANK", node );
    }

    /// <inheritdoc />
    public override void VisitCumulativeDistributionWindowFunction(SqlCumulativeDistributionWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "CUMULATIVE_DISTRIBUTION", node );
    }

    /// <inheritdoc />
    public override void VisitNTileWindowFunction(SqlNTileWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "N_TILE", node );
    }

    /// <inheritdoc />
    public override void VisitLagWindowFunction(SqlLagWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "LAG", node );
    }

    /// <inheritdoc />
    public override void VisitLeadWindowFunction(SqlLeadWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "LEAD", node );
    }

    /// <inheritdoc />
    public override void VisitFirstValueWindowFunction(SqlFirstValueWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "FIRST_VALUE", node );
    }

    /// <inheritdoc />
    public override void VisitLastValueWindowFunction(SqlLastValueWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "LAST_VALUE", node );
    }

    /// <inheritdoc />
    public override void VisitNthValueWindowFunction(SqlNthValueWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "NTH_VALUE", node );
    }

    /// <inheritdoc />
    public override void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", $"{{{node.GetType().GetDebugString()}}}", node );
    }

    /// <inheritdoc />
    public override void VisitEqualTo(SqlEqualToConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "==", node.Right );
    }

    /// <inheritdoc />
    public override void VisitNotEqualTo(SqlNotEqualToConditionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "<>", node.Right );
    }

    /// <inheritdoc />
    public override void VisitConditionValue(SqlConditionValueNode node)
    {
        Context.Sql.Append( "CONDITION_VALUE" );
        VisitChild( node.Condition );
    }

    /// <inheritdoc />
    public override void VisitRawRecordSet(SqlRawRecordSetNode node)
    {
        AppendDelimitedRecordSetInfo( node.Info );
        AppendDelimitedAlias( node.Alias );
    }

    /// <inheritdoc />
    public override void VisitDataSource(SqlDataSourceNode node)
    {
        Context.Sql.Append( "FROM" ).AppendSpace();

        if ( node is SqlDummyDataSourceNode )
            Context.Sql.Append( '<' ).Append( "DUMMY" ).Append( '>' );
        else
        {
            this.Visit( node.From );

            foreach ( var join in node.Joins )
            {
                Context.AppendIndent();
                VisitJoinOn( join );
            }
        }

        VisitTraits( node.Traits );
    }

    /// <inheritdoc />
    public override void VisitSelectField(SqlSelectFieldNode node)
    {
        VisitChild( node.Expression );
        if ( node.Alias is not null )
            AppendDelimitedAlias( node.Alias );
    }

    /// <inheritdoc />
    public override void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        var isChild = Context.ChildDepth > 0;
        if ( isChild )
            Context.AppendIndent();

        this.Visit( node.DataSource );
        VisitTraits( node.Traits );
        Context.AppendIndent().Append( "SELECT" );

        if ( node.Selection.Count == 0 )
            return;

        using ( Context.TempIndentIncrease() )
        {
            foreach ( var selection in node.Selection )
            {
                Context.AppendIndent();
                this.Visit( selection );
                Context.Sql.AppendComma();
            }

            Context.Sql.ShrinkBy( 1 );
        }

        if ( isChild )
            Context.AppendShortIndent();
    }

    /// <inheritdoc />
    public override void VisitCompoundQuery(SqlCompoundQueryExpressionNode node)
    {
        var isChild = Context.ChildDepth > 0;
        if ( isChild )
            Context.AppendIndent();

        this.Visit( node.FirstQuery );

        foreach ( var component in node.FollowingQueries )
        {
            Context.AppendIndent();
            VisitCompoundQueryComponent( component );
        }

        VisitTraits( node.Traits );

        if ( isChild )
            Context.AppendShortIndent();
    }

    /// <inheritdoc />
    public override void VisitFilterTrait(SqlFilterTraitNode node)
    {
        Context.Sql.Append( node.IsConjunction ? "AND" : "OR" ).AppendSpace().Append( "WHERE" ).AppendSpace();
        this.Visit( node.Filter );
    }

    /// <inheritdoc />
    public override void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node)
    {
        Context.Sql.Append( node.IsConjunction ? "AND" : "OR" ).AppendSpace().Append( "HAVING" ).AppendSpace();
        this.Visit( node.Filter );
    }

    /// <inheritdoc />
    public override void VisitLimitTrait(SqlLimitTraitNode node)
    {
        Context.Sql.Append( "LIMIT" ).AppendSpace();
        VisitChild( node.Value );
    }

    /// <inheritdoc />
    public override void VisitOffsetTrait(SqlOffsetTraitNode node)
    {
        Context.Sql.Append( "OFFSET" ).AppendSpace();
        VisitChild( node.Value );
    }

    /// <inheritdoc />
    public override void VisitCommonTableExpressionTrait(SqlCommonTableExpressionTraitNode node)
    {
        Context.Sql.Append( "WITH" );
        if ( node.CommonTableExpressions.Count == 0 )
            return;

        Context.Sql.AppendSpace();
        foreach ( var cte in node.CommonTableExpressions )
        {
            VisitCommonTableExpression( cte );
            Context.Sql.AppendComma();
            Context.AppendIndent();
        }

        Context.Sql.ShrinkBy( Environment.NewLine.Length + Context.Indent + 1 );
    }

    /// <inheritdoc />
    public override void VisitWindowTrait(SqlWindowTraitNode node)
    {
        Context.Sql.Append( "OVER" ).AppendSpace();
        VisitWindowDefinition( node.Definition );
    }

    /// <inheritdoc />
    public override void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        Context.Sql.Append( node.IsRecursive ? "RECURSIVE" : "ORDINAL" ).AppendSpace();
        AppendDelimitedName( node.Name );
        Context.Sql.AppendSpace();
        VisitChild( node.Query );
    }

    /// <inheritdoc />
    public override void VisitTypeCast(SqlTypeCastExpressionNode node)
    {
        Context.Sql.Append( "CAST" ).Append( '(' );
        VisitChild( node.Value );
        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace().Append( node.TargetType.GetDebugString() ).Append( ')' );
    }

    /// <inheritdoc />
    public override void VisitInsertInto(SqlInsertIntoNode node)
    {
        Context.Sql.Append( "INSERT INTO" ).AppendSpace();

        AppendDelimitedRecordSetName( node.RecordSet.AsSelf() );
        AppendDelimitedAlias( node.RecordSet.Alias );

        Context.Sql.AppendSpace().Append( '(' );

        if ( node.DataFields.Count > 0 )
        {
            foreach ( var dataField in node.DataFields )
            {
                this.Visit( dataField );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }

        Context.Sql.Append( ')' );

        Context.AppendIndent();
        this.Visit( node.Source );
    }

    /// <inheritdoc />
    public override void VisitUpdate(SqlUpdateNode node)
    {
        Context.Sql.Append( "UPDATE" ).AppendSpace();
        this.Visit( node.DataSource );
        Context.AppendIndent().Append( "SET" );

        if ( node.Assignments.Count == 0 )
            return;

        using ( Context.TempIndentIncrease() )
        {
            foreach ( var assignment in node.Assignments )
            {
                Context.AppendIndent();
                VisitValueAssignment( assignment );
                Context.Sql.AppendComma();
            }

            Context.Sql.ShrinkBy( 1 );
        }
    }

    /// <inheritdoc />
    public override void VisitUpsert(SqlUpsertNode node)
    {
        Context.Sql.Append( "UPSERT" ).AppendSpace();

        AppendDelimitedRecordSetName( node.RecordSet.AsSelf() );
        AppendDelimitedAlias( node.RecordSet.Alias );

        Context.Sql.AppendSpace().Append( "USING" );
        Context.AppendIndent();
        this.Visit( node.Source );

        if ( node.ConflictTarget.Count > 0 )
        {
            Context.AppendIndent().Append( "WITH" ).AppendSpace().Append( "CONFLICT" ).AppendSpace().Append( "TARGET" );
            Context.Sql.AppendSpace().Append( '(' );

            foreach ( var target in node.ConflictTarget )
            {
                this.Visit( target );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 ).Append( ')' );
        }

        Context.AppendIndent().Append( "INSERT" ).AppendSpace().Append( '(' );
        if ( node.InsertDataFields.Count > 0 )
        {
            foreach ( var dataField in node.InsertDataFields )
            {
                this.Visit( dataField );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }

        Context.Sql.Append( ')' );
        Context.AppendIndent().Append( "ON" ).AppendSpace().Append( "CONFLICT" ).AppendSpace().Append( "SET" );
        if ( node.UpdateAssignments.Count > 0 )
        {
            using ( Context.TempIndentIncrease() )
            {
                foreach ( var assignment in node.UpdateAssignments )
                {
                    Context.AppendIndent();
                    VisitValueAssignment( assignment );
                    Context.Sql.AppendComma();
                }

                Context.Sql.ShrinkBy( 1 );
            }
        }
    }

    /// <inheritdoc />
    public override void VisitValueAssignment(SqlValueAssignmentNode node)
    {
        VisitInfixBinaryOperator( node.DataField, symbol: "=", node.Value );
    }

    /// <inheritdoc />
    public override void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        Context.Sql.Append( "DELETE" ).AppendSpace();
        this.Visit( node.DataSource );
    }

    /// <inheritdoc />
    public override void VisitTruncate(SqlTruncateNode node)
    {
        Context.Sql.Append( "TRUNCATE" ).AppendSpace();
        AppendDelimitedRecordSetName( node.Table );
    }

    /// <inheritdoc />
    public override void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        AppendDelimitedName( node.Name );
        AppendExpressionType( node.Type );

        if ( node.DefaultValue is not null )
        {
            Context.Sql.AppendSpace().Append( "DEFAULT" ).AppendSpace();
            VisitChild( node.DefaultValue );
        }

        if ( node.Computation is not null )
        {
            Context.Sql.AppendSpace().Append( "GENERATED" ).AppendSpace();
            VisitChild( node.Computation.Value.Expression );
            Context.Sql.AppendSpace().Append( node.Computation.Value.Storage.ToString().ToUpperInvariant() );
        }
    }

    /// <inheritdoc />
    public override void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node)
    {
        Context.Sql.Append( "PRIMARY" ).AppendSpace().Append( "KEY" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.Name );
        Context.Sql.AppendSpace().Append( '(' );

        if ( node.Columns.Count > 0 )
        {
            foreach ( var column in node.Columns )
            {
                VisitOrderBy( column );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }

        Context.Sql.Append( ')' );
    }

    /// <inheritdoc />
    public override void VisitForeignKeyDefinition(SqlForeignKeyDefinitionNode node)
    {
        Context.Sql.Append( "FOREIGN" ).AppendSpace().Append( "KEY" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.Name );
        Context.Sql.AppendSpace().Append( '(' );

        if ( node.Columns.Count > 0 )
        {
            foreach ( var column in node.Columns )
            {
                VisitChild( column );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }

        Context.Sql.Append( ')' ).AppendSpace().Append( "REFERENCES" ).AppendSpace();
        AppendDelimitedRecordSetName( node.ReferencedTable );
        Context.Sql.AppendSpace().Append( '(' );

        if ( node.ReferencedColumns.Count > 0 )
        {
            foreach ( var column in node.ReferencedColumns )
            {
                VisitChild( column );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }

        Context.Sql.Append( ')' ).AppendSpace();
        Context.Sql.Append( "ON" ).AppendSpace().Append( "DELETE" ).AppendSpace().Append( node.OnDeleteBehavior.Name ).AppendSpace();
        Context.Sql.Append( "ON" ).AppendSpace().Append( "UPDATE" ).AppendSpace().Append( node.OnUpdateBehavior.Name );
    }

    /// <inheritdoc />
    public override void VisitCheckDefinition(SqlCheckDefinitionNode node)
    {
        Context.Sql.Append( "CHECK" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.Name );
        Context.Sql.AppendSpace();
        VisitChildWrappedInParentheses( node.Condition );
    }

    /// <inheritdoc />
    public override void VisitCreateTable(SqlCreateTableNode node)
    {
        Context.Sql.Append( "CREATE" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        if ( node.IfNotExists )
            Context.Sql.Append( "IF" ).AppendSpace().Append( "NOT" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        AppendDelimitedRecordSetInfo( node.Info );
        Context.Sql.AppendSpace().Append( '(' );
        VisitCreateTableDefinition( node );
        Context.AppendIndent().Append( ')' );
    }

    /// <inheritdoc />
    public override void VisitCreateView(SqlCreateViewNode node)
    {
        Context.Sql.Append( "CREATE" ).AppendSpace();
        if ( node.ReplaceIfExists )
            Context.Sql.Append( "OR" ).AppendSpace().Append( "REPLACE" ).AppendSpace();

        Context.Sql.Append( "VIEW" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Info );
        Context.Sql.AppendSpace().Append( "AS" );
        Context.AppendIndent();
        this.Visit( node.Source );
    }

    /// <inheritdoc />
    public override void VisitCreateIndex(SqlCreateIndexNode node)
    {
        Context.Sql.Append( "CREATE" ).AppendSpace();
        if ( node.ReplaceIfExists )
            Context.Sql.Append( "OR" ).AppendSpace().Append( "REPLACE" ).AppendSpace();

        if ( node.IsUnique )
            Context.Sql.Append( "UNIQUE" ).AppendSpace();

        Context.Sql.Append( "INDEX" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.Name );
        Context.Sql.AppendSpace().Append( "ON" ).AppendSpace();
        AppendDelimitedRecordSetName( node.Table );

        Context.Sql.AppendSpace().Append( '(' );
        if ( node.Columns.Count > 0 )
        {
            using ( Context.TempIndentIncrease() )
            {
                foreach ( var column in node.Columns )
                {
                    VisitOrderBy( column );
                    Context.Sql.AppendComma().AppendSpace();
                }

                Context.Sql.ShrinkBy( 2 );
            }
        }

        Context.Sql.Append( ')' );

        if ( node.Filter is not null )
        {
            Context.Sql.AppendSpace().Append( "WHERE" ).AppendSpace();
            VisitChild( node.Filter );
        }
    }

    /// <inheritdoc />
    public override void VisitRenameTable(SqlRenameTableNode node)
    {
        Context.Sql.Append( "RENAME" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendSpace().Append( "TO" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.NewName );
    }

    /// <inheritdoc />
    public override void VisitRenameColumn(SqlRenameColumnNode node)
    {
        Context.Sql.Append( "RENAME" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendDot();
        AppendDelimitedName( node.OldName );
        Context.Sql.AppendSpace().Append( "TO" ).AppendSpace();
        AppendDelimitedName( node.NewName );
    }

    /// <inheritdoc />
    public override void VisitAddColumn(SqlAddColumnNode node)
    {
        Context.Sql.Append( "ADD" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendDot();
        VisitColumnDefinition( node.Definition );
    }

    /// <inheritdoc />
    public override void VisitDropColumn(SqlDropColumnNode node)
    {
        Context.Sql.Append( "DROP" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendDot();
        AppendDelimitedName( node.Name );
    }

    /// <inheritdoc />
    public override void VisitDropTable(SqlDropTableNode node)
    {
        Context.Sql.Append( "DROP" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        if ( node.IfExists )
            Context.Sql.Append( "IF" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        AppendDelimitedRecordSetInfo( node.Table );
    }

    /// <inheritdoc />
    public override void VisitDropView(SqlDropViewNode node)
    {
        Context.Sql.Append( "DROP" ).AppendSpace().Append( "VIEW" ).AppendSpace();
        if ( node.IfExists )
            Context.Sql.Append( "IF" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        AppendDelimitedRecordSetInfo( node.View );
    }

    /// <inheritdoc />
    public override void VisitDropIndex(SqlDropIndexNode node)
    {
        Context.Sql.Append( "DROP" ).AppendSpace().Append( "INDEX" ).AppendSpace();
        if ( node.IfExists )
            Context.Sql.Append( "IF" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        AppendDelimitedSchemaObjectName( node.Name );
        Context.Sql.AppendSpace().Append( "ON" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
    }

    /// <inheritdoc />
    public override void VisitStatementBatch(SqlStatementBatchNode node)
    {
        Context.Sql.Append( "BATCH" );
        Context.AppendIndent();
        Context.Sql.Append( '(' );

        using ( Context.TempIndentIncrease() )
        {
            Context.AppendIndent();
            base.VisitStatementBatch( node );
        }

        Context.AppendIndent();
        Context.Sql.Append( ')' );
    }

    /// <inheritdoc />
    public override void VisitBeginTransaction(SqlBeginTransactionNode node)
    {
        Context.Sql
            .Append( "BEGIN" )
            .AppendSpace()
            .Append( node.IsolationLevel.ToString().ToUpperInvariant() )
            .AppendSpace()
            .Append( "TRANSACTION" );
    }

    /// <inheritdoc />
    public override void VisitCustom(SqlNodeBase node)
    {
        if ( node is SqlInternalRecordSetNode internalRecordSet )
        {
            Context.Sql.Append( '(' );
            AppendDelimitedRecordSetName( internalRecordSet );
            Context.Sql.AppendSpace().Append( "FROM" ).AppendSpace();
            AppendDelimitedRecordSetName( internalRecordSet.Base );
            Context.Sql.Append( ')' );
            return;
        }

        Context.Sql.Append( '{' ).Append( node.GetType().GetDebugString() ).Append( '}' );
    }

    /// <inheritdoc />
    public override void AppendDelimitedRecordSetName(SqlRecordSetNode node)
    {
        if ( node.IsAliased )
            AppendDelimitedName( node.Alias );
        else
            AppendDelimitedRecordSetInfo( node.Info );
    }

    /// <inheritdoc />
    [Pure]
    protected override bool DoesChildNodeRequireParentheses(SqlNodeBase node)
    {
        return true;
    }

    /// <inheritdoc />
    protected override void VisitCustomWindowFrame(SqlWindowFrameNode node)
    {
        Context.Sql.Append( '{' ).Append( node.GetType().GetDebugString() ).Append( '}' ).AppendSpace().Append( "BETWEEN" ).AppendSpace();
        AppendWindowFrameBoundary( node.Start );
        Context.Sql.AppendSpace().Append( "AND" ).AppendSpace();
        AppendWindowFrameBoundary( node.End );
    }

    private void AppendExpressionType(TypeNullability? type)
    {
        Context.Sql.AppendSpace().Append( ':' ).AppendSpace();
        if ( type is null )
            Context.Sql.Append( '?' );
        else
            Context.Sql.Append( type.Value.ToString() );
    }

    private void VisitSimpleAggregateFunction(string prefix, string functionName, SqlAggregateFunctionExpressionNode node)
    {
        Context.Sql.Append( prefix ).Append( '_' ).Append( functionName );
        VisitFunctionArguments( node.Arguments );

        using ( Context.TempIndentIncrease() )
            VisitTraits( node.Traits );
    }

    private void VisitTraits(Chain<SqlTraitNode> traits)
    {
        foreach ( var trait in traits )
        {
            Context.AppendIndent();
            this.Visit( trait );
        }
    }
}
