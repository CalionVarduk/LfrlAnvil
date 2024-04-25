using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using MySqlConnector;

namespace LfrlAnvil.MySql;

public class MySqlNodeInterpreter : SqlNodeInterpreter
{
    private const string MaxLimit = "18446744073709551615";
    private readonly string? _indexPrefixLength;
    private SqlRecordSetNode? _upsertUpdateSourceReplacement;

    public MySqlNodeInterpreter(MySqlNodeInterpreterOptions options, SqlNodeInterpreterContext context)
        : base( context, beginNameDelimiter: '`', endNameDelimiter: '`' )
    {
        Options = options;
        _upsertUpdateSourceReplacement = null;
        TypeDefinitions = Options.TypeDefinitions ?? new MySqlColumnTypeDefinitionProviderBuilder().Build();
        CommonSchemaName = Options.CommonSchemaName ?? MySqlHelpers.DefaultVersionHistoryName.Schema;
        _indexPrefixLength = Options.IndexPrefixLength?.ToString( CultureInfo.InvariantCulture );
    }

    public MySqlNodeInterpreterOptions Options { get; }
    public MySqlColumnTypeDefinitionProvider TypeDefinitions { get; }
    public string CommonSchemaName { get; }

    public override void VisitLiteral(SqlLiteralNode node)
    {
        var sql = node.GetSql( TypeDefinitions );
        Context.Sql.Append( sql );
    }

    public override void VisitParameter(SqlParameterNode node)
    {
        Context.Sql.Append( '@' ).Append( node.Name );
        AddContextParameter( node );
    }

