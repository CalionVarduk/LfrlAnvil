using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public class SqlNodeMutatorContext
{
    [Pure]
    public SqlNodeBase Visit(SqlNodeBase node)
    {
        var mutator = new SqlNodeMutator( this );
        mutator.Visit( node );
        return mutator.GetResult();
    }

    [Pure]
    protected internal virtual MutationResult Mutate(SqlNodeBase node, SqlNodeAncestors ancestors)
    {
        return Node( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static MutationResult Node(SqlNodeBase value)
    {
        return new MutationResult( value, isNode: true );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static MutationResult Leaf(SqlNodeBase value)
    {
        return new MutationResult( value, isNode: false );
    }

    protected internal readonly struct MutationResult
    {
        internal MutationResult(SqlNodeBase value, bool isNode)
        {
            Value = value;
            IsNode = isNode;
        }

        public SqlNodeBase Value { get; }
        public bool IsNode { get; }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static implicit operator MutationResult(SqlNodeBase value)
        {
            return Leaf( value );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Deconstruct(out SqlNodeBase value, out bool isNode)
        {
            value = Value;
            isNode = IsNode;
        }
    }
}
