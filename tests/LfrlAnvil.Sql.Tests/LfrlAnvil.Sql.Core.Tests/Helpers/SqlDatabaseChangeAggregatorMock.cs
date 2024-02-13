using System.Collections.Generic;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlDatabaseChangeAggregatorMock : SqlDatabaseChangeAggregator
{
    public SqlDatabaseChangeAggregatorMock(SqlDatabaseChangeTrackerMock changes)
        : base( changes )
    {
        ChangeQueue = new List<string>();
    }

    public List<string> ChangeQueue { get; }

    protected override void HandleCreation(SqlObjectBuilder obj)
    {
        ChangeQueue.Add( $"CREATE {obj}" );
    }

    protected override void HandleRemoval(SqlObjectBuilder obj)
    {
        ChangeQueue.Add( $"REMOVE {obj}" );
    }

    protected override void HandleModification(SqlObjectBuilder obj, SqlObjectChangeDescriptor descriptor, object? originalValue)
    {
        var originalValueText = originalValue is null ? "<null>" : originalValue.ToString() ?? string.Empty;
        ChangeQueue.Add( $"ALTER {obj} ({descriptor} FROM {originalValueText})" );
    }

    public override void Clear()
    {
        ChangeQueue.Clear();
    }
}
