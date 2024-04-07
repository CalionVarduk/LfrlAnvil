using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.PostgreSql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql;

public class PostgreSqlNodeInterpreter : SqlNodeInterpreter
{
    private SqlRecordSetNode? _upsertUpdateSourceReplacement;

    public PostgreSqlNodeInterpreter(PostgreSqlNodeInterpreterOptions options, SqlNodeInterpreterContext context)
        : base( context, beginNameDelimiter: '"', endNameDelimiter: '"' )
    {
        Options = options;
        _upsertUpdateSourceReplacement = null;
        TypeDefinitions = Options.TypeDefinitions ?? new PostgreSqlColumnTypeDefinitionProviderBuilder().Build();
    }

    public PostgreSqlNodeInterpreterOptions Options { get; }
    public PostgreSqlColumnTypeDefinitionProvider TypeDefinitions { get; }

    public override void VisitLiteral(SqlLiteralNode node)
    {
        var sql = node.GetSql( TypeDefinitions );
        Context.Sql.Append( sql );
    }

    public override void VisitBitwiseXor(SqlBitwiseXorExpressionNode node)
    {
        VisitInfixBinaryOperator( node.Left, symbol: "#", node.Right );
    }

    public override void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node)
    {
        Context.Sql.Append( "CURRENT_DATE" );
    }

    public override void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node)
    {
        Context.Sql.Append( "LOCALTIME" );
    }

    public override void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node)
    {
        Context.Sql.Append( "LOCALTIMESTAMP" );
    }

    public override void VisitCurrentUtcDateTimeFunction(SqlCurrentUtcDateTimeFunctionExpressionNode node)
    {
        Context.Sql.Append( "CURRENT_TIMESTAMP" );
    }

    public override void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        Context.Sql.Append( "CAST" ).Append( '(' ).Append( "EXTRACT" ).Append( '(' );
        Context.Sql.Append( "EPOCH" ).AppendSpace().Append( "FROM" ).AppendSpace().Append( "CURRENT_TIMESTAMP" ).Append( ')' );
        Context.Sql.AppendSpace().Append( '*' ).AppendSpace().Append( '1' ).Append( '0', repeatCount: 7 );
        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace().Append( PostgreSqlDataType.Int8.Name ).Append( ')' );
    }

    public override void VisitExtractDateFunction(SqlExtractDateFunctionExpressionNode node)
    {
        VisitChild( node.Arguments[0] );
        AppendPostgreStyleCast( PostgreSqlDataType.Date );
    }

    public override void VisitExtractTimeOfDayFunction(SqlExtractTimeOfDayFunctionExpressionNode node)
    {
        VisitChild( node.Arguments[0] );
        AppendPostgreStyleCast( PostgreSqlDataType.Time );
    }

    public override void VisitExtractDayFunction(SqlExtractDayFunctionExpressionNode node)
    {
        var unit = node.Unit switch
        {
            SqlTemporalUnit.Year => "DOY",
            SqlTemporalUnit.Month => "DAY",
            _ => "DOW"
        };

        Context.Sql.Append( "EXTRACT" ).Append( '(' ).Append( unit ).AppendSpace().Append( "FROM" ).AppendSpace();
        VisitChild( node.Arguments[0] );
        Context.Sql.Append( ')' );
        AppendPostgreStyleCast( PostgreSqlDataType.Int2 );
    }

    public override void VisitExtractTemporalUnitFunction(SqlExtractTemporalUnitFunctionExpressionNode node)
    {
        var unit = node.Unit switch
        {
            SqlTemporalUnit.Year => "YEAR",
            SqlTemporalUnit.Month => "MONTH",
            SqlTemporalUnit.Week => "WEEK",
            SqlTemporalUnit.Day => "DAY",
            SqlTemporalUnit.Hour => "HOUR",
            SqlTemporalUnit.Minute => "MINUTE",
            SqlTemporalUnit.Second => "SECOND",
            SqlTemporalUnit.Millisecond => "MILLISECONDS",
            _ => "MICROSECONDS"
        };

        if ( node.Unit < SqlTemporalUnit.Second )
            Context.Sql.Append( '(' );

        Context.Sql.Append( "EXTRACT" ).Append( '(' ).Append( unit ).AppendSpace().Append( "FROM" ).AppendSpace();
        VisitChild( node.Arguments[0] );
        Context.Sql.Append( ')' );

        if ( node.Unit < SqlTemporalUnit.Second )
        {
            Context.Sql.AppendSpace().Append( '%' ).AppendSpace().Append( '1' );
            Context.Sql.Append( '0', repeatCount: node.Unit == SqlTemporalUnit.Millisecond ? 3 : 6 );

            if ( node.Unit == SqlTemporalUnit.Nanosecond )
                Context.Sql.AppendSpace().Append( '*' ).AppendSpace().Append( '1' ).Append( '0', repeatCount: 3 );

            Context.Sql.Append( ')' );
        }

        var type = node.Unit is < SqlTemporalUnit.Millisecond or SqlTemporalUnit.Year ? PostgreSqlDataType.Int4 : PostgreSqlDataType.Int2;
        AppendPostgreStyleCast( type );
    }

    public override void VisitTemporalAddFunction(SqlTemporalAddFunctionExpressionNode node)
    {
        var unit = node.Unit switch
        {
            SqlTemporalUnit.Year => "YEARS",
            SqlTemporalUnit.Month => "MONTHS",
            SqlTemporalUnit.Week => "WEEKS",
            SqlTemporalUnit.Day => "DAYS",
            SqlTemporalUnit.Hour => "HOURS",
            SqlTemporalUnit.Minute => "MINS",
            _ => "SECS"
        };

        VisitChild( node.Arguments[0] );
        Context.Sql.AppendSpace().Append( '+' ).AppendSpace();
        Context.Sql.Append( "MAKE_INTERVAL" ).Append( '(' ).Append( unit ).AppendSpace().Append( '=' ).Append( '>' ).AppendSpace();
        VisitChild( node.Arguments[1] );

        if ( node.Unit < SqlTemporalUnit.Second )
        {
            var zeroCount = -(( int )node.Unit - 3) * 3;
            Context.Sql.AppendSpace().Append( '/' ).AppendSpace();
            Context.Sql.Append( '1' ).Append( '0', repeatCount: zeroCount ).Append( '.' ).Append( '0' );
        }

        Context.Sql.Append( ')' );
    }

    public override void VisitTemporalDiffFunction(SqlTemporalDiffFunctionExpressionNode node)
    {
        if ( node.Unit < SqlTemporalUnit.Day )
        {
            Context.Sql.Append( "TRUNC" ).Append( '(' );
            Context.Sql.Append( "EXTRACT" ).Append( '(' ).Append( "EPOCH" ).AppendSpace().Append( "FROM" ).AppendSpace();
            VisitInfixBinaryOperator( node.Arguments[1], "-", node.Arguments[0] );
            Context.Sql.Append( ')' );

            if ( node.Unit < SqlTemporalUnit.Second )
            {
                var zeroCount = -(( int )node.Unit - 3) * 3;
                Context.Sql.AppendSpace().Append( '*' ).AppendSpace().Append( '1' ).Append( '0', repeatCount: zeroCount );
            }
            else if ( node.Unit > SqlTemporalUnit.Second )
            {
                Context.Sql.AppendSpace().Append( '/' ).AppendSpace();
                if ( node.Unit == SqlTemporalUnit.Minute )
                    Context.Sql.Append( '6' ).Append( '0' );
                else
                    Context.Sql.Append( '3' ).Append( '6' ).Append( '0', repeatCount: 2 );
            }

            Context.Sql.Append( ')' );
            AppendPostgreStyleCast( PostgreSqlDataType.Int8 );
            return;
        }

        if ( node.Unit < SqlTemporalUnit.Month )
        {
            if ( node.Unit == SqlTemporalUnit.Week )
                Context.Sql.Append( "TRUNC" ).Append( '(' );

            Context.Sql.Append( "EXTRACT" ).Append( '(' ).Append( "DAY" ).AppendSpace().Append( "FROM" ).AppendSpace();
            VisitChild( node.Arguments[1] );
            AppendPostgreStyleCast( PostgreSqlDataType.Timestamp );
            Context.Sql.AppendSpace().Append( '-' ).AppendSpace();
            VisitChild( node.Arguments[0] );
            AppendPostgreStyleCast( PostgreSqlDataType.Timestamp );
            Context.Sql.Append( ')' );

            if ( node.Unit == SqlTemporalUnit.Week )
                Context.Sql.AppendSpace().Append( '/' ).AppendSpace().Append( '7' ).Append( ')' );
        }
        else
        {
            if ( node.Unit == SqlTemporalUnit.Month )
                Context.Sql.Append( '(' );

            Context.Sql.Append( "EXTRACT" ).Append( '(' ).Append( "YEAR" ).AppendSpace().Append( "FROM" ).AppendSpace();
            Context.Sql.Append( "AGE" ).Append( '(' );
            VisitChild( node.Arguments[1] );
            Context.Sql.AppendComma().AppendSpace();
            VisitChild( node.Arguments[0] );
            Context.Sql.Append( ')', repeatCount: 2 );

            if ( node.Unit == SqlTemporalUnit.Month )
            {
                Context.Sql.AppendSpace().Append( '*' ).AppendSpace().Append( '1' ).Append( '2' );
                Context.Sql.AppendSpace().Append( '+' ).AppendSpace();
                Context.Sql.Append( "EXTRACT" ).Append( '(' ).Append( "MONTH" ).AppendSpace().Append( "FROM" ).AppendSpace();
                Context.Sql.Append( "AGE" ).Append( '(' );
                VisitChild( node.Arguments[1] );
                Context.Sql.AppendComma().AppendSpace();
                VisitChild( node.Arguments[0] );
                Context.Sql.Append( ')', repeatCount: 3 );
            }
        }

        AppendPostgreStyleCast( PostgreSqlDataType.Int4 );
    }

    public override void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node)
    {
        VisitSimpleFunction( "UUID_GENERATE_V4", node );
    }

    public override void VisitLengthFunction(SqlLengthFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CHAR_LENGTH", node );
    }

    public override void VisitByteLengthFunction(SqlByteLengthFunctionExpressionNode node)
    {
        VisitSimpleFunction( "LENGTH", node );
    }

    public override void VisitToLowerFunction(SqlToLowerFunctionExpressionNode node)
    {
        VisitSimpleFunction( "LOWER", node );
    }

    public override void VisitToUpperFunction(SqlToUpperFunctionExpressionNode node)
    {
        VisitSimpleFunction( "UPPER", node );
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
        VisitSimpleFunction( "BTRIM", node );
    }

    public override void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SUBSTR", node );
    }

    public override void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node)
    {
        VisitSimpleFunction( "REPLACE", node );
    }

    public override void VisitReverseFunction(SqlReverseFunctionExpressionNode node)
    {
        VisitSimpleFunction( "REVERSE", node );
    }

    public override void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node)
    {
        VisitSimpleFunction( "STRPOS", node );
    }

    public override void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node)
    {
        Context.Sql.Append( "CASE" ).AppendSpace().Append( "WHEN" ).AppendSpace().Append( "STRPOS" ).Append( '(' );
        Context.Sql.Append( "REVERSE" ).Append( '(' );
        VisitChild( node.Arguments[0] );
        Context.Sql.Append( ')' ).AppendComma().AppendSpace().Append( "REVERSE" ).Append( '(' );
        VisitChild( node.Arguments[1] );
        Context.Sql.Append( ')', repeatCount: 2 ).AppendSpace().Append( '=' ).AppendSpace().Append( '0' );
        Context.Sql.AppendSpace().Append( "THEN" ).AppendSpace().Append( '0' ).AppendSpace().Append( "ELSE" ).AppendSpace();
        Context.Sql.Append( "CHAR_LENGTH" ).Append( '(' );
        VisitChild( node.Arguments[0] );
        Context.Sql.Append( ')' ).AppendSpace().Append( '-' ).AppendSpace().Append( "STRPOS" ).Append( '(' );
        Context.Sql.Append( "REVERSE" ).Append( '(' );
        VisitChild( node.Arguments[0] );
        Context.Sql.Append( ')' ).AppendComma().AppendSpace().Append( "REVERSE" ).Append( '(' );
        VisitChild( node.Arguments[1] );
        Context.Sql.Append( ')', repeatCount: 2 ).AppendSpace().Append( '-' ).AppendSpace();
        Context.Sql.Append( "GREATEST" ).Append( '(' ).Append( "CHAR_LENGTH" ).Append( '(' );
        VisitChild( node.Arguments[1] );
        Context.Sql.Append( ')' ).AppendComma().AppendSpace().Append( "CASE" ).AppendSpace().Append( "WHEN" ).AppendSpace();
        VisitChild( node.Arguments[1] );
        Context.Sql.AppendSpace().Append( "IS" ).AppendSpace().Append( "NULL" ).AppendSpace().Append( "THEN" ).AppendSpace();
        Context.Sql.Append( "NULL" ).AppendSpace().Append( "ELSE" ).AppendSpace().Append( '1' ).AppendSpace();
        Context.Sql.Append( "END" ).Append( ')' ).AppendSpace().Append( '+' ).AppendSpace().Append( '2' ).AppendSpace().Append( "END" );
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

    public override void VisitRoundFunction(SqlRoundFunctionExpressionNode node)
    {
        VisitSimpleFunction( "ROUND", node );
    }

    public override void VisitPowerFunction(SqlPowerFunctionExpressionNode node)
    {
        VisitSimpleFunction( "POWER", node );
    }

    public override void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SQRT", node );
    }

    public override void VisitMinFunction(SqlMinFunctionExpressionNode node)
    {
        if ( node.Arguments.Count == 1 )
            VisitChild( node.Arguments[0] );
        else
            VisitSimpleFunction( "LEAST", node );
    }

    public override void VisitMaxFunction(SqlMaxFunctionExpressionNode node)
    {
        if ( node.Arguments.Count == 1 )
            VisitChild( node.Arguments[0] );
        else
            VisitSimpleFunction( "GREATEST", node );
    }

    public override void VisitNamedAggregateFunction(SqlNamedAggregateFunctionExpressionNode node)
    {
        var traits = ExtractAggregateFunctionTraits( node.Traits );
        AppendDelimitedSchemaObjectName( node.Name );
        VisitAggregateFunctionArgumentsAndTraits( node.Arguments, traits );
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
        var traits = ExtractAggregateFunctionTraits( node.Traits );
        Context.Sql.Append( "STRING_AGG" );
        VisitAggregateFunctionArgumentsAndTraits(
            node.Arguments.Count == 1 ? new[] { node.Arguments[0], SqlNode.Literal( "," ) } : node.Arguments,
            traits );
    }

    public override void VisitRowNumberWindowFunction(SqlRowNumberWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "ROW_NUMBER", node );
    }

    public override void VisitRankWindowFunction(SqlRankWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "RANK", node );
    }

    public override void VisitDenseRankWindowFunction(SqlDenseRankWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "DENSE_RANK", node );
    }

    public override void VisitCumulativeDistributionWindowFunction(SqlCumulativeDistributionWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "CUME_DIST", node );
    }

    public override void VisitNTileWindowFunction(SqlNTileWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "NTILE", node );
    }

    public override void VisitLagWindowFunction(SqlLagWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "LAG", node );
    }

    public override void VisitLeadWindowFunction(SqlLeadWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "LEAD", node );
    }

    public override void VisitFirstValueWindowFunction(SqlFirstValueWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "FIRST_VALUE", node );
    }

    public override void VisitLastValueWindowFunction(SqlLastValueWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "LAST_VALUE", node );
    }

    public override void VisitNthValueWindowFunction(SqlNthValueWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "NTH_VALUE", node );
    }

    public override void VisitJoinOn(SqlDataSourceJoinOnNode node)
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

    public override void VisitLimitTrait(SqlLimitTraitNode node)
    {
        Context.Sql.Append( "LIMIT" ).AppendSpace();
        VisitChild( node.Value );
    }

    public override void VisitOffsetTrait(SqlOffsetTraitNode node)
    {
        Context.Sql.Append( "OFFSET" ).AppendSpace();
        VisitChild( node.Value );
    }

    public override void VisitTypeCast(SqlTypeCastExpressionNode node)
    {
        Context.Sql.Append( "CAST" ).Append( '(' );
        VisitChild( node.Value );
        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace();

        var typeDefinition = node.TargetTypeDefinition ?? TypeDefinitions.GetByType( node.TargetType );
        Context.Sql.Append( typeDefinition.DataType.Name ).Append( ')' );
    }

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

    public sealed override void VisitUpdate(SqlUpdateNode node)
    {
        var traits = ExtractDataSourceTraits( node.DataSource.Traits );

        if ( IsValidSingleTableUpdateStatement( node.DataSource, in traits ) )
        {
            VisitUpdateWithSingleTable( node, in traits );
            return;
        }

        if ( IsValidUpdateFromStatement( node.DataSource, in traits ) )
        {
            VisitUpdateFrom( node, in traits );
            return;
        }

        var targetInfo = ExtractTargetInfo( node );
        var updateVisitor = CreateUpdateAssignmentsVisitor( node );

        node = CreateSimplifiedUpdateFrom( targetInfo, node, updateVisitor );
        traits = ExtractDataSourceTraits( node.DataSource.Traits );
        Assume.True( IsValidUpdateFromStatement( node.DataSource, in traits ) );
        VisitSimplifiedUpdateFrom( node, targetInfo, in traits );
    }

    public sealed override void VisitUpsert(SqlUpsertNode node)
    {
        var conflictTarget = node.ConflictTarget.Count > 0 ? node.ConflictTarget : ExtractUpsertConflictTargets( node );

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

    public sealed override void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        var traits = ExtractDataSourceTraits( node.DataSource.Traits );

        if ( IsValidSingleTableDeleteStatement( node.DataSource, in traits ) )
        {
            VisitDeleteFromWithSingleTable( node, traits );
            return;
        }

        if ( IsValidMultiTableDeleteStatement( node.DataSource, in traits ) )
        {
            VisitDeleteFromWithMultiTable( node, traits );
            return;
        }

        var targetInfo = ExtractTargetInfo( node );

        node = CreateSimplifiedDeleteFrom( targetInfo, node );
        traits = ExtractDataSourceTraits( node.DataSource.Traits );
        Assume.True( IsValidMultiTableDeleteStatement( node.DataSource, in traits ) );
        VisitDeleteFromWithMultiTable( node, traits );
    }

    public override void VisitTruncate(SqlTruncateNode node)
    {
        base.VisitTruncate( node );
        Context.Sql.AppendSpace().Append( "RESTART" ).AppendSpace().Append( "IDENTITY" );
    }

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
            var storage = node.Computation.Value.Storage == SqlColumnComputationStorage.Virtual
                && Options.IsVirtualGeneratedColumnStorageParsingEnabled
                    ? "VIRTUAL"
                    : "STORED";

            Context.Sql.AppendSpace().Append( "GENERATED" ).AppendSpace().Append( "ALWAYS" ).AppendSpace().Append( "AS" ).AppendSpace();
            VisitChildWrappedInParentheses( node.Computation.Value.Expression );
            Context.Sql.AppendSpace().Append( storage );
        }
    }

    public override void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node)
    {
        Context.Sql.Append( "CONSTRAINT" ).AppendSpace();
        AppendDelimitedName( node.Name.Object );
        Context.Sql.AppendSpace().Append( "PRIMARY" ).AppendSpace().Append( "KEY" ).AppendSpace().Append( '(' );

        if ( node.Columns.Count > 0 )
        {
            foreach ( var column in node.Columns )
            {
                this.Visit( column.Expression );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }

        Context.Sql.Append( ')' );
    }

    public override void VisitForeignKeyDefinition(SqlForeignKeyDefinitionNode node)
    {
        Context.Sql.Append( "CONSTRAINT" ).AppendSpace();
        AppendDelimitedName( node.Name.Object );
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

    public override void VisitCheckDefinition(SqlCheckDefinitionNode node)
    {
        Context.Sql.Append( "CONSTRAINT" ).AppendSpace();
        AppendDelimitedName( node.Name.Object );
        Context.Sql.AppendSpace().Append( "CHECK" ).AppendSpace();
        VisitChildWrappedInParentheses( node.Condition );
    }

    public override void VisitCreateTable(SqlCreateTableNode node)
    {
        using ( TempIgnoreAllRecordSets() )
        {
            Context.Sql.Append( "CREATE" ).AppendSpace();
            if ( node.Info.IsTemporary )
                Context.Sql.Append( "TEMPORARY" ).AppendSpace();

            Context.Sql.Append( "TABLE" ).AppendSpace();
            if ( node.IfNotExists )
                Context.Sql.Append( "IF" ).AppendSpace().Append( "NOT" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

            AppendDelimitedSchemaObjectName( node.Info.Name );
            Context.Sql.AppendSpace().Append( '(' );
            VisitCreateTableDefinition( node );
            Context.AppendIndent().Append( ')' );
        }
    }

    public override void VisitCreateView(SqlCreateViewNode node)
    {
        Context.Sql.Append( "CREATE" ).AppendSpace();
        if ( node.ReplaceIfExists )
            Context.Sql.Append( "OR" ).AppendSpace().Append( "REPLACE" ).AppendSpace();

        if ( node.Info.IsTemporary )
            Context.Sql.Append( "TEMPORARY" ).AppendSpace();

        Context.Sql.Append( "VIEW" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.Info.Name );
        Context.Sql.AppendSpace().Append( "AS" );
        Context.AppendIndent();
        this.Visit( node.Source );
    }

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
            AppendDelimitedName( node.Name.Object );
            Context.Sql.AppendSpace().Append( "ON" ).AppendSpace();
            AppendDelimitedRecordSetName( node.Table );

            Context.Sql.AppendSpace().Append( '(' );
            if ( node.Columns.Count > 0 )
            {
                using ( Context.TempIndentIncrease() )
                {
                    foreach ( var column in node.Columns )
                    {
                        if ( column.Expression is SqlDataFieldNode dataField )
                            this.Visit( dataField );
                        else
                            VisitChildWrappedInParentheses( column.Expression );

                        Context.Sql.AppendSpace().Append( column.Ordering.Name ).AppendComma().AppendSpace();
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

    public override void VisitRenameTable(SqlRenameTableNode node)
    {
        if ( ! node.Table.Name.Schema.Equals( node.NewName.Schema, StringComparison.OrdinalIgnoreCase ) )
        {
            Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
            AppendDelimitedSchemaObjectName( node.Table.Name );
            Context.Sql.AppendSpace().Append( "SET" ).AppendSpace().Append( "SCHEMA" ).AppendSpace();
            AppendDelimitedName( node.NewName.Schema );
            Context.Sql.AppendSemicolon();
            Context.AppendIndent();
        }

        Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedSchemaObjectName( SqlSchemaObjectName.Create( node.NewName.Schema, node.Table.Name.Object ) );
        Context.Sql.AppendSpace().Append( "RENAME" ).AppendSpace().Append( "TO" ).AppendSpace();
        AppendDelimitedName( node.NewName.Object );
    }

    public override void VisitRenameColumn(SqlRenameColumnNode node)
    {
        Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.Table.Name );
        Context.Sql.AppendSpace().Append( "RENAME" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        AppendDelimitedName( node.OldName );
        Context.Sql.AppendSpace().Append( "TO" ).AppendSpace();
        AppendDelimitedName( node.NewName );
    }

    public override void VisitAddColumn(SqlAddColumnNode node)
    {
        Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.Table.Name );
        Context.Sql.AppendSpace().Append( "ADD" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        VisitColumnDefinition( node.Definition );
    }

    public override void VisitDropColumn(SqlDropColumnNode node)
    {
        Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.Table.Name );
        Context.Sql.AppendSpace().Append( "DROP" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        AppendDelimitedName( node.Name );
    }

    public override void VisitDropTable(SqlDropTableNode node)
    {
        Context.Sql.Append( "DROP" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        if ( node.IfExists )
            Context.Sql.Append( "IF" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        AppendDelimitedSchemaObjectName( node.Table.Name );
    }

    public override void VisitDropView(SqlDropViewNode node)
    {
        Context.Sql.Append( "DROP" ).AppendSpace().Append( "VIEW" ).AppendSpace();
        if ( node.IfExists )
            Context.Sql.Append( "IF" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        AppendDelimitedSchemaObjectName( node.View.Name );
    }

    public override void VisitDropIndex(SqlDropIndexNode node)
    {
        Context.Sql.Append( "DROP" ).AppendSpace().Append( "INDEX" ).AppendSpace();
        if ( node.IfExists )
            Context.Sql.Append( "IF" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        AppendDelimitedSchemaObjectName( node.Name );
    }

    public override void VisitBeginTransaction(SqlBeginTransactionNode node)
    {
        var isolationLevel = node.IsolationLevel switch
        {
            IsolationLevel.ReadUncommitted => "READ UNCOMMITTED",
            IsolationLevel.ReadCommitted => "READ COMMITTED",
            IsolationLevel.Serializable => "SERIALIZABLE",
            _ => "REPEATABLE READ"
        };

        Context.Sql.Append( "BEGIN" ).AppendSpace().Append( "TRANSACTION" ).AppendSpace();
        Context.Sql.Append( "ISOLATION" ).AppendSpace().Append( "LEVEL" ).AppendSpace().Append( isolationLevel );
    }

    public sealed override void AppendDelimitedTemporaryObjectName(string name)
    {
        AppendDelimitedName( name );
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

            case SqlNodeType.FunctionExpression:
                return ReinterpretCast.To<SqlFunctionExpressionNode>( node ).FunctionType
                    is SqlFunctionType.LastIndexOf or SqlFunctionType.TemporalAdd;

            case SqlNodeType.AggregateFunctionExpression:
                foreach ( var trait in ReinterpretCast.To<SqlAggregateFunctionExpressionNode>( node ).Traits )
                {
                    if ( trait.NodeType != SqlNodeType.DistinctTrait && trait.NodeType != SqlNodeType.SortTrait )
                        return true;
                }

                return false;
        }

        return true;
    }

    protected void VisitSimpleAggregateFunction(string functionName, SqlAggregateFunctionExpressionNode node)
    {
        var traits = ExtractAggregateFunctionTraits( node.Traits );
        Context.Sql.Append( functionName );
        VisitAggregateFunctionArgumentsAndTraits( node.Arguments, traits );
    }

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

        if ( traits.Ordering.Count > 0 )
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

    protected void VisitOptionalLimitExpression(SqlExpressionNode? limit)
    {
        if ( limit is null )
            return;

        Context.AppendIndent().Append( "LIMIT" ).AppendSpace();
        VisitChild( limit );
    }

    protected void VisitOptionalOffsetExpression(SqlExpressionNode? offset)
    {
        if ( offset is null )
            return;

        Context.AppendIndent().Append( "OFFSET" ).AppendSpace();
        VisitChild( offset );
    }

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

    protected virtual void VisitInsertIntoFromGenericSource(SqlInsertIntoNode node)
    {
        VisitInsertIntoFields( node );
        Context.AppendIndent();
        this.Visit( node.Source );
    }

    protected virtual void VisitUpdateWithSingleTable(SqlUpdateNode node, in SqlDataSourceTraits traits)
    {
        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "UPDATE" ).AppendSpace();
        this.Visit( node.DataSource.From );
        VisitUpdateAssignmentRange( node.Assignments );
        VisitDataSourceAfterTraits( in traits );
    }

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

    protected virtual void VisitSimplifiedUpdateFrom(
        SqlUpdateNode node,
        ChangeTargetInfo targetInfo,
        in SqlDataSourceTraits traits)
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

    protected virtual void VisitUpsertFromDataSourceQuery(
        SqlUpsertNode node,
        ReadOnlyArray<SqlDataFieldNode> conflictTarget,
        SqlDataSourceQueryExpressionNode query)
    {
        Assume.Equals( node.Source, query );
        var traits = ExtractDataSourceTraits( ExtractDataSourceTraits( query.DataSource.Traits ), query.Traits );
        VisitDataSourceBeforeTraits( in traits );
        VisitInsertIntoFields( node.RecordSet, node.InsertDataFields );
        Context.AppendIndent();
        VisitDataSourceQuerySelection( query, in traits );
        VisitDataSource( query.DataSource );
        VisitDataSourceAfterTraits( in traits );
        AppendUpsertOnConflict( node, conflictTarget );
    }

    protected virtual void VisitUpsertFromCompoundQuery(
        SqlUpsertNode node,
        ReadOnlyArray<SqlDataFieldNode> conflictTarget,
        SqlCompoundQueryExpressionNode query)
    {
        Assume.Equals( node.Source, query );
        var traits = ExtractQueryTraits( query.Traits );
        VisitQueryBeforeTraits( in traits );
        VisitInsertIntoFields( node.RecordSet, node.InsertDataFields );
        Context.AppendIndent();
        VisitCompoundQueryComponents( query, in traits );
        VisitQueryAfterTraits( in traits );
        AppendUpsertOnConflict( node, conflictTarget );
    }

    protected virtual void VisitUpsertFromGenericSource(SqlUpsertNode node, ReadOnlyArray<SqlDataFieldNode> conflictTarget)
    {
        VisitInsertIntoFields( node.RecordSet, node.InsertDataFields );
        Context.AppendIndent();
        this.Visit( node.Source );
        AppendUpsertOnConflict( node, conflictTarget );
    }

    protected void AppendUpsertOnConflict(SqlUpsertNode node, ReadOnlyArray<SqlDataFieldNode> conflictTarget)
    {
        Assume.IsNotEmpty( conflictTarget );
        Context.AppendIndent().Append( "ON" ).AppendSpace().Append( "CONFLICT" ).AppendSpace().Append( '(' );
        foreach ( var target in conflictTarget )
        {
            AppendDelimitedName( target.Name );
            Context.Sql.AppendComma().AppendSpace();
        }

        Context.Sql.ShrinkBy( 2 ).Append( ')' ).AppendSpace().Append( "DO" ).AppendSpace().Append( "UPDATE" );

        _upsertUpdateSourceReplacement ??= SqlNode.RawRecordSet( PostgreSqlHelpers.UpsertExcludedRecordSetName );
        using ( TempReplaceRecordSet( node.UpdateSource, _upsertUpdateSourceReplacement ) )
            VisitUpdateAssignmentRange( node.UpdateAssignments );
    }

    protected virtual void VisitDeleteFromWithSingleTable(SqlDeleteFromNode node, in SqlDataSourceTraits traits)
    {
        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "DELETE" ).AppendSpace().Append( "FROM" ).AppendSpace();
        this.Visit( node.DataSource.From );
        VisitDataSourceAfterTraits( in traits );
    }

    protected virtual void VisitDeleteFromWithMultiTable(SqlDeleteFromNode node, in SqlDataSourceTraits traits)
    {
        Assume.ContainsExactly( node.DataSource.Joins, 1 );
        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "DELETE" ).AppendSpace().Append( "FROM" ).AppendSpace();
        this.Visit( node.DataSource.From );

        var join = node.DataSource.Joins[0];
        Context.AppendIndent().Append( "USING" ).AppendSpace();
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

    protected override void VisitDataSourceAfterTraits(in SqlDataSourceTraits traits)
    {
        base.VisitDataSourceAfterTraits( in traits );
        VisitOptionalLimitExpression( traits.Limit );
        VisitOptionalOffsetExpression( traits.Offset );
    }

    protected override void VisitQueryAfterTraits(in SqlQueryTraits traits)
    {
        base.VisitQueryAfterTraits( in traits );
        VisitOptionalLimitExpression( traits.Limit );
        VisitOptionalOffsetExpression( traits.Offset );
    }

    [Pure]
    protected virtual bool IsValidSingleTableDeleteStatement(SqlDataSourceNode node, in SqlDataSourceTraits traits)
    {
        return node.RecordSets.Count == 1
            && traits.Distinct is null
            && traits.Aggregations.Count == 0
            && traits.AggregationFilter is null
            && traits.Windows.Count == 0
            && traits.Ordering.Count == 0
            && traits.Limit is null
            && traits.Offset is null
            && traits.Custom.Count == 0;
    }

    [Pure]
    protected virtual bool IsValidMultiTableDeleteStatement(SqlDataSourceNode node, in SqlDataSourceTraits traits)
    {
        return node.RecordSets.Count == 2
            && node.Joins[0].JoinType is SqlJoinType.Inner or SqlJoinType.Cross
            && traits.Distinct is null
            && traits.Aggregations.Count == 0
            && traits.AggregationFilter is null
            && traits.Windows.Count == 0
            && traits.Ordering.Count == 0
            && traits.Limit is null
            && traits.Offset is null
            && traits.Custom.Count == 0;
    }

    [Pure]
    protected virtual bool IsValidSingleTableUpdateStatement(SqlDataSourceNode node, in SqlDataSourceTraits traits)
    {
        return node.RecordSets.Count == 1
            && traits.Distinct is null
            && traits.Aggregations.Count == 0
            && traits.AggregationFilter is null
            && traits.Windows.Count == 0
            && traits.Ordering.Count == 0
            && traits.Limit is null
            && traits.Offset is null
            && traits.Custom.Count == 0;
    }

    [Pure]
    protected virtual bool IsValidUpdateFromStatement(SqlDataSourceNode node, in SqlDataSourceTraits traits)
    {
        return node.RecordSets.Count == 2
            && node.Joins[0].JoinType is SqlJoinType.Inner or SqlJoinType.Cross
            && traits.Distinct is null
            && traits.Aggregations.Count == 0
            && traits.AggregationFilter is null
            && traits.Windows.Count == 0
            && traits.Ordering.Count == 0
            && traits.Limit is null
            && traits.Offset is null
            && traits.Custom.Count == 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void AppendPostgreStyleCast(PostgreSqlDataType dataType)
    {
        Context.Sql.Append( ':', repeatCount: 2 ).Append( dataType.Name );
    }
}
