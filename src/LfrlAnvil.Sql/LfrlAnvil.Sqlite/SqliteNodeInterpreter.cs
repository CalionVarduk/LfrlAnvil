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
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public class SqliteNodeInterpreter : SqlNodeInterpreter
{
    private SqlRecordSetNode? _upsertUpdateSourceReplacement;

    /// <summary>
    /// Creates a new <see cref="SqliteNodeInterpreter"/> instance.
    /// </summary>
    /// <param name="options">
    /// <see cref="SqliteNodeInterpreterOptions"/> instance associated with this interpreter that defines its behavior.
    /// </param>
    /// <param name="context">Underlying <see cref="SqlNodeInterpreterContext"/> instance.</param>
    public SqliteNodeInterpreter(SqliteNodeInterpreterOptions options, SqlNodeInterpreterContext context)
        : base( context, beginNameDelimiter: '"', endNameDelimiter: '"' )
    {
        _upsertUpdateSourceReplacement = null;
        Options = options;
        TypeDefinitions = Options.TypeDefinitions ?? new SqliteColumnTypeDefinitionProviderBuilder().Build();
    }

    /// <summary>
    /// <see cref="SqliteNodeInterpreterOptions"/> instance associated with this interpreter that defines its behavior.
    /// </summary>
    public SqliteNodeInterpreterOptions Options { get; }

    /// <summary>
    /// <see cref="SqliteColumnTypeDefinitionProvider"/> instance associated with this interpreter.
    /// </summary>
    public SqliteColumnTypeDefinitionProvider TypeDefinitions { get; }

    /// <inheritdoc />
    public override void VisitLiteral(SqlLiteralNode node)
    {
        var sql = node.GetSql( TypeDefinitions );
        Context.Sql.Append( sql );
    }

    /// <inheritdoc />
    public override void VisitParameter(SqlParameterNode node)
    {
        if ( node.Index is not null && Options.ArePositionalParametersEnabled )
            Context.Sql.Append( '?' ).Append( node.Index.Value + 1 );
        else
            Context.Sql.Append( '@' ).Append( node.Name );

        AddContextParameter( node );
    }

    /// <inheritdoc />
    public override void VisitModulo(SqlModuloExpressionNode node)
    {
        Context.Sql.Append( "MOD" ).Append( '(' );
        VisitChild( node.Left );
        Context.Sql.AppendComma().AppendSpace();
        VisitChild( node.Right );
        Context.Sql.Append( ')' );
    }

    /// <inheritdoc />
    public override void VisitBitwiseXor(SqlBitwiseXorExpressionNode node)
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

    /// <inheritdoc />
    public override void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node)
    {
        VisitSimpleFunction( "GET_CURRENT_DATE", node );
    }

    /// <inheritdoc />
    public override void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node)
    {
        VisitSimpleFunction( "GET_CURRENT_TIME", node );
    }

    /// <inheritdoc />
    public override void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node)
    {
        VisitSimpleFunction( "GET_CURRENT_DATETIME", node );
    }

    /// <inheritdoc />
    public override void VisitCurrentUtcDateTimeFunction(SqlCurrentUtcDateTimeFunctionExpressionNode node)
    {
        VisitSimpleFunction( "GET_CURRENT_UTC_DATETIME", node );
    }

    /// <inheritdoc />
    public override void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        VisitSimpleFunction( "GET_CURRENT_TIMESTAMP", node );
    }

    /// <inheritdoc />
    public override void VisitExtractDateFunction(SqlExtractDateFunctionExpressionNode node)
    {
        VisitSimpleFunction( "DATE", node );
    }

    /// <inheritdoc />
    public override void VisitExtractTimeOfDayFunction(SqlExtractTimeOfDayFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TIME_OF_DAY", node );
    }

    /// <inheritdoc />
    public override void VisitExtractDayFunction(SqlExtractDayFunctionExpressionNode node)
    {
        var unit = node.Unit switch
        {
            SqlTemporalUnit.Year => SqliteHelpers.TemporalDayOfYearUnit,
            SqlTemporalUnit.Month => SqliteHelpers.TemporalDayOfMonthUnit,
            _ => SqliteHelpers.TemporalDayOfWeekUnit
        };

        Context.Sql.Append( "EXTRACT_TEMPORAL" ).Append( '(' ).Append( unit ).AppendComma().AppendSpace();
        VisitChild( node.Arguments[0] );
        Context.Sql.Append( ')' );
    }

    /// <inheritdoc />
    public override void VisitExtractTemporalUnitFunction(SqlExtractTemporalUnitFunctionExpressionNode node)
    {
        var unit = SqliteHelpers.GetDbTemporalUnit( node.Unit );
        Context.Sql.Append( "EXTRACT_TEMPORAL" ).Append( '(' ).Append( unit ).AppendComma().AppendSpace();
        VisitChild( node.Arguments[0] );
        Context.Sql.Append( ')' );
    }

    /// <inheritdoc />
    public override void VisitTemporalAddFunction(SqlTemporalAddFunctionExpressionNode node)
    {
        var unit = SqliteHelpers.GetDbTemporalUnit( node.Unit );
        Context.Sql.Append( "TEMPORAL_ADD" ).Append( '(' ).Append( unit ).AppendComma().AppendSpace();
        VisitChild( node.Arguments[1] );
        Context.Sql.AppendComma().AppendSpace();
        VisitChild( node.Arguments[0] );
        Context.Sql.Append( ')' );
    }

    /// <inheritdoc />
    public override void VisitTemporalDiffFunction(SqlTemporalDiffFunctionExpressionNode node)
    {
        var unit = SqliteHelpers.GetDbTemporalUnit( node.Unit );
        Context.Sql.Append( "TEMPORAL_DIFF" ).Append( '(' ).Append( unit ).AppendComma().AppendSpace();
        VisitChild( node.Arguments[0] );
        Context.Sql.AppendComma().AppendSpace();
        VisitChild( node.Arguments[1] );
        Context.Sql.Append( ')' );
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
        VisitSimpleFunction( "OCTET_LENGTH", node );
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
        VisitSimpleFunction( "LTRIM", node );
    }

    /// <inheritdoc />
    public override void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node)
    {
        VisitSimpleFunction( "RTRIM", node );
    }

    /// <inheritdoc />
    public override void VisitTrimFunction(SqlTrimFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TRIM", node );
    }

    /// <inheritdoc />
    public override void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SUBSTR", node );
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
        VisitSimpleFunction( "INSTR", node );
    }

    /// <inheritdoc />
    public override void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node)
    {
        VisitSimpleFunction( "INSTR_LAST", node );
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
        VisitSimpleFunction( "CEIL", node );
    }

    /// <inheritdoc />
    public override void VisitFloorFunction(SqlFloorFunctionExpressionNode node)
    {
        VisitSimpleFunction( "FLOOR", node );
    }

    /// <inheritdoc />
    public override void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node)
    {
        VisitSimpleFunction( node.Arguments.Count == 1 ? "TRUNC" : "TRUNC2", node );
    }

    /// <inheritdoc />
    public override void VisitRoundFunction(SqlRoundFunctionExpressionNode node)
    {
        VisitSimpleFunction( "ROUND", node );
    }

    /// <inheritdoc />
    public override void VisitPowerFunction(SqlPowerFunctionExpressionNode node)
    {
        VisitSimpleFunction( "POW", node );
    }

    /// <inheritdoc />
    public override void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SQRT", node );
    }

    /// <inheritdoc />
    public override void VisitMinFunction(SqlMinFunctionExpressionNode node)
    {
        if ( node.Arguments.Count == 1 )
            VisitChild( node.Arguments[0] );
        else
            VisitSimpleFunction( "MIN", node );
    }

    /// <inheritdoc />
    public override void VisitMaxFunction(SqlMaxFunctionExpressionNode node)
    {
        if ( node.Arguments.Count == 1 )
            VisitChild( node.Arguments[0] );
        else
            VisitSimpleFunction( "MAX", node );
    }

    /// <inheritdoc />
    public override void VisitNamedAggregateFunction(SqlNamedAggregateFunctionExpressionNode node)
    {
        var traits = ExtractAggregateFunctionTraits( node.Traits );
        AppendDelimitedSchemaObjectName( node.Name );
        VisitAggregateFunctionArgumentsAndTraits( node.Arguments, traits );
    }

    /// <inheritdoc />
    public override void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "MIN", node );
    }

    /// <inheritdoc />
    public override void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "MAX", node );
    }

    /// <inheritdoc />
    public override void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AVG", node );
    }

    /// <inheritdoc />
    public override void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "SUM", node );
    }

    /// <inheritdoc />
    public override void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "COUNT", node );
    }

    /// <inheritdoc />
    public override void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "GROUP_CONCAT", node );
    }

    /// <inheritdoc />
    public override void VisitRowNumberWindowFunction(SqlRowNumberWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "ROW_NUMBER", node );
    }

    /// <inheritdoc />
    public override void VisitRankWindowFunction(SqlRankWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "RANK", node );
    }

    /// <inheritdoc />
    public override void VisitDenseRankWindowFunction(SqlDenseRankWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "DENSE_RANK", node );
    }

    /// <inheritdoc />
    public override void VisitCumulativeDistributionWindowFunction(SqlCumulativeDistributionWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "CUME_DIST", node );
    }

    /// <inheritdoc />
    public override void VisitNTileWindowFunction(SqlNTileWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "NTILE", node );
    }

    /// <inheritdoc />
    public override void VisitLagWindowFunction(SqlLagWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "LAG", node );
    }

    /// <inheritdoc />
    public override void VisitLeadWindowFunction(SqlLeadWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "LEAD", node );
    }

    /// <inheritdoc />
    public override void VisitFirstValueWindowFunction(SqlFirstValueWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "FIRST_VALUE", node );
    }

    /// <inheritdoc />
    public override void VisitLastValueWindowFunction(SqlLastValueWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "LAST_VALUE", node );
    }

    /// <inheritdoc />
    public override void VisitNthValueWindowFunction(SqlNthValueWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "NTH_VALUE", node );
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
    public override void VisitTypeCast(SqlTypeCastExpressionNode node)
    {
        Context.Sql.Append( "CAST" ).Append( '(' );
        VisitChild( node.Value );
        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace();

        var typeDefinition = node.TargetTypeDefinition ?? TypeDefinitions.GetByType( node.TargetType );
        Context.Sql.Append( typeDefinition.DataType.Name ).Append( ')' );
    }

    /// <inheritdoc />
    public sealed override void VisitInsertInto(SqlInsertIntoNode node)
    {
        switch ( node.Source.NodeType )
        {
            case SqlNodeType.DataSourceQuery:
            {
                VisitInsertIntoFromDataSourceQuery( node, ReinterpretCast.To<SqlDataSourceQueryExpressionNode>( node.Source ) );
                break;
            }
            case SqlNodeType.CompoundQuery:
            {
                VisitInsertIntoFromCompoundQuery( node, ReinterpretCast.To<SqlCompoundQueryExpressionNode>( node.Source ) );
                break;
            }
            default:
            {
                VisitInsertIntoFromGenericSource( node );
                break;
            }
        }
    }

    /// <inheritdoc />
    public sealed override void VisitUpdate(SqlUpdateNode node)
    {
        var traits = ExtractDataSourceTraits( node.DataSource.Traits );

        if ( IsValidSingleTableUpdateOrDeleteStatement( node.DataSource, in traits ) )
        {
            VisitUpdateWithSimpleDataSource( node, in traits );
            return;
        }

        if ( IsValidUpdateFromStatement( node.DataSource, in traits ) )
        {
            VisitUpdateFrom( node, in traits );
            return;
        }

        var targetInfo = ExtractTargetInfo( node );
        var updateVisitor = CreateUpdateAssignmentsVisitor( node );

        if ( updateVisitor is null || ! updateVisitor.ContainsComplexAssignments() )
        {
            VisitUpdateWithComplexDataSourceAndSimpleAssignments( node, targetInfo, in traits );
            return;
        }

        if ( Options.IsUpdateFromEnabled )
        {
            node = CreateSimplifiedUpdateFrom( targetInfo, node, updateVisitor );
            traits = ExtractDataSourceTraits( node.DataSource.Traits );
            Assume.True( IsValidUpdateFromStatement( node.DataSource, in traits ) );
            VisitSimplifiedUpdateFrom( node, targetInfo, in traits );
            return;
        }

        VisitUpdateWithComplexDataSourceAndComplexAssignments( node, targetInfo, updateVisitor.GetIndexesOfComplexAssignments() );
    }

    /// <inheritdoc />
    public sealed override void VisitUpsert(SqlUpsertNode node)
    {
        if ( (Options.UpsertOptions & SqliteUpsertOptions.Supported) == SqliteUpsertOptions.Disabled )
            throw new UnrecognizedSqlNodeException( this, node );

        var conflictTarget = node.ConflictTarget.Count == 0
            && (Options.UpsertOptions & SqliteUpsertOptions.AllowEmptyConflictTarget) == SqliteUpsertOptions.Disabled
                ? ExtractUpsertConflictTargets( node )
                : node.ConflictTarget;

        switch ( node.Source.NodeType )
        {
            case SqlNodeType.DataSourceQuery:
            {
                VisitUpsertFromDataSourceQuery( node, conflictTarget, ReinterpretCast.To<SqlDataSourceQueryExpressionNode>( node.Source ) );
                break;
            }
            case SqlNodeType.CompoundQuery:
            {
                VisitUpsertFromCompoundQuery( node, conflictTarget, ReinterpretCast.To<SqlCompoundQueryExpressionNode>( node.Source ) );
                break;
            }
            default:
            {
                VisitUpsertFromGenericSource( node, conflictTarget );
                break;
            }
        }
    }

    /// <inheritdoc />
    public sealed override void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        var traits = ExtractDataSourceTraits( node.DataSource.Traits );

        if ( IsValidSingleTableUpdateOrDeleteStatement( node.DataSource, in traits ) )
        {
            VisitDeleteFromWithSimpleDataSource( node, in traits );
            return;
        }

        var targetInfo = ExtractTargetInfo( node );
        VisitDeleteFromWithComplexDataSource( node, targetInfo, in traits );
    }

    /// <inheritdoc />
    public override void VisitTruncate(SqlTruncateNode node)
    {
        Context.Sql.Append( "DELETE" ).AppendSpace().Append( "FROM" ).AppendSpace();
        AppendDelimitedRecordSetName( node.Table );
        Context.Sql.AppendSemicolon();
        Context.AppendIndent();
        Context.Sql.Append( "DELETE" ).AppendSpace().Append( "FROM" ).AppendSpace();
        if ( node.Table.Info.IsTemporary )
            Context.Sql.Append( "temp" ).AppendDot();

        AppendDelimitedName( "SQLITE_SEQUENCE" );
        Context.Sql.AppendSpace().Append( "WHERE" ).AppendSpace();
        AppendDelimitedName( "name" );
        Context.Sql.AppendSpace().Append( '=' ).AppendSpace().Append( '\'' );

        if ( node.Table.IsAliased )
            Context.Sql.Append( node.Table.Alias );
        else
            AppendSchemaObjectName( node.Table.Info.Name.Schema, node.Table.Info.Name.Object );

        Context.Sql.Append( '\'' );
    }

    /// <inheritdoc />
    public override void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        var typeDefinition = node.TypeDefinition ?? TypeDefinitions.GetByType( node.Type.UnderlyingType );
        AppendDelimitedName( node.Name );
        Context.Sql.AppendSpace().Append( typeDefinition.DataType.Name );

        if ( ! node.Type.IsNullable )
            Context.Sql.AppendSpace().Append( "NOT" ).AppendSpace().Append( "NULL" );

        if ( node.DefaultValue is not null )
        {
            Context.Sql.AppendSpace().Append( "DEFAULT" ).AppendSpace();
            VisitChildWrappedInParentheses( node.DefaultValue );
        }

        if ( node.Computation is not null )
        {
            var storage = node.Computation.Value.Storage == SqlColumnComputationStorage.Virtual ? "VIRTUAL" : "STORED";
            Context.Sql.AppendSpace().Append( "GENERATED" ).AppendSpace().Append( "ALWAYS" ).AppendSpace().Append( "AS" ).AppendSpace();
            VisitChildWrappedInParentheses( node.Computation.Value.Expression );
            Context.Sql.AppendSpace().Append( storage );
        }
    }

    /// <inheritdoc />
    public override void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node)
    {
        Context.Sql.Append( "CONSTRAINT" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.Name );
        Context.Sql.AppendSpace().Append( "PRIMARY" ).AppendSpace().Append( "KEY" ).AppendSpace().Append( '(' );

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
        Context.Sql.Append( "CONSTRAINT" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.Name );
        Context.Sql.AppendSpace().Append( "FOREIGN" ).AppendSpace().Append( "KEY" ).AppendSpace().Append( '(' );

        if ( node.Columns.Count > 0 )
        {
            foreach ( var column in node.Columns )
            {
                AppendDelimitedName( column.Name );
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
                AppendDelimitedName( column.Name );
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
        Context.Sql.Append( "CONSTRAINT" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.Name );
        Context.Sql.AppendSpace().Append( "CHECK" ).AppendSpace();
        VisitChildWrappedInParentheses( node.Condition );
    }

    /// <inheritdoc />
    public override void VisitCreateTable(SqlCreateTableNode node)
    {
        using ( TempIgnoreAllRecordSets() )
        {
            Context.Sql.Append( "CREATE" ).AppendSpace().Append( "TABLE" ).AppendSpace();
            if ( node.IfNotExists )
                Context.Sql.Append( "IF" ).AppendSpace().Append( "NOT" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

            AppendDelimitedRecordSetInfo( node.Info );
            Context.Sql.AppendSpace().Append( '(' );
            VisitCreateTableDefinition( node );
            Context.AppendIndent().Append( ')' ).AppendSpace().Append( "WITHOUT" ).AppendSpace().Append( "ROWID" );

            if ( Options.IsStrictModeEnabled )
                Context.Sql.AppendComma().AppendSpace().Append( "STRICT" );
        }
    }

    /// <inheritdoc />
    public override void VisitCreateView(SqlCreateViewNode node)
    {
        if ( node.ReplaceIfExists )
        {
            VisitDropView( node.ToDropView( ifExists: true ) );
            Context.Sql.AppendSemicolon();
            Context.AppendIndent();
        }

        Context.Sql.Append( "CREATE" ).AppendSpace().Append( "VIEW" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Info );
        Context.Sql.AppendSpace().Append( "AS" );
        Context.AppendIndent();
        this.Visit( node.Source );
    }

    /// <inheritdoc />
    public override void VisitCreateIndex(SqlCreateIndexNode node)
    {
        using ( TempIgnoreAllRecordSets() )
        {
            if ( node.ReplaceIfExists )
            {
                VisitDropIndex( node.ToDropIndex( ifExists: true ) );
                Context.Sql.AppendSemicolon();
                Context.AppendIndent();
            }

            Context.Sql.Append( "CREATE" ).AppendSpace();
            if ( node.IsUnique )
                Context.Sql.Append( "UNIQUE" ).AppendSpace();

            Context.Sql.Append( "INDEX" ).AppendSpace();
            if ( node.Table.Info.IsTemporary )
            {
                AppendDelimitedTemporaryObjectName( node.Name.Object );
                Context.Sql.AppendSpace().Append( "ON" ).AppendSpace();
                AppendDelimitedSchemaObjectName( node.Table.Info.Name );
            }
            else
            {
                AppendDelimitedSchemaObjectName( node.Name );
                Context.Sql.AppendSpace().Append( "ON" ).AppendSpace();
                AppendDelimitedRecordSetName( node.Table );
            }

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
    }

    /// <inheritdoc />
    public override void VisitRenameTable(SqlRenameTableNode node)
    {
        Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendSpace().Append( "RENAME" ).AppendSpace().Append( "TO" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.NewName );
    }

    /// <inheritdoc />
    public override void VisitRenameColumn(SqlRenameColumnNode node)
    {
        Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendSpace().Append( "RENAME" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        AppendDelimitedName( node.OldName );
        Context.Sql.AppendSpace().Append( "TO" ).AppendSpace();
        AppendDelimitedName( node.NewName );
    }

    /// <inheritdoc />
    public override void VisitAddColumn(SqlAddColumnNode node)
    {
        Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendSpace().Append( "ADD" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        VisitColumnDefinition( node.Definition );
    }

    /// <inheritdoc />
    public override void VisitDropColumn(SqlDropColumnNode node)
    {
        Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendSpace().Append( "DROP" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
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

        if ( node.Table.IsTemporary )
            AppendDelimitedTemporaryObjectName( node.Name.Object );
        else
            AppendDelimitedSchemaObjectName( node.Name );
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public sealed override void AppendDelimitedTemporaryObjectName(string name)
    {
        Context.Sql.Append( "temp" ).AppendDot();
        AppendDelimitedName( name );
    }

    /// <inheritdoc />
    public sealed override void AppendDelimitedSchemaObjectName(string schemaName, string objName)
    {
        Context.Sql.Append( BeginNameDelimiter );
        AppendSchemaObjectName( schemaName, objName );
        Context.Sql.Append( EndNameDelimiter );
    }

    /// <inheritdoc />
    protected override void AddContextParameter(SqlParameterNode node)
    {
        if ( Options.ArePositionalParametersEnabled )
            base.AddContextParameter( node );
        else
            Context.AddParameter( node.Name, node.Type, index: null );
    }

    /// <summary>
    /// Appends schema object's name to <see cref="SqlNodeInterpreter.Context"/>.
    /// </summary>
    /// <param name="schemaName">Schema name to append.</param>
    /// <param name="objName">Object name to append.</param>
    protected void AppendSchemaObjectName(string schemaName, string objName)
    {
        if ( schemaName.Length > 0 )
            Context.Sql.Append( schemaName ).Append( '_' );

        Context.Sql.Append( objName );
    }

    /// <inheritdoc />
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

    /// <summary>
    /// Visits a simple SQL aggregate function node.
    /// </summary>
    /// <param name="functionName">Name of the aggregate function.</param>
    /// <param name="node">SQL aggregate function expression node.</param>
    protected void VisitSimpleAggregateFunction(string functionName, SqlAggregateFunctionExpressionNode node)
    {
        var traits = ExtractAggregateFunctionTraits( node.Traits );
        Context.Sql.Append( functionName );
        VisitAggregateFunctionArgumentsAndTraits( node.Arguments, traits );
    }

    /// <summary>
    /// Sequentially visits all arguments and traits of a simple SQL aggregate function node.
    /// </summary>
    /// <param name="arguments">Collection of arguments to visit.</param>
    /// <param name="traits">Collection of traits to visit.</param>
    protected void VisitAggregateFunctionArgumentsAndTraits(ReadOnlyArray<SqlExpressionNode> arguments, SqlAggregateFunctionTraits traits)
    {
        Context.Sql.Append( '(' );

        if ( arguments.Count > 0 )
        {
            using ( Context.TempIndentIncrease() )
            {
                if ( traits.Distinct is not null )
                {
                    VisitDistinctTrait( traits.Distinct );
                    Context.Sql.AppendSpace();
                }

                foreach ( var arg in arguments )
                {
                    VisitChild( arg );
                    Context.Sql.AppendComma().AppendSpace();
                }
            }

            Context.Sql.ShrinkBy( 2 );
        }

        if ( traits.Ordering.Count > 0 && Options.IsAggregateFunctionOrderingEnabled )
        {
            if ( arguments.Count > 0 )
                Context.Sql.AppendSpace();

            Context.Sql.Append( "ORDER" ).AppendSpace().Append( "BY" ).AppendSpace();
            foreach ( var ordering in traits.Ordering )
            {
                foreach ( var orderBy in ordering )
                {
                    VisitOrderBy( orderBy );
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

        if ( traits.Window is not null )
        {
            Context.Sql.AppendSpace().Append( "OVER" ).AppendSpace();
            AppendDelimitedName( traits.Window.Name );
        }
    }

    /// <summary>
    /// Visits optional <see cref="SqlExpressionNode"/> values representing <paramref name="limit"/> and <paramref name="offset"/>.
    /// </summary>
    /// <param name="limit">Limit value to visit.</param>
    /// <param name="offset">Offset value to visit.</param>
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

    /// <summary>
    /// Visits an <see cref="SqlInsertIntoNode"/> with source of <see cref="SqlDataSourceQueryExpressionNode"/> type.
    /// </summary>
    /// <param name="node">Insert into node to visit.</param>
    /// <param name="query">Source query.</param>
    protected virtual void VisitInsertIntoFromDataSourceQuery(SqlInsertIntoNode node, SqlDataSourceQueryExpressionNode query)
    {
        Assume.Equals( node.Source, query );
        var traits = ExtractDataSourceTraits( ExtractDataSourceTraits( query.DataSource.Traits ), query.Traits );
        VisitDataSourceBeforeTraits( in traits );
        VisitInsertIntoFields( node );
        Context.AppendIndent();
        VisitDataSourceQuerySelection( query, in traits );
        VisitDataSource( query.DataSource );
        VisitDataSourceAfterTraits( in traits );
    }

    /// <summary>
    /// Visits an <see cref="SqlInsertIntoNode"/> with source of <see cref="SqlCompoundQueryExpressionNode"/> type.
    /// </summary>
    /// <param name="node">Insert into node to visit.</param>
    /// <param name="query">Source query.</param>
    protected virtual void VisitInsertIntoFromCompoundQuery(SqlInsertIntoNode node, SqlCompoundQueryExpressionNode query)
    {
        Assume.Equals( node.Source, query );
        var traits = ExtractQueryTraits( query.Traits );
        VisitQueryBeforeTraits( in traits );
        VisitInsertIntoFields( node );
        Context.AppendIndent();
        VisitCompoundQueryComponents( query, in traits );
        VisitQueryAfterTraits( in traits );
    }

    /// <summary>
    /// Visits an <see cref="SqlInsertIntoNode"/> with source of non-query type.
    /// </summary>
    /// <param name="node">Insert into node to visit.</param>
    protected virtual void VisitInsertIntoFromGenericSource(SqlInsertIntoNode node)
    {
        VisitInsertIntoFields( node );
        Context.AppendIndent();
        this.Visit( node.Source );
    }

    /// <summary>
    /// Visits an <see cref="SqlUpdateNode"/> with a simple data source.
    /// </summary>
    /// <param name="node">Update node to visit.</param>
    /// <param name="traits">Collection of traits.</param>
    /// <remarks>See <see cref="IsValidSingleTableUpdateOrDeleteStatement"/> for more information.</remarks>
    protected virtual void VisitUpdateWithSimpleDataSource(SqlUpdateNode node, in SqlDataSourceTraits traits)
    {
        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "UPDATE" ).AppendSpace();
        this.Visit( node.DataSource.From );
        VisitUpdateAssignmentRange( node.Assignments );
        VisitDataSourceAfterTraits( in traits );
    }

    /// <summary>
    /// Visits an <see cref="SqlUpdateNode"/> using an <b>UPDATE FROM</b> statement.
    /// </summary>
    /// <param name="node">Update node to visit.</param>
    /// <param name="traits">Collection of traits.</param>
    /// <remarks>See <see cref="IsValidUpdateFromStatement"/> for more information.</remarks>
    protected virtual void VisitUpdateFrom(SqlUpdateNode node, in SqlDataSourceTraits traits)
    {
        Assume.ContainsExactly( node.DataSource.Joins, 1 );
        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "UPDATE" ).AppendSpace();
        this.Visit( node.DataSource.From );
        VisitUpdateAssignmentRange( node.Assignments );

        var join = node.DataSource.Joins[0];
        Context.AppendIndent().Append( "FROM" ).AppendSpace();
        this.Visit( join.InnerRecordSet );

        if ( join.JoinType == SqlJoinType.Cross )
            VisitDataSourceAfterTraits( in traits );
        else
        {
            Assume.Equals( join.JoinType, SqlJoinType.Inner );
            var traitsWithJoinFilter = traits.Filter is null
                ? traits with { Filter = join.OnExpression }
                : traits with { Filter = join.OnExpression.And( traits.Filter ) };

            VisitDataSourceAfterTraits( in traitsWithJoinFilter );
        }
    }

    /// <summary>
    /// Visits an <see cref="SqlUpdateNode"/> using an <b>UPDATE FROM</b> statement simplified
    /// from a complex data source with complex value assignments.
    /// </summary>
    /// <param name="node">Update node to visit.</param>
    /// <param name="targetInfo"><see cref="SqlNodeInterpreter.ChangeTargetInfo"/> instance associated with the operation.</param>
    /// <param name="traits">Collection of traits.</param>
    protected virtual void VisitSimplifiedUpdateFrom(SqlUpdateNode node, ChangeTargetInfo targetInfo, in SqlDataSourceTraits traits)
    {
        Assume.ContainsExactly( node.DataSource.Joins, 1 );
        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "UPDATE" ).AppendSpace();
        this.Visit( node.DataSource.From );

        using ( TempReplaceRecordSet( targetInfo.Target, targetInfo.BaseTarget ) )
            VisitUpdateAssignmentRange( node.Assignments );

        var join = node.DataSource.Joins[0];
        Assume.Equals( join.JoinType, SqlJoinType.Inner );
        Context.AppendIndent().Append( "FROM" ).AppendSpace();
        this.Visit( join.InnerRecordSet );

        var traitsWithJoinFilter = traits.Filter is null
            ? traits with { Filter = join.OnExpression }
            : traits with { Filter = join.OnExpression.And( traits.Filter ) };

        VisitDataSourceAfterTraits( in traitsWithJoinFilter );
    }

    /// <summary>
    /// Visits an <see cref="SqlUpdateNode"/> with a complex data source with simple value assignments.
    /// </summary>
    /// <param name="node">Update node to visit.</param>
    /// <param name="targetInfo"><see cref="SqlNodeInterpreter.ChangeTargetInfo"/> instance associated with the operation.</param>
    /// <param name="traits">Collection of traits.</param>
    protected virtual void VisitUpdateWithComplexDataSourceAndSimpleAssignments(
        SqlUpdateNode node,
        ChangeTargetInfo targetInfo,
        in SqlDataSourceTraits traits)
    {
        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "UPDATE" ).AppendSpace();
        AppendDelimitedRecordSetName( targetInfo.BaseTarget );

        using ( TempReplaceRecordSet( targetInfo.Target, targetInfo.BaseTarget ) )
            VisitUpdateAssignmentRange( node.Assignments );

        var filter = CreateComplexDeleteOrUpdateFilter( targetInfo, node.DataSource );
        Context.AppendIndent();
        VisitFilterTrait( filter );
    }

    /// <summary>
    /// Visits an <see cref="SqlUpdateNode"/> simplified from a complex data source with complex value assignments.
    /// </summary>
    /// <param name="node">Update node to visit.</param>
    /// <param name="targetInfo"><see cref="SqlNodeInterpreter.ChangeTargetInfo"/> instance associated with the operation.</param>
    /// <param name="indexesOfComplexAssignments">Collection of 0-based indexes of all complex value assignments.</param>
    protected virtual void VisitUpdateWithComplexDataSourceAndComplexAssignments(
        SqlUpdateNode node,
        ChangeTargetInfo targetInfo,
        ReadOnlySpan<int> indexesOfComplexAssignments)
    {
        node = CreateSimplifiedUpdateWithComplexAssignments( targetInfo, node, indexesOfComplexAssignments );
        var traits = ExtractDataSourceTraits( node.DataSource.Traits );
        Assume.True( IsValidSingleTableUpdateOrDeleteStatement( node.DataSource, in traits ) );

        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "UPDATE" ).AppendSpace();
        AppendDelimitedRecordSetName( node.DataSource.From );

        using ( TempReplaceRecordSet( targetInfo.Target, targetInfo.BaseTarget ) )
            VisitUpdateAssignmentRange( node.Assignments );

        VisitDataSourceAfterTraits( in traits );
    }

    /// <summary>
    /// Visits an <see cref="SqlUpsertNode"/> with source of <see cref="SqlDataSourceQueryExpressionNode"/> type.
    /// </summary>
    /// <param name="node">Upsert node to visit.</param>
    /// <param name="conflictTarget">Collection of data fields from the table that define the insertion conflict target.</param>
    /// <param name="query">Source query.</param>
    protected virtual void VisitUpsertFromDataSourceQuery(
        SqlUpsertNode node,
        ReadOnlyArray<SqlDataFieldNode> conflictTarget,
        SqlDataSourceQueryExpressionNode query)
    {
        Assume.Equals( node.Source, query );
        var traits = ExtractDataSourceTraits( ExtractDataSourceTraits( query.DataSource.Traits ), query.Traits );
        if ( traits.Filter is null )
            traits = traits with { Filter = SqlNode.True() };

        VisitDataSourceBeforeTraits( in traits );
        VisitInsertIntoFields( node.RecordSet, node.InsertDataFields );
        Context.AppendIndent();
        VisitDataSourceQuerySelection( query, in traits );
        VisitDataSource( query.DataSource );
        VisitDataSourceAfterTraits( in traits );
        AppendUpsertOnConflict( node, conflictTarget );
    }

    /// <summary>
    /// Visits an <see cref="SqlUpsertNode"/> with source of <see cref="SqlCompoundQueryExpressionNode"/> type.
    /// </summary>
    /// <param name="node">Upsert node to visit.</param>
    /// <param name="conflictTarget">Collection of data fields from the table that define the insertion conflict target.</param>
    /// <param name="query">Source query.</param>
    protected virtual void VisitUpsertFromCompoundQuery(
        SqlUpsertNode node,
        ReadOnlyArray<SqlDataFieldNode> conflictTarget,
        SqlCompoundQueryExpressionNode query)
    {
        Assume.Equals( node.Source, query );
        var traits = ExtractQueryTraits( query.Traits );
        VisitQueryBeforeTraits( in traits );
        VisitInsertIntoFields( node.RecordSet, node.InsertDataFields );

        Context.AppendIndent().Append( "SELECT" ).AppendSpace().Append( '*' ).AppendSpace().Append( "FROM" ).AppendSpace().Append( '(' );
        using ( Context.TempIndentIncrease() )
        {
            Context.AppendIndent();
            VisitCompoundQueryComponents( query, in traits );
            VisitQueryAfterTraits( in traits );
        }

        Context.AppendIndent().Append( ')' );
        AppendDelimitedAlias( "source" );
        Context.AppendIndent().Append( "WHERE" ).AppendSpace().Append( "TRUE" );

        AppendUpsertOnConflict( node, conflictTarget );
    }

    /// <summary>
    /// Visits an <see cref="SqlUpsertNode"/> with source of non-query type.
    /// </summary>
    /// <param name="node">Upsert node to visit.</param>
    /// <param name="conflictTarget">Collection of data fields from the table that define the insertion conflict target.</param>
    protected virtual void VisitUpsertFromGenericSource(SqlUpsertNode node, ReadOnlyArray<SqlDataFieldNode> conflictTarget)
    {
        VisitInsertIntoFields( node.RecordSet, node.InsertDataFields );
        Context.AppendIndent();
        this.Visit( node.Source );
        AppendUpsertOnConflict( node, conflictTarget );
    }

    /// <summary>
    /// Appends an SQL upsert's conflict target to <see cref="SqlNodeInterpreter.Context"/>.
    /// </summary>
    /// <param name="node">SQL upsert node.</param>
    /// <param name="conflictTarget">Conflict target to append.</param>
    protected void AppendUpsertOnConflict(SqlUpsertNode node, ReadOnlyArray<SqlDataFieldNode> conflictTarget)
    {
        Context.AppendIndent().Append( "ON" ).AppendSpace().Append( "CONFLICT" ).AppendSpace();
        if ( conflictTarget.Count > 0 )
        {
            Context.Sql.Append( '(' );
            foreach ( var target in conflictTarget )
            {
                AppendDelimitedName( target.Name );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 ).Append( ')' ).AppendSpace();
        }

        Context.Sql.Append( "DO" ).AppendSpace().Append( "UPDATE" );

        _upsertUpdateSourceReplacement ??= SqlNode.RawRecordSet( SqliteHelpers.UpsertExcludedRecordSetName );
        using ( TempReplaceRecordSet( node.UpdateSource, _upsertUpdateSourceReplacement ) )
            VisitUpdateAssignmentRange( node.UpdateAssignments );
    }

    /// <summary>
    /// Visits an <see cref="SqlDeleteFromNode"/> with a simple data source.
    /// </summary>
    /// <param name="node">Delete from node to visit.</param>
    /// <param name="traits">Collection of traits.</param>
    /// <remarks>See <see cref="IsValidSingleTableUpdateOrDeleteStatement"/> for more information.</remarks>
    protected virtual void VisitDeleteFromWithSimpleDataSource(SqlDeleteFromNode node, in SqlDataSourceTraits traits)
    {
        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "DELETE" ).AppendSpace().Append( "FROM" ).AppendSpace();
        this.Visit( node.DataSource.From );
        VisitDataSourceAfterTraits( in traits );
    }

    /// <summary>
    /// Visits an <see cref="SqlDeleteFromNode"/> with a complex data source.
    /// </summary>
    /// <param name="node">Delete from node to visit.</param>
    /// <param name="targetInfo"><see cref="SqlNodeInterpreter.ChangeTargetInfo"/> instance associated with the operation.</param>
    /// <param name="traits">Collection of traits.</param>
    protected virtual void VisitDeleteFromWithComplexDataSource(
        SqlDeleteFromNode node,
        ChangeTargetInfo targetInfo,
        in SqlDataSourceTraits traits)
    {
        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "DELETE" ).AppendSpace().Append( "FROM" ).AppendSpace();
        AppendDelimitedRecordSetName( targetInfo.BaseTarget );

        var filter = CreateComplexDeleteOrUpdateFilter( targetInfo, node.DataSource );
        Context.AppendIndent();
        VisitFilterTrait( filter );
    }

    /// <inheritdoc />
    protected override void VisitDataSourceAfterTraits(in SqlDataSourceTraits traits)
    {
        base.VisitDataSourceAfterTraits( in traits );
        VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
    }

    /// <inheritdoc />
    protected override void VisitQueryAfterTraits(in SqlQueryTraits traits)
    {
        base.VisitQueryAfterTraits( in traits );
        VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
    }

    /// <summary>
    /// Checks whether or not a data source of an <see cref="SqlUpdateNode"/> or <see cref="SqlDeleteFromNode"/> is considered simple.
    /// </summary>
    /// <param name="node">Data source node to check.</param>
    /// <param name="traits">Collection of traits.</param>
    /// <returns><b>true</b> when data source is considered valid, otherwise <b>false</b>.</returns>
    /// <remarks>
    /// By default, data source is considered to be simple when it contains a single record set. In addition, it may only be decorated
    /// with common table expressions and a filter. Sorting, limit and offset traits are allowed as well,
    /// unless the <see cref="SqliteNodeInterpreterOptions.IsUpdateOrDeleteLimitEnabled"/> setting of the <see cref="Options"/> is disabled.
    /// </remarks>
    [Pure]
    protected virtual bool IsValidSingleTableUpdateOrDeleteStatement(SqlDataSourceNode node, in SqlDataSourceTraits traits)
    {
        return node.RecordSets.Count == 1
            && traits.Distinct is null
            && traits.Aggregations.Count == 0
            && traits.AggregationFilter is null
            && traits.Windows.Count == 0
            && (Options.IsUpdateOrDeleteLimitEnabled || (traits.Ordering.Count == 0 && traits.Limit is null && traits.Offset is null))
            && traits.Custom.Count == 0;
    }

    /// <summary>
    /// Checks whether or not a data source of an <see cref="SqlUpdateNode"/> is considered valid for an <b>UPDATE FROM</b> statement.
    /// </summary>
    /// <param name="node">Data source node to check.</param>
    /// <param name="traits">Collection of traits.</param>
    /// <returns><b>true</b> when data source is considered valid, otherwise <b>false</b>.</returns>
    /// <remarks>
    /// By default, <see cref="SqliteNodeInterpreterOptions.IsUpdateFromEnabled"/> setting of the <see cref="Options"/> must be enabled.
    /// Then, data source is considered to be valid when it contains exactly two record sets, and only with <see cref="SqlJoinType.Inner"/>
    /// or <see cref="SqlJoinType.Cross"/> join. In addition, it may only be decorated with common table expressions and a filter.
    /// Sorting, limit and offset traits are allowed as well,
    /// unless the <see cref="SqliteNodeInterpreterOptions.IsUpdateOrDeleteLimitEnabled"/> setting of the <see cref="Options"/> is disabled.
    /// </remarks>
    [Pure]
    protected virtual bool IsValidUpdateFromStatement(SqlDataSourceNode node, in SqlDataSourceTraits traits)
    {
        return Options.IsUpdateFromEnabled
            && node.RecordSets.Count == 2
            && node.Joins[0].JoinType is SqlJoinType.Inner or SqlJoinType.Cross
            && traits.Distinct is null
            && traits.Aggregations.Count == 0
            && traits.AggregationFilter is null
            && traits.Windows.Count == 0
            && (Options.IsUpdateOrDeleteLimitEnabled || (traits.Ordering.Count == 0 && traits.Limit is null && traits.Offset is null))
            && traits.Custom.Count == 0;
    }

    /// <summary>
    /// Creates a simplified version of an <see cref="SqlUpdateNode"/> with a complex data source and complex value assignments.
    /// </summary>
    /// <param name="targetInfo"><see cref="SqlNodeInterpreter.ChangeTargetInfo"/> instance associated with the operation.</param>
    /// <param name="node">Source update node.</param>
    /// <param name="indexesOfComplexAssignments">Collection of 0-based indexes of all complex value assignments.</param>
    /// <returns>New <see cref="SqlUpdateNode"/> instance.</returns>
    [Pure]
    protected static SqlUpdateNode CreateSimplifiedUpdateWithComplexAssignments(
        ChangeTargetInfo targetInfo,
        SqlUpdateNode node,
        ReadOnlySpan<int> indexesOfComplexAssignments)
    {
        Ensure.IsNotNull( targetInfo.Target.Alias );

        var updateTraits = Chain<SqlTraitNode>.Empty;
        var cteTraits = Chain<SqlTraitNode>.Empty;
        foreach ( var trait in node.DataSource.Traits )
        {
            if ( trait.NodeType == SqlNodeType.CommonTableExpressionTrait )
                updateTraits = updateTraits.Extend( trait );
            else
                cteTraits = cteTraits.Extend( trait );
        }

        var cteIdentityFieldNames = new string[targetInfo.IdentityColumnNames.Length];
        var cteComplexAssignmentFieldNames = indexesOfComplexAssignments.Length > 0
            ? new string[indexesOfComplexAssignments.Length]
            : Array.Empty<string>();

        var cteSelection = new SqlSelectNode[targetInfo.IdentityColumnNames.Length + indexesOfComplexAssignments.Length];
        var assignments = node.Assignments.AsSpan().ToArray();

        var i = 0;
        foreach ( var name in targetInfo.IdentityColumnNames )
        {
            var fieldName = $"ID_{name}_{i}";
            cteIdentityFieldNames[i] = fieldName;
            cteSelection[i] = targetInfo.Target.GetUnsafeField( name ).As( fieldName );
            ++i;
        }

        foreach ( var assignmentIndex in indexesOfComplexAssignments )
        {
            var assignment = assignments[assignmentIndex];
            var fieldName = $"VAL_{assignment.DataField.Name}_{assignmentIndex}";
            cteComplexAssignmentFieldNames[i - cteIdentityFieldNames.Length] = fieldName;
            cteSelection[i++] = assignment.Value.As( fieldName );
        }

        var cte = node.DataSource.SetTraits( cteTraits ).Select( cteSelection ).ToCte( $"_{Guid.NewGuid():N}" );
        var cteDataSource = cte.RecordSet.ToDataSource();

        var pkBaseColumnNode = targetInfo.BaseTarget.GetUnsafeField( targetInfo.IdentityColumnNames[0] );
        var pkCteColumnNode = cteDataSource.From.GetUnsafeField( cteIdentityFieldNames[0] );
        var pkFilter = pkBaseColumnNode == pkCteColumnNode;

        i = 1;
        foreach ( var name in targetInfo.IdentityColumnNames.AsSpan( 1 ) )
        {
            pkBaseColumnNode = targetInfo.BaseTarget.GetUnsafeField( name );
            pkCteColumnNode = cteDataSource.From.GetUnsafeField( cteIdentityFieldNames[i++] );
            pkFilter = pkFilter.And( pkBaseColumnNode == pkCteColumnNode );
        }

        var pkFilterTrait = SqlNode.FilterTrait( pkFilter, isConjunction: true );
        var filteredCteDataSource = cteDataSource.SetTraits(
            Chain.Create<SqlTraitNode>( pkFilterTrait ).Extend( SqlNode.LimitTrait( SqlNode.Literal( 1 ) ) ) );

        i = 0;
        foreach ( var assignmentIndex in indexesOfComplexAssignments )
        {
            var assignment = assignments[assignmentIndex];
            var selection = filteredCteDataSource.From.GetUnsafeField( cteComplexAssignmentFieldNames[i++] ).AsSelf();
            assignments[assignmentIndex] = assignment.DataField.Assign( filteredCteDataSource.Select( selection ) );
        }

        SqlConditionNode updateFilter = cteIdentityFieldNames.Length == 1
            ? targetInfo.BaseTarget.GetUnsafeField( targetInfo.IdentityColumnNames[0] )
                .InQuery( cteDataSource.Select( cteDataSource.From.GetUnsafeField( cteIdentityFieldNames[0] ).AsSelf() ) )
            : cteDataSource.AddTrait( pkFilterTrait ).Exists();

        updateTraits = updateTraits
            .Extend( SqlNode.CommonTableExpressionTrait( cte ) )
            .Extend( SqlNode.FilterTrait( updateFilter, isConjunction: true ) );

        return targetInfo.BaseTarget.ToDataSource().SetTraits( updateTraits ).ToUpdate( assignments );
    }
}
