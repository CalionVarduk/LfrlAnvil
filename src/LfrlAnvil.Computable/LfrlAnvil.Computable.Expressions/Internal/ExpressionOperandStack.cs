using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class ExpressionOperandStack : IReadOnlyList<Expression>
{
    private const int BaseCapacity = 7;

    private Expression?[] _expressions;

    internal ExpressionOperandStack()
    {
        _expressions = new Expression?[BaseCapacity];
        Count = 0;
    }

    public int Count { get; private set; }

    public Expression this[int index] => _expressions[Count - index - 1]!;

    internal void Push(Expression operand)
    {
        if ( Count == _expressions.Length )
        {
            var newExpressions = new Expression?[(_expressions.Length << 1) + 1];
            for ( var i = 0; i < Count; ++i )
                newExpressions[i] = _expressions[i];

            _expressions = newExpressions;
        }

        _expressions[Count++] = operand;
    }

    internal Expression Pop()
    {
        Assume.IsGreaterThan( Count, 0, nameof( Count ) );

        var index = Count-- - 1;
        var result = _expressions[index]!;
        _expressions[index] = null;
        return result;
    }

    internal void Pop(int count)
    {
        Assume.IsInRange( count, 1, Count, nameof( count ) );

        Count -= count;
        Array.Clear( _expressions, Count, count );
    }

    internal void PopInto(int count, Expression[] buffer, int startIndex)
    {
        Assume.IsInRange( count, 0, Count, nameof( count ) );
        Assume.IsLessThanOrEqualTo( startIndex + count, buffer.Length, nameof( startIndex ) + '+' + nameof( count ) );

        if ( count == 0 )
            return;

        var oldCount = Count;
        Count -= count;

        for ( var i = Count; i < oldCount; ++i )
            buffer[i - Count + startIndex] = _expressions[i]!;

        Array.Clear( _expressions, Count, count );
    }

    [Pure]
    public IEnumerator<Expression> GetEnumerator()
    {
        return _expressions.Take( Count ).GetEnumerator()!;
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
