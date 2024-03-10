using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Objects.Builders;

public readonly struct SqlIndexBuilderColumns<T>
    where T : class, ISqlColumnBuilder
{
    public static readonly SqlIndexBuilderColumns<T> Empty = new SqlIndexBuilderColumns<T>( ReadOnlyArray<SqlOrderByNode>.Empty );

    public SqlIndexBuilderColumns(ReadOnlyArray<SqlOrderByNode> expressions)
    {
        Expressions = expressions;
    }

    public ReadOnlyArray<SqlOrderByNode> Expressions { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? TryGet(int index)
    {
        return TryGetColumnFromNode( Expressions[index].Expression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool IsExpression(int index)
    {
        return Expressions[index].Expression.NodeType != SqlNodeType.ColumnBuilder;
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( Expressions );
    }

    public struct Enumerator
    {
        private ReadOnlyArray<SqlOrderByNode>.Enumerator _enumerator;

        internal Enumerator(ReadOnlyArray<SqlOrderByNode> expressions)
        {
            _enumerator = expressions.GetEnumerator();
        }

        public T? Current => TryGetColumnFromNode( _enumerator.Current.Expression );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static T? TryGetColumnFromNode(SqlExpressionNode node)
    {
        return node.NodeType == SqlNodeType.ColumnBuilder
            ? ReinterpretCast.To<T>( ReinterpretCast.To<SqlColumnBuilderNode>( node ).Value )
            : null;
    }
}
