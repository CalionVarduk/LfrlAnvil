using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

internal sealed class MathExpressionTokenStack : IReadOnlyList<MathExpressionTokenStack.Entry>
{
    internal readonly struct Entry
    {
        internal readonly IntermediateToken Token;
        internal readonly MathExpressionBuilderState.Expectation Expectation;

        internal Entry(IntermediateToken token, MathExpressionBuilderState.Expectation expectation)
        {
            Token = token;
            Expectation = expectation;
        }
    }

    private readonly List<Entry> _expressions;

    internal MathExpressionTokenStack()
    {
        _expressions = new List<Entry>();
    }

    public int Count => _expressions.Count;
    public Entry this[int index] => _expressions[_expressions.Count - index - 1];

    public void Push(IntermediateToken token, MathExpressionBuilderState.Expectation expectation)
    {
        _expressions.Add( new Entry( token, expectation ) );
    }

    public Entry Pop()
    {
        var result = Peek();
        _expressions.RemoveLast();
        return result;
    }

    [Pure]
    public Entry Peek()
    {
        return _expressions[^1];
    }

    public bool TryPeek(out Entry result)
    {
        if ( _expressions.Count == 0 )
        {
            result = default;
            return false;
        }

        result = Peek();
        return true;
    }

    public bool TryPop(out Entry result)
    {
        if ( _expressions.Count == 0 )
        {
            result = default;
            return false;
        }

        result = Pop();
        return true;
    }

    [Pure]
    public IEnumerator<Entry> GetEnumerator()
    {
        return _expressions.GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
