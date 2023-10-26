using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
using LfrlAnvil.Sqlite.Exceptions;

namespace LfrlAnvil.Sqlite;

public class SqliteNodeInterpreter : SqlNodeInterpreter
{
    private ComplexUpdateInfo _updateInfo;

    public SqliteNodeInterpreter(SqliteColumnTypeDefinitionProvider columnTypeDefinitions, SqlNodeInterpreterContext context)
        : base( context, beginNameDelimiter: '"', endNameDelimiter: '"' )
    {
        ColumnTypeDefinitions = columnTypeDefinitions;
        _updateInfo = default;
    }

    public SqliteColumnTypeDefinitionProvider ColumnTypeDefinitions { get; }
    protected ComplexUpdateInfo UpdateInfo => _updateInfo;

    public override void VisitRawDataField(SqlRawDataFieldNode node)
    {
        if ( ! TryReplaceDataField( node ) )
            base.VisitRawDataField( node );
    }

    public override void VisitLiteral(SqlLiteralNode node)
    {
        var sql = node.GetSql( ColumnTypeDefinitions );
        Context.Sql.Append( sql );
    }

    public override void VisitColumn(SqlColumnNode node)
    {
        if ( ! TryReplaceDataField( node ) )
            base.VisitColumn( node );
    }

    public override void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        if ( ! TryReplaceDataField( node ) )
            base.VisitColumnBuilder( node );
    }

    public override void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        if ( ! TryReplaceDataField( node ) )
            base.VisitQueryDataField( node );
    }

    public override void VisitViewDataField(SqlViewDataFieldNode node)
    {
        if ( ! TryReplaceDataField( node ) )
            base.VisitViewDataField( node );
    }

    public override void VisitModulo(SqlModuloExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "MOD" ).Append( '(' );
            VisitChild( node.Left );
            Context.Sql.AppendComma().AppendSpace();
            VisitChild( node.Right );
            Context.Sql.Append( ')' );
        }
    }

    public override void VisitBitwiseXor(SqlBitwiseXorExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
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
    }

    public override void VisitRecordsAffectedFunction(SqlRecordsAffectedFunctionExpressionNode node)
    {
        VisitSimpleFunction( "CHANGES", node );
    }

    public override void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node)
    {
        VisitSimpleFunction( "GET_CURRENT_DATE", node );
    }

    public override void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node)
    {
        VisitSimpleFunction( "GET_CURRENT_TIME", node );
    }

    public override void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node)
    {
        VisitSimpleFunction( "GET_CURRENT_DATETIME", node );
    }

    public override void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        VisitSimpleFunction( "GET_CURRENT_TIMESTAMP", node );
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
        VisitSimpleFunction( "LTRIM", node );
    }

    public override void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node)
    {
        VisitSimpleFunction( "RTRIM", node );
    }

    public override void VisitTrimFunction(SqlTrimFunctionExpressionNode node)
    {
        VisitSimpleFunction( "TRIM", node );
    }

    public override void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node)
    {
        VisitSimpleFunction( "SUBSTR", node );
    }

    public override void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node)
    {
        VisitSimpleFunction( "REPLACE", node );
    }

    public override void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node)
    {
        VisitSimpleFunction( "INSTR", node );
    }

    public override void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node)
    {
        VisitSimpleFunction( "INSTR_LAST", node );
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
        {
            using ( Context.TempParentNodeUpdate( node ) )
                VisitChild( node.Arguments.Span[0] );
        }
        else
            VisitSimpleFunction( "MIN", node );
    }

    public override void VisitMaxFunction(SqlMaxFunctionExpressionNode node)
    {
        if ( node.Arguments.Length == 1 )
        {
            using ( Context.TempParentNodeUpdate( node ) )
                VisitChild( node.Arguments.Span[0] );
        }
        else
            VisitSimpleFunction( "MAX", node );
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
        VisitSimpleAggregateFunction( "GROUP_CONCAT", node );
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

    public override void VisitDataSource(SqlDataSourceNode node)
    {
        if ( node is SqlDummyDataSourceNode )
            return;

        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "FROM" ).AppendSpace();
            this.Visit( node.From );

            foreach ( var join in node.Joins )
            {
                Context.AppendIndent();
                VisitJoinOn( join );
            }
        }
    }

    public override void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        var hasParentNode = Context.ParentNode is not null;
        if ( hasParentNode )
            Context.AppendIndent();

        using ( Context.TempParentNodeUpdate( node ) )
        {
            var traits = ExtractDataSourceTraits( ExtractDataSourceTraits( node.DataSource.Traits ), node.Traits );
            VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions );
            VisitDataSourceQuerySelection( node, traits.Distinct );
            VisitDataSource( node.DataSource );
            VisitOptionalFilterCondition( traits.Filter );
            VisitOptionalAggregationRange( traits.Aggregations );
            VisitOptionalAggregationFilterCondition( traits.AggregationFilter );
            VisitOptionalOrderingRange( traits.Ordering );
            VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
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
            var traits = ExtractQueryTraits( default, node.Traits );
            VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions );
            VisitCompoundQueryComponents( node );
            VisitOptionalOrderingRange( traits.Ordering );
            VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
        }

        if ( hasParentNode )
            Context.AppendShortIndent();
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

    public override void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            if ( node.IsRecursive )
                Context.Sql.Append( "RECURSIVE" ).AppendSpace();

            AppendDelimitedName( node.Name );
            Context.Sql.AppendSpace().Append( "AS" ).AppendSpace();
            VisitChild( node.Query );
        }
    }

    public override void VisitTypeCast(SqlTypeCastExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "CAST" ).Append( '(' );
            VisitChild( node.Value );
            Context.Sql.AppendSpace().Append( "AS" ).AppendSpace();

            var typeDefinition = ColumnTypeDefinitions.GetByType( node.TargetType );
            Context.Sql.Append( typeDefinition.DbType.Name ).Append( ')' );
        }
    }

    public override void VisitInsertInto(SqlInsertIntoNode node)
    {
        switch ( node.Source.NodeType )
        {
            case SqlNodeType.DataSourceQuery:
            {
                using ( Context.TempParentNodeUpdate( node ) )
                {
                    var query = ReinterpretCast.To<SqlDataSourceQueryExpressionNode>( node.Source );
                    var traits = ExtractDataSourceTraits( ExtractDataSourceTraits( query.DataSource.Traits ), query.Traits );

                    VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions );
                    VisitInsertIntoFields( node );

                    Context.AppendIndent();
                    VisitDataSourceQuerySelection( query, traits.Distinct );
                    VisitDataSource( query.DataSource );
                    VisitOptionalFilterCondition( traits.Filter );
                    VisitOptionalAggregationRange( traits.Aggregations );
                    VisitOptionalAggregationFilterCondition( traits.AggregationFilter );
                    VisitOptionalOrderingRange( traits.Ordering );
                    VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
                }

                break;
            }
            case SqlNodeType.CompoundQuery:
            {
                using ( Context.TempParentNodeUpdate( node ) )
                {
                    var query = ReinterpretCast.To<SqlCompoundQueryExpressionNode>( node.Source );
                    var traits = ExtractQueryTraits( query.Traits );

                    VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions );
                    VisitInsertIntoFields( node );

                    Context.AppendIndent();
                    VisitCompoundQueryComponents( query );
                    VisitOptionalOrderingRange( traits.Ordering );
                    VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
                }

                break;
            }
            default:
            {
                using ( Context.TempParentNodeUpdate( node ) )
                    VisitInsertIntoFields( node );

                Context.AppendIndent();
                this.Visit( node.Source );
                break;
            }
        }
    }

    public override void VisitUpdate(SqlUpdateNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            var traits = ExtractDataSourceTraits( node.DataSource.Traits );
            VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions );

            if ( IsUpdateOrDeleteDataSourceSimple( node.DataSource, traits ) )
            {
                Context.Sql.Append( "UPDATE" ).AppendSpace();
                AppendDelimitedRecordSetName( node.DataSource.From );
                VisitUpdateAssignmentRange( node.Assignments.Span );
                VisitOptionalFilterCondition( traits.Filter );
                return;
            }

            var targetInfo = ExtractTargetUpdateInfo( node );

            ComplexUpdateAssignmentsVisitor? updateVisitor = null;
            if ( node.DataSource.Joins.Length > 0 )
            {
                updateVisitor = new ComplexUpdateAssignmentsVisitor( node.DataSource );
                foreach ( var assignment in node.Assignments )
                    updateVisitor.Visit( assignment.Value );
            }

            if ( updateVisitor is null || ! updateVisitor.ContainsDataFieldsToReplace() )
            {
                Context.Sql.Append( "UPDATE" ).AppendSpace();
                AppendDelimitedRecordSetName( targetInfo.BaseTarget );
                VisitUpdateAssignmentRange( node.Assignments.Span );
                VisitComplexDeleteOrUpdateDataSourceFilter( targetInfo, node.DataSource, traits );
            }
            else
                VisitUpdateWithComplexAssignments( targetInfo, node, traits, updateVisitor );
        }
    }

    public override void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            var traits = ExtractDataSourceTraits( node.DataSource.Traits );
            VisitOptionalCommonTableExpressionRange( traits.CommonTableExpressions );
            Context.Sql.Append( "DELETE" ).AppendSpace().Append( "FROM" ).AppendSpace();

            if ( IsUpdateOrDeleteDataSourceSimple( node.DataSource, traits ) )
            {
                AppendDelimitedRecordSetName( node.DataSource.From );
                VisitOptionalFilterCondition( traits.Filter );
                return;
            }

            var targetInfo = ExtractTargetDeleteInfo( node );
            AppendDelimitedRecordSetName( targetInfo.BaseTarget );
            VisitComplexDeleteOrUpdateDataSourceFilter( targetInfo, node.DataSource, traits );
        }
    }

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

    public override void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        var typeDefinition = ColumnTypeDefinitions.GetByType( node.Type.BaseType );
        AppendDelimitedName( node.Name );
        Context.Sql.AppendSpace().Append( typeDefinition.DbType.Name );

        if ( ! node.Type.IsNullable )
            Context.Sql.AppendSpace().Append( "NOT" ).AppendSpace().Append( "NULL" );

        if ( node.DefaultValue is not null )
        {
            Context.Sql.AppendSpace().Append( "DEFAULT" ).AppendSpace();

            using ( Context.TempParentNodeUpdate( node ) )
                VisitChildWrappedInParentheses( node.DefaultValue );
        }
    }

    public override void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node)
    {
        Context.Sql.Append( "CONSTRAINT" ).AppendSpace();
        AppendDelimitedName( node.Name );
        Context.Sql.AppendSpace().Append( "PRIMARY" ).AppendSpace().Append( "KEY" ).AppendSpace().Append( '(' );

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
        Context.Sql.Append( "CONSTRAINT" ).AppendSpace();
        AppendDelimitedName( node.Name );
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
        using ( Context.TempParentNodeUpdate( node ) )
        {
            Context.Sql.Append( "CONSTRAINT" ).AppendSpace();
            AppendDelimitedName( node.Name );
            Context.Sql.AppendSpace().Append( "CHECK" ).AppendSpace();
            VisitChildWrappedInParentheses( node.Predicate );
        }
    }

    public override void VisitCreateTable(SqlCreateTableNode node)
    {
        using ( SwapIgnoredRecordSet( node.RecordSet ) )
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

            Context.AppendIndent().Append( ')' ).AppendSpace().Append( "WITHOUT" ).AppendSpace().Append( "ROWID" );
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
        using ( SwapIgnoredRecordSet( node.Table ) )
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
        Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendSpace().Append( "RENAME" ).AppendSpace().Append( "TO" ).AppendSpace();
        AppendDelimitedSchemaObjectName( node.NewName );
    }

    public override void VisitRenameColumn(SqlRenameColumnNode node)
    {
        Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendSpace().Append( "RENAME" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        AppendDelimitedName( node.OldName );
        Context.Sql.AppendSpace().Append( "TO" ).AppendSpace();
        AppendDelimitedName( node.NewName );
    }

    public override void VisitAddColumn(SqlAddColumnNode node)
    {
        Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendSpace().Append( "ADD" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        VisitColumnDefinition( node.Definition );
    }

    public override void VisitDropColumn(SqlDropColumnNode node)
    {
        Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        AppendDelimitedRecordSetInfo( node.Table );
        Context.Sql.AppendSpace().Append( "DROP" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
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

    public override void AppendDelimitedRecordSetName(SqlRecordSetNode node)
    {
        if ( node.IsAliased )
        {
            AppendDelimitedName( node.Alias );
            return;
        }

        if ( node.NodeType == SqlNodeType.RawRecordSet )
        {
            Context.Sql.Append( node.Info.Name.Object );
            return;
        }

        AppendDelimitedRecordSetInfo( node.Info );
    }

    public sealed override void AppendDelimitedTemporaryObjectName(string name)
    {
        Context.Sql.Append( "temp" ).AppendDot();
        AppendDelimitedName( name );
    }

    public sealed override void AppendDelimitedSchemaObjectName(string schemaName, string objName)
    {
        Context.Sql.Append( BeginNameDelimiter );
        AppendSchemaObjectName( schemaName, objName );
        Context.Sql.Append( EndNameDelimiter );
    }

    protected void AppendSchemaObjectName(string schemaName, string objName)
    {
        if ( schemaName.Length > 0 )
            Context.Sql.Append( schemaName ).Append( '_' );

        Context.Sql.Append( objName );
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

    protected void VisitSimpleAggregateFunction(string functionName, SqlAggregateFunctionExpressionNode node)
    {
        using ( Context.TempParentNodeUpdate( node ) )
        {
            var traits = ExtractAggregateFunctionTraits( node.Traits );
            Context.Sql.Append( functionName ).Append( '(' );

            if ( node.Arguments.Length > 0 )
            {
                using ( Context.TempIndentIncrease() )
                {
                    if ( traits.Distinct is not null )
                    {
                        VisitDistinctTrait( traits.Distinct );
                        Context.Sql.AppendSpace();
                    }

                    foreach ( var arg in node.Arguments )
                    {
                        VisitChild( arg );
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

        Context.AppendIndent().Append( "LIMIT" ).AppendSpace().Append( '-' ).Append( '1' );
        Context.Sql.AppendSpace().Append( "OFFSET" ).AppendSpace();
        VisitChild( offset );
    }

    protected void VisitInsertIntoFields(SqlInsertIntoNode node)
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

    [Pure]
    protected static bool IsUpdateOrDeleteDataSourceSimple(SqlDataSourceNode node, SqlDataSourceTraits traits)
    {
        return ! node.From.IsAliased &&
            node.RecordSets.Count == 1 &&
            traits.Distinct is null &&
            traits.Aggregations.Count == 0 &&
            traits.AggregationFilter is null &&
            traits.Ordering.Count == 0 &&
            traits.Limit is null &&
            traits.Offset is null &&
            traits.Custom.Count == 0;
    }

    protected void VisitComplexDeleteOrUpdateDataSourceFilter(
        TargetDeleteOrUpdateInfo targetInfo,
        SqlDataSourceNode dataSource,
        SqlDataSourceTraits traits)
    {
        Context.AppendIndent().Append( "WHERE" ).AppendSpace();

        if ( targetInfo.IdentityColumnNames.Length == 1 )
        {
            AppendDelimitedRecordSetName( targetInfo.BaseTarget );
            Context.Sql.AppendDot();
            AppendDelimitedName( targetInfo.IdentityColumnNames[0] );
            Context.Sql.AppendSpace().Append( "IN" ).AppendSpace().Append( '(' );

            using ( Context.TempIndentIncrease() )
            {
                Context.AppendIndent().Append( "SELECT" );
                VisitOptionalDistinctMarker( traits.Distinct );
                Context.Sql.AppendSpace();
                AppendDelimitedRecordSetName( targetInfo.Target );
                Context.Sql.AppendDot();
                AppendDelimitedName( targetInfo.IdentityColumnNames[0] );

                Context.AppendIndent();
                VisitDataSource( dataSource );
                VisitOptionalFilterCondition( traits.Filter );
                VisitOptionalAggregationRange( traits.Aggregations );
                VisitOptionalAggregationFilterCondition( traits.AggregationFilter );
                VisitOptionalOrderingRange( traits.Ordering );
                VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
            }
        }
        else
        {
            Context.Sql.Append( "EXISTS" ).AppendSpace().Append( '(' );

            using ( Context.TempIndentIncrease() )
            {
                Context.AppendIndent().Append( "SELECT" );
                VisitOptionalDistinctMarker( traits.Distinct );
                Context.Sql.AppendSpace().Append( '*' );

                Context.AppendIndent();
                VisitDataSource( dataSource );
                Context.AppendIndent().Append( "WHERE" ).AppendSpace();

                foreach ( var columnName in targetInfo.IdentityColumnNames )
                {
                    Context.Sql.Append( '(' );
                    AppendDelimitedRecordSetName( targetInfo.BaseTarget );
                    Context.Sql.AppendDot();
                    AppendDelimitedName( columnName );
                    Context.Sql.AppendSpace().Append( '=' ).AppendSpace();
                    AppendDelimitedRecordSetName( targetInfo.Target );
                    Context.Sql.AppendDot();
                    AppendDelimitedName( columnName );
                    Context.Sql.Append( ')' ).AppendSpace().Append( "AND" ).AppendSpace();
                }

                if ( traits.Filter is null )
                    Context.Sql.ShrinkBy( 5 );
                else
                    VisitChild( traits.Filter );

                VisitOptionalAggregationRange( traits.Aggregations );
                VisitOptionalAggregationFilterCondition( traits.AggregationFilter );
                VisitOptionalOrderingRange( traits.Ordering );
                VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
            }
        }

        Context.AppendIndent().Append( ')' );
    }

    protected void VisitUpdateWithComplexAssignments(
        TargetDeleteOrUpdateInfo targetInfo,
        SqlUpdateNode node,
        SqlDataSourceTraits traits,
        ComplexUpdateAssignmentsVisitor updateVisitor)
    {
        Assume.IsNotNull( targetInfo.Target.Alias, nameof( targetInfo.Target.Alias ) );
        var cteName = $"_{Guid.NewGuid():N}";

        if ( traits.CommonTableExpressions.Count == 0 )
            Context.Sql.Append( "WITH" ).AppendSpace();
        else
        {
            Context.Sql.ShrinkBy( Environment.NewLine.Length + Context.Indent ).AppendComma();
            Context.AppendIndent();
        }

        AppendDelimitedName( cteName );
        Context.Sql.AppendSpace().Append( "AS" ).AppendSpace().Append( '(' );

        using ( Context.TempIndentIncrease() )
        {
            Context.AppendIndent().Append( "SELECT" );
            VisitOptionalDistinctMarker( traits.Distinct );

            using ( Context.TempIndentIncrease() )
            {
                foreach ( var identityColumnName in targetInfo.IdentityColumnNames )
                {
                    Context.AppendIndent();
                    AppendDelimitedRecordSetName( targetInfo.Target );
                    Context.Sql.AppendDot();
                    AppendDelimitedName( identityColumnName );
                    Context.Sql.AppendSpace().Append( "AS" ).AppendSpace().Append( BeginNameDelimiter );
                    Context.Sql.Append( targetInfo.Target.Alias ).Append( '_' ).Append( identityColumnName );
                    Context.Sql.Append( EndNameDelimiter ).AppendComma();
                }

                foreach ( var dataField in updateVisitor.GetDataFieldsToReplace() )
                {
                    Context.AppendIndent();
                    this.Visit( dataField );
                    Context.Sql.AppendSpace().Append( "AS" ).AppendSpace();
                    AppendDelimitedPrefixedReplacementDataFieldName( dataField );
                    Context.Sql.AppendComma();
                }

                Context.Sql.ShrinkBy( 1 );
            }

            Context.AppendIndent();
            VisitDataSource( node.DataSource );
            VisitOptionalFilterCondition( traits.Filter );
            VisitOptionalAggregationRange( traits.Aggregations );
            VisitOptionalAggregationFilterCondition( traits.AggregationFilter );
            VisitOptionalOrderingRange( traits.Ordering );
            VisitOptionalLimitAndOffsetExpressions( traits.Limit, traits.Offset );
        }

        Context.AppendIndent().Append( ')' );
        Context.AppendIndent();

        Context.Sql.Append( "UPDATE" ).AppendSpace();
        AppendDelimitedRecordSetName( targetInfo.BaseTarget );

        if ( ! UpdateInfo.IsDefault )
            throw new SqlNodeVisitorException( Resources.NestedUpdateAttempt, this, node );

        try
        {
            _updateInfo = new ComplexUpdateInfo( updateVisitor, targetInfo, cteName );
            VisitUpdateAssignmentRange( node.Assignments.Span );
            Context.AppendIndent().Append( "WHERE" ).AppendSpace();

            if ( targetInfo.IdentityColumnNames.Length == 1 )
            {
                AppendDelimitedRecordSetName( targetInfo.BaseTarget );
                Context.Sql.AppendDot();
                AppendDelimitedName( targetInfo.IdentityColumnNames[0] );
                Context.Sql.AppendSpace().Append( "IN" ).AppendSpace().Append( '(' ).Append( "SELECT" ).AppendSpace();
                Context.Sql.Append( BeginNameDelimiter ).Append( targetInfo.Target.Alias ).Append( '_' );
                Context.Sql.Append( targetInfo.IdentityColumnNames[0] ).Append( EndNameDelimiter );
                Context.Sql.AppendSpace().Append( "FROM" ).AppendSpace();
                AppendDelimitedName( cteName );
                Context.Sql.Append( ')' );
            }
            else
            {
                Context.Sql.Append( "EXISTS" ).AppendSpace().Append( '(' );
                Context.Sql.Append( "SELECT" ).AppendSpace().Append( '*' ).AppendSpace().Append( "FROM" ).AppendSpace();
                AppendDelimitedName( cteName );
                Context.Sql.AppendSpace().Append( "WHERE" ).AppendSpace();
                _updateInfo.AppendIdentityColumnsFilter( this );
                Context.Sql.Append( ')' );
            }
        }
        finally
        {
            _updateInfo = default;
        }
    }

    protected void AppendDelimitedPrefixedReplacementDataFieldName(SqlDataFieldNode node)
    {
        Context.Sql.Append( BeginNameDelimiter );

        if ( node.RecordSet.IsAliased )
            Context.Sql.Append( node.RecordSet.Alias );
        else if ( node.RecordSet.Info.IsTemporary )
            Context.Sql.Append( "temp" ).Append( '_' ).Append( node.RecordSet.Info.Name.Object );
        else
            AppendSchemaObjectName( node.RecordSet.Info.Name.Schema, node.RecordSet.Info.Name.Object );

        Context.Sql.Append( '_' ).Append( node.Name ).Append( EndNameDelimiter );
    }

    protected bool TryReplaceDataField(SqlDataFieldNode node)
    {
        if ( ! _updateInfo.ShouldReplaceDataField( node ) )
            return false;

        Context.Sql.Append( '(' ).Append( "SELECT" ).AppendSpace();
        AppendDelimitedPrefixedReplacementDataFieldName( node );
        Context.Sql.AppendSpace().Append( "FROM" ).AppendSpace();
        AppendDelimitedName( _updateInfo.CteName );
        Context.Sql.AppendSpace().Append( "WHERE" ).AppendSpace();
        _updateInfo.AppendIdentityColumnsFilter( this );
        Context.Sql.AppendSpace().Append( "LIMIT" ).AppendSpace().Append( '1' ).Append( ')' );
        return true;
    }

    [Pure]
    protected static TargetDeleteOrUpdateInfo ExtractTableDeleteOrUpdateInfo(SqlTableNode node)
    {
        var identityColumns = node.Table.PrimaryKey.Index.Columns.Span;
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

    protected readonly struct ComplexUpdateInfo
    {
        private readonly string? _cteName;

        public ComplexUpdateInfo(ComplexUpdateAssignmentsVisitor visitor, TargetDeleteOrUpdateInfo targetInfo, string cteName)
        {
            Visitor = visitor;
            TargetInfo = targetInfo;
            _cteName = cteName;
        }

        [MemberNotNullWhen( false, nameof( Visitor ) )]
        public bool IsDefault => Visitor is null;

        public ComplexUpdateAssignmentsVisitor? Visitor { get; }
        public TargetDeleteOrUpdateInfo TargetInfo { get; }

        public string CteName
        {
            get
            {
                Assume.IsNotNull( _cteName, nameof( _cteName ) );
                return _cteName;
            }
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool ShouldReplaceDataField(SqlDataFieldNode node)
        {
            return Visitor?.ShouldReplaceDataField( node ) == true;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void AppendIdentityColumnsFilter(SqliteNodeInterpreter interpreter)
        {
            Assume.IsNotNull( TargetInfo.Target.Alias, nameof( TargetInfo.Target.Alias ) );

            foreach ( var identityColumnName in TargetInfo.IdentityColumnNames )
            {
                interpreter.Context.Sql.Append( '(' );
                interpreter.AppendDelimitedRecordSetName( TargetInfo.BaseTarget );
                interpreter.Context.Sql.AppendDot();
                interpreter.AppendDelimitedName( identityColumnName );
                interpreter.Context.Sql.AppendSpace().Append( '=' ).AppendSpace();
                interpreter.AppendDelimitedName( CteName );
                interpreter.Context.Sql.AppendDot().Append( interpreter.BeginNameDelimiter );
                interpreter.Context.Sql.Append( TargetInfo.Target.Alias ).Append( '_' );
                interpreter.Context.Sql.Append( identityColumnName ).Append( interpreter.EndNameDelimiter );
                interpreter.Context.Sql.Append( ')' ).AppendSpace().Append( "AND" ).AppendSpace();
            }

            interpreter.Context.Sql.ShrinkBy( 5 );
        }
    }

    protected sealed class ComplexUpdateAssignmentsVisitor : SqlNodeVisitor
    {
        private readonly SqlRecordSetNode[] _joinedRecordSets;
        private List<SqlDataFieldNode>? _dataFieldsToReplace;

        public ComplexUpdateAssignmentsVisitor(SqlDataSourceNode dataSource)
        {
            Assume.IsGreaterThan( dataSource.Joins.Length, 0, nameof( dataSource.Joins.Length ) );

            var index = 0;
            _joinedRecordSets = new SqlRecordSetNode[dataSource.Joins.Length];
            foreach ( var join in dataSource.Joins )
                _joinedRecordSets[index++] = join.InnerRecordSet;

            _dataFieldsToReplace = null;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool ContainsDataFieldsToReplace()
        {
            return _dataFieldsToReplace is not null;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool ShouldReplaceDataField(SqlDataFieldNode node)
        {
            return _dataFieldsToReplace is not null && _dataFieldsToReplace.Contains( node );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ReadOnlySpan<SqlDataFieldNode> GetDataFieldsToReplace()
        {
            return CollectionsMarshal.AsSpan( _dataFieldsToReplace );
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

        private void VisitDataField(SqlDataFieldNode node)
        {
            if ( Array.IndexOf( _joinedRecordSets, node.RecordSet ) == -1 )
                return;

            if ( _dataFieldsToReplace is null )
            {
                _dataFieldsToReplace = new List<SqlDataFieldNode> { node };
                return;
            }

            if ( ! _dataFieldsToReplace.Contains( node ) )
                _dataFieldsToReplace.Add( node );
        }
    }

    [Pure]
    private TargetDeleteOrUpdateInfo ExtractTableBuilderDeleteOrUpdateInfo(
        SqlNodeBase source,
        SqlTableBuilderNode node,
        bool isUpdate)
    {
        string[] identityColumnNames;
        var primaryKey = node.Table.PrimaryKey;

        if ( primaryKey is not null )
        {
            var identityColumns = primaryKey.Index.Columns.Span;
            identityColumnNames = new string[identityColumns.Length];
            for ( var i = 0; i < identityColumns.Length; ++i )
                identityColumnNames[i] = identityColumns[i].Column.Name;
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
