using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.MySql.Internal.Expressions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using MySqlConnector;

namespace LfrlAnvil.MySql;

public class MySqlNodeInterpreter : SqlNodeInterpreter
{
    private readonly string _maxLimit;
    private readonly string _indexPrefixLength;

    public MySqlNodeInterpreter(
        MySqlColumnTypeDefinitionProvider columnTypeDefinitions,
        string commonSchemaName,
        SqlNodeInterpreterContext context)
        : base( context, beginNameDelimiter: '`', endNameDelimiter: '`' )
    {
        ColumnTypeDefinitions = columnTypeDefinitions;
        CommonSchemaName = commonSchemaName;
        _maxLimit = ulong.MaxValue.ToString( CultureInfo.InvariantCulture );
        _indexPrefixLength = IndexPrefixLength.ToString( CultureInfo.InvariantCulture );
    }

    public MySqlColumnTypeDefinitionProvider ColumnTypeDefinitions { get; }
    public int IndexPrefixLength { get; } = 500;
    public string CommonSchemaName { get; }

    public override void VisitLiteral(SqlLiteralNode node)
    {
        var sql = node.GetSql( ColumnTypeDefinitions );
        Context.Sql.Append( sql );
    }

    public override void VisitRecordsAffectedFunction(SqlRecordsAffectedFunctionExpressionNode node)
    {
        VisitSimpleFunction( "ROW_COUNT", node );
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

    public override void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        Context.Sql.Append( "CAST" ).Append( '(' ).Append( "UNIX_TIMESTAMP" ).Append( '(' );
        Context.Sql.Append( "NOW" ).Append( '(' ).Append( '6' ).Append( ')' ).Append( ')' );
        Context.Sql.AppendSpace().Append( '*' ).AppendSpace().Append( '1' ).Append( '0', repeatCount: 7 );
        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace().Append( "SIGNED" ).Append( ')' );
    }

