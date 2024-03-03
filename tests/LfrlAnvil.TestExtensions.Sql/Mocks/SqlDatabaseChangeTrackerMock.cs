using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlDatabaseChangeTrackerMock : SqlDatabaseChangeTracker
{
    protected override void CompletePendingCreateObjectChanges(SqlObjectBuilder obj)
    {
        AddAction( SqlDatabaseBuilderCommandAction.CreateSql( $"CREATE {obj};", ActionTimeout ) );
    }

    protected override void CompletePendingRemoveObjectChanges(SqlObjectBuilder obj)
    {
        AddAction( SqlDatabaseBuilderCommandAction.CreateSql( $"REMOVE {obj};", ActionTimeout ) );
    }

    protected override void CompletePendingAlterObjectChanges(SqlObjectBuilder obj, SqlDatabaseChangeAggregator changeAggregator)
    {
        var aggregator = ReinterpretCast.To<SqlDatabaseChangeAggregatorMock>( changeAggregator );
        var aggregatedChanges = string.Join( $"{Environment.NewLine}  ", aggregator.ChangeQueue.Order().Prepend( $"ALTER {obj}" ) );
        AddAction( SqlDatabaseBuilderCommandAction.CreateSql( $"{aggregatedChanges};", ActionTimeout ) );
    }

    [Pure]
    protected override SqlDatabaseChangeAggregatorMock CreateAlterObjectChangeAggregator()
    {
        return new SqlDatabaseChangeAggregatorMock( this );
    }

    internal new void SetModeAndAttach(SqlDatabaseCreateMode mode)
    {
        base.SetModeAndAttach( mode );
    }
}
