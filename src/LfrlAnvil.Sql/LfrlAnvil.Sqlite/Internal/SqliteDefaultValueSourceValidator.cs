using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteDefaultValueSourceValidator : SqliteSourceNodeValidator
{
    public override void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitSelectExpression(SqlSelectExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCompoundQueryComponent(SqlCompoundQueryComponentNode node)
    {
        AddForbiddenNode( node );
    }

    [Pure]
    internal Chain<string> GetErrors()
    {
        var errors = Chain<string>.Empty;
        if ( ForbiddenNodes is null || ForbiddenNodes.Count == 0 )
            return errors;

        foreach ( var node in ForbiddenNodes )
            errors = errors.Extend( ExceptionResources.UnexpectedNode( node ) );

        return errors;
    }
}
