using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

public sealed class MathExpressionOperandStack : IReadOnlyList<Expression>
{
    private readonly List<Expression> _expressions;

    internal MathExpressionOperandStack()
    {
        _expressions = new List<Expression>();
    }

    public int Count => _expressions.Count;
    public Expression this[int index] => _expressions[_expressions.Count - index - 1];

    public void Push(Expression operand)
    {
        _expressions.Add( operand );
    }

    public Expression Pop()
    {
        var result = _expressions[^1];
        _expressions.RemoveLast();
        return result;
    }

    [Pure]
    public IEnumerator<Expression> GetEnumerator()
    {
        return _expressions.GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
