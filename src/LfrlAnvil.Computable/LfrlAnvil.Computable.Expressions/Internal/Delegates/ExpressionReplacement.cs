using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal.Delegates;

internal readonly struct ExpressionReplacement
{
    internal readonly Expression Original;
    internal readonly Expression Replacement;

    internal ExpressionReplacement(Expression original)
    {
        Original = original;
        Replacement = original;
    }

    internal ExpressionReplacement(Expression original, Expression replacement)
    {
        Original = original;
        Replacement = replacement;
    }

    [Pure]
    public override string ToString()
    {
        return $"{Original} => {Replacement}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExpressionReplacement SetReplacement(Expression replacement)
    {
        return new ExpressionReplacement( Original, replacement );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool IsMatched(Expression expression)
    {
        return ReferenceEquals( Original, expression );
    }
}
