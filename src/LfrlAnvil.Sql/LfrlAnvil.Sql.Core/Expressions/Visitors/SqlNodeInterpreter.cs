using System;
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
    }

    public SqlNodeInterpreterContext Context { get; }

    public virtual void VisitRawExpression(SqlRawExpressionNode node)
    {
        AppendMultilineSql( node.Sql );

        foreach ( var parameter in node.Parameters.Span )
            Context.AddParameter( parameter.Name, parameter.Type );
    }

    public virtual void VisitRawDataField(SqlRawDataFieldNode node)
    {
        AppendRecordSetName( node.RecordSet );
        Context.Sql.AppendDot();
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
        AppendRecordSetName( node.RecordSet );
        Context.Sql.AppendDot();
        AppendDelimitedName( node.Name );
    }

    public virtual void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        AppendRecordSetName( node.RecordSet );
        Context.Sql.AppendDot();
        AppendDelimitedName( node.Name );
    }

    public virtual void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        AppendRecordSetName( node.RecordSet );
        Context.Sql.AppendDot();
        AppendDelimitedName( node.Name );
    }

    public virtual void VisitViewDataField(SqlViewDataFieldNode node)
    {
        AppendRecordSetName( node.RecordSet );
        Context.Sql.AppendDot();
        AppendDelimitedName( node.Name );
    }

    public virtual void VisitNegate(SqlNegateExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitPrefixUnaryOperator( node.Value, symbol: "-" );
    }

    public virtual void VisitAdd(SqlAddExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "+", node.Right );
    }

    public virtual void VisitConcat(SqlConcatExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "||", node.Right );
    }

    public virtual void VisitSubtract(SqlSubtractExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "-", node.Right );
    }

    public virtual void VisitMultiply(SqlMultiplyExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "*", node.Right );
    }

    public virtual void VisitDivide(SqlDivideExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "/", node.Right );
    }

    public virtual void VisitModulo(SqlModuloExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "%", node.Right );
    }

    public virtual void VisitBitwiseNot(SqlBitwiseNotExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitPrefixUnaryOperator( node.Value, symbol: "~" );
    }

    public virtual void VisitBitwiseAnd(SqlBitwiseAndExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "&", node.Right );
    }

    public virtual void VisitBitwiseOr(SqlBitwiseOrExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "|", node.Right );
    }

    public virtual void VisitBitwiseXor(SqlBitwiseXorExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "^", node.Right );
    }

    public virtual void VisitBitwiseLeftShift(SqlBitwiseLeftShiftExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "<<", node.Right );
    }

    public virtual void VisitBitwiseRightShift(SqlBitwiseRightShiftExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: ">>", node.Right );
    }

    public virtual void VisitSwitchCase(SqlSwitchCaseNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "WHEN" ).AppendSpace();
            this.Visit( node.Condition );

            using ( Context.TempIndentIncrease() )
            {
                Context.AppendIndent().Append( "THEN" ).AppendSpace();
                VisitChild( node.Expression );
            }
        }
    }

    public virtual void VisitSwitch(SqlSwitchExpressionNode node)
    {
        var hasParentNode = Context.ParentNode is not null;
        if ( hasParentNode )
            Context.AppendIndent();

        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "CASE" );

            using ( Context.TempIndentIncrease() )
            {
                foreach ( var @case in node.Cases.Span )
                {
                    Context.AppendIndent();
                    VisitSwitchCase( @case );
                }

                Context.AppendIndent().Append( "ELSE" ).AppendSpace();
                VisitChild( node.Default );
            }

            Context.AppendIndent().Append( "END" );

            if ( hasParentNode )
                Context.AppendShortIndent();
        }
    }

    public abstract void VisitRecordsAffectedFunction(SqlRecordsAffectedFunctionExpressionNode node);

    public virtual void VisitCoalesceFunction(SqlCoalesceFunctionExpressionNode node)
    {
        if ( node.Arguments.Length == 1 )
        {
            using ( Context.TempParentNodeUpdate( node ) )
                VisitChild( node.Arguments.Span[0] );
        }
        else
            VisitSimpleFunction( "COALESCE", node );
    }

    public abstract void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node);
    public abstract void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node);
    public abstract void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node);
    public abstract void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node);
    public abstract void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node);
    public abstract void VisitLengthFunction(SqlLengthFunctionExpressionNode node);
    public abstract void VisitToLowerFunction(SqlToLowerFunctionExpressionNode node);
    public abstract void VisitToUpperFunction(SqlToUpperFunctionExpressionNode node);
    public abstract void VisitTrimStartFunction(SqlTrimStartFunctionExpressionNode node);
    public abstract void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node);
    public abstract void VisitTrimFunction(SqlTrimFunctionExpressionNode node);
    public abstract void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node);
    public abstract void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node);
    public abstract void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node);
    public abstract void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node);
    public abstract void VisitSignFunction(SqlSignFunctionExpressionNode node);
    public abstract void VisitAbsFunction(SqlAbsFunctionExpressionNode node);
    public abstract void VisitCeilingFunction(SqlCeilingFunctionExpressionNode node);
    public abstract void VisitFloorFunction(SqlFloorFunctionExpressionNode node);
    public abstract void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node);
    public abstract void VisitPowerFunction(SqlPowerFunctionExpressionNode node);
    public abstract void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node);
    public abstract void VisitMinFunction(SqlMinFunctionExpressionNode node);
    public abstract void VisitMaxFunction(SqlMaxFunctionExpressionNode node);

    public virtual void VisitCustomFunction(SqlFunctionExpressionNode node)
    {
        throw new UnrecognizedSqlNodeException( this, node );
    }

    public abstract void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node);
    public abstract void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node);
    public abstract void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node);
    public abstract void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node);
    public abstract void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node);
    public abstract void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node);

    public virtual void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        throw new UnrecognizedSqlNodeException( this, node );
    }

    public virtual void VisitRawCondition(SqlRawConditionNode node)
    {
        AppendMultilineSql( node.Sql );

        foreach ( var parameter in node.Parameters.Span )
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
        using ( Context.TempParentNodeUpdate( node ) )
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
    }

    public virtual void VisitNotEqualTo(SqlNotEqualToConditionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
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
    }

    public virtual void VisitGreaterThan(SqlGreaterThanConditionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: ">", node.Right );
    }

    public virtual void VisitLessThan(SqlLessThanConditionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "<", node.Right );
    }

    public virtual void VisitGreaterThanOrEqualTo(SqlGreaterThanOrEqualToConditionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: ">=", node.Right );
    }

    public virtual void VisitLessThanOrEqualTo(SqlLessThanOrEqualToConditionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "<=", node.Right );
    }

    public virtual void VisitAnd(SqlAndConditionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
            VisitInfixBinaryOperator( node.Left, symbol: "AND", node.Right );
    }

    public virtual void VisitOr(SqlOrConditionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
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
        using ( Context.TempParentNodeUpdate( node ) )
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
    }

    public virtual void VisitExists(SqlExistsConditionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            if ( node.IsNegated )
                Context.Sql.Append( "NOT" ).AppendSpace();

            Context.Sql.Append( "EXISTS" ).AppendSpace();
            VisitChild( node.Query );
        }
    }

    public virtual void VisitLike(SqlLikeConditionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
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
    }

    public virtual void VisitIn(SqlInConditionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            VisitChild( node.Value );

            Context.Sql.AppendSpace();
            if ( node.IsNegated )
                Context.Sql.Append( "NOT" ).AppendSpace();

            Context.Sql.Append( "IN" ).AppendSpace().Append( '(' );

            foreach ( var expr in node.Expressions.Span )
            {
                VisitChild( expr );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 ).Append( ')' );
        }
    }

    public virtual void VisitInQuery(SqlInQueryConditionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            VisitChild( node.Value );

            Context.Sql.AppendSpace();
            if ( node.IsNegated )
                Context.Sql.Append( "NOT" ).AppendSpace();

            Context.Sql.Append( "IN" ).AppendSpace();
            VisitChild( node.Query );
        }
    }

    public virtual void VisitRawRecordSet(SqlRawRecordSetNode node)
    {
        Context.Sql.Append( node.BaseName );
        if ( node.IsAliased )
            AppendDelimitedAlias( node.Name );
    }

    public virtual void VisitTableRecordSet(SqlTableRecordSetNode node)
    {
        AppendSchemaObjectName( node.Table.Schema.Name, node.Table.Name );
        if ( node.IsAliased )
            AppendDelimitedAlias( node.Name );
    }

    public virtual void VisitTableBuilderRecordSet(SqlTableBuilderRecordSetNode node)
    {
        AppendSchemaObjectName( node.Table.Schema.Name, node.Table.Name );
        if ( node.IsAliased )
            AppendDelimitedAlias( node.Name );
    }

    public virtual void VisitViewRecordSet(SqlViewRecordSetNode node)
    {
        AppendSchemaObjectName( node.View.Schema.Name, node.View.Name );
        if ( node.IsAliased )
            AppendDelimitedAlias( node.Name );
    }

    public virtual void VisitViewBuilderRecordSet(SqlViewBuilderRecordSetNode node)
    {
        AppendSchemaObjectName( node.View.Schema.Name, node.View.Name );
        if ( node.IsAliased )
            AppendDelimitedAlias( node.Name );
    }

    public virtual void VisitQueryRecordSet(SqlQueryRecordSetNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            VisitChild( node.Query );
            AppendDelimitedAlias( node.Name );
        }
    }

    public virtual void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node)
    {
        AppendDelimitedName( node.CommonTableExpression.Name );
        if ( node.IsAliased )
            AppendDelimitedAlias( node.Alias );
    }

    public abstract void VisitTemporaryTableRecordSet(SqlTemporaryTableRecordSetNode node);

    public virtual void VisitJoinOn(SqlDataSourceJoinOnNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
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
    }

    public abstract void VisitDataSource(SqlDataSourceNode node);

    public virtual void VisitSelectField(SqlSelectFieldNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            if ( node.Alias is not null )
            {
                VisitChild( node.Expression );
                AppendDelimitedAlias( node.Alias );
            }
            else
                this.Visit( node.Expression );
        }
    }

    public virtual void VisitSelectCompoundField(SqlSelectCompoundFieldNode node)
    {
        AppendDelimitedName( node.Name );
    }

    public virtual void VisitSelectRecordSet(SqlSelectRecordSetNode node)
    {
        AppendRecordSetName( node.RecordSet );
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
        foreach ( var parameter in node.Parameters.Span )
            Context.AddParameter( parameter.Name, parameter.Type );

        if ( Context.ParentNode is not null )
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
        using ( Context.TempParentNodeUpdate( node ) )
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
    }

    public virtual void VisitDistinctTrait(SqlDistinctTraitNode node)
    {
        Context.Sql.Append( "DISTINCT" );
    }

    public virtual void VisitFilterTrait(SqlFilterTraitNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "WHERE" ).AppendSpace();
            this.Visit( node.Filter );
        }
    }

    public virtual void VisitAggregationTrait(SqlAggregationTraitNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "GROUP BY" );
            if ( node.Expressions.Length == 0 )
                return;

            Context.Sql.AppendSpace();
            foreach ( var expr in node.Expressions.Span )
            {
                VisitChild( expr );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }
    }

    public virtual void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "HAVING" ).AppendSpace();
            this.Visit( node.Filter );
        }
    }

    public virtual void VisitSortTrait(SqlSortTraitNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "ORDER BY" );
            if ( node.Ordering.Length == 0 )
                return;

            Context.Sql.AppendSpace();
            foreach ( var orderBy in node.Ordering.Span )
            {
                VisitOrderBy( orderBy );
                Context.Sql.AppendComma().AppendSpace();
            }

            Context.Sql.ShrinkBy( 2 );
        }
    }

    public abstract void VisitLimitTrait(SqlLimitTraitNode node);
    public abstract void VisitOffsetTrait(SqlOffsetTraitNode node);

    public virtual void VisitCommonTableExpressionTrait(SqlCommonTableExpressionTraitNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "WITH" );
            if ( node.CommonTableExpressions.Length == 0 )
                return;

            Context.Sql.AppendSpace();
            foreach ( var cte in node.CommonTableExpressions.Span )
            {
                VisitCommonTableExpression( cte );
                Context.Sql.AppendComma();
                Context.AppendIndent();
            }

            Context.Sql.ShrinkBy( Environment.NewLine.Length + Context.Indent + 1 );
        }
    }

    public virtual void VisitOrderBy(SqlOrderByNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            VisitChild( node.Expression );
            Context.Sql.AppendSpace().Append( node.Ordering.Name );
        }
    }

    public abstract void VisitCommonTableExpression(SqlCommonTableExpressionNode node);
    public abstract void VisitTypeCast(SqlTypeCastExpressionNode node);

    public virtual void VisitValues(SqlValuesNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
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
    }

    public abstract void VisitInsertInto(SqlInsertIntoNode node);
    public abstract void VisitUpdate(SqlUpdateNode node);

    public virtual void VisitValueAssignment(SqlValueAssignmentNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            AppendDelimitedName( node.DataField.Name );
            Context.Sql.AppendSpace().Append( '=' ).AppendSpace();
            VisitChild( node.Value );
        }
    }

    public abstract void VisitDeleteFrom(SqlDeleteFromNode node);
    public abstract void VisitColumnDefinition(SqlColumnDefinitionNode node);
    public abstract void VisitCreateTemporaryTable(SqlCreateTemporaryTableNode node);
    public abstract void VisitDropTemporaryTable(SqlDropTemporaryTableNode node);

    public virtual void VisitStatementBatch(SqlStatementBatchNode node)
    {
        if ( node.Statements.Length == 0 )
            return;

        foreach ( var statement in node.Statements.Span )
        {
            this.Visit( statement );
            Context.Sql.AppendSemicolon().AppendLine();
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

    public abstract void AppendRecordSetName(SqlRecordSetNode node);

    public void AppendDelimitedName(string name)
    {
        Context.Sql.Append( BeginNameDelimiter ).Append( name ).Append( EndNameDelimiter );
    }

    public void AppendDelimitedAlias(string alias)
    {
        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace();
        AppendDelimitedName( alias );
    }

    public void AppendMultilineSql(string sql)
    {
        if ( Context.Indent <= 0 )
        {
            Context.Sql.Append( sql );
            return;
        }

        var startIndex = 0;
        while ( startIndex < sql.Length )
        {
            var newLineIndex = sql.IndexOf( Environment.NewLine, startIndex, StringComparison.Ordinal );
            if ( newLineIndex < 0 )
            {
                Context.Sql.Append( sql.AsSpan( startIndex ) );
                break;
            }

            Context.Sql.Append( sql.AsSpan( startIndex, newLineIndex - startIndex ) ).Indent( Context.Indent );
            startIndex = newLineIndex + Environment.NewLine.Length;
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
        var distinct = @base.Distinct;
        var filter = @base.Filter;
        var aggregations = @base.Aggregations.ToExtendable();
        var aggregationFilter = @base.AggregationFilter;
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
                        commonTableExpressions = commonTableExpressions.Extend( cteTrait.CommonTableExpressions );

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
            distinct,
            filter,
            aggregations,
            aggregationFilter,
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
                        commonTableExpressions = commonTableExpressions.Extend( cteTrait.CommonTableExpressions );

                    break;
                }
                default:
                {
                    custom = custom.Extend( trait );
                    break;
                }
            }
        }

        return new SqlQueryTraits( commonTableExpressions, ordering, limit, offset, custom );
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
                default:
                {
                    custom = custom.Extend( trait );
                    break;
                }
            }
        }

        return new SqlAggregateFunctionTraits( distinct, filter, custom );
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

        using ( Context.TempIndentIncrease() )
            this.Visit( node );

        Context.Sql.Append( ')' );
    }

    protected void VisitSimpleFunction(string functionName, SqlFunctionExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( functionName ).Append( '(' );

            if ( node.Arguments.Length > 0 )
            {
                using ( Context.TempIndentIncrease() )
                {
                    foreach ( var arg in node.Arguments.Span )
                    {
                        VisitChild( arg );
                        Context.Sql.AppendComma().AppendSpace();
                    }
                }

                Context.Sql.ShrinkBy( 2 );
            }

            Context.Sql.Append( ')' );
        }
    }

    protected void VisitOptionalCommonTableExpressionRange(Chain<ReadOnlyMemory<SqlCommonTableExpressionNode>> commonTableExpressions)
    {
        if ( commonTableExpressions.Count == 0 )
            return;

        Context.Sql.Append( "WITH" ).AppendSpace();

        foreach ( var cteRange in commonTableExpressions )
        {
            foreach ( var cte in cteRange.Span )
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
            foreach ( var aggregation in aggregationRange.Span )
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

    protected void VisitOptionalOrderingRange(Chain<ReadOnlyMemory<SqlOrderByNode>> ordering)
    {
        if ( ordering.Count == 0 )
            return;

        Context.AppendIndent().Append( "ORDER BY" ).AppendSpace();

        foreach ( var orderByRange in ordering )
        {
            foreach ( var orderBy in orderByRange.Span )
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

    protected void AppendSchemaObjectName(string schemaName, string objName)
    {
        if ( schemaName.Length > 0 )
        {
            AppendDelimitedName( schemaName );
            Context.Sql.AppendDot();
        }

        AppendDelimitedName( objName );
    }

    [Pure]
    protected abstract bool DoesChildNodeRequireParentheses(SqlNodeBase node);
}
