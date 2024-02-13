using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

public abstract class SqlDatabaseChangeAggregator
{
    protected SqlDatabaseChangeAggregator(SqlDatabaseChangeTracker changes)
    {
        Changes = changes;
    }

    protected SqlDatabaseChangeTracker Changes { get; }

    public void Add(SqlObjectBuilder target, SqlObjectChangeDescriptor descriptor, object? originalValue)
    {
        var state = ReferenceEquals( Changes.ActiveObject, target )
            ? Changes.ActiveObjectExistenceState
            : Changes.GetExistenceState( target );

        switch ( state )
        {
            case SqlObjectExistenceState.Created:
                Assume.Equals( descriptor, SqlObjectChangeDescriptor.IsRemoved );
                HandleCreation( target );
                break;
            case SqlObjectExistenceState.Removed:
                Assume.Equals( Changes.ContainsChange( target, SqlObjectChangeDescriptor.IsRemoved ), true );
                if ( descriptor.Equals( SqlObjectChangeDescriptor.IsRemoved ) )
                    HandleRemoval( target );

                break;
            default:
                HandleModification( target, descriptor, originalValue );
                break;
        }
    }

    public abstract void Clear();

    protected abstract void HandleCreation(SqlObjectBuilder obj);
    protected abstract void HandleRemoval(SqlObjectBuilder obj);
    protected abstract void HandleModification(SqlObjectBuilder obj, SqlObjectChangeDescriptor descriptor, object? originalValue);
}
