using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents an object capable of recursive traversal over an SQL syntax tree that is responsible for
/// checking the validity of SQL syntax trees in the context of a constant expression e.g. column's default value.
/// </summary>
public class SqlConstantExpressionValidator : SqlExpressionValidator
{
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

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection.</remarks>
    public override void VisitTableBuilder(SqlTableBuilderNode node)
    {
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
        var forbiddenNodes = ForbiddenNodes;
        if ( forbiddenNodes.Length == 0 )
            return errors;

        foreach ( var node in forbiddenNodes )
            errors = errors.Extend( ExceptionResources.UnexpectedNode( node ) );

        return errors;
    }
}
