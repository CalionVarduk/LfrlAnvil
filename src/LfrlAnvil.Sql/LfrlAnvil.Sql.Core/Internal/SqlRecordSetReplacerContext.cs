using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Internal;

internal sealed class SqlRecordSetReplacerContext : SqlNodeMutatorContext
{
    private readonly SqlRecordSetNode _original;
    private readonly SqlRecordSetNode _replacement;

    internal SqlRecordSetReplacerContext(SqlRecordSetNode original, SqlRecordSetNode replacement)
    {
        _original = original;
        _replacement = replacement;
    }

    protected internal override MutationResult Mutate(SqlNodeBase node, SqlNodeAncestors ancestors)
    {
        return ReferenceEquals( node, _original ) ? _replacement : base.Mutate( node, ancestors );
    }
}
