using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents an aggregator of changes of a single altered <see cref="SqlObjectBuilder"/> instance.
/// </summary>
public abstract class SqlDatabaseChangeAggregator
{
    /// <summary>
    /// Creates a new <see cref="SqlDatabaseChangeAggregator"/> instance.
    /// </summary>
    /// <param name="changes"><see cref="SqlDatabaseChangeTracker"/> that this aggregator belongs to.</param>
    protected SqlDatabaseChangeAggregator(SqlDatabaseChangeTracker changes)
    {
        Changes = changes;
    }

    /// <summary>
    /// <see cref="SqlDatabaseChangeTracker"/> that this aggregator belongs to.
    /// </summary>
    protected SqlDatabaseChangeTracker Changes { get; }

    /// <summary>
    /// Registers a new change.
    /// </summary>
    /// <param name="target">Created, removed or modified object.</param>
    /// <param name="descriptor">Change descriptor.</param>
    /// <param name="originalValue">Original value before the change.</param>
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
                Assume.True( Changes.ContainsChange( target, SqlObjectChangeDescriptor.IsRemoved ) );
                if ( descriptor.Equals( SqlObjectChangeDescriptor.IsRemoved ) )
                    HandleRemoval( target );

                break;
            default:
                HandleModification( target, descriptor, originalValue );
                break;
        }
    }

    /// <summary>
    /// Resets the state of this aggregator.
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// Handler for a new object creation.
    /// </summary>
    /// <param name="obj">Created object.</param>
    protected abstract void HandleCreation(SqlObjectBuilder obj);

    /// <summary>
    /// Handler for an object removal.
    /// </summary>
    /// <param name="obj">Removed object.</param>
    protected abstract void HandleRemoval(SqlObjectBuilder obj);

    /// <summary>
    /// Handler for an object modification.
    /// </summary>
    /// <param name="obj">Modified object.</param>
    /// <param name="descriptor">Change descriptor.</param>
    /// <param name="originalValue">Original value before the change.</param>
    protected abstract void HandleModification(SqlObjectBuilder obj, SqlObjectChangeDescriptor descriptor, object? originalValue);
}
