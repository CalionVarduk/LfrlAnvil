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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents an object capable of recursive traversal over an SQL syntax tree that is responsible for
/// checking the validity of SQL syntax trees in the context of a single table e.g. columns of an index.
/// This validator also tracks all columns referenced by a syntax tree.
/// </summary>
public class SqlTableScopeExpressionValidator : SqlExpressionValidator
{
    private readonly Dictionary<ulong, SqlColumnBuilder> _referencedColumns;

    /// <summary>
    /// Creates a new <see cref="SqlTableScopeExpressionValidator"/> instance.
    /// </summary>
    /// <param name="table"><see cref="SqlTableBuilder"/> instance that defines available data fields.</param>
    protected internal SqlTableScopeExpressionValidator(SqlTableBuilder table)
    {
        Table = table;
        _referencedColumns = new Dictionary<ulong, SqlColumnBuilder>();
    }

    /// <summary>
    /// <see cref="SqlTableBuilder"/> instance that defines available data fields.
    /// </summary>
    public SqlTableBuilder Table { get; }

    /// <summary>
    /// Collection of all referenced columns.
    /// </summary>
    protected Dictionary<ulong, SqlColumnBuilder>.ValueCollection ReferencedColumns => _referencedColumns.Values;

    /// <inheritdoc cref="SqlNodeVisitor.VisitColumnBuilder(SqlColumnBuilderNode)" />
    /// <remarks>
    /// Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection only when
    /// it does not belong to the <see cref="Table"/>.
    /// </remarks>
    public override void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        if ( ! ReferenceEquals( node.Value.Table, Table ) )
        {
            AddForbiddenNode( node );
            return;
        }

        var column = ReinterpretCast.To<SqlColumnBuilder>( node.Value );
        if ( column.IsRemoved )
            AddForbiddenNode( node );
        else
            AddReferencedColumn( column );

        this.Visit( node.RecordSet );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitCurrentUtcDateTimeFunction(SqlCurrentUtcDateTimeFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitNamedAggregateFunction(SqlNamedAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitRowNumberWindowFunction(SqlRowNumberWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitRankWindowFunction(SqlRankWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitDenseRankWindowFunction(SqlDenseRankWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitCumulativeDistributionWindowFunction(SqlCumulativeDistributionWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitNTileWindowFunction(SqlNTileWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitLagWindowFunction(SqlLagWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitLeadWindowFunction(SqlLeadWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitFirstValueWindowFunction(SqlFirstValueWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitLastValueWindowFunction(SqlLastValueWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitNthValueWindowFunction(SqlNthValueWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitRawRecordSet(SqlRawRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitNamedFunctionRecordSet(SqlNamedFunctionRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc cref="SqlNodeVisitor.VisitTableBuilder(SqlTableBuilderNode)" />
    /// <remarks>
    /// Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection only when it is not <see cref="Table"/>.
    /// </remarks>
    public override void VisitTableBuilder(SqlTableBuilderNode node)
    {
        if ( ! ReferenceEquals( node.Table, Table ) )
            AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitViewBuilder(SqlViewBuilderNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitQueryRecordSet(SqlQueryRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitJoinOn(SqlDataSourceJoinOnNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitDataSource(SqlDataSourceNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitSelectField(SqlSelectFieldNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitSelectCompoundField(SqlSelectCompoundFieldNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitSelectRecordSet(SqlSelectRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitSelectAll(SqlSelectAllNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitRawQuery(SqlRawQueryExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitCompoundQuery(SqlCompoundQueryExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitCompoundQueryComponent(SqlCompoundQueryComponentNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitDistinctTrait(SqlDistinctTraitNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitFilterTrait(SqlFilterTraitNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitAggregationTrait(SqlAggregationTraitNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitSortTrait(SqlSortTraitNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitLimitTrait(SqlLimitTraitNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitOffsetTrait(SqlOffsetTraitNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitCommonTableExpressionTrait(SqlCommonTableExpressionTraitNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitWindowDefinitionTrait(SqlWindowDefinitionTraitNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitWindowTrait(SqlWindowTraitNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitOrderBy(SqlOrderByNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitWindowDefinition(SqlWindowDefinitionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitWindowFrame(SqlWindowFrameNode node)
    {
        AddForbiddenNode( node );
    }

    /// <summary>
    /// Returns a collection of all accumulated errors.
    /// </summary>
    /// <returns>Collection of all accumulated errors.</returns>
    [Pure]
    public virtual Chain<string> GetErrors()
    {
        var errors = Chain<string>.Empty;
        var forbiddenNode = ForbiddenNodes;
        if ( forbiddenNode.Length == 0 )
            return errors;

        foreach ( var node in forbiddenNode )
        {
            switch ( node.NodeType )
            {
                case SqlNodeType.ColumnBuilder:
                {
                    var builder = ReinterpretCast.To<SqlColumnBuilderNode>( node );
                    errors = errors.Extend(
                        ReferenceEquals( builder.Value.Table, Table )
                            ? ExceptionResources.ColumnIsArchived( builder )
                            : ExceptionResources.ColumnBelongsToAnotherTable( builder ) );

                    break;
                }
                default:
                    errors = errors.Extend( ExceptionResources.UnexpectedNode( node ) );
                    break;
            }
        }

        return errors;
    }

    /// <summary>
    /// Creates a new array from <see cref="ReferencedColumns"/>.
    /// </summary>
    /// <returns>New array from <see cref="ReferencedColumns"/>.</returns>
    [Pure]
    public SqlColumnBuilder[] GetReferencedColumns()
    {
        return ReferencedColumns.ToArray();
    }

    /// <summary>
    /// Adds the provided <paramref name="column"/> to <see cref="ReferencedColumns"/>.
    /// </summary>
    /// <param name="column">Column to add.</param>
    protected void AddReferencedColumn(SqlColumnBuilder column)
    {
        _referencedColumns.TryAdd( column.Id, column );
    }
}
