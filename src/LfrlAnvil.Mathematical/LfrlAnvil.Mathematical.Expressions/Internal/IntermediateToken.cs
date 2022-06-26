using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

internal readonly struct IntermediateToken
{
    private IntermediateToken(IntermediateTokenType type, StringSlice symbol, MathExpressionTokenSet? tokenSet = null)
    {
        Type = type;
        Symbol = symbol;
        TokenSet = tokenSet;
    }

    public IntermediateTokenType Type { get; }
    public StringSlice Symbol { get; }
    public MathExpressionTokenSet? TokenSet { get; }

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] \"{Symbol}\" at {Symbol.StartIndex}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateOpenParenthesis(StringSlice symbol)
    {
        Debug.Assert( symbol.Equals( TokenConstants.OpenParenthesis ), "symbol is OpenParenthesis" );
        return new IntermediateToken( IntermediateTokenType.OpenParenthesis, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateCloseParenthesis(StringSlice symbol)
    {
        Debug.Assert( symbol.Equals( TokenConstants.CloseParenthesis ), "symbol is CloseParenthesis" );
        return new IntermediateToken( IntermediateTokenType.CloseParenthesis, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateInlineFunctionSeparator(StringSlice symbol)
    {
        Debug.Assert( symbol.Equals( TokenConstants.InlineFunctionSeparator ), "symbol is InlineFunctionSeparator" );
        return new IntermediateToken( IntermediateTokenType.InlineFunctionSeparator, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateFunctionParameterSeparator(StringSlice symbol)
    {
        Debug.Assert( symbol.Equals( TokenConstants.FunctionParameterSeparator ), "symbol is FunctionParameterSeparator" );
        return new IntermediateToken( IntermediateTokenType.FunctionParameterSeparator, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateMemberAccess(StringSlice symbol)
    {
        Debug.Assert( symbol.Equals( TokenConstants.MemberAccess ), "symbol is MemberAccess" );
        return new IntermediateToken( IntermediateTokenType.MemberAccess, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateStringConstant(StringSlice symbol)
    {
        Debug.Assert( symbol.Length > 0, "symbol.Length > 0" );
        return new IntermediateToken( IntermediateTokenType.StringConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateNumberConstant(StringSlice symbol)
    {
        Debug.Assert( symbol.Length > 0, "symbol.Length > 0" );
        var firstChar = symbol[0];
        Debug.Assert( char.IsDigit( firstChar ), "first symbol character is a digit" );

        return new IntermediateToken( IntermediateTokenType.NumberConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateBooleanConstant(StringSlice symbol)
    {
        Debug.Assert( TokenConstants.IsBooleanTrue( symbol ) || TokenConstants.IsBooleanFalse( symbol ), "symbol is boolean constant" );
        return new IntermediateToken( IntermediateTokenType.BooleanConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateTokenSet(StringSlice symbol, MathExpressionTokenSet tokens)
    {
        Debug.Assert( symbol.Length > 0, "symbol.Length > 0" );
        return new IntermediateToken( IntermediateTokenType.TokenSet, symbol, tokens );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateArgument(StringSlice symbol)
    {
        Debug.Assert( symbol.Length > 0, "symbol.Length > 0" );
        return new IntermediateToken( IntermediateTokenType.Argument, symbol );
    }
}
