using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public sealed class SqlNodeDebugInterpreter : SqlNodeInterpreter
{
    public SqlNodeDebugInterpreter()
        : base( SqlNodeInterpreterContext.Create(), beginNameDelimiter: '[', endNameDelimiter: ']' ) { }

    public override void VisitRawDataField(SqlRawDataFieldNode node)
    {
        base.VisitRawDataField( node );
        AppendExpressionType( node.Type );
    }

    public override void VisitLiteral(SqlLiteralNode node)
    {
        Context.Sql.Append( '"' ).Append( node.GetValue() ).Append( '"' );
        AppendExpressionType( node.Type );
    }

    public override void VisitParameter(SqlParameterNode node)
    {
        base.VisitParameter( node );
        AppendExpressionType( node.Type );
    }

    public override void VisitColumn(SqlColumnNode node)
    {
        base.VisitColumn( node );
        AppendExpressionType( node.Type );
    }

    public override void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        base.VisitColumnBuilder( node );
        AppendExpressionType( node.Type );
    }

    public override void VisitRecordsAffectedFunction(SqlRecordsAffectedFunctionExpressionNode node)
    {
        VisitSimpleFunction( "RECORDS_AFFECTED", node );
    }

    public override void VisitCoalesceFunction(SqlCoalesceFunctionExpressionNode node)
    {
        VisitSimpleFunction( "COALESCE", node );
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
        VisitSimpleFunction( "TRIM_START", node );
    }

    public override void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TRIM_END", node );
    }

    public override void VisitTrimFunction(SqlTrimFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TRIM", node );
    }

    public override void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SUBSTRING", node );
    }

    public override void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node)
    {
        VisitSimpleFunction( "REPLACE", node );
    }

    public override void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node)
    {
        VisitSimpleFunction( "INDEX_OF", node );
    }

    public override void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node)
    {
        VisitSimpleFunction( "LAST_INDEX_OF", node );
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
        VisitSimpleFunction( "CEILING", node );
    }

    public override void VisitFloorFunction(SqlFloorFunctionExpressionNode node)
    {
        VisitSimpleFunction( "FLOOR", node );
    }

    public override void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TRUNCATE", node );
    }

    public override void VisitPowerFunction(SqlPowerFunctionExpressionNode node)
    {
        VisitSimpleFunction( "POWER", node );
    }

    public override void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SQUARE_ROOT", node );
    }

    public override void VisitMinFunction(SqlMinFunctionExpressionNode node)
    {
        VisitSimpleFunction( "MIN", node );
    }

    public override void VisitMaxFunction(SqlMaxFunctionExpressionNode node)
    {
        VisitSimpleFunction( "MAX", node );
    }

    public override void VisitCustomFunction(SqlFunctionExpressionNode node)
    {
        VisitSimpleFunction( $"{{{node.GetType().GetDebugString()}}}", node );
    }

    public override void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", "MIN", node );
    }

    public override void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", "MAX", node );
    }

    public override void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", "AVERAGE", node );
    }

    public override void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", "SUM", node );
    }

    public override void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", "COUNT", node );
    }

    public override void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", "STRING_CONCAT", node );
    }

    public override void VisitRowNumberWindowFunction(SqlRowNumberWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "ROW_NUMBER", node );
    }

    public override void VisitRankWindowFunction(SqlRankWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "RANK", node );
    }

    public override void VisitDenseRankWindowFunction(SqlDenseRankWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "DENSE_RANK", node );
    }

    public override void VisitCumulativeDistributionWindowFunction(SqlCumulativeDistributionWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "CUMULATIVE_DISTRIBUTION", node );
    }

    public override void VisitNTileWindowFunction(SqlNTileWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "N_TILE", node );
    }

    public override void VisitLagWindowFunction(SqlLagWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "LAG", node );
    }

    public override void VisitLeadWindowFunction(SqlLeadWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "LEAD", node );
    }

    public override void VisitFirstValueWindowFunction(SqlFirstValueWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "FIRST_VALUE", node );
    }

    public override void VisitLastValueWindowFunction(SqlLastValueWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "LAST_VALUE", node );
    }

    public override void VisitNthValueWindowFunction(SqlNthValueWindowFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "WND", "NTH_VALUE", node );
    }

    public override void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        VisitSimpleAggregateFunction( "AGG", $"{{{node.GetType().GetDebugString()}}}", node );
    }

    public override void VisitTrue(SqlTrueNode node)
    {
        Context.Sql.Append( "TRUE" );
    }

    public override void VisitFalse(SqlFalseNode node)
    {
        Context.Sql.Append( "FALSE" );
    }

    public override void VisitEqualTo(SqlEqualToConditionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "==", node.Right );
    }

    public override void VisitNotEqualTo(SqlNotEqualToConditionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "<>", node.Right );
    }

    public override void VisitConditionValue(SqlConditionValueNode node)
    {
        Context.Sql.Append( "CONDITION_VALUE" );
        VisitChild( node.Condition );
    }

    public override void VisitRawRecordSet(SqlRawRecordSetNode node)
    {
        AppendDelimitedRecordSetInfo( node.Info );
        AppendDelimitedAlias( node.Alias );
    }

    public override void VisitDataSource(SqlDataSourceNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
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
    }

    public override void VisitSelectField(SqlSelectFieldNode node)
    {
        VisitChild( node.Expression );
        if ( node.Alias is not null )
            AppendDelimitedAlias( node.Alias );
    }

    public override void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        var hasParentNode = Context.ParentNode is not null;
        if ( hasParentNode )
            Context.AppendIndent();

        using ( Context.TempParentNodeUpdate( node ) )
        {
            this.Visit( node.DataSource );
            VisitTraits( node.Traits );
            Context.AppendIndent().Append( "SELECT" );

            if ( node.Selection.Length == 0 )
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
            VisitChild( node.FirstQuery );

            foreach ( var component in node.FollowingQueries )
            {
                Context.AppendIndent();
                VisitCompoundQueryComponent( component );
            }

            VisitTraits( node.Traits );
        }

        if ( hasParentNode )
            Context.AppendShortIndent();
    }

    public override void VisitFilterTrait(SqlFilterTraitNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( node.IsConjunction ? "AND" : "OR" ).AppendSpace().Append( "WHERE" ).AppendSpace();
            this.Visit( node.Filter );
        }
    }

    public override void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( node.IsConjunction ? "AND" : "OR" ).AppendSpace().Append( "HAVING" ).AppendSpace();
            this.Visit( node.Filter );
        }
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

    public override void VisitWindowTrait(SqlWindowTraitNode node)
    {
        Context.Sql.Append( "OVER" ).AppendSpace();
        VisitWindowDefinition( node.Definition );
    }

    public override void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( node.IsRecursive ? "RECURSIVE" : "ORDINAL" ).AppendSpace();
            AppendDelimitedName( node.Name );
            Context.Sql.AppendSpace();
            VisitChild( node.Query );
        }
    }

    public override void VisitTypeCast(SqlTypeCastExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "CAST" ).Append( '(' );
            VisitChild( node.Value );
            Context.Sql.AppendSpace().Append( "AS" ).AppendSpace().Append( node.TargetType.GetDebugString() ).Append( ')' );
        }
    }

    public override void VisitInsertInto(SqlInsertIntoNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "INSERT INTO" ).AppendSpace();

            AppendDelimitedRecordSetName( node.RecordSet.AsSelf() );
            AppendDelimitedAlias( node.RecordSet.Alias );

            Context.Sql.AppendSpace().Append( '(' );

            if ( node.DataFields.Length > 0 )
            {
                foreach ( var dataField in node.DataFields )
                {
                    this.Visit( dataField );
                    Context.Sql.AppendComma().AppendSpace();
                }

                Context.Sql.ShrinkBy( 2 );
            }

            Context.Sql.Append( ')' );
        }

        Context.AppendIndent();
        this.Visit( node.Source );
    }

    public override void VisitUpdate(SqlUpdateNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "UPDATE" ).AppendSpace();
            this.Visit( node.DataSource );
            Context.AppendIndent().Append( "SET" );

            if ( node.Assignments.Length == 0 )
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
    }

    public override void VisitValueAssignment(SqlValueAssignmentNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.DataField, symbol: "=", node.Value );
    }

    public override void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "DELETE" ).AppendSpace();
            this.Visit( node.DataSource );
        }
    }

    public override void VisitTruncate(SqlTruncateNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "TRUNCATE" ).AppendSpace();
            AppendDelimitedRecordSetName( node.Table );
        }
    }

    public override void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        AppendDelimitedName( node.Name );
        AppendExpressionType( node.Type );

        if ( node.DefaultValue is not null )
        {
            Context.Sql.AppendSpace().Append( "DEFAULT" ).AppendSpace();
            VisitChild( node.DefaultValue );
        }
    }

    public override void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node)
    {
        Context.Sql.Append( "PRIMARY" ).AppendSpace().Append( "KEY" ).AppendSpace();
        AppendDelimitedName( node.Name );
        Context.Sql.AppendSpace().Append( '(' );

        if ( node.Columns.Length > 0 )
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

    public override void VisitForeignKeyDefinition(SqlForeignKeyDefinitionNode node)
    {
        Context.Sql.Append( "FOREIGN" ).AppendSpace().Append( "KEY" ).AppendSpace();
        AppendDelimitedName( node.Name );
        Context.Sql.AppendSpace().Append( '(' );

        if ( node.Columns.Length > 0 )
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

        if ( node.ReferencedColumns.Length > 0 )
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

    public override void VisitCheckDefinition(SqlCheckDefinitionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "CHECK" ).AppendSpace();
            AppendDelimitedName( node.Name );
            Context.Sql.AppendSpace();
            VisitChildWrappedInParentheses( node.Condition );
        }
    }

    public override void VisitCreateTable(SqlCreateTableNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "CREATE" ).AppendSpace().Append( "TABLE" ).AppendSpace();
            if ( node.IfNotExists )
                Context.Sql.Append( "IF" ).AppendSpace().Append( "NOT" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

            AppendDelimitedRecordSetInfo( node.Info );
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
        Context.Sql.Append( "CREATE" ).AppendSpace().Append( "VIEW" ).AppendSpace();
        if ( node.IfNotExists )
            Context.Sql.Append( "IF" ).AppendSpace().Append( "NOT" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        AppendDelimitedRecordSetInfo( node.Info );
        Context.Sql.AppendSpace().Append( "AS" );
        Context.AppendIndent();
        this.Visit( node.Source );
    }

    public override void VisitCreateIndex(SqlCreateIndexNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "CREATE" ).AppendSpace();
            if ( node.IsUnique )
                Context.Sql.Append( "UNIQUE" ).AppendSpace();

            Context.Sql.Append( "INDEX" ).AppendSpace();
            if ( node.IfNotExists )
                Context.Sql.Append( "IF" ).AppendSpace().Append( "NOT" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

            AppendDelimitedSchemaObjectName( node.Name );
            Context.Sql.AppendSpace().Append( "ON" ).AppendSpace();
            AppendDelimitedRecordSetName( node.Table );

            Context.Sql.AppendSpace().Append( '(' );
            if ( node.Columns.Length > 0 )
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

    public override void VisitRenameTable(SqlRenameTableNode node)
    {
        Context.Sql.Append( "RENAME" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendSpace().Append( "TO" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.NewName );
    }

    public override void VisitRenameColumn(SqlRenameColumnNode node)
    {
        Context.Sql.Append( "RENAME" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendDot();
        AppendDelimitedName( node.OldName );
        Context.Sql.AppendSpace().Append( "TO" ).AppendSpace();
        AppendDelimitedName( node.NewName );
    }

    public override void VisitAddColumn(SqlAddColumnNode node)
    {
        Context.Sql.Append( "ADD" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendDot();
        VisitColumnDefinition( node.Definition );
    }

    public override void VisitDropColumn(SqlDropColumnNode node)
    {
        Context.Sql.Append( "DROP" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendDot();
        AppendDelimitedName( node.Name );
    }

    public override void VisitDropTable(SqlDropTableNode node)
    {
        Context.Sql.Append( "DROP" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        if ( node.IfExists )
            Context.Sql.Append( "IF" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        AppendDelimitedRecordSetInfo( node.Table );
    }

    public override void VisitDropView(SqlDropViewNode node)
    {
        Context.Sql.Append( "DROP" ).AppendSpace().Append( "VIEW" ).AppendSpace();
        if ( node.IfExists )
            Context.Sql.Append( "IF" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        AppendDelimitedRecordSetInfo( node.View );
    }

    public override void VisitDropIndex(SqlDropIndexNode node)
    {
        Context.Sql.Append( "DROP" ).AppendSpace().Append( "INDEX" ).AppendSpace();
        if ( node.IfExists )
            Context.Sql.Append( "IF" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        AppendDelimitedSchemaObjectName( node.Name );
    }

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

    public override void VisitBeginTransaction(SqlBeginTransactionNode node)
    {
        Context.Sql
            .Append( "BEGIN" )
            .AppendSpace()
            .Append( node.IsolationLevel.ToString().ToUpperInvariant() )
            .AppendSpace()
            .Append( "TRANSACTION" );
    }

    public override void VisitCustom(SqlNodeBase node)
    {
        Context.Sql.Append( '{' ).Append( node.GetType().GetDebugString() ).Append( '}' );
    }

    public override void AppendDelimitedRecordSetName(SqlRecordSetNode node)
    {
        if ( node.IsAliased )
            AppendDelimitedName( node.Alias );
        else
            AppendDelimitedRecordSetInfo( node.Info );
    }

    [Pure]
    protected override bool DoesChildNodeRequireParentheses(SqlNodeBase node)
    {
        return true;
    }

    protected override void VisitCustomWindowFrame(SqlWindowFrameNode node)
    {
        Context.Sql.Append( '{' ).Append( node.GetType().GetDebugString() ).Append( '}' ).AppendSpace().Append( "BETWEEN" ).AppendSpace();
        AppendWindowFrameBoundary( node.Start );
        Context.Sql.AppendSpace().Append( "AND" ).AppendSpace();
        AppendWindowFrameBoundary( node.End );
    }

    private void AppendExpressionType(SqlExpressionType? type)
    {
        Context.Sql.AppendSpace().Append( ':' ).AppendSpace();
        if ( type is null )
            Context.Sql.Append( '?' );
        else
            Context.Sql.Append( type.Value.ToString() );
    }

    private void VisitSimpleAggregateFunction(string prefix, string functionName, SqlAggregateFunctionExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( prefix ).Append( '_' ).Append( functionName ).Append( '(' );

            if ( node.Arguments.Length > 0 )
            {
                using ( Context.TempIndentIncrease() )
                {
                    foreach ( var arg in node.Arguments )
                    {
                        VisitChild( arg );
                        Context.Sql.AppendComma().AppendSpace();
                    }
                }

                Context.Sql.ShrinkBy( 2 );
            }

            Context.Sql.Append( ')' );

            using ( Context.TempIndentIncrease() )
                VisitTraits( node.Traits );
        }
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