    public override void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node)
    {
        AppendDelimitedSchemaObjectName( SqlSchemaObjectName.Create( CommonSchemaName, MySqlHelpers.GuidFunctionName ) );
        VisitFunctionArguments( node.Arguments.Span );
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
        if ( node.Arguments.Length == 1 )
            VisitSimpleFunction( "LTRIM", node );
        else
        {
            var args = node.Arguments.Span;
            Context.Sql.Append( "TRIM" ).Append( '(' ).Append( "LEADING" ).AppendSpace();
            VisitChild( args[1] );
            Context.Sql.AppendSpace().Append( "FROM" ).AppendSpace();
            VisitChild( args[0] );
            Context.Sql.Append( ')' );
        }
    }

    public override void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node)
    {
        if ( node.Arguments.Length == 1 )
            VisitSimpleFunction( "RTRIM", node );
        else
        {
            var args = node.Arguments.Span;
            Context.Sql.Append( "TRIM" ).Append( '(' ).Append( "TRAILING" ).AppendSpace();
            VisitChild( args[1] );
            Context.Sql.AppendSpace().Append( "FROM" ).AppendSpace();
            VisitChild( args[0] );
            Context.Sql.Append( ')' );
        }
    }

    public override void VisitTrimFunction(SqlTrimFunctionExpressionNode node)
    {
        if ( node.Arguments.Length == 1 )
            VisitSimpleFunction( "TRIM", node );
        else
        {
            var args = node.Arguments.Span;
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
        var args = node.Arguments.Span;
        Context.Sql.Append( "CHAR_LENGTH" ).Append( '(' );
        VisitChild( args[0] );
        Context.Sql.Append( ')' ).AppendSpace().Append( '-' ).AppendSpace().Append( "CHAR_LENGTH" ).Append( '(' );
        Context.Sql.Append( "SUBSTRING_INDEX" ).Append( '(' );
        VisitChild( args[0] );
        Context.Sql.AppendComma().AppendSpace();
        VisitChild( args[1] );
        Context.Sql.AppendComma().AppendSpace().Append( '-' ).Append( '1' ).Append( ')' ).Append( ')' );
        Context.Sql.AppendSpace().Append( '-' ).AppendSpace().Append( "CHAR_LENGTH" ).Append( '(' );
        VisitChild( args[1] );
        Context.Sql.Append( ')' ).AppendSpace().Append( '+' ).AppendSpace().Append( '1' );
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
        if ( node.Arguments.Length == 1 )
        {
            Context.Sql.Append( "TRUNCATE" ).Append( '(' );
            VisitChild( node.Arguments.Span[0] );
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
        if ( node.Arguments.Length == 1 )
            VisitChild( node.Arguments.Span[0] );
        else
            VisitSimpleFunction( "LEAST", node );
    }

    public override void VisitMaxFunction(SqlMaxFunctionExpressionNode node)
    {
        if ( node.Arguments.Length == 1 )
            VisitChild( node.Arguments.Span[0] );
        else
            VisitSimpleFunction( "GREATEST", node );
    }

    public override void VisitNamedAggregateFunction(SqlNamedAggregateFunctionExpressionNode node)
    {
        var traits = ExtractAggregateFunctionTraits( node.Traits );
        AppendDelimitedSchemaObjectName( node.Name );
        VisitAggregateFunctionArgumentsAndTraits( node.Arguments.Span, traits );
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

        var arguments = node.Arguments.Span;
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

            var sortTraits = FilterTraits(
                traits.Custom,
                static t => t.NodeType == SqlNodeType.SortTrait && ReinterpretCast.To<SqlSortTraitNode>( t ).Ordering.Length > 0 );

            if ( sortTraits.Count > 0 )
            {
                Context.Sql.AppendSpace().Append( "ORDER" ).AppendSpace().Append( "BY" ).AppendSpace();
                foreach ( var trait in sortTraits )
                {
                    foreach ( var orderBy in ReinterpretCast.To<SqlSortTraitNode>( trait ).Ordering )
                    {
                        VisitOrderBy( orderBy );
                        Context.Sql.AppendComma().AppendSpace();
                    }
                }

                Context.Sql.ShrinkBy( 2 );
            }

            if ( arguments.Length > 1 )
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

    public override void VisitJoinOn(SqlDataSourceJoinOnNode node)
    {
        var joinType = node.JoinType switch
        {
            SqlJoinType.Left => "LEFT",
            SqlJoinType.Right => "RIGHT",
            SqlJoinType.Full => throw new UnrecognizedSqlNodeException( this, node ),
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

    public override void VisitDataSource(SqlDataSourceNode node)
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

    public override void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        var isChild = Context.ChildDepth > 0;
        if ( isChild )
            Context.AppendIndent();

        var traits = ExtractDataSourceTraits( ExtractDataSourceTraits( node.DataSource.Traits ), node.Traits );
        VisitDataSourceBeforeTraits( in traits );
        VisitDataSourceQuerySelection( node, traits.Distinct );
        VisitDataSource( node.DataSource );
        VisitDataSourceAfterTraits( in traits );

        if ( isChild )
            Context.AppendShortIndent();
    }

    public override void VisitCompoundQuery(SqlCompoundQueryExpressionNode node)
    {
        var isChild = Context.ChildDepth > 0;
        if ( isChild )
            Context.AppendIndent();

        var traits = ExtractQueryTraits( default, node.Traits );
        VisitQueryBeforeTraits( in traits );
        VisitCompoundQueryComponents( node );
        VisitQueryAfterTraits( in traits );

        if ( isChild )
            Context.AppendShortIndent();
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

        var typeDefinition = ColumnTypeDefinitions.GetByType( node.TargetType );
        var name = typeDefinition.DataType.Value switch
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
            MySqlDbType.String => GetChar( typeDefinition.DataType ),
            MySqlDbType.VarString => GetChar( typeDefinition.DataType ),
            MySqlDbType.VarChar => GetChar( typeDefinition.DataType ),
            MySqlDbType.TinyText => GetChar( typeDefinition.DataType ),
            MySqlDbType.Text => GetChar( typeDefinition.DataType ),
            MySqlDbType.MediumText => GetChar( typeDefinition.DataType ),
            MySqlDbType.LongText => GetChar( typeDefinition.DataType ),
            MySqlDbType.Binary => GetBinary( typeDefinition.DataType ),
            MySqlDbType.VarBinary => GetBinary( typeDefinition.DataType ),
            MySqlDbType.TinyBlob => GetBinary( typeDefinition.DataType ),
            MySqlDbType.Blob => GetBinary( typeDefinition.DataType ),
            MySqlDbType.MediumBlob => GetBinary( typeDefinition.DataType ),
            MySqlDbType.LongBlob => GetBinary( typeDefinition.DataType ),
            _ => typeDefinition.DataType.Name
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

    public override void VisitInsertInto(SqlInsertIntoNode node)
    {
        switch ( node.Source.NodeType )
        {
            case SqlNodeType.DataSourceQuery:
            {
                var query = ReinterpretCast.To<SqlDataSourceQueryExpressionNode>( node.Source );
                var traits = ExtractDataSourceTraits( ExtractDataSourceTraits( query.DataSource.Traits ), query.Traits );

                VisitDataSourceBeforeTraits( RemoveCommonTableExpressions( in traits ) );
                VisitInsertIntoFields( node );
                Context.AppendIndent();
                VisitDataSourceBeforeTraits( in traits );
                VisitDataSourceQuerySelection( query, traits.Distinct );
                VisitDataSource( query.DataSource );
                VisitDataSourceAfterTraits( in traits );
                break;
            }
            case SqlNodeType.CompoundQuery:
            {
                var query = ReinterpretCast.To<SqlCompoundQueryExpressionNode>( node.Source );
                var traits = ExtractQueryTraits( query.Traits );

                VisitQueryBeforeTraits( RemoveCommonTableExpressions( in traits ) );
                VisitInsertIntoFields( node );
                Context.AppendIndent();
                VisitQueryBeforeTraits( in traits );
                VisitCompoundQueryComponents( query );
                VisitQueryAfterTraits( in traits );
                break;
            }
            default:
            {
                VisitInsertIntoFields( node );
                Context.AppendIndent();
                this.Visit( node.Source );
                break;
            }
        }
    }

    public override void VisitUpdate(SqlUpdateNode node)
    {
        var traits = ExtractDataSourceTraits( node.DataSource.Traits );

        if ( IsValidSingleTableUpdateStatement( node.DataSource, in traits ) )
        {
            VisitDataSourceBeforeTraits( in traits );
            Context.Sql.Append( "UPDATE" ).AppendSpace();
            this.Visit( node.DataSource.From );
            VisitUpdateAssignmentRange( node.Assignments.Span );
            VisitDataSourceAfterTraits( in traits );
            return;
        }

        if ( IsValidMultiTableUpdateStatement( node.DataSource, in traits ) )
        {
            VisitDataSourceBeforeTraits( in traits );
            Context.Sql.Append( "UPDATE" ).AppendSpace();

            this.Visit( node.DataSource.From );
            foreach ( var join in node.DataSource.Joins )
            {
                Context.AppendIndent();
                VisitJoinOn( join );
            }

            Context.Sql.AppendSpace().Append( "SET" );

            var assignments = node.Assignments.Span;
            if ( assignments.Length > 0 )
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

            VisitDataSourceAfterTraits( in traits );
            return;
        }

        var targetInfo = ExtractTargetUpdateInfo( node );
        ComplexUpdateAssignmentsVisitor? updateVisitor = null;
        if ( node.DataSource.Joins.Length > 0 )
        {
            updateVisitor = new ComplexUpdateAssignmentsVisitor( node.DataSource );
            updateVisitor.VisitAssignmentRange( node.Assignments.Span );
        }

        if ( updateVisitor is null || ! updateVisitor.ContainsComplexAssignments() )
        {
            VisitDataSourceBeforeTraits( in traits );
            Context.Sql.Append( "UPDATE" ).AppendSpace();
            AppendDelimitedRecordSetName( targetInfo.BaseTarget );

            using ( TempReplaceRecordSet( targetInfo.Target, targetInfo.BaseTarget ) )
                VisitUpdateAssignmentRange( node.Assignments.Span );

            var filter = CreateComplexDeleteOrUpdateFilter( targetInfo, node.DataSource );
            Context.AppendIndent();
            VisitFilterTrait( filter );
            return;
        }

        node = CreateSimplifiedUpdateWithComplexAssignments( targetInfo, node, updateVisitor );
        traits = ExtractDataSourceTraits( node.DataSource.Traits );
        Assume.True( IsValidSingleTableUpdateStatement( node.DataSource, in traits ) );

        VisitDataSourceBeforeTraits( in traits );
        Context.Sql.Append( "UPDATE" ).AppendSpace();
        AppendDelimitedRecordSetName( node.DataSource.From );

        using ( TempReplaceRecordSet( targetInfo.Target, targetInfo.BaseTarget ) )
            VisitUpdateAssignmentRange( node.Assignments.Span );

        VisitDataSourceAfterTraits( in traits );
    }

    public override void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        var traits = ExtractDataSourceTraits( node.DataSource.Traits );

        if ( IsValidSingleTableDeleteStatement( node.DataSource, in traits ) )
        {
            VisitDataSourceBeforeTraits( in traits );
            Context.Sql.Append( "DELETE" ).AppendSpace().Append( "FROM" ).AppendSpace();
            AppendDelimitedRecordSetName( node.DataSource.From );
            VisitDataSourceAfterTraits( in traits );
            return;
        }

        if ( IsValidMultiTableDeleteStatement( node.DataSource, in traits ) )
        {
            VisitDataSourceBeforeTraits( in traits );
            Context.Sql.Append( "DELETE" ).AppendSpace();
            AppendDelimitedRecordSetName( node.DataSource.From );
            Context.AppendIndent();
            VisitDataSource( node.DataSource );
            VisitDataSourceAfterTraits( in traits );
            return;
        }

        var targetInfo = ExtractTargetDeleteInfo( node );
        VisitDataSourceBeforeTraits( in traits );

        Context.Sql.Append( "DELETE" ).AppendSpace().Append( "FROM" ).AppendSpace();
        AppendDelimitedRecordSetName( targetInfo.BaseTarget );

        var filter = CreateComplexDeleteOrUpdateFilter( targetInfo, node.DataSource );
        Context.AppendIndent();
        VisitFilterTrait( filter );
    }

    public override void VisitTruncate(SqlTruncateNode node)
    {
        Context.Sql.Append( "TRUNCATE" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedRecordSetName( node.Table );
    }

    public override void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        var typeDefinition = node.TypeDefinition ?? ColumnTypeDefinitions.GetByType( node.Type.UnderlyingType );
        AppendDelimitedName( node.Name );
        Context.Sql.AppendSpace().Append( typeDefinition.DataType.Name );

        if ( ! node.Type.IsNullable )
            Context.Sql.AppendSpace().Append( "NOT" ).AppendSpace().Append( "NULL" );

        if ( node.DefaultValue is not null )
        {
            Context.Sql.AppendSpace().Append( "DEFAULT" ).AppendSpace();
            var requiresWrappingInParentheses = node.DefaultValue.NodeType is not SqlNodeType.Literal and not SqlNodeType.Null ||
                (typeDefinition.DataType is MySqlDataType mySqlDataType && IsTextOrBlobType( mySqlDataType ));

            if ( requiresWrappingInParentheses )
                VisitChildWrappedInParentheses( node.DefaultValue );
            else
                this.Visit( node.DefaultValue );
        }
    }

    public override void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node)
    {
        Context.Sql.Append( "CONSTRAINT" ).AppendSpace();
        AppendDelimitedName( node.Name.Object );
        Context.Sql.AppendSpace().Append( "PRIMARY" ).AppendSpace().Append( "KEY" ).AppendSpace().Append( '(' );

        if ( node.Columns.Length > 0 )
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

        if ( node.Columns.Length > 0 )
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

        if ( node.ReferencedColumns.Length > 0 )
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

                if ( node.Columns.Length > 0 || node.PrimaryKey is not null || node.ForeignKeys.Length > 0 || node.Checks.Length > 0 )
                    Context.Sql.ShrinkBy( 1 );
            }

            Context.AppendIndent().Append( ')' );
        }
    }

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
            if ( node.Columns.Length > 0 )
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

            if ( node.Filter is not null )
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

    public override void VisitCustom(SqlNodeBase node)
    {
        if ( node is MySqlAlterTableNode alterTable )
            VisitAlterTable( alterTable );
        else
            base.VisitCustom( node );
    }

    public void VisitAlterTable(MySqlAlterTableNode node)
    {
        using ( TempIgnoreAllRecordSets() )
        {
            Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
            AppendDelimitedRecordSetInfo( node.Info );

            using ( Context.TempIndentIncrease() )
            {
                var containsChange = false;

                foreach ( var name in node.OldForeignKeys )
                {
                    Context.AppendIndent().Append( "DROP" ).AppendSpace().Append( "FOREIGN" ).AppendSpace().Append( "KEY" ).AppendSpace();
                    AppendDelimitedName( name );
                    Context.Sql.AppendComma();
                    containsChange = true;
                }

                if ( node.NewPrimaryKey is not null )
                {
                    Context.AppendIndent();
                    Context.Sql.Append( "DROP" ).AppendSpace().Append( "PRIMARY" ).AppendSpace().Append( "KEY" ).AppendComma();
                    containsChange = true;
                }

                foreach ( var name in node.OldChecks )
                {
                    Context.AppendIndent().Append( "DROP" ).AppendSpace().Append( "CHECK" ).AppendSpace();
                    AppendDelimitedName( name );
                    Context.Sql.AppendComma();
                    containsChange = true;
                }

                foreach ( var (oldName, newName) in node.RenamedIndexes )
                {
                    Context.AppendIndent();
                    Context.Sql.Append( "RENAME" ).AppendSpace().Append( "INDEX" ).AppendSpace();
                    AppendDelimitedName( oldName );
                    Context.Sql.AppendSpace().Append( "TO" ).AppendSpace();
                    AppendDelimitedName( newName );
                    Context.Sql.AppendComma();
                    containsChange = true;
                }

                foreach ( var name in node.OldColumns )
                {
                    Context.AppendIndent().Append( "DROP" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
                    AppendDelimitedName( name );
                    Context.Sql.AppendComma();
                    containsChange = true;
                }

                foreach ( var (oldName, column) in node.ChangedColumns )
                {
                    Context.AppendIndent().Append( "CHANGE" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
                    AppendDelimitedName( oldName );
                    Context.Sql.AppendSpace();
                    VisitColumnDefinition( column );
                    Context.Sql.AppendComma();
                    containsChange = true;
                }

                foreach ( var column in node.NewColumns )
                {
                    Context.AppendIndent().Append( "ADD" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
                    VisitColumnDefinition( column );
                    Context.Sql.AppendComma();
                    containsChange = true;
                }

                if ( node.NewPrimaryKey is not null )
                {
                    Context.AppendIndent();
                    Context.Sql.Append( "ADD" ).AppendSpace();
                    VisitPrimaryKeyDefinition( node.NewPrimaryKey );
                    Context.Sql.AppendComma();
                }

                foreach ( var check in node.NewChecks )
                {
                    Context.AppendIndent().Append( "ADD" ).AppendSpace();
                    VisitCheckDefinition( check );
                    Context.Sql.AppendComma();
                    containsChange = true;
                }

                foreach ( var foreignKey in node.NewForeignKeys )
                {
                    Context.AppendIndent().Append( "ADD" ).AppendSpace();
                    VisitForeignKeyDefinition( foreignKey );
                    Context.Sql.AppendComma();
                    containsChange = true;
                }

                if ( containsChange )
                    Context.Sql.ShrinkBy( 1 );
            }
        }
    }

    public override void AppendDelimitedRecordSetName(SqlRecordSetNode node)
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
                return ReinterpretCast.To<SqlFunctionExpressionNode>( node ).FunctionType == SqlFunctionType.LastIndexOf;

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
        VisitAggregateFunctionArgumentsAndTraits( node.Arguments.Span, traits );
    }

    protected void VisitAggregateFunctionArgumentsAndTraits(ReadOnlySpan<SqlExpressionNode> arguments, SqlAggregateFunctionTraits traits)
    {
        Context.Sql.Append( '(' );

        if ( arguments.Length > 0 )
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
                    VisitChild( arguments[0] );
                    Context.Sql.AppendSpace().Append( "ELSE" ).AppendSpace().Append( "NULL" ).AppendSpace().Append( "END" );

                    arguments = arguments.Slice( 1 );
                    if ( arguments.Length > 0 )
                        Context.Sql.AppendComma().AppendSpace();
                }

                foreach ( var arg in arguments )
                {
                    VisitChild( arg );
                    Context.Sql.AppendComma().AppendSpace();
                }
            }

            if ( arguments.Length > 0 )
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

        Context.AppendIndent().Append( "LIMIT" ).AppendSpace().Append( _maxLimit );
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
                var typeDefinition = ColumnTypeDefinitions.GetByType( type.Value.UnderlyingType );
                if ( IsTextOrBlobType( typeDefinition.DataType ) )
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
        return dataType.Value is MySqlDbType.TinyText or MySqlDbType.Text
            or MySqlDbType.MediumText or MySqlDbType.LongText or MySqlDbType.TinyBlob or MySqlDbType.Blob or MySqlDbType.MediumBlob
            or MySqlDbType.LongBlob;
    }

    protected virtual void VisitInsertIntoFields(SqlInsertIntoNode node)
    {
        Context.Sql.Append( "INSERT INTO" ).AppendSpace();
        AppendDelimitedRecordSetName( node.RecordSet );
        Context.Sql.AppendSpace().Append( '(' );

        if ( node.DataFields.Length > 0 )
        {
            foreach ( var dataField in node.DataFields )
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

    protected void VisitCompoundQueryComponents(SqlCompoundQueryExpressionNode node)
    {
        VisitChild( node.FirstQuery );
        foreach ( var component in node.FollowingQueries )
        {
            Context.AppendIndent();
            VisitCompoundQueryComponent( component );
        }
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
        VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
    }

    protected virtual void VisitQueryBeforeTraits(in SqlQueryTraits traits)
    {
        VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions, traits.ContainsRecursiveCommonTableExpression );
    }

    protected virtual void VisitQueryAfterTraits(in SqlQueryTraits traits)
    {
        VisitOptionalOrderingRange( traits.Ordering );
        VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static SqlDataSourceTraits RemoveCommonTableExpressions(in SqlDataSourceTraits traits)
    {
        return traits with
        {
            CommonTableExpressions = Chain<ReadOnlyMemory<SqlCommonTableExpressionNode>>.Empty,
            ContainsRecursiveCommonTableExpression = false
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static SqlQueryTraits RemoveCommonTableExpressions(in SqlQueryTraits traits)
    {
        return traits with
        {
            CommonTableExpressions = Chain<ReadOnlyMemory<SqlCommonTableExpressionNode>>.Empty,
            ContainsRecursiveCommonTableExpression = false
        };
    }

    [Pure]
    protected static bool IsValidSingleTableDeleteStatement(SqlDataSourceNode node, in SqlDataSourceTraits traits)
    {
        return node.RecordSets.Count == 1 &&
            ! node.From.IsAliased &&
            traits.Distinct is null &&
            traits.Aggregations.Count == 0 &&
            traits.AggregationFilter is null &&
            traits.Windows.Count == 0 &&
            traits.Offset is null &&
            traits.Custom.Count == 0;
    }

    [Pure]
    protected static bool IsValidMultiTableDeleteStatement(SqlDataSourceNode node, in SqlDataSourceTraits traits)
    {
        return traits.Distinct is null &&
            traits.Aggregations.Count == 0 &&
            traits.AggregationFilter is null &&
            traits.Windows.Count == 0 &&
            traits.Ordering.Count == 0 &&
            traits.Limit is null &&
            traits.Offset is null &&
            traits.Custom.Count == 0;
    }

    [Pure]
    protected static bool IsValidSingleTableUpdateStatement(SqlDataSourceNode node, in SqlDataSourceTraits traits)
    {
        return node.RecordSets.Count == 1 &&
            traits.Distinct is null &&
            traits.Aggregations.Count == 0 &&
            traits.AggregationFilter is null &&
            traits.Windows.Count == 0 &&
            traits.Offset is null &&
            traits.Custom.Count == 0;
    }

    [Pure]
    protected static bool IsValidMultiTableUpdateStatement(SqlDataSourceNode node, in SqlDataSourceTraits traits)
    {
        return traits.Distinct is null &&
            traits.Aggregations.Count == 0 &&
            traits.AggregationFilter is null &&
            traits.Windows.Count == 0 &&
            traits.Ordering.Count == 0 &&
            traits.Limit is null &&
            traits.Offset is null &&
            traits.Custom.Count == 0;
    }

    [Pure]
    protected static SqlFilterTraitNode CreateComplexDeleteOrUpdateFilter(TargetDeleteOrUpdateInfo targetInfo, SqlDataSourceNode dataSource)
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
            dataSource = dataSource.SetTraits( traits ).Select( pkColumnNode.AsSelf() ).AsSet( $"_{Guid.NewGuid():N}" ).ToDataSource();
            return SqlNode.FilterTrait( pkBaseColumnNode.InQuery( dataSource.Select( dataSource.GetAll() ) ), isConjunction: true );
        }

        var pkSelection = new SqlSelectNode[targetInfo.IdentityColumnNames.Length];
        for ( var i = 0; i < targetInfo.IdentityColumnNames.Length; ++i )
            pkSelection[i] = targetInfo.Target.GetUnsafeField( targetInfo.IdentityColumnNames[i] ).AsSelf();

        dataSource = dataSource.SetTraits( traits ).Select( pkSelection ).AsSet( $"_{Guid.NewGuid():N}" ).ToDataSource();

        pkColumnNode = dataSource.From.GetUnsafeField( pkColumnNode.Name );
        var pkFilter = pkBaseColumnNode == pkColumnNode;
        foreach ( var name in targetInfo.IdentityColumnNames.AsSpan( 1 ) )
        {
            pkBaseColumnNode = targetInfo.BaseTarget.GetUnsafeField( name );
            pkColumnNode = dataSource.From.GetUnsafeField( name );
            pkFilter = pkFilter.And( pkBaseColumnNode == pkColumnNode );
        }

        return SqlNode.FilterTrait( dataSource.AndWhere( pkFilter ).Exists(), isConjunction: true );
    }

    [Pure]
    protected static SqlUpdateNode CreateSimplifiedUpdateWithComplexAssignments(
        TargetDeleteOrUpdateInfo targetInfo,
        SqlUpdateNode node,
        ComplexUpdateAssignmentsVisitor updateVisitor)
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

        var indexesOfComplexAssignments = updateVisitor.GetIndexesOfComplexAssignments();
        var cteIdentityFieldNames = new string[targetInfo.IdentityColumnNames.Length];
        var cteComplexAssignmentFieldNames = new string[indexesOfComplexAssignments.Length];
        var cteSelection = new SqlSelectNode[targetInfo.IdentityColumnNames.Length + indexesOfComplexAssignments.Length];
        var assignments = node.Assignments.ToArray();

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

    [Pure]
    protected static TargetDeleteOrUpdateInfo ExtractTableDeleteOrUpdateInfo(SqlTableNode node)
    {
        var identityColumns = node.Table.Constraints.PrimaryKey.Index.Columns.Span;
        var identityColumnNames = new string[identityColumns.Length];
        for ( var i = 0; i < identityColumns.Length; ++i )
            identityColumnNames[i] = identityColumns[i].Column.Name;

        return new TargetDeleteOrUpdateInfo( node, node.AsSelf(), identityColumnNames );
    }

    [Pure]
    protected TargetDeleteOrUpdateInfo ExtractTableBuilderDeleteInfo(SqlDeleteFromNode node, SqlTableBuilderNode target)
    {
        return ExtractTableBuilderDeleteOrUpdateInfo( node, target, isUpdate: false );
    }

    [Pure]
    protected TargetDeleteOrUpdateInfo ExtractTableBuilderUpdateInfo(SqlUpdateNode node, SqlTableBuilderNode target)
    {
        return ExtractTableBuilderDeleteOrUpdateInfo( node, target, isUpdate: true );
    }

    [Pure]
    protected TargetDeleteOrUpdateInfo ExtractNewTableDeleteInfo(
        SqlDeleteFromNode node,
        SqlNewTableNode target)
    {
        return ExtractNewTableDeleteOrUpdateInfo( node, target, isUpdate: false );
    }

    [Pure]
    protected TargetDeleteOrUpdateInfo ExtractNewTableUpdateInfo(SqlUpdateNode node, SqlNewTableNode target)
    {
        return ExtractNewTableDeleteOrUpdateInfo( node, target, isUpdate: true );
    }

    [Pure]
    protected TargetDeleteOrUpdateInfo ExtractTargetDeleteInfo(SqlDeleteFromNode node)
    {
        var from = node.DataSource.From;
        var info = from.NodeType switch
        {
            SqlNodeType.Table => ExtractTableDeleteOrUpdateInfo( ReinterpretCast.To<SqlTableNode>( from ) ),
            SqlNodeType.TableBuilder => ExtractTableBuilderDeleteInfo(
                node,
                ReinterpretCast.To<SqlTableBuilderNode>( from ) ),
            SqlNodeType.NewTable => ExtractNewTableDeleteInfo(
                node,
                ReinterpretCast.To<SqlNewTableNode>( from ) ),
            _ => throw new SqlNodeVisitorException( Resources.DeleteTargetIsNotTable, this, node )
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
            SqlNodeType.Table => ExtractTableDeleteOrUpdateInfo( ReinterpretCast.To<SqlTableNode>( from ) ),
            SqlNodeType.TableBuilder => ExtractTableBuilderUpdateInfo(
                node,
                ReinterpretCast.To<SqlTableBuilderNode>( from ) ),
            SqlNodeType.NewTable => ExtractNewTableUpdateInfo(
                node,
                ReinterpretCast.To<SqlNewTableNode>( from ) ),
            _ => throw new SqlNodeVisitorException( Resources.UpdateTargetIsNotTable, this, node )
        };

        if ( ! from.IsAliased )
            throw new SqlNodeVisitorException( Resources.UpdateTargetIsNotAliased, this, node );

        return info;
    }

    protected readonly record struct TargetDeleteOrUpdateInfo(
        SqlRecordSetNode Target,
        SqlRecordSetNode BaseTarget,
        string[] IdentityColumnNames);

    protected sealed class ComplexUpdateAssignmentsVisitor : SqlNodeVisitor
    {
        private readonly SqlRecordSetNode[] _joinedRecordSets;
        private List<int>? _indexesOfComplexAssignments;
        private int _nextAssignmentIndex;

        public ComplexUpdateAssignmentsVisitor(SqlDataSourceNode dataSource)
        {
            Assume.IsGreaterThan( dataSource.Joins.Length, 0 );

            var index = 0;
            _joinedRecordSets = new SqlRecordSetNode[dataSource.Joins.Length];
            foreach ( var join in dataSource.Joins )
                _joinedRecordSets[index++] = join.InnerRecordSet;

            _indexesOfComplexAssignments = null;
            _nextAssignmentIndex = 0;
        }

        public void VisitAssignmentRange(ReadOnlySpan<SqlValueAssignmentNode> assignments)
        {
            Assume.Equals( _nextAssignmentIndex, 0 );
            foreach ( var assignment in assignments )
            {
                this.Visit( assignment.Value );
                ++_nextAssignmentIndex;
            }
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

    [Pure]
    private TargetDeleteOrUpdateInfo ExtractTableBuilderDeleteOrUpdateInfo(
        SqlNodeBase source,
        SqlTableBuilderNode node,
        bool isUpdate)
    {
        string[] identityColumnNames;
        var primaryKey = node.Table.Constraints.TryGetPrimaryKey();

        if ( primaryKey is not null )
        {
            var identityColumns = primaryKey.Index.Columns;
            identityColumnNames = new string[identityColumns.Count];

            var i = 0;
            foreach ( var c in identityColumns )
                identityColumnNames[i++] = c.Column.Name;
        }
        else
        {
            var index = 0;
            var identityColumns = node.Table.Columns;
            if ( identityColumns.Count == 0 )
            {
                var reason = isUpdate ? Resources.UpdateTargetDoesNotHaveAnyColumns : Resources.DeleteTargetDoesNotHaveAnyColumns;
                throw new SqlNodeVisitorException( reason, this, source );
            }

            identityColumnNames = new string[identityColumns.Count];
            foreach ( var column in identityColumns )
                identityColumnNames[index++] = column.Name;
        }

        return new TargetDeleteOrUpdateInfo( node, node.AsSelf(), identityColumnNames );
    }

    [Pure]
    private TargetDeleteOrUpdateInfo ExtractNewTableDeleteOrUpdateInfo(
        SqlNodeBase source,
        SqlNewTableNode node,
        bool isUpdate)
    {
        string[] identityColumnNames;
        var primaryKey = node.CreationNode.PrimaryKey;

        if ( primaryKey is not null )
        {
            var identityColumns = primaryKey.Columns.Span;
            if ( identityColumns.Length == 0 )
            {
                var reason = isUpdate ? Resources.UpdateTargetDoesNotHaveAnyColumns : Resources.DeleteTargetDoesNotHaveAnyColumns;
                throw new SqlNodeVisitorException( reason, this, source );
            }

            identityColumnNames = new string[identityColumns.Length];
            for ( var i = 0; i < identityColumns.Length; ++i )
            {
                if ( identityColumns[i].Expression is not SqlDataFieldNode dataField )
                {
                    var reason = Resources.DeleteOrUpdateTargetPrimaryKeyColumnIsComplexExpression(
                        isUpdate,
                        i,
                        identityColumns[i].Expression );

                    throw new SqlNodeVisitorException( reason, this, source );
                }

                identityColumnNames[i] = dataField.Name;
            }
        }
        else
        {
            var index = 0;
            var identityColumns = node.CreationNode.Columns.Span;
            if ( identityColumns.Length == 0 )
            {
                var reason = isUpdate ? Resources.UpdateTargetDoesNotHaveAnyColumns : Resources.DeleteTargetDoesNotHaveAnyColumns;
                throw new SqlNodeVisitorException( reason, this, source );
            }

            identityColumnNames = new string[identityColumns.Length];
            foreach ( var column in identityColumns )
                identityColumnNames[index++] = column.Name;
        }

        return new TargetDeleteOrUpdateInfo( node, node.AsSelf(), identityColumnNames );
    }
}
