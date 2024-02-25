using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlDatabaseChangeTrackerMock : SqlDatabaseChangeTracker
{
    protected override SqlDatabaseBuilderCommandAction? PrepareCreateObjectAction(SqlObjectBuilder obj)
    {
        return SqlDatabaseBuilderCommandAction.CreateSql( $"CREATE {obj};", ActionTimeout );
    }

    protected override SqlDatabaseBuilderCommandAction? PrepareRemoveObjectAction(SqlObjectBuilder obj)
    {
        return SqlDatabaseBuilderCommandAction.CreateSql( $"REMOVE {obj};", ActionTimeout );
    }

    protected override SqlDatabaseBuilderCommandAction? PrepareAlterObjectAction(
        SqlObjectBuilder obj,
        SqlDatabaseChangeAggregator changeAggregator)
    {
        var aggregator = ReinterpretCast.To<SqlDatabaseChangeAggregatorMock>( changeAggregator );
        var aggregatedChanges = string.Join( $"{Environment.NewLine}  ", aggregator.ChangeQueue.Order().Prepend( $"ALTER {obj}" ) );
        return SqlDatabaseBuilderCommandAction.CreateSql( $"{aggregatedChanges};", ActionTimeout );
    }

    [Pure]
    protected override SqlDatabaseChangeAggregatorMock CreateAlterObjectChangeAggregator()
    {
        return new SqlDatabaseChangeAggregatorMock( this );
    }
}