    public override void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CURRENT_DATE", node );
    }

    public override void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node)
    {
        Context.Sql.Append( "CURRENT_TIME" ).Append( '(' ).Append( '6' ).Append( ')' );
    }

    public override void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node)
    {
        Context.Sql.Append( "NOW" ).Append( '(' ).Append( '6' ).Append( ')' );
    }

    public override void VisitCurrentUtcDateTimeFunction(SqlCurrentUtcDateTimeFunctionExpressionNode node)
    {
        Context.Sql.Append( "UTC_TIMESTAMP" ).Append( '(' ).Append( '6' ).Append( ')' );
    }

    public override void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        Context.Sql.Append( "CAST" ).Append( '(' ).Append( "UNIX_TIMESTAMP" ).Append( '(' );
        Context.Sql.Append( "NOW" ).Append( '(' ).Append( '6' ).Append( ')' ).Append( ')' );
        Context.Sql.AppendSpace().Append( '*' ).AppendSpace().Append( '1' ).Append( '0', repeatCount: 7 );
        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace().Append( "SIGNED" ).Append( ')' );
    }

    public override void VisitExtractDateFunction(SqlExtractDateFunctionExpressionNode node)
    {
        VisitSimpleFunction( "DATE", node );
    }

    public override void VisitExtractTimeOfDayFunction(SqlExtractTimeOfDayFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TIME", node );
    }

    public override void VisitExtractDayFunction(SqlExtractDayFunctionExpressionNode node)
    {
        switch ( node.Unit )
        {
            case SqlTemporalUnit.Year:
                VisitSimpleFunction( "DAYOFYEAR", node );
                break;

            case SqlTemporalUnit.Month:
                VisitSimpleFunction( "DAYOFMONTH", node );
                break;

            case SqlTemporalUnit.Week:
                VisitSimpleFunction( "WEEKDAY", node );
                break;
        }
    }

    public override void VisitExtractTemporalUnitFunction(SqlExtractTemporalUnitFunctionExpressionNode node)
    {
        switch ( node.Unit )
        {
            case SqlTemporalUnit.Year:
                VisitSimpleFunction( "YEAR", node );
                break;

            case SqlTemporalUnit.Month:
                VisitSimpleFunction( "MONTH", node );
                break;

            case SqlTemporalUnit.Week:
                VisitSimpleFunction( "WEEKOFYEAR", node );
                break;

            case SqlTemporalUnit.Day:
                VisitSimpleFunction( "DAYOFMONTH", node );
                break;

            case SqlTemporalUnit.Hour:
                VisitSimpleFunction( "HOUR", node );
                break;

            case SqlTemporalUnit.Minute:
                VisitSimpleFunction( "MINUTE", node );
                break;

            case SqlTemporalUnit.Second:
                VisitSimpleFunction( "SECOND", node );
                break;

            case SqlTemporalUnit.Millisecond:
                VisitSimpleFunction( "MICROSECOND", node );
                Context.Sql.AppendSpace().Append( "DIV" ).AppendSpace().Append( '1' ).Append( '0', repeatCount: 3 );
                break;

            case SqlTemporalUnit.Microsecond:
                VisitSimpleFunction( "MICROSECOND", node );
                break;

            case SqlTemporalUnit.Nanosecond:
                VisitSimpleFunction( "MICROSECOND", node );
                Context.Sql.AppendSpace().Append( '*' ).AppendSpace().Append( '1' ).Append( '0', repeatCount: 3 );
                break;
        }
    }

    public override void VisitTemporalAddFunction(SqlTemporalAddFunctionExpressionNode node)
    {
        switch ( node.Unit )
        {
            case SqlTemporalUnit.Nanosecond:
                Context.Sql.Append( "TIMESTAMPADD" ).Append( '(' ).Append( "MICROSECOND" ).AppendComma().AppendSpace();
                VisitChild( node.Arguments[1] );
                Context.Sql.AppendSpace().Append( "DIV" ).AppendSpace().Append( '1' ).Append( '0', repeatCount: 3 );
                Context.Sql.AppendComma().AppendSpace();
                VisitChild( node.Arguments[0] );
                Context.Sql.Append( ')' );
                break;

            case SqlTemporalUnit.Millisecond:
                Context.Sql.Append( "TIMESTAMPADD" ).Append( '(' ).Append( "MICROSECOND" ).AppendComma().AppendSpace();
                VisitChild( node.Arguments[1] );
                Context.Sql.AppendSpace().Append( '*' ).AppendSpace().Append( '1' ).Append( '0', repeatCount: 3 );
                Context.Sql.AppendComma().AppendSpace();
                VisitChild( node.Arguments[0] );
                Context.Sql.Append( ')' );
                break;

            default:
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
                    _ => "MICROSECOND"
                };

                Context.Sql.Append( "TIMESTAMPADD" ).Append( '(' ).Append( unit ).AppendComma().AppendSpace();
                VisitChild( node.Arguments[1] );
                Context.Sql.AppendComma().AppendSpace();
                VisitChild( node.Arguments[0] );
                Context.Sql.Append( ')' );
                break;
            }
        }
    }

    public override void VisitTemporalDiffFunction(SqlTemporalDiffFunctionExpressionNode node)
    {
        switch ( node.Unit )
        {
            case SqlTemporalUnit.Nanosecond:
                Context.Sql.Append( "TIMESTAMPDIFF" ).Append( '(' ).Append( "MICROSECOND" ).AppendComma().AppendSpace();
                VisitChild( node.Arguments[0] );
                Context.Sql.AppendComma().AppendSpace();
                VisitChild( node.Arguments[1] );
                Context.Sql.Append( ')' ).AppendSpace().Append( '*' ).AppendSpace().Append( '1' ).Append( '0', repeatCount: 3 );
                break;

            case SqlTemporalUnit.Millisecond:
                Context.Sql.Append( "TIMESTAMPDIFF" ).Append( '(' ).Append( "MICROSECOND" ).AppendComma().AppendSpace();
                VisitChild( node.Arguments[0] );
                Context.Sql.AppendComma().AppendSpace();
                VisitChild( node.Arguments[1] );
                Context.Sql.Append( ')' ).AppendSpace().Append( "DIV" ).AppendSpace().Append( '1' ).Append( '0', repeatCount: 3 );
                break;

            default:
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
                    _ => "MICROSECOND"
                };

                Context.Sql.Append( "TIMESTAMPDIFF" ).Append( '(' ).Append( unit ).AppendComma().AppendSpace();
                VisitChild( node.Arguments[0] );
                Context.Sql.AppendComma().AppendSpace();
                VisitChild( node.Arguments[1] );
                Context.Sql.Append( ')' );
                break;
            }
        }
    }

    public override void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node)
    {
        AppendDelimitedSchemaObjectName( SqlSchemaObjectName.Create( CommonSchemaName, MySqlHelpers.GuidFunctionName ) );
        VisitFunctionArguments( node.Arguments );
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
        if ( node.Arguments.Count == 1 )
            VisitSimpleFunction( "LTRIM", node );
        else
        {
            var args = node.Arguments;
            Context.Sql.Append( "TRIM" ).Append( '(' ).Append( "LEADING" ).AppendSpace();
            VisitChild( args[1] );
            Context.Sql.AppendSpace().Append( "FROM" ).AppendSpace();
            VisitChild( args[0] );
            Context.Sql.Append( ')' );
        }
    }

    public override void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node)
    {
        if ( node.Arguments.Count == 1 )
            VisitSimpleFunction( "RTRIM", node );
        else
        {
            var args = node.Arguments;
            Context.Sql.Append( "TRIM" ).Append( '(' ).Append( "TRAILING" ).AppendSpace();
            VisitChild( args[1] );
            Context.Sql.AppendSpace().Append( "FROM" ).AppendSpace();
            VisitChild( args[0] );
            Context.Sql.Append( ')' );
        }
    }

    public override void VisitTrimFunction(SqlTrimFunctionExpressionNode node)
    {
        if ( node.Arguments.Count == 1 )
            VisitSimpleFunction( "TRIM", node );
        else
        {
            var args = node.Arguments;
            Context.Sql.Append( "TRIM" ).Append( '(' ).Append( "BOTH" ).AppendSpace();
            VisitChild( args[1] );
            Context.Sql.AppendSpace().Append( "FROM" ).AppendSpace();
            VisitChild( args[0] );
            Context.Sql.Append( ')' );
        }
    }

    public override void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SUBSTRING", node );
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
        VisitSimpleFunction( "INSTR", node );
    }

    public override void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node)
    {
        var args = node.Arguments;
        Context.Sql.Append( "LEAST" ).Append( '(' ).Append( "GREATEST" ).Append( '(' ).Append( "CHAR_LENGTH" ).Append( '(' );
        VisitChild( args[0] );
        Context.Sql.Append( ')' ).AppendSpace().Append( '-' ).AppendSpace().Append( "CHAR_LENGTH" ).Append( '(' );
        Context.Sql.Append( "SUBSTRING_INDEX" ).Append( '(' );
        VisitChild( args[0] );
        Context.Sql.AppendComma().AppendSpace();
        VisitChild( args[1] );
        Context.Sql.AppendComma().AppendSpace().Append( '-' ).Append( '1' ).Append( ')', repeatCount: 2 );
        Context.Sql.AppendSpace().Append( '-' ).AppendSpace().Append( "CHAR_LENGTH" ).Append( '(' );
        VisitChild( args[1] );
        Context.Sql.Append( ')' ).AppendSpace().Append( '+' ).AppendSpace().Append( '1' );
        Context.Sql.AppendComma().AppendSpace().Append( '0' ).Append( ')' ).AppendComma().AppendSpace();
        Context.Sql.Append( "CHAR_LENGTH" ).Append( '(' );
        VisitChild( args[0] );
        Context.Sql.Append( ')', repeatCount: 2 );
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
        if ( node.Arguments.Count == 1 )
        {
            Context.Sql.Append( "TRUNCATE" ).Append( '(' );
            VisitChild( node.Arguments[0] );
            Context.Sql.AppendComma().AppendSpace().Append( '0' ).Append( ')' );
        }
        else
            VisitSimpleFunction( "TRUNCATE", node );
    }

    public override void VisitRoundFunction(SqlRoundFunctionExpressionNode node)
    {
        VisitSimpleFunction( "ROUND", node );
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
        Context.Sql.Append( "GROUP_CONCAT" ).Append( '(' );

        var arguments = node.Arguments;
        using ( Context.TempIndentIncrease() )
        {
            if ( traits.Distinct is not null )
            {
                VisitDistinctTrait( traits.Distinct );
                Context.Sql.AppendSpace();
            }

            if ( traits.Filter is not null )
            {
                Context.Sql.Append( "CASE" ).AppendSpace().Append( "WHEN" ).AppendSpace();
                this.Visit( traits.Filter );
                Context.Sql.AppendSpace().Append( "THEN" ).AppendSpace();
                VisitChild( arguments[0] );
                Context.Sql.AppendSpace().Append( "ELSE" ).AppendSpace().Append( "NULL" ).AppendSpace().Append( "END" );
            }
            else
                VisitChild( arguments[0] );

            if ( traits.Ordering.Count > 0 )
            {
                Context.Sql.AppendSpace().Append( "ORDER" ).AppendSpace().Append( "BY" ).AppendSpace();
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

            if ( arguments.Count > 1 )
            {
                Context.Sql.AppendSpace().Append( "SEPARATOR" ).AppendSpace();
                VisitChild( arguments[1] );
            }
        }

        Context.Sql.Append( ')' );

        if ( traits.Window is not null )
        {
            Context.Sql.AppendSpace().Append( "OVER" ).AppendSpace();
            AppendDelimitedName( traits.Window.Name );
        }
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
            SqlJoinType.Full => Options.IsFullJoinParsingEnabled ? "FULL" : throw new UnrecognizedSqlNodeException( this, node ),
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
        const string signed = "SIGNED";
        const string unsigned = "UNSIGNED";

        Context.Sql.Append( "CAST" ).Append( '(' );
        VisitChild( node.Value );
        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace();

        var typeDefinition = node.TargetTypeDefinition ?? TypeDefinitions.GetByType( node.TargetType );
        var dataType = SqlHelpers.CastOrThrow<MySqlDataType>( MySqlDialect.Instance, typeDefinition.DataType );
        var name = dataType.Value switch
        {
            MySqlDbType.Bool => signed,
            MySqlDbType.Byte => signed,
            MySqlDbType.Int16 => signed,
            MySqlDbType.Int24 => signed,
            MySqlDbType.Int32 => signed,
            MySqlDbType.Int64 => signed,
            MySqlDbType.UByte => unsigned,
            MySqlDbType.UInt16 => unsigned,
            MySqlDbType.UInt24 => unsigned,
            MySqlDbType.UInt32 => unsigned,
            MySqlDbType.UInt64 => unsigned,
            MySqlDbType.String => GetChar( dataType ),
            MySqlDbType.VarString => GetChar( dataType ),
            MySqlDbType.VarChar => GetChar( dataType ),
            MySqlDbType.TinyText => GetChar( dataType ),
            MySqlDbType.Text => GetChar( dataType ),
            MySqlDbType.MediumText => GetChar( dataType ),
            MySqlDbType.LongText => GetChar( dataType ),
            MySqlDbType.Binary => GetBinary( dataType ),
            MySqlDbType.VarBinary => GetBinary( dataType ),
            MySqlDbType.TinyBlob => GetBinary( dataType ),
            MySqlDbType.Blob => GetBinary( dataType ),
            MySqlDbType.MediumBlob => GetBinary( dataType ),
            MySqlDbType.LongBlob => GetBinary( dataType ),
            _ => dataType.Name
        };

        Context.Sql.Append( name ).Append( ')' );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static string GetChar(MySqlDataType type)
        {
            var parameters = type.Parameters;
            return parameters.Length == 0 ? "CHAR" : $"CHAR({parameters[0]})";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static string GetBinary(MySqlDataType type)
        {
            var parameters = type.Parameters;
            return parameters.Length == 0 ? "BINARY" : $"BINARY({parameters[0]})";
        }
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

        if ( IsValidMultiTableUpdateStatement( node.DataSource, in traits ) )
        {
            VisitUpdateWithMultiTable( node, in traits );
            return;
        }

        var targetInfo = ExtractTargetInfo( node );
        var updateVisitor = CreateUpdateAssignmentsVisitor( node );

        node = CreateSimplifiedUpdateFrom( targetInfo, node, updateVisitor );
        traits = ExtractDataSourceTraits( node.DataSource.Traits );
        Assume.True( IsValidMultiTableUpdateStatement( node.DataSource, in traits ) );
        VisitSimplifiedUpdateWithMultiTable( node, targetInfo, in traits );
    }

    public sealed override void VisitUpsert(SqlUpsertNode node)
    {
        switch ( node.Source.NodeType )
        {
            case SqlNodeType.DataSourceQuery:
            {
                VisitUpsertFromDataSourceQuery( node, ReinterpretCast.To<SqlDataSourceQueryExpressionNode>( node.Source ) );
                break;
            }
            case SqlNodeType.CompoundQuery:
            {
                VisitUpsertFromCompoundQuery( node, ReinterpretCast.To<SqlCompoundQueryExpressionNode>( node.Source ) );
                break;
            }
            case SqlNodeType.RawQuery:
            {
                VisitUpsertFromRawQuery( node, ReinterpretCast.To<SqlRawQueryExpressionNode>( node.Source ) );
                break;
            }
            default:
            {
                VisitUpsertFromGenericSource( node );
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

    public override void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        var typeDefinition = node.TypeDefinition ?? TypeDefinitions.GetByType( node.Type.UnderlyingType );
        AppendDelimitedName( node.Name );
        Context.Sql.AppendSpace().Append( typeDefinition.DataType.Name );

        if ( ! node.Type.IsNullable && node.Computation is null )
            Context.Sql.AppendSpace().Append( "NOT" ).AppendSpace().Append( "NULL" );

        if ( node.DefaultValue is not null )
        {
            Context.Sql.AppendSpace().Append( "DEFAULT" ).AppendSpace();

            if ( node.DefaultValue.NodeType is not SqlNodeType.Literal and not SqlNodeType.Null
                || (typeDefinition.DataType is MySqlDataType mySqlDataType && IsTextOrBlobType( mySqlDataType )) )
                VisitChildWrappedInParentheses( node.DefaultValue );
            else
                this.Visit( node.DefaultValue );
        }

        if ( node.Computation is not null )
        {
            var storage = node.Computation.Value.Storage == SqlColumnComputationStorage.Virtual ? "VIRTUAL" : "STORED";
            Context.Sql.AppendSpace().Append( "GENERATED" ).AppendSpace().Append( "ALWAYS" ).AppendSpace().Append( "AS" ).AppendSpace();
            VisitChildWrappedInParentheses( node.Computation.Value.Expression );
            Context.Sql.AppendSpace().Append( storage );

            if ( ! node.Type.IsNullable )
                Context.Sql.AppendSpace().Append( "NOT" ).AppendSpace().Append( "NULL" );
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
                VisitIndexedExpression( column );
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
        if ( node.Info.IsTemporary && Options.AreTemporaryViewsForbidden )
            throw new SqlNodeVisitorException( Resources.TemporaryViewsAreForbidden, this, node );

        Context.Sql.Append( "CREATE" ).AppendSpace();
        if ( node.ReplaceIfExists )
            Context.Sql.Append( "OR" ).AppendSpace().Append( "REPLACE" ).AppendSpace();

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
                        VisitIndexedExpression( column );
                        Context.Sql.AppendComma().AppendSpace();
                    }

                    Context.Sql.ShrinkBy( 2 );
                }
            }

            Context.Sql.Append( ')' );

            if ( node.Filter is not null && Options.IsIndexFilterParsingEnabled )
            {
                Context.Sql.AppendSpace().Append( "WHERE" ).AppendSpace();
                VisitChild( node.Filter );
            }
        }
    }

    public override void VisitRenameTable(SqlRenameTableNode node)
    {
        Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.Table.Name );
        Context.Sql.AppendSpace().Append( "RENAME" ).AppendSpace().Append( "TO" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.NewName );
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
        Context.Sql.Append( "DROP" ).AppendSpace();
        if ( node.Table.IsTemporary )
            Context.Sql.Append( "TEMPORARY" ).AppendSpace();

        Context.Sql.Append( "TABLE" ).AppendSpace();
        if ( node.IfExists )
            Context.Sql.Append( "IF" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        AppendDelimitedSchemaObjectName( node.Table.Name );
    }

    public override void VisitDropView(SqlDropViewNode node)
    {
        if ( node.View.IsTemporary && Options.AreTemporaryViewsForbidden )
            throw new SqlNodeVisitorException( Resources.TemporaryViewsAreForbidden, this, node );

        Context.Sql.Append( "DROP" ).AppendSpace().Append( "VIEW" ).AppendSpace();
        if ( node.IfExists )
            Context.Sql.Append( "IF" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        AppendDelimitedSchemaObjectName( node.View.Name );
    }

    public override void VisitDropIndex(SqlDropIndexNode node)
    {
        if ( node.IfExists )
        {
            Context.Sql.Append( "CALL" ).AppendSpace();
            AppendDelimitedSchemaObjectName( SqlSchemaObjectName.Create( CommonSchemaName, MySqlHelpers.DropIndexIfExistsProcedureName ) );
            Context.Sql.Append( '(' );

            if ( node.Name.Schema.Length == 0 )
                VisitNull( SqlNode.Null() );
            else
                VisitLiteral( ReinterpretCast.To<SqlLiteralNode>( SqlNode.Literal( node.Name.Schema ) ) );

            Context.Sql.AppendComma().AppendSpace();
            VisitLiteral( ReinterpretCast.To<SqlLiteralNode>( SqlNode.Literal( node.Table.Name.Object ) ) );
            Context.Sql.AppendComma().AppendSpace();
            VisitLiteral( ReinterpretCast.To<SqlLiteralNode>( SqlNode.Literal( node.Name.Object ) ) );
            Context.Sql.Append( ')' );
        }
        else
        {
            Context.Sql.Append( "DROP" ).AppendSpace().Append( "INDEX" ).AppendSpace();
            AppendDelimitedName( node.Name.Object );
            Context.Sql.AppendSpace().Append( "ON" ).AppendSpace();
            AppendDelimitedRecordSetInfo( node.Table );
        }
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

        Context.Sql.Append( "SET" ).AppendSpace().Append( "SESSION" ).AppendSpace().Append( "TRANSACTION" ).AppendSpace();
        Context.Sql.Append( "ISOLATION" ).AppendSpace().Append( "LEVEL" ).AppendSpace().Append( isolationLevel ).AppendSemicolon();
        Context.AppendIndent();
        Context.Sql.Append( "START" ).AppendSpace().Append( "TRANSACTION" );

        if ( node.IsolationLevel == IsolationLevel.Snapshot )
            Context.Sql.AppendSpace().Append( "WITH" ).AppendSpace().Append( "CONSISTENT" ).AppendSpace().Append( "SNAPSHOT" );
    }

    public sealed override void AppendDelimitedTemporaryObjectName(string name)
    {
        AppendDelimitedName( name );
    }

    protected override void AddContextParameter(SqlParameterNode node)
    {
        Context.AddParameter( node.Name, node.Type, index: null );
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
            {
                return ReinterpretCast.To<SqlFunctionExpressionNode>( node ).FunctionType switch
                {
                    SqlFunctionType.ExtractTemporalUnit => ReinterpretCast.To<SqlExtractTemporalUnitFunctionExpressionNode>( node ).Unit
                        is SqlTemporalUnit.Millisecond or SqlTemporalUnit.Nanosecond,
                    SqlFunctionType.TemporalDiff => ReinterpretCast.To<SqlTemporalDiffFunctionExpressionNode>( node ).Unit
                        is SqlTemporalUnit.Millisecond or SqlTemporalUnit.Nanosecond,
                    _ => false
                };
            }
            case SqlNodeType.AggregateFunctionExpression:
                foreach ( var trait in ReinterpretCast.To<SqlAggregateFunctionExpressionNode>( node ).Traits )
                {
                    if ( trait.NodeType != SqlNodeType.DistinctTrait && trait.NodeType != SqlNodeType.FilterTrait )
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

        var args = arguments.AsSpan();
        if ( args.Length > 0 )
        {
            using ( Context.TempIndentIncrease() )
            {
                if ( traits.Distinct is not null )
                {
                    VisitDistinctTrait( traits.Distinct );
                    Context.Sql.AppendSpace();
                }

                if ( traits.Filter is not null )
                {
                    Context.Sql.Append( "CASE" ).AppendSpace().Append( "WHEN" ).AppendSpace();
                    this.Visit( traits.Filter );
                    Context.Sql.AppendSpace().Append( "THEN" ).AppendSpace();
                    VisitChild( args[0] );
                    Context.Sql.AppendSpace().Append( "ELSE" ).AppendSpace().Append( "NULL" ).AppendSpace().Append( "END" );

                    args = args.Slice( 1 );
                    if ( args.Length > 0 )
                        Context.Sql.AppendComma().AppendSpace();
                }

                foreach ( var arg in args )
                {
                    VisitChild( arg );
                    Context.Sql.AppendComma().AppendSpace();
                }
            }

            if ( args.Length > 0 )
                Context.Sql.ShrinkBy( 2 );
        }

        Context.Sql.Append( ')' );

        if ( traits.Window is not null )
        {
            Context.Sql.AppendSpace().Append( "OVER" ).AppendSpace();
            AppendDelimitedName( traits.Window.Name );
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

        Context.AppendIndent().Append( "LIMIT" ).AppendSpace().Append( MaxLimit );
        Context.Sql.AppendSpace().Append( "OFFSET" ).AppendSpace();
        VisitChild( offset );
    }

    protected virtual void VisitIndexedExpression(SqlOrderByNode node)
    {
        if ( node.Expression is SqlDataFieldNode dataField )
        {
            this.Visit( dataField );

            var type = dataField switch
            {
                SqlRawDataFieldNode n => n.Type,
                SqlColumnNode n => n.Type,
                SqlColumnBuilderNode n => n.Type,
                _ => null
            };

            if ( type is not null )
            {
                var typeDefinition = TypeDefinitions.GetByType( type.Value.UnderlyingType );
                if ( _indexPrefixLength is not null && typeDefinition.DataType is MySqlDataType dataType && IsTextOrBlobType( dataType ) )
                    Context.Sql.Append( '(' ).Append( _indexPrefixLength ).Append( ')' );
            }
        }
        else
            VisitChildWrappedInParentheses( node.Expression );

        Context.Sql.AppendSpace().Append( node.Ordering.Name );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static bool IsTextOrBlobType(MySqlDataType dataType)
    {
        return dataType.Value is MySqlDbType.TinyText
            or MySqlDbType.Text
            or MySqlDbType.MediumText
            or MySqlDbType.LongText
            or MySqlDbType.TinyBlob
            or MySqlDbType.Blob
            or MySqlDbType.MediumBlob
            or MySqlDbType.LongBlob;
    }

    protected virtual void VisitInsertIntoFromDataSourceQuery(SqlInsertIntoNode node, SqlDataSourceQueryExpressionNode query)
    {
        Assume.Equals( node.Source, query );
        var traits = ExtractDataSourceTraits( ExtractDataSourceTraits( query.DataSource.Traits ), query.Traits );
        VisitDataSourceBeforeTraits( RemoveCommonTableExpressions( in traits ) );
        VisitInsertIntoFields( node );
        Context.AppendIndent();
        VisitDataSourceBeforeTraits( in traits );
        VisitDataSourceQuerySelection( query, in traits );
        VisitDataSource( query.DataSource );
        VisitDataSourceAfterTraits( in traits );
    }

    protected virtual void VisitInsertIntoFromCompoundQuery(SqlInsertIntoNode node, SqlCompoundQueryExpressionNode query)
    {
        Assume.Equals( node.Source, query );
        var traits = ExtractQueryTraits( query.Traits );
        VisitQueryBeforeTraits( RemoveCommonTableExpressions( in traits ) );
        VisitInsertIntoFields( node );
        Context.AppendIndent();
        VisitQueryBeforeTraits( in traits );
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

    protected virtual void VisitUpdateWithMultiTable(SqlUpdateNode node, in SqlDataSourceTraits traits)
    {
        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "UPDATE" ).AppendSpace();

        this.Visit( node.DataSource.From );
        foreach ( var join in node.DataSource.Joins )
        {
            Context.AppendIndent();
            VisitJoinOn( join );
        }

        VisitMultiUpdateAssignmentRange( node.Assignments );
        VisitDataSourceAfterTraits( in traits );
    }

    protected virtual void VisitSimplifiedUpdateWithMultiTable(
        SqlUpdateNode node,
        ChangeTargetInfo targetInfo,
        in SqlDataSourceTraits traits)
    {
        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "UPDATE" ).AppendSpace();

        this.Visit( node.DataSource.From );
        foreach ( var join in node.DataSource.Joins )
        {
            Context.AppendIndent();
            VisitJoinOn( join );
        }

        using ( TempReplaceRecordSet( targetInfo.Target, targetInfo.BaseTarget ) )
            VisitMultiUpdateAssignmentRange( node.Assignments );

        VisitDataSourceAfterTraits( in traits );
    }

    protected virtual void VisitUpsertFromDataSourceQuery(SqlUpsertNode node, SqlDataSourceQueryExpressionNode query)
    {
        Assume.Equals( node.Source, query );
        var traits = ExtractDataSourceTraits( ExtractDataSourceTraits( query.DataSource.Traits ), query.Traits );
        VisitDataSourceBeforeTraits( RemoveCommonTableExpressions( in traits ) );
        VisitInsertIntoFields( node.RecordSet, node.InsertDataFields );
        Context.AppendIndent();
        VisitDataSourceBeforeTraits( in traits );

        Context.Sql.Append( "SELECT" ).AppendSpace().Append( '*' ).AppendSpace().Append( "FROM" ).AppendSpace().Append( '(' );
        using ( Context.TempIndentIncrease() )
        {
            Context.AppendIndent();
            VisitDataSourceQuerySelection( query, in traits );
            VisitDataSource( query.DataSource );
            VisitDataSourceAfterTraits( in traits );
        }

        Context.AppendIndent().Append( ')' ).AppendSpace();
        AppendUpsertSourceAlias( node, includeFieldNames: true );
        AppendUpsertOnDuplicateKey( node );
    }

    protected virtual void VisitUpsertFromCompoundQuery(SqlUpsertNode node, SqlCompoundQueryExpressionNode query)
    {
        Assume.Equals( node.Source, query );
        var traits = ExtractQueryTraits( query.Traits );
        VisitQueryBeforeTraits( RemoveCommonTableExpressions( in traits ) );
        VisitInsertIntoFields( node.RecordSet, node.InsertDataFields );
        Context.AppendIndent();
        VisitQueryBeforeTraits( in traits );

        Context.Sql.Append( "SELECT" ).AppendSpace().Append( '*' ).AppendSpace().Append( "FROM" ).AppendSpace().Append( '(' );
        using ( Context.TempIndentIncrease() )
        {
            Context.AppendIndent();
            VisitCompoundQueryComponents( query, in traits );
            VisitQueryAfterTraits( in traits );
        }

        Context.AppendIndent().Append( ')' ).AppendSpace();
        AppendUpsertSourceAlias( node, includeFieldNames: true );
        AppendUpsertOnDuplicateKey( node );
    }

    protected virtual void VisitUpsertFromRawQuery(SqlUpsertNode node, SqlRawQueryExpressionNode query)
    {
        Assume.Equals( node.Source, query );
        VisitInsertIntoFields( node.RecordSet, node.InsertDataFields );

        Context.AppendIndent().Append( "SELECT" ).AppendSpace().Append( '*' ).AppendSpace().Append( "FROM" ).AppendSpace().Append( '(' );
        using ( Context.TempIndentIncrease() )
        {
            Context.AppendIndent();
            VisitRawQuery( query );
        }

        Context.AppendIndent().Append( ')' ).AppendSpace();
        AppendUpsertSourceAlias( node, includeFieldNames: true );
        AppendUpsertOnDuplicateKey( node );
    }

    protected virtual void VisitUpsertFromGenericSource(SqlUpsertNode node)
    {
        VisitInsertIntoFields( node.RecordSet, node.InsertDataFields );
        Context.AppendIndent();
        this.Visit( node.Source );
        Context.AppendIndent();
        AppendUpsertSourceAlias( node, includeFieldNames: false );
        AppendUpsertOnDuplicateKey( node );
    }

    protected void AppendUpsertSourceAlias(SqlUpsertNode node, bool includeFieldNames)
    {
        Context.Sql.Append( "AS" ).AppendSpace();
        AppendDelimitedName( MySqlHelpers.GetUpdateSourceAlias( Options ) );
        if ( ! includeFieldNames )
            return;

        Context.Sql.Append( '(' );
        if ( node.InsertDataFields.Count > 0 )
        {
            foreach ( var dataField in node.InsertDataFields )
            {
                AppendDelimitedName( dataField.Name );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }

        Context.Sql.Append( ')' );
    }

    protected void AppendUpsertOnDuplicateKey(SqlUpsertNode node)
    {
        Context.AppendIndent().Append( "ON" ).AppendSpace().Append( "DUPLICATE" ).AppendSpace();
        Context.Sql.Append( "KEY" ).AppendSpace().Append( "UPDATE" );

        if ( node.UpdateAssignments.Count > 0 )
        {
            _upsertUpdateSourceReplacement ??= SqlNode.RawRecordSet(
                SqlRecordSetInfo.Create( MySqlHelpers.GetUpdateSourceAlias( Options ) ) );

            using ( TempReplaceRecordSet( node.UpdateSource, _upsertUpdateSourceReplacement ) )
            using ( Context.TempIndentIncrease() )
            {
                foreach ( var assignment in node.UpdateAssignments )
                {
                    Context.AppendIndent();
                    VisitValueAssignment( assignment );
                    Context.Sql.AppendComma();
                }
            }

            Context.Sql.ShrinkBy( 1 );
        }
    }

    protected virtual void VisitDeleteFromWithSingleTable(SqlDeleteFromNode node, in SqlDataSourceTraits traits)
    {
        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "DELETE" ).AppendSpace().Append( "FROM" ).AppendSpace();
        AppendDelimitedRecordSetName( node.DataSource.From );
        VisitDataSourceAfterTraits( in traits );
    }

    protected virtual void VisitDeleteFromWithMultiTable(SqlDeleteFromNode node, in SqlDataSourceTraits traits)
    {
        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "DELETE" ).AppendSpace();
        AppendDelimitedRecordSetName( node.DataSource.From );
        Context.AppendIndent();
        VisitDataSource( node.DataSource );
        VisitDataSourceAfterTraits( in traits );
    }

    protected override void VisitDataSourceAfterTraits(in SqlDataSourceTraits traits)
    {
        base.VisitDataSourceAfterTraits( in traits );
        VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
    }

    protected override void VisitQueryAfterTraits(in SqlQueryTraits traits)
    {
        base.VisitQueryAfterTraits( in traits );
        VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static SqlDataSourceTraits RemoveCommonTableExpressions(in SqlDataSourceTraits traits)
    {
        return traits with
        {
            CommonTableExpressions = Chain<ReadOnlyArray<SqlCommonTableExpressionNode>>.Empty,
            ContainsRecursiveCommonTableExpression = false
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static SqlQueryTraits RemoveCommonTableExpressions(in SqlQueryTraits traits)
    {
        return traits with
        {
            CommonTableExpressions = Chain<ReadOnlyArray<SqlCommonTableExpressionNode>>.Empty,
            ContainsRecursiveCommonTableExpression = false
        };
    }

    [Pure]
    protected virtual bool IsValidSingleTableDeleteStatement(SqlDataSourceNode node, in SqlDataSourceTraits traits)
    {
        return node.RecordSets.Count == 1
            && ! node.From.IsAliased
            && traits.Distinct is null
            && traits.Aggregations.Count == 0
            && traits.AggregationFilter is null
            && traits.Windows.Count == 0
            && traits.Offset is null
            && traits.Custom.Count == 0;
    }

    [Pure]
    protected virtual bool IsValidMultiTableDeleteStatement(SqlDataSourceNode node, in SqlDataSourceTraits traits)
    {
        return traits.Distinct is null
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
            && traits.Offset is null
            && traits.Custom.Count == 0;
    }

    [Pure]
    protected virtual bool IsValidMultiTableUpdateStatement(SqlDataSourceNode node, in SqlDataSourceTraits traits)
    {
        return traits.Distinct is null
            && traits.Aggregations.Count == 0
            && traits.AggregationFilter is null
            && traits.Windows.Count == 0
            && traits.Ordering.Count == 0
            && traits.Limit is null
            && traits.Offset is null
            && traits.Custom.Count == 0;
    }

    protected void VisitMultiUpdateAssignmentRange(ReadOnlyArray<SqlValueAssignmentNode> assignments)
    {
        Context.Sql.AppendSpace().Append( "SET" );

        if ( assignments.Count > 0 )
        {
            using ( Context.TempIndentIncrease() )
            {
                foreach ( var assignment in assignments )
                {
                    Context.AppendIndent();
                    this.Visit( assignment.DataField );
                    Context.Sql.AppendSpace().Append( '=' ).AppendSpace();
                    VisitChild( assignment.Value );
                    Context.Sql.AppendComma();
                }
            }

            Context.Sql.ShrinkBy( 1 );
        }
    }
}
